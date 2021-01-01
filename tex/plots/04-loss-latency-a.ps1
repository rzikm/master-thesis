. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$WindowsLoopbackDataRoot\loss-latency.csv" `
  -Query @{Drop=$dropZeroAmount} `
  -XAxis MessageSize `
  -YAxis $latencyColumn `
  -GnuplotExtra @"
set xlabel "$messageSizeLabel"
set ylabel "$latencyLabel"
"@
