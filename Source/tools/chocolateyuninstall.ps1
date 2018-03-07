# Use the .NET Core APIs to determine the current platform; if a runtime
# exception is thrown, we are on FullCLR, not .NET Core.
try {
    $Runtime = [System.Runtime.InteropServices.RuntimeInformation]
    $OSPlatform = [System.Runtime.InteropServices.OSPlatform]

    $IsCoreCLR = $true
    $IsLinux = $Runtime::IsOSPlatform($OSPlatform::Linux)
    $IsOSX = $Runtime::IsOSPlatform($OSPlatform::OSX)
    $IsWindows = $Runtime::IsOSPlatform($OSPlatform::Windows)
} catch {
    # If these are already set read-only, we're on core, and done here
    try {
        $IsCoreCLR = $false
        $IsLinux = $false
        $IsOSX = $false
        $IsWindows = $true
    } catch {
    }
}

if ($IsWindows) {
    if (!$KernelFolder) {
        $KernelFolder = Join-Path $Env:AppData "jupyter\kernels\"
    }
    $Targets = @("Windows", "WindowsPowerShell")
}
if ($IsLinux) {
    if (!$KernelFolder) {
        $KernelFolder = "~/.local/share/jupyter/kernels"
    }
    $Targets = @("Linux")
}
if ($IsOSX) {
    if (!$KernelFolder) {
        $KernelFolder = "~/Library/Jupyter/kernels"
    }
    $Targets = @("Mac")
}

if (Test-Path $KernelFolder) {
    $path = Join-Path $KernelFolder "PowerShell"
    if (Test-Path $path) {
        Remove-Item $path -Recurse
    }

    if ($Targets -contains "WindowsPowerShell") {
        $path = Join-Path $KernelFolder "WindowsPowerShell"
        if (Test-Path $path) {
            Remove-Item $path -Recurse
        }
    }
}