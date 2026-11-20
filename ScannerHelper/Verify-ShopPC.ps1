# Checks the shop PC is ready for the scanner helper and the VPS website.
# READ-ONLY: it installs nothing and changes nothing. Run it from the helper folder:
#   .\Verify-ShopPC.ps1
# Add -SkipCashLogin to skip typing the cash machine password.
param([switch]$SkipCashLogin)

# The scanner files are 32-bit, so this has to run in the 32-bit PowerShell.
if ([Environment]::Is64BitProcess) {
    $args32 = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $PSCommandPath)
    if ($SkipCashLogin) { $args32 += "-SkipCashLogin" }
    & "$env:windir\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" @args32
    exit
}

$here = $PSScriptRoot
$pass = 0; $fail = 0
function Result($name, $ok, $detail) {
    if ($ok) { $script:pass++; $tag = "PASS" } else { $script:fail++; $tag = "FAIL" }
    "{0}  {1}  {2}" -f $tag, $name.PadRight(34), $detail
}

"=== DrWash shop PC check ($env:COMPUTERNAME, $(Get-Date -Format 'yyyy-MM-dd HH:mm')) ===`n"

# --- Settings from the helper's own config
$cfg = Get-Content (Join-Path $here "appsettings.json") -Raw | ConvertFrom-Json
$cashUrl = [Uri]$cfg.Cash.BaseUrl
$cashUser = $cfg.Cash.Username

# --- 1. Scanner prerequisites
Result "Visual C++ 2010 (x86) runtime" (Test-Path "$env:windir\SysWOW64\msvcr100.dll") "msvcr100.dll in SysWOW64"
$missing = @('DevCapture.dll','sdtapi.dll','ReadBarCode.dll','BarcodeDll.dll','libzbar-0.dll','libzbar-0.dll.dll','pdflib.dll','WltRS.dll') |
    Where-Object { -not (Test-Path (Join-Path $here $_)) }
Result "Scanner files next to this script" ($missing.Count -eq 0) $(if ($missing.Count -eq 0) { "all present" } else { "missing: " + ($missing -join ', ') })

# --- 2. Scanner library and device
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class ShopCheck {
  [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Unicode)] public static extern bool SetDllDirectory(string p);
  [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Unicode)] public static extern IntPtr LoadLibrary(string f);
  [DllImport("DevCapture.dll", CallingConvention=CallingConvention.Cdecl)] public static extern int GetDeviceCount();
}
"@
[ShopCheck]::SetDllDirectory($here) | Out-Null
$h = [ShopCheck]::LoadLibrary((Join-Path $here "DevCapture.dll"))
if ($h -eq [IntPtr]::Zero) {
    Result "Scanner library loads" $false ("Win32 error " + [Runtime.InteropServices.Marshal]::GetLastWin32Error() + " (126 = a dependency is missing)")
    Result "Scanner device detected" $false "library did not load"
} else {
    Result "Scanner library loads" $true "DevCapture.dll OK"
    try {
        $count = [ShopCheck]::GetDeviceCount()
        Result "Scanner device detected" ($count -gt 0) "devices found: $count"
    } catch {
        Result "Scanner device detected" $false $_.Exception.Message
    }
}

# --- 3. Cash machine on the shop network
$tcp = Test-NetConnection $cashUrl.Host -Port $cashUrl.Port -WarningAction SilentlyContinue
Result "Cash machine reachable" $tcp.TcpTestSucceeded "$($cashUrl.Host):$($cashUrl.Port)"

if ($SkipCashLogin) {
    "SKIP  Cash machine login                  (skipped)"
} elseif (-not $tcp.TcpTestSucceeded) {
    Result "Cash machine login" $false "cannot reach the machine"
} else {
    $secure = Read-Host "Cash machine API password for '$cashUser' (press Enter to skip)" -AsSecureString
    $plain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
    if ([string]::IsNullOrEmpty($plain)) {
        "SKIP  Cash machine login                  (no password entered)"
    } else {
        try {
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            [Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }  # the machine's certificate is self-signed
            $tok = Invoke-RestMethod -Method Post -Uri ($cashUrl.AbsoluteUri.TrimEnd('/') + $cfg.Cash.TokenPath) `
                -Body @{ grant_type = "password"; username = $cashUser; password = $plain } -TimeoutSec 15
            Result "Cash machine login" ($null -ne $tok.access_token) "token received"
            $st = Invoke-RestMethod -Uri ($cashUrl.AbsoluteUri.TrimEnd('/') + "/status?eventVersion=2") `
                -Headers @{ Authorization = "Bearer $($tok.access_token)" } -TimeoutSec 15
            $last = $st.events | Select-Object -Last 1
            Result "Cash machine status call" $true "latest event: $($last.event)"
        } catch {
            Result "Cash machine login / status" $false $_.Exception.Message
        } finally {
            [Net.ServicePointManager]::ServerCertificateValidationCallback = $null
            $plain = $null
        }
    }
}

# --- 4. The website on the VPS
try {
    $r = Invoke-WebRequest "https://app.drwash.co.uk" -UseBasicParsing -TimeoutSec 20
    Result "Website opens (app.drwash.co.uk)" ($r.StatusCode -eq 200) "HTTP $($r.StatusCode)"
} catch {
    Result "Website opens (app.drwash.co.uk)" $false $_.Exception.Message
}

# --- 5. Printer
$real = Get-Printer -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notmatch 'PDF|XPS|OneNote|Fax|Remote' }
Result "A real printer is installed" ($null -ne $real -and @($real).Count -gt 0) $(if ($real) { (@($real | ForEach-Object { "$($_.Name) [$($_.PrinterStatus)]" }) -join '; ') } else { "only virtual printers found" })
$default = (Get-CimInstance Win32_Printer | Where-Object { $_.Default }).Name
"INFO  Default printer                      $default"

# --- 6. Chrome and the helper port
$chrome = @("$env:ProgramFiles\Google\Chrome\Application\chrome.exe", "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe", "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe") | Where-Object { Test-Path $_ }
Result "Google Chrome installed" ($null -ne $chrome) $(if ($chrome) { "found" } else { "not found" })
$port = Test-NetConnection 127.0.0.1 -Port $cfg.Helper.Port -WarningAction SilentlyContinue
"INFO  Helper port $($cfg.Helper.Port)                    " + $(if ($port.TcpTestSucceeded) { "in use (the helper may already be running)" } else { "free" })

"`n=== Summary: $pass passed, $fail failed ==="
if ($fail -eq 0) { "Everything checks out. You can install the helper (Install-ScannerHelper.ps1)." }
else { "Fix the FAIL lines first, then run this script again." }
