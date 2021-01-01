. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$WindowsLoopbackDataRoot\multi-stream-throughput.csv" `
  -Query @{Streams=1;MessageSize=$bigMessageSize} `
  -XAxis Connections `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$throughputLabel"
"@
