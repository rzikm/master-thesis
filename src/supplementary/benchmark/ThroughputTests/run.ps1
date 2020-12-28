<#

This script can be used to reproduce the performance measurements using the ThroughputTests
benchmarking application. The tests can be executed locally, or on remote machines (via PowerShell
remoting)

When using RunAll and RunAllDistributed functions, the output is both displayed on the standard
output (using Format-Table) and saved in a CSV file. The name of the file is separate for each type
of test and is configured in this script (search for OutFile property in GetAllRuns function).

Important note for running with simulated loss:

    Measurements in lossy environments are supported only on Linux via the Traffic Control
    capabilities of the Linux kernel itself. This script manually calls the `tc` utility to set
    desired values. However, this requires administrator privileges, so the session must be logged
    in as root in order to simulate loss.

Usage:

To start running the tests, first source this file using following statement. This is needed only
once at the beginning.
PS> . ./run.ps1

# Run all tests locally
PS> RunAll

# Run all tests remotely, this requires PowerShell to be installed also on remote machines.
# For more info on PowerShell Remoting (including setup guide for Windows and Linux), see
# (https://docs.microsoft.com/en-us/powershell/scripting/learn/remoting/ssh-remoting-in-powershell-core?view=powershell-7.1)
PS> # the sessions need to be setup just once
PS> $serverSession = New-PSSession -HostName ... -Username ...
PS> $clientSession = New-PSSession -HostName ... -Username ...
PS> # change to the directory where the ThroughputTests binaries are located
PS> Invoke-Command $serverSession, $clientSession { cd /path/to/ThroughputTests }
PS> # now run the actual tests. results are stored on the calling machine (as if run locally)
PS> RunAllDistributed -ServerSession $serverSession -ClientSession $clientSession

# Run (selected) individual tests, the results of these runs are returned as a PSCustomObject
PS> Run -Impl managed -Connections 1 -MessageSize 256 -Streams 8 -Duration 15 -WarmupTime 5 -Throughput

# You can also run selected tests on remote machines by supplying the remoting sessions.
PS> Run -ServerSession $serverSession -ClientSession $clientSession -Impl managed ....

#>

## Following constants should be adapted as needed

# address (or hostname) on which the server can be contacted from the client if run distributed
$RemoteServerAddress = "10.10.50.21"

# The network interface that should set
$networkInterface = "eth0"

# port on which to contact client, note that TCP+SSL always uses $serverPort + 1
$serverPort = 5000

# name of the binary to execute, this must be present in the working directory
$binPath = "ThroughputTests"

# path to the working directory when run locally. this expects that the script (and commands), are
# run from the ThroughputTests project directory. When run remotely, this value is ignored. Instead,
# PowerShell sessions must be 'cd' to correct directory
$workDir = "bin/Release/net6.0/"

# path to the certificate file (relative to working directory of the app process)
$certificateFile = "Certs/cert.crt"

# path to the certificate private key file (relative to working directory of the app process)
$keyFile = "Certs/cert.key"

function GetAllRuns
{
    $duration = 15
    $warmup = 5

    $smallMessageSize = 256
    $medMessageSize = 1024
    $bigMessageSize = 4096

    # Calls to ProduceParameterSets transform produces a list parameter sets for individual runs.
    # This produces all possible combinations of the values specified here

    $multiStreamSets = @{
        Impl = "managed", 'msquic'
        Connections = 1, 4, 16, 64, 256
        Streams = 1, 32
        MessageSize = $smallMessageSize, $bigMessageSize
    } | ProduceParameterSets

    $singleStreamSets = @{
        Impl = "tcp", "managed", 'msquic'
        Connections = 1, 8, 32, 128, 512
        MessageSize = $smallMessageSize, $bigMessageSize
    } | ProduceParameterSets

    $lossSets = @{
        Impl = "tcp", "managed", 'msquic'
        Connections = 1
        MessageSize = $smallMessageSize, $medMessageSize, $bigMessageSize
        NetworkOpts = @(@{Lag=25; Drop=0.004}, @{Lag=25; Drop=0.1})
    } | ProduceParameterSets

    $multiLossSets = @{
        Impl = "tcp", "managed", 'msquic'
        Connections = 1
        Streams = 32 # this will be set to 1 for tcp
        MessageSize = $smallMessageSize, $medMessageSize, $bigMessageSize
        NetworkOpts = @(@{Lag=25; Drop=0.004}, @{Lag=25; Drop=0.1})
    } | ProduceParameterSets

    # Individual runs can be disabled by commenting them out

    $cols = "Impl", "Connections", "Streams", "MessageSize"
    $Runs = [ordered]@{
        # "Single stream perf - latency" = @{
        #     Parameters = $singleStreamSets

        #     ExtraArgs = @{
        #         Duration = $duration
        #         WarmupTime = $warmup
        #     }

        #     Columns = $cols + $extraColumnsLatency

        #     OutFile = "single-stream-latency.csv"
        # }
        # "Single stream perf - throughput" = @{
        #     Parameters = $singleStreamSets

        #     ExtraArgs = @{
        #         Duration = $duration
        #         WarmupTime = $warmup
        #         Throughput = $true
        #     }

        #     Columns = $cols + $extraColumns

        #     OutFile = "single-stream-throughput.csv"
        # }
        # "Multi stream perf - throughput" = @{
        #     Parameters = $multiStreamSets

        #     ExtraArgs = @{
        #         Duration = $duration
        #         WarmupTime = $warmup
        #         Throughput = $true
        #     }

        #     Columns = $cols + $extraColumns

        #     OutFile = "multi-stream-throughput.csv"
        # }
        # "Multi stream perf - latency" = @{
        #     Parameters = $multiStreamSets

        #     ExtraArgs = @{
        #         Duration = $duration
        #         WarmupTime = $warmup
        #     }

        #     Columns = $cols + $extraColumnsLatency

        #     OutFile = "multi-stream-latency_append.csv"
        # }
        # "Loss - latency" = @{
        #     Parameters = $lossSets

        #     ExtraArgs = @{
        #         Duration = $duration
        #         WarmupTime = $warmup
        #     }

        #     Columns = $cols + "Lag", "Drop" + $extraColumnsLatency

        #     OutFile = "loss-latency.csv"
        # }
        "Loss - Throughput" = @{
            Parameters = $lossSets

            ExtraArgs = @{
                Duration = $duration
                WarmupTime = $warmup
                Throughput = $true
            }

            Columns = $cols + "Lag", "Drop" + $extraColumns

            OutFile = "loss-throughput.csv"
        }
        # "MultiLoss - Throughput" = @{
        #     Parameters = $multiLossSets

        #     ExtraArgs = @{
        #         Duration = $duration
        #         WarmupTime = $warmup
        #         Throughput = $true
        #     }

        #     Columns = $cols + "Lag", "Drop" + $extraColumns

        #     OutFile = "multi-loss-throughput.csv"
        # }
    }

    $Runs
}

function Run
{
    [CmdletBinding(DefaultParameterSetName = "Local")]
    param(
        # Session on which to start the client process, if not provided, the client will run in the
        # current session
        [Parameter(ParameterSetName = "Distributed", Mandatory)]
        [Parameter()]
        $ClientSession,

        # Session on which to start the server process, if not provided, the server will run in the
        # current session
        [Parameter(ParameterSetName = "Distributed", Mandatory)]
        [Parameter()]
        $ServerSession,

        # Hostname or IP address by which client session can contact the server, without port
        # This parameter is set automatically either to loopback or $remoteServerAddress
        [Parameter()]
        [string] $ServerHostName,

        # Path to the directory with the benchmarking application binaries.
        [Parameter(ParameterSetName = "Local")]
        [string] $WorkingDirectory,

        # Number of samples to collect
        [Parameter()]
        [ValidateRange(1, [int]::MaxValue)]
        [int] $Samples = 1,

        # Number of parallel connections to make
        [Parameter()]
        [int] $Connections = 1,

        # Number of parallel streams in each connection. If $Impl is 'tcp', then this parameter is
        # ignored.
        [Parameter()]
        [int] $Streams = 1,

        # Size of the messages exchanged between server and clients
        [Parameter()]
        [int] $MessageSize = 256,

        # Time for which to collect data before reporting the results
        [Parameter()]
        [double] $Duration = 5,

        # Time for which the data collection should be delayed to let the implementation be JITed
        [Parameter()]
        [double] $WarmupTime = 5,

        # If set, then the application sends as much data as possible to measure throughput of the
        # implementation
        [Parameter()]
        [switch] $Throughput,

        # Sets the congestion control algorithm to be used by the managed implementation
        # The default is New Reno. The CUBIC cc algorithm is not fully implemented.
        [Parameter()]
        [ValidateSet("NewReno", "Cubic")]
        [string] $ManagedCCAlg = "NewReno",

        # Additional network options, Supported only on Linux and requires root privileges
        [Parameter()]
        [Hashtable] $NetworkOpts,

        # Implementation to use for running
        [Parameter()]
        [ValidateSet("tcp", "managed", "msquic", "mock")]
        [string] $Impl = "managed"
    )

    # helper function for handling distributed and non-distributed version of the measurement
    function InvokeHelper
    {
        param(
            $Session,

            [ScriptBlock] $ScriptBlock,

            $ArgumentList
        )

        if ($Session)
        {
            Invoke-Command -Session $Session -ScriptBlock $ScriptBlock -ArgumentList $ArgumentList
        }
        else
        {
            Invoke-Command -ScriptBlock $ScriptBlock -ArgumentList $ArgumentList
        }
    }

    # Infer default parameter values, if not provided
    if ($PSCmdlet.ParameterSetName -eq "Distributed")
    {
        if (!$ServerHostName)
        {
            $ServerHostName = $remoteServerAddress
        }
    }
    else # Local
    {
        if (!$ServerHostName)
        {
            $ServerHostName = "127.0.0.1"
        }
        if (!$WorkingDirectory)
        {
            $WorkingDirectory = Resolve-Path $workDir
        }
    }

    $clientArgs = @("client")

    if ($Throughput)
    {
        $clientArgs += "--no-wait"
    }

    if ($Impl -eq "tcp")
    {
        $clientArgs += "--tcp"
        $clientArgs += "--endpoint","$($ServerHostName):$($serverPort + 1)"

        # enforce valid number of streams for TCP
        $Streams = 1
    }
    else
    {
        $clientArgs += "--endpoint","$($ServerHostName):$($serverPort)"
    }

    $clientArgs +=
        "--connections", $Connections,
        "--streams", $Streams,
        "--message-size", $MessageSize,
        "--reporting-interval", $Duration,
        "--csv-output"

    $serverArgs = @(
        "server",
        "--endpoint","*:$($serverPort)"
        "--certificate-file", $certificateFile,
        "--key-file", $keyFile
    )

    $provider = ""
    if ($Impl -eq 'msquic')
    {
        $provider = "msquic"
    }

    # helper function for starting process with redirected IO streams. Return ID of the process
    function StartProcess
    {
        param($bin, $workDir, $argumentList)

        $pinfo = New-Object System.Diagnostics.ProcessStartInfo
        $pinfo.FileName = Join-Path $workDir $bin
        $pinfo.RedirectStandardOutput = $true
        $pinfo.RedirectStandardError = $true
        $pinfo.UseShellExecute = $false
        $pinfo.WorkingDirectory = $workDir
        $argumentList | ForEach-Object { $pinfo.ArgumentList.Add($_) }
        $p = New-Object System.Diagnostics.Process
        $p.StartInfo = $pinfo
        $null = $p.Start()

        return $p
    }

    function SetTrafficControl
    {
        param($device, $delay, $loss)

        # remove any existing setting if present
        $current = tc qdisc show dev $device
        if ($current -match "(delay)|(loss)")
        {
            tc qdisc del dev $device root netem
        }

        $pars = "limit", "1000000"

        if ($delay)
        {
            $pars += "delay", "$($delay / 2)ms"
        }

        if ($loss)
        {
            $pars += "loss", "$($loss)%"
        }

        tc qdisc add dev $device root netem @pars
    }

    function UnsetTrafficControl
    {
        param($device)

        tc qdisc del dev $device root netem
    }

    # copy the function definition because we will be possibly executing the stuff on other machines
    $envDef = $function:StartProcess, $function:SetTrafficControl, $function:UnsetTrafficControl |
      ForEach-Object { $_.Ast.Extent.Text } | Out-String


    if ($NetworkOpts)
    {
        InvokeHelper `
          -Session $ServerSession, $ClientSession `
          -ArgumentList $envDef, $networkInterface, $NetworkOpts `
          {
              param($env, $device, $netOpts)

              # pull the definition of the functions into the scope
              Invoke-Expression $env

              if (!$IsLinux)
              {
                  Write-Warning "Setting network opts is supported only on Linux"
                  return
              }

              SetTrafficControl $device $netOpts.Lag $netOpts.Drop
          }
    }

    # repeat until success
    while ($true)
    {
        try
        {
            # start server, return PID of the process
            $serverPS = InvokeHelper `
              -ArgumentList $envdef, $provider, $WorkingDirectory, $binPath, $serverArgs, $ManagedCCAlg `
              -Session $ServerSession {
                  param($env, $provider, $workdir, $binPath, $serverArgs, $cc)

                  # pull the definition of the functions into the scope
                  Invoke-Expression $env

                  if (!$workDir)
                  {
                      $workDir = Get-Location
                  }

                  # set internal QUIC implementation switches
                  $ENV:DOTNETQUIC_PROVIDER=$provider
                  $ENV:DOTNETQUIC_CC=$cc

                  # this one is needed for Linux in order to properly load OpenSSL and MsQuic from the binary directory
                  $ENV:LD_LIBRARY_PATH=$workDir

                  if ($netOps)
                  {
                      SetTrafficControl
                  }

                  $p = StartProcess $binPath $workdir $serverArgs

                  # wait until ready
                  $null = $p.StandardOutput.ReadLine()

                  $p.Id
              }

            # run client and collect output
            $res = InvokeHelper `
              -ArgumentList $envdef, $provider, $WorkingDirectory, $binPath, $clientArgs, $Samples, $ManagedCCAlg `
              -Session $ClientSession {
                  param($env, $provider, $workdir, $binPath, $clientArgs, $Samples, $cc)

                  # pull the definition of the functions into the scope
                  Invoke-Expression $env

                  if (!$workDir)
                  {
                      $workDir = Get-Location
                  }

                  # set internal QUIC implementation switches
                  $ENV:DOTNETQUIC_PROVIDER=$provider
                  $ENV:DOTNETQUIC_CC=$cc

                  # this one is needed for Linux in order to properly load OpenSSL and MsQuic from the binary directory
                  $ENV:LD_LIBRARY_PATH=$workDir

                  $p = StartProcess $binPath $workdir $clientArgs

                  # we are interested only in the first $Samples + 1 lines (output includes header)
                  $out = @()
                  for ($i = 0; $i -le $Samples; $i++)
                  {
                      $out += $p.StandardOutput.ReadLine()
                  }

                  Write-Debug "$res"

                  if ($p.HasExited)
                  {
                      Write-Warning "Process exited prematurely, trying again"
                      Write-Warning "$out"
                      return
                  }

                  # we have what we need, kill the app now
                  # Normally, we would let the app close itself, but when running MsQuic, the CLR memory
                  # gets corrupted when large number of connections are used.
                  $p | Stop-Process

                  $out
              }


        }
        finally
        {
            if ($serverPS)
            {
                # stop server
                InvokeHelper -ArgumentList $serverPS -Session $ServerSession {
                    param($serverPS)
                    Stop-Process $serverPS
                }
            }
        }

        if (!$res)
        {
            # no results collected, warning already reported, just try again
            continue;
        }

        $res = $res | ConvertFrom-Csv

        # remove extraneous columns
        $res.PSObject.Properties.Remove('Drift (%)')
        $res.PSObject.Properties.Remove('Current RPS')

        if (!$res.'Average RPS')
        {
            Write-Warning "Run seems to be corrupted, retrying"
            continue
        }

        break;
    }

    if ($clumsy)
    {
        $clumsy | Stop-Job
    }

    # add additional info about the environment
    if ($NetworkOpts)
    {
        InvokeHelper `
          -Session $ServerSession, $ClientSession `
          -ArgumentList $envDef, $networkInterface `
          {
              param($env, $device)

              # pull the definition of the functions into the scope
              Invoke-Expression $env

              if (!$IsLinux)
              {
                  # warning already reported
                  return
              }

              UnsetTrafficControl $device
          }

        foreach($key in $NetworkOpts.Keys)
        {
            $res = $res |
              Add-Member -MemberType NoteProperty -Name $Key -Value ($NetworkOpts.$key) -PassThru
        }
    }

    $res |
        Add-Member -MemberType NoteProperty -Name Impl -Value $Impl -PassThru |
        Add-Member -MemberType NoteProperty -Name Streams -Value $Streams -PassThru |
        Add-Member -MemberType NoteProperty -Name Connections -Value $Connections -PassThru |
        Add-Member -MemberType NoteProperty -Name MessageSize -Value $MessageSize -PassThru

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
    $Runs = GetAllRuns

    foreach($run in $Runs.Keys)
    {
        Write-Host "$($run):"

        $params = $Runs.$run

        $params.ExtraArgs = $params.ExtraArgs + @{
            WorkingDirectory = Resolve-Path $workDir
        }
        Write-Warning "Resolved: $(Resolve-Path $workDir)"

        RunMeasurementSet @params
    }
}

function RunAllDistributed
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        $ClientSession,

        [Parameter(Mandatory)]
        $ServerSession
    )

    $Runs = GetAllRuns

    foreach($run in $Runs.Keys)
    {
        Write-Host "$($run):"

        $params = $Runs.$run

        $params.ExtraArgs = $params.ExtraArgs + @{
            ClientSession = $ClientSession
            ServerSession = $ServerSession
            ServerHostName = $RemoteServerAddress
        }

        RunMeasurementSet @params
    }
}
