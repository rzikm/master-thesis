$latencyColumn = "Latency-p99 (ms)"
$latencyColumnMedian = "Latency-p50 (ms)"
$throughputColumn = "Throughput (MiB/s)"
$colors = @{
    'managed' = 'royalblue' #4169e1
    'msquic' = 'dark-red' #8b0000
    'tcp' = 'sea-green' #2e8b57
}

$messageSizeLabel = "Message Size (B)"
$latencyLabel = "99th percentile latency (ms)"
$latencyLabelMedian = "median latency (ms)"
$throughputLabel = "Throughput (MiB/s)"
$connectionsLabel = "Connections"

$dropZeroAmount =0.004
$dropSmallAmount = 0.1

$DataRoot = "$PSScriptRoot\..\measurements"
$LabDataRoot = "$PSScriptRoot\..\measurements\lab"

$bigMessageSize = 4096
$smallMessageSize = 256

function CreateTexPlot
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $DataFile,

        [Parameter(Mandatory)]
        [PSObject] $Query,

        [Parameter()]
        [string] $GnuplotExtra,

        [Parameter()]
        [double] $Width = 2.8,

        [Parameter()]
        [double] $Height = 2.5,

        [Parameter(Mandatory)]
        [string] $XAxis,

        [Parameter(Mandatory)]
        [string] $YAxis,

        [Parameter()]
        [double]$VerticalMargin = 0.25
    )

    # create the tex output file
    $outFile = $filename = (Resolve-Path -Relative $MyInvocation.PSCommandPath) -replace 'ps1$','tex'

    $data = Import-Csv $DataFile

    $plotData = @(@{
        Data = TransformPlotData -Data $data -GroupBy Impl -Query $Query -XAxis $XAxis -YAxis $YAxis
        Title = ""
        ExtraGnuplotScript = $GnuplotExtra
    })

    New-PlotData -Data $plotData -Filename $outFile -OutputTerminal 'epslatex' -Width $Width -Height $Height `
       -VerticalMargin $VerticalMargin
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

function HashtableToString([hashtable] $Table)
{
    return "{ $(($Table.Keys | ForEach-Object { "$_=$($Table.$_)" }) -join "; ") }"
}


function TransformPlotData
{
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline)]
        [PSObject[]] $Data,

        [Parameter()]
        [string] $GroupBy,

        [Parameter()]
        [PSObject] $Query,

        [Parameter()]
        [string] $XAxis,

        [Parameter()]
        [string] $YAxis
    )

    process
    {
        foreach ($key in $Query.Keys)
        {
            $Data = $Data | Where-Object { $_.$key -eq $Query.$key }
        }

        $Data | Group-Object $XAxis -AsHashTable | ForEach-Object GetEnumerator |
        ForEach-Object {
            $plotData = [ordered]@{ $XAxis = $_.Key }

            $_.Value | Group-Object $GroupBy -AsHashTable | ForEach-Object GetEnumerator | Sort-Object Key |
              ForEach-Object {
                $key = $_.Key
                $value = $_.Value.$YAxis
                $plotData.$key = $value
            }

            $plotData
        } | Sort-Object { [int]$_.$XAxis }
    }
}

function New-PlotData
{
    [CmdLetBinding()]
    param(
        # array of data to plot, each in a separate plot
        [Parameter()]
        $Data,

        [Parameter()]
        [string]$Filename,

        [Parameter()]
        $PlotColumns = 1,

        [Parameter()]
        [string]$OutputTerminal = 'svg',

        [Parameter()]
        [double]$Width = 500,

        [Parameter()]
        [double]$Height = 300,

        [Parameter()]
        [double]$VerticalMargin = 0.2
    )

    $plotCount = $Data.Length

    $gapwidth = 1
    $boxwidth = 0.8

    $gnuplotScript = @"
set terminal $OutputTerminal size $($Width * $PlotColumns),$($Height * $plotCount / $PlotColumns)
set output "$($Filename.Replace("\","/"))"

$(if ($plotCount -ne 1) { "set multiplot layout $($plotCount/$PlotColumns),$PlotColumns" })

set datafile separator ","

set style data histogram
set style histogram cluster gap $gapwidth
set style fill solid border -1

set boxwidth $boxwidth
set bmargin

set key center top horizontal outside
set key auto columnheader
set key samplen 1
set key font ",15"

set grid ytics
set yrange [0:]

set xtics nomirror

set rmargin 0.5

set offsets -0.4, -0.3, graph $VerticalMargin, 0

"@

    foreach ($dataSet in $Data)
    {
        $columns = @($dataSet.Data[0].Keys)
        $columnCount = $columns.Length - 1

        $gnuplotScript += "set title '$($dataSet.Title)'`n"
        $gnuplotScript += "$($Data.ExtraGnuplotScript)`n"

        # Horizontal offset is given in cluster offsets (1 = offset between two clusters)
        $barWidth = 1 / ($columnCount + $gapwidth)

        $gnuplotScript += "plot " +
        ((2..($columns.Length) | ForEach-Object {
              $columnIndex = $_ - 2

              $horizontalOffset = ($columnIndex - ($columnCount - 1) / 2.0) * $barWidth
              $verticalOffset = $LabelYOffset

              "'-' using $($_):xtic(1) lt rgb '$($colors.($columns[$_ - 1]))'"
              "'-' using (`$0 + $horizontalOffset):$($_):(sprintf(`"\\\\scriptsize %3.2f`", `$$_)) with labels notitle left rotate offset 0,0.15"

          }) -join ",\`n") + "`n"

        $plotDataCsv = $dataSet.Data | ForEach-Object { [PSCustomObject]$_ } | ConvertTo-Csv -UseQuotes AsNeeded | Out-String
        $plotDataCsv += "e`n"

        $gnuplotScript += $plotDataCsv * $columnCount * 2
    }

    $gnuplotScript | gnuplot -p
}

function PlotData
{
    [CmdLetBinding()]
    param(
        [Parameter()]
        [string] $Filename,

        [Parameter()]
        [PSObject] $Query,

        [Parameter()]
        [string] $XAxis,

        [Parameter()]
        [string] $YAxis
    )

    $data = Import-Csv $Filename

    $plotData = TransformPlotData -Data $data -GroupBy Impl -Query $Query -XAxis $XAxis -YAxis $YAxis

    New-PlotData -Data @{Data = $plotData} -Filename "out.svg"
    Invoke-Item "out.svg"
}

function PlotAndDisplayData
{
    [CmdLetBinding()]
    param(
        [Parameter()]
        $Data,

        [Parameter()]
        [string]$Filename,

        [Parameter()]
        $ParameterSets,

        [Parameter()]
        [string] $XAxis,

        [Parameter()]
        [string] $YAxis
    )

    $columns = $ParameterSets.(@($ParameterSets.Keys)[0]).Count

    $sets = $ParameterSets | ProduceParameterSets

    $dataSets = $sets | ForEach-Object {
        @{
            Data = @(TransformPlotData -Data $Data -GroupBy Impl -Query $_ -XAxis $XAxis -YAxis $YAxis)
            Title = (HashtableToString $_)
        }
    }

    New-PlotData -PlotColumns $columns -Data $dataSets -Filename $filename
    Invoke-Item $filename
}

function PlotLossLatency
{
    $data = Import-Csv $DataRoot\loss-latency.csv
    $filename = 'loss-latency.svg'

    $sets = [ordered]@{
        Drop = "",0.5,1
    }

    PlotAndDisplayData -Data $data -Filename $filename -ParameterSets $sets -XAxis MessageSize -YAxis $latencyColumn
}

function PlotLossThroughput
{
    $data = Import-Csv $DataRoot\loss-throughput.csv
    $filename = 'loss-throughput.svg'

    $sets = [ordered]@{
        Drop = "",0.5,1
    }

    PlotAndDisplayData -Data $data -Filename $filename -ParameterSets $sets -XAxis MessageSize -YAxis $throughputColumn
}

function PlotMultiStreamThroughput
{
    $data = Import-Csv $DataRoot\multi-stream-throughput.csv
    $filename = 'multi-stream-throughput.svg'

    $sets = [ordered]@{
        Streams = 1,32
        MessageSize = $smallMessageSize, $bigMessageSize
    }

    PlotAndDisplayData -Data $data -Filename $filename -ParameterSets $sets -XAxis Connections -YAxis $throughputColumn
}

function PlotMultiStreamLatency
{
    $data = Import-Csv $DataRoot\multi-stream-latency.csv
    $filename = 'multi-stream-latency.svg'

    $sets = [ordered]@{
        Streams = 1,4,16
        MessageSize = $smallMessageSize, 1024, $bigMessageSize
    }

    PlotAndDisplayData -Data $data -Filename $filename -ParameterSets $sets -XAxis Connections -YAxis $latencyColumn
}

function PlotSingleStreamThroughput
{
    $data = Import-Csv $DataRoot\single-stream-throughput.csv
    $filename = 'single-stream-throughput.svg'

    $sets = [ordered]@{
        MessageSize = $smallMessageSize, 1024, $bigMessageSize
    }

    PlotAndDisplayData -Data $data -Filename $filename -ParameterSets $sets -XAxis Connections -YAxis $throughputColumn
}

function PlotSingleStreamLatency
{
    $data = Import-Csv $DataRoot\single-stream-latency.csv
    $filename = 'single-stream-latency.svg'

    $sets = [ordered]@{
        MessageSize = $smallMessageSize, 1024, $bigMessageSize
    }

    PlotAndDisplayData -Data $data -Filename $filename -ParameterSets $sets -XAxis Connections -YAxis $latencyColumn
}
