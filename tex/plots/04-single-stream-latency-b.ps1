. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$WindowsLoopbackDataRoot\single-stream-latency.csv" `
  -Query @{MessageSize=$bigMessageSize} `
  -XAxis Connections `
  -YAxis $latencyColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$latencyLabel"
"@
