. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$DataRoot\multi-stream-latency.csv" `
  -Query @{Streams=1;MessageSize=256} `
  -XAxis Connections `
  -YAxis $latencyColumn `
  -GnuplotExtra @"
set xlabel "$connectionsLabel"
set ylabel "$latencyLabel"
"@ -Width 2.8 `
  -Height 2.0
