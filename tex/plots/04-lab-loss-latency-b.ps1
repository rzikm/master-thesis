. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LabDataRoot\loss-latency.csv" `
  -Query @{Drop=$dropSmallAmount} `
  -XAxis MessageSize `
  -YAxis $latencyColumn `
  -GnuplotExtra @"
set xlabel "$messageSizeLabel"
set ylabel "$latencyLabel"
"@
