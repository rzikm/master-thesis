. $PSScriptRoot\..\utils\plot.ps1

CreateTexPlot `
  -DataFile "$LabDataRoot\multi-loss-throughput.csv" `
  -Query @{Drop=$dropZeroAmount;Lag=25} `
  -XAxis MessageSize `
  -YAxis $throughputColumn `
  -GnuplotExtra @"
set xlabel "$messageSizeLabel"
set ylabel "$throughputLabel"
"@
