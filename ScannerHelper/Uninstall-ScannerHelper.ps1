# Removes the automatic start and stops the helper.
Unregister-ScheduledTask -TaskName "DrWashScannerHelper" -Confirm:$false -ErrorAction SilentlyContinue
Get-Process DrWashScannerHelper -ErrorAction SilentlyContinue | Stop-Process -Force
"Scanner helper removed."
