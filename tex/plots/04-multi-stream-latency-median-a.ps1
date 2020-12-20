. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$DataRoot\multi-stream-latency.csv" `
  -Query @{Streams=1;MessageSize=$bigMessageSize} `
  -XAxis Connections `
  -YAxis $latencyColumnMedian `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$latencyLabelMedian"
"@
