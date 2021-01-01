. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LinuxLabDataRoot\multi-stream-throughput.csv" `
  -Query @{Streams=1;MessageSize=$smallMessageSize} `
  -XAxis Connections `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$throughputLabel"
"@
