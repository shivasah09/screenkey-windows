# SCREENKEY - Launches the editable C# Windows keyboard using built-in .NET Framework.
# No Python, downloads, account, or internet required.
$ErrorActionPreference = 'Stop'
try {
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing
    $code = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ScreenKey.cs') -Raw -Encoding UTF8
    Add-Type -TypeDefinition $code -ReferencedAssemblies @('System.Windows.Forms.dll', 'System.Drawing.dll') -ErrorAction Stop
    [System.Windows.Forms.Application]::EnableVisualStyles()
    [System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false)
    [System.Windows.Forms.Application]::Run((New-Object ScreenKeyForm))
}
catch {
    $detail = ($_ | Out-String)
    $log = Join-Path $PSScriptRoot 'ScreenKey-error.log'
    try { $detail | Set-Content -LiteralPath $log -Encoding UTF8 } catch {}
    [System.Windows.Forms.MessageBox]::Show("ScreenKey could not start. Check ScreenKey-error.log for details.`n`n" + $detail,
        'ScreenKey Error', [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Error) | Out-Null
}