. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LinuxLabDataRoot\single-stream-throughput.csv" `
  -Query @{MessageSize=4096} `
  -XAxis Connections `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$throughputLabel"
"@
