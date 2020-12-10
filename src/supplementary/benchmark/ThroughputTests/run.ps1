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

        $clumsy = clumsy $clumsyArgs &
    }

    $arguments = @(
        "--certificate-file", "certs\cert.crt",
        "--key-file", "certs\cert.key",
        "--connections", $Connections,
        "--streams", $Streams,
        "--message-size", $MessageSize,
        "--reporting-interval", $Duration,
        # "--test-duration", $Duration,
        # "--reporting-interval", 0,
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
        $procArgs = "$PSScriptRoot\bin\Release\net6.0\ThroughputTests.dll", "inproc" + $arguments

        Write-Debug "$procArgs"

        $pinfo = New-Object System.Diagnostics.ProcessStartInfo
        $pinfo.FileName = "dotnet"
        $pinfo.RedirectStandardOutput = $true
        $pinfo.UseShellExecute = $false
        $pinfo.WorkingDirectory = "$PSScriptRoot\bin\Release\net6.0"
        $procArgs | ForEach-Object { $pinfo.ArgumentList.Add($_) }
        $p = New-Object System.Diagnostics.Process
        $p.StartInfo = $pinfo
        $p.Start() | Out-Null

        $out = $p.StandardOutput.ReadLine(),$p.StandardOutput.ReadLine()

        Write-Debug "$res"

        if ($p.HasExited)
        {
            Write-Warning "Process exited prematurely, trying again"
            continue
        }

        # we have what we need
        $p | kill

        $res = $out | ConvertFrom-Csv |
            Add-Member -MemberType NoteProperty -Name Impl -Value $Impl -PassThru |
            Add-Member -MemberType NoteProperty -Name Streams -Value $Streams -PassThru |
            Add-Member -MemberType NoteProperty -Name Connections -Value $Connections -PassThru |
            Add-Member -MemberType NoteProperty -Name MessageSize -Value $MessageSize -PassThru

        $res.PSObject.Properties.Remove('Drift (%)')
        $res.PSObject.Properties.Remove('Current RPS')

        if (!$res.'Average RPS')
        {
            Write-Warning "Run seems to be corrupted, retrying"

            Write-Debug $out[0]
            Write-Debug $out[1]
            Write-Debug $p.StandardOutput.ReadToEnd()
            continue
        }

        break;
    }

    if ($NetworkOpts)
    {
        $clumsy | Stop-Job

        foreach($key in $NetworkOpts.Keys)
        {
            $res = $res |
              Add-Member -MemberType NoteProperty -Name $Key -Value ($NetworkOpts.$key) -PassThru
        }
    }

    $res
}

function AugmentParameterSet
{
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline)]
        [Hashtable[]] $ParameterSet,

        [Parameter()]
        [Hashtable] $Augment
    )

    foreach ($set in $ParameterSet)
    {
        $newSet = $set.Clone();

        foreach ($key in $Augment.Keys)
        {
            $newSet.$key = $Augment.$key
        }

        $newSet
    }
}

function ProduceParameterSets([Parameter(ValueFromPipeline)]$Parameters)
{
    # "transpose" the params array
    $paramSets = @(@{})

    foreach ($key in $Parameters.Keys)
    {
        $paramSets = $Parameters.$key | ForEach-Object {
            AugmentParameterSet -ParameterSet $paramSets -Augment @{$key = $_}
        }
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
        $Columns,

        [Parameter(Mandatory)]
        [string] $OutFile
    )

    $Parameters | ForEach-Object { Run @_ @ExtraArgs } |
      Select-Object -Property $columns |
      ConvertTo-Csv | Tee-Object -File $OutFile | ConvertFrom-Csv |
      Format-Table
}

function RunAll
{
    $multiStreamSetsBase = @{
        Impl = "managed", 'msquic'
        Connections = 1, 4, 16, 64, 256
    } | ProduceParameterSets

    $multiStreamSets = @{Streams=1;MessageSize=256},@{Streams=32;MessageSize=4096} | ForEach-Object {
        AugmentParameterSet -ParameterSet $multiStreamSetsBase -Augment $_
    }

    # $singleStreamSets = @{
    #     Impl = "tcp", "managed", 'msquic'
    #     Connections = 1, 8, 32, 128, 512
    #     MessageSize = 256, 4096
    # } | ProduceParameterSets
    $singleStreamSets = @{
        Impl = "tcp", "managed", 'msquic'
        Connections = 512
        MessageSize = 4096
    } | ProduceParameterSets

    $lossSets = @{
        Impl = "tcp", "managed", 'msquic'
        Connections = 1
        MessageSize = 1024
        NetworkOpts = @(@{Lag=1; Ood=5}, @{Lag=1; Drop=1; Ood=5})
    } | ProduceParameterSets


    $cols = "Impl","Connections","MessageSize"
    $Runs = @{
        # "Single stream perf - latency" = @{
        #     Parameters = $singleStreamSets

        #     ExtraArgs = @{
        #         Duration = 5
        #         WarmupTime = 5
        #     }

        #     Columns = $cols + $extraColumnsLatency

        #     OutFile = "single-stream-latency.csv"
        # }
        # "Single stream perf - throughput" = @{
        #     Parameters = $singleStreamSets

        #     ExtraArgs = @{
        #         Duration = 5
        #         WarmupTime = 5
        #         Throughput = $true
        #     }

        #     Columns = $cols + $extraColumns

        #     OutFile = "single-stream-throughput_append.csv"
        # }
        # "Multi stream perf - throughput" = @{
        #     Parameters = $multiStreamSets

        #     ExtraArgs = @{
        #         Duration = 5
        #         WarmupTime = 5
        #         Throughput = $true
        #     }

        #     Columns = $cols + "Streams" + $extraColumns

        #     OutFile = "multi-stream-throughput.csv"
        # }
        # "Multi stream perf - latency" = @{
        #     Parameters = $multiStreamSets

        #     ExtraArgs = @{
        #         Duration = 5
        #         WarmupTime = 5
        #     }

        #     Columns = $cols + "Streams" + $extraColumnsLatency

        #     OutFile = "multi-stream-latency.csv"
        # }
        "Loss - latency" = @{
            Parameters = $lossSets

            ExtraArgs = @{
                Duration = 5
                WarmupTime = 5
            }

            Columns = $cols  + ,"Drop" + $extraColumnsLatency

            OutFile = "loss-latency_append.csv"
        }
        "Loss - Throughput" = @{
            Parameters = $lossSets

            ExtraArgs = @{
                Duration = 5
                WarmupTime = 5
                Throughput = $true
            }

            Columns = $cols + ,"Drop" + $extraColumns

            OutFile = "loss-throughput_append.csv"
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
