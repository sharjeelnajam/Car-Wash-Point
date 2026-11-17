# Run this once on the shop PC, in PowerShell as Administrator, from the folder that contains DrWashScannerHelper.exe.
# It stores the cash machine login on this PC, makes the helper start automatically when the current
# user signs in, and starts it now.
$exe = Join-Path $PSScriptRoot "DrWashScannerHelper.exe"
if (-not (Test-Path $exe)) { Write-Error "DrWashScannerHelper.exe not found next to this script."; exit 1 }

# Cash machine login. The password is only saved on this PC, in appsettings.Local.json.
$localSettings = Join-Path $PSScriptRoot "appsettings.Local.json"
if (-not (Test-Path $localSettings)) {
    Write-Host "The helper also talks to the cash machine for the website."
    $secure = Read-Host "Enter the cash machine API password (the same one the app used before)" -AsSecureString
    $plain  = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
    if ([string]::IsNullOrWhiteSpace($plain)) { Write-Error "No password entered."; exit 1 }
    @{ Cash = @{ Password = $plain } } | ConvertTo-Json -Depth 3 | Set-Content -Path $localSettings -Encoding UTF8
    $plain = $null
}

$taskName = "DrWashScannerHelper"
$action   = New-ScheduledTaskAction -Execute $exe -WorkingDirectory $PSScriptRoot
$trigger  = New-ScheduledTaskTrigger -AtLogOn -User $env:USERNAME
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero) -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)

# Stop any running copy so the new settings are used.
Get-Process DrWashScannerHelper -ErrorAction SilentlyContinue | Stop-Process -Force
Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Settings $settings -RunLevel Limited -User $env:USERNAME | Out-Null
Start-ScheduledTask -TaskName $taskName
Start-Sleep 3

try {
    $r = Invoke-RestMethod http://127.0.0.1:5555/status -TimeoutSec 5
    "Helper is running. Scanner connected: $($r.connected) (devices: $($r.count))"
} catch {
    "Helper did not answer yet. Check helper.log in $PSScriptRoot"
}
