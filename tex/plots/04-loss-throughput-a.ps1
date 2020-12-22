. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$DataRoot\loss-throughput.csv" `
  -Query @{Drop=$dropZeroAmount} `
  -XAxis MessageSize `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$messageSizeLabel"
set ylabel "$throughputLabel"
"@
