. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$DataRoot\multi-stream-throughput.csv" `
  -Query @{Streams=32;MessageSize=$smallMessageSize} `
  -XAxis Connections `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$throughputLabel"
"@
