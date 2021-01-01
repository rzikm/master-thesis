. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$WindowsLoopbackDataRoot\single-stream-throughput.csv" `
  -Query @{MessageSize=256} `
  -XAxis Connections `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$throughputLabel"
"@
