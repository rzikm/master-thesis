. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LinuxLabDataRoot\loss-throughput.csv" `
  -Query @{Drop=$dropSmallAmount;Lag=25;Streams=1} `
  -XAxis MessageSize `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$messageSizeLabel"
set ylabel "$throughputLabel"
"@
