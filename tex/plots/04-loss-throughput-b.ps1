. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$WindowsLoopbackDataRoot\loss-throughput.csv" `
  -Query @{Drop=$dropSmallAmount} `
  -XAxis MessageSize `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$messageSizeLabel"
set ylabel "$throughputLabel"
"@
