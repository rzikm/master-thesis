##requires -RunAsAdministrator

function Run
{
    [CmdletBinding()]
    param(
        [Parameter()]
        [int] $Connections = 1,

        [Parameter()]
        [int] $Streams = 1,

        [Parameter()]
        [int] $MessageSize = 256,

        [Parameter()]
        [double] $Duration = 5,

        [Parameter()]
        [double] $WarmupTime = 5,

        [Parameter()]
        [switch] $Throughput,

        [Parameter()]
        [Hashtable] $NetworkOpts,

        [Parameter()]
        [ValidateSet("tcp", "managed", "msquic", "mock")]
        [string] $Impl = "managed"
    )

    if ($NetworkOpts)
    {
        # init clumsy process
        $clumsyArgs = @(
            "--filter", "outbound and ip.DstAddr >= 127.0.0.1 and ip.DstAddr <= 127.255.255.255"
        )

        if ($NetworkOpts.Lag)
        {
            $clumsyArgs += "--lag", "on", "--lag-outbound", "on", "--lag-time", $NetworkOpts.Lag
        }

        if ($NetworkOpts.Drop)
        {
            $clumsyArgs += "--drop", "on", "--drop-outbound", "on", "--drop-chance", $NetworkOpts.Drop
        }

        if ($NetworkOpts.Ood)
        {
            $clumsyArgs += "--ood", "on", "--ood-outbound", "on", "--ood-chance", $NetworkOpts.Ood
        }

        $clumsyArgs

        $clumsy = clumsy $clumsyArgs &
    }

    $arguments = @(
        "--certificate-file", "$PSScriptRoot\..\..\..\..\certs\cert.crt",
        "--key-file", "$PSScriptRoot\..\..\..\..\certs\cert.key",
        "--connections", $Connections,
        "--streams", $Streams,
        "--message-size", $MessageSize,
        "--reporting-interval", 0,
        "--test-duration", $Duration,
        "--csv-output"
    )

    if ($Throughput)
    {
        $arguments += "--no-wait"
    }

    if ($Impl -eq "tcp")
    {
        $Env:DOTNETQUIC_PROVIDER = ""
        $arguments += "--tcp"
    }
    else
    {
        $Env:DOTNETQUIC_PROVIDER = $Impl
    }

    while ($true)
    {
        $procArgs = "run", "-c", "Release", "--no-build", "--", "inproc" + $arguments

        $pinfo = New-Object System.Diagnostics.ProcessStartInfo
        $pinfo.FileName = "dotnet"
        $pinfo.RedirectStandardOutput = $true
        $pinfo.UseShellExecute = $false
        $pinfo.WorkingDirectory = $PSScriptRoot
        $procArgs | ForEach-Object { $pinfo.ArgumentList.Add($_) }
        $p = New-Object System.Diagnostics.Process
        $p.StartInfo = $pinfo
        $p.Start() | Out-Null

        $res = $p.StandardOutput.ReadLine(),$p.StandardOutput.ReadLine()

        # we have what we need
        $p | kill


        $p | Wait-Process -Timeout ($WarmupTime + $Duration + 15) -ErrorAction SilentlyContinue -ErrorVariable timeouted
        if ($timeouted)
        {
            # just to be sure, check the output
            $p | kill
            $res = $p.StandardOutput.ReadToEnd().Split([Environment]::NewLine)[0,1]
            if (!$res[0] -or !$res[1] -or $res.Length -ne 2)
            {
                Write-Warning "Run failed due to timeout, trying again"
                continue
            }
        }
        elseif ($p.ExitCode -ne 0)
        {
            Write-Warning "Process exited with nonzero, trying again"
            continue
        }
        else
        {
            $res = $p.StandardOutput.ReadToEnd().Split([Environment]::NewLine)[0,1]
        }

        $res = $res | ConvertFrom-Csv |
        Add-Member -MemberType NoteProperty -Name Impl -Value $Impl -PassThru |
        Add-Member -MemberType NoteProperty -Name Streams -Value $Streams -PassThru |
        Add-Member -MemberType NoteProperty -Name Connections -Value $Connections -PassThru |
        Add-Member -MemberType NoteProperty -Name MessageSize -Value $MessageSize -PassThru

        $res.PSObject.Properties.Remove('Drift (%)')
        $res.PSObject.Properties.Remove('Current RPS')

        $res
        break;
    }

    if ($NetworkOpts)
    {
        $clumsy | Stop-Job
    }
}

function ProduceParameterSets([Parameter(ValueFromPipeline)]$Parameters)
{
    # "transpose" the params array
    $paramSets = @(@{})

    foreach ($key in $Parameters.Keys)
    {
        $newParameterSets = @()

        foreach($value in $Parameters.$key)
        {
            foreach($set in $paramSets)
            {
                $newSet = $set.Clone();
                $newSet.$key = $value

                $newParameterSets += $newSet
            }
        }

        $paramSets = $newParameterSets
    }

    $paramSets
}

$extraColumns = "Average RPS", "Throughput (MiB/s)"
$extraColumnsLatency = $extraColumns + "Latency-avg (ms)", "Latency-p50 (ms)", "Latency-p95 (ms)", "Latency-p99 (ms)"

function RunMeasurementSet
{
    param(
        [Parameter(Mandatory)]
        $Parameters,

        [Parameter(Mandatory)]
        $ExtraArgs,

        [Parameter(Mandatory)]
        $ExtraColumns,

        [Parameter(Mandatory)]
        [string] $OutFile
    )

    $columns = $Parameters.keys + $ExtraColumns

    $Parameters | ProduceParameterSets | ForEach-Object { Run @_ @ExtraArgs } |
      Select-Object -Property $columns |
      ConvertTo-Csv | Tee-Object -File result.csv | ConvertFrom-Csv |
      Format-Table
}

function RunAll
{
    # single stream perf - latency
    $Runs = @{
        "Single stream perf - latency" = @{
            Parameters = @{
                Impl = "tcp", "managed", 'msquic'
                Connections = 1, 4, 16, 64, 256
                MessageSize = 256, 1024, 4096
            }

            ExtraArgs = @{
                Duration = 5
                WarmupTime = 5
            }

            ExtraColumns = $extraColumnsLatency

            OutFile = "single-stream-latency.csv"
        }
        "Single stream perf - throughput" = @{
            Parameters = @{
                Impl = "managed", 'msquic'
                Connections = 1, 4, 16, 64, 256
                MessageSize = 256, 1024, 4096
                Streams = 1, 4, 16
            }

            ExtraArgs = @{
                Duration = 5
                WarmupTime = 5
                Throughput = $true
            }

            ExtraColumns = $extraColumns

            OutFile = "single-stream-throughput.csv"
        }
        "Loss - latency" = @{
            Parameters = @{
                Impl = "tcp", "managed", 'msquic'
                Connections = 1
                MessageSize = 256, 1024, 4096
            }

            ExtraArgs = @{
                Duration = 5
                WarmupTime = 5
                NetworkOpts = @{
                    Lag = 5
                    Drop = 2
                }
            }

            ExtraColumns = $extraColumnsLatency

            OutFile = "loss-latency.csv"
        }
        "Loss - Throughput" = @{
            Parameters = @{
                Impl = "tcp", "managed", 'msquic'
                Connections = 1
                MessageSize = 256, 1024, 4096
            }

            ExtraArgs = @{
                Duration = 5
                WarmupTime = 5
                Throughput = $true
                NetworkOpts = @{
                    Lag = 5
                    Drop = 2
                }
            }

            ExtraColumns = $extraColumns

            OutFile = "loss-throughput.csv"
        }
    }

    foreach($run in $Runs.Keys)
    {
        Write-Host "$($run):"

        $params = $Runs.$run

        RunMeasurementSet @params
    }
}

RunAll
