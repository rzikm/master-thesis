. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LinuxLabDataRoot\multi-stream-latency.csv" `
  -Query @{Streams=1;MessageSize=$smallMessageSize} `
  -XAxis Connections `
  -YAxis $latencyColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$latencyLabel"
"@
