. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LabDataRoot\multi-stream-throughput.csv" `
  -Query @{Streams=1;MessageSize=$smallMessageSize} `
  -XAxis Connections `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$throughputLabel"
"@
