. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LinuxLabDataRoot\single-stream-latency.csv" `
  -Query @{MessageSize=$smallMessageSize} `
  -XAxis Connections `
  -YAxis $latencyColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$latencyLabel"
"@
