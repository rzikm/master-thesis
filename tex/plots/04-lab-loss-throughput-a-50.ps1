. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LinuxLabDataRoot\loss-throughput.csv" `
  -Query @{Drop=$dropZeroAmount;Lag=50} `
  -XAxis MessageSize `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$messageSizeLabel"
set ylabel "$throughputLabel"
"@
