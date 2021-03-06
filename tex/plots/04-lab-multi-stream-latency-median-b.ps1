. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LinuxLabDataRoot\multi-stream-latency.csv" `
  -Query @{Streams=32;MessageSize=$bigMessageSize} `
  -XAxis Connections `
  -YAxis $latencyColumnMedian `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$latencyLabelMedian"
"@
