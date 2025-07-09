# Save as Show-GPU-WMI.ps1 and run with:
#   powershell -ExecutionPolicy Bypass -File .\Show-GPU-WMI.ps1

$namespaces = 'root\cimv2','root\wmi'

foreach ($ns in $namespaces) {
    Write-Host "`n=== Namespace: $ns ===" -ForegroundColor Cyan

    # 1) Find all classes with "Video" or "GPU" in their name
    $gpuClasses = Get-CimClass -Namespace $ns |
                  Where-Object Name -match 'Video|GPU'

    if (-not $gpuClasses) {
        Write-Host "No matching classes in $ns." -ForegroundColor Yellow
        continue
    }

    foreach ($cls in $gpuClasses) {
        Write-Host "`n--- Class: $($cls.Name) ---" -ForegroundColor Green
        try {
            # 2) List every instance and all properties
            Get-CimInstance -Namespace $ns -ClassName $cls.Name |
                Format-List *
        }
        catch {
            Write-Host "  [Error reading instances]" -ForegroundColor Red
        }
    }
}

# 3) Always include the standard GPU controller info
Write-Host "`n=== Win32_VideoController Instances ===" -ForegroundColor Cyan
Get-CimInstance -ClassName Win32_VideoController |
    Select-Object `
        @{n='Name';e={$_.Name}}, `
        @{n='Vendor';e={$_.AdapterCompatibility}}, `
        @{n='PNPDeviceID';e={$_.PNPDeviceID}} |
    Format-Table -AutoSize
