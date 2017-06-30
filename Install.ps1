[CmdletBinding()]
param(
    # The path to put our kernel.json folders in
    $KernelFolder = $(Join-Path $Env:AppData "jupyter\kernels\"),

    # The path where the kernel executables are (should contain the 'net461' and 'netcoreapp2.0' folders)
    $InstallPath = $PSScriptRoot,

    # Force installing DotNet
    [switch]$InstallDotnet
)

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
    # If these are already set, then they're read-only and we're done
    try {
        $IsCoreCLR = $false
        $IsLinux = $false
        $IsOSX = $false
        $IsWindows = $true
    } catch {
    }
}

$dotnetCLIChannel = "preview"
$dotnetCLIRequiredVersion = "2.0.0-preview1-005977"
# On Windows paths is separated by semicolon
$TestModulePathSeparator = ':'

if ($IsWindows) {
    $TestModulePathSeparator = ';'
    $IsAdmin = (New-Object Security.Principal.WindowsPrincipal ([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
    # Can't use $env:HOME - not available on older systems (e.g. in AppVeyor)
    $nugetPackagesRoot = "${env:HOMEDRIVE}${env:HOMEPATH}\.nuget\packages"
} else {
    $nugetPackagesRoot = "${env:HOME}/.nuget/packages"
}

if ($IsLinux) {
    $LinuxInfo = Get-Content /etc/os-release -Raw | ConvertFrom-StringData

    $IsUbuntu = $LinuxInfo.ID -match 'ubuntu'
    $IsUbuntu14 = $IsUbuntu -and $LinuxInfo.VERSION_ID -match '14.04'
    $IsUbuntu16 = $IsUbuntu -and $LinuxInfo.VERSION_ID -match '16.04'
    $IsCentOS = $LinuxInfo.ID -match 'centos' -and $LinuxInfo.VERSION_ID -match '7'
    $IsFedora = $LinuxInfo.ID -match 'fedora' -and $LinuxInfo.VERSION_ID -ge 24
    $IsOpenSUSE = $LinuxInfo.ID -match 'opensuse'
    $IsOpenSUSE13 = $IsOpenSUSE -and $LinuxInfo.VERSION_ID -match '13'
    ${IsOpenSUSE42.1} = $IsOpenSUSE -and $LinuxInfo.VERSION_ID -match '42.1'
    $IsRedHatFamily = $IsCentOS -or $IsFedora -or $IsOpenSUSE

    # Workaround for temporary LD_LIBRARY_PATH hack for Fedora 24
    # https://github.com/PowerShell/PowerShell/issues/2511
    if ($IsFedora -and (Test-Path ENV:\LD_LIBRARY_PATH)) {
        Remove-Item -Force ENV:\LD_LIBRARY_PATH
        Get-ChildItem ENV:
    }
}

function script:precheck([string]$command, [string]$missedMessage) {
    $c = Get-Command $command -ErrorAction SilentlyContinue
    if (-not $c) {
        if ($missedMessage -ne $null) {
            Write-Warning $missedMessage
        }
        return $false
    } else {
        return $true
    }
}

function Install-Kernel {

    $originalPath = $env:PATH
    $dotnetPath = if ($IsWindows) {
        "$env:LocalAppData\Microsoft\dotnet"
    } else {
        "$env:HOME/.dotnet"
    }

    if (-not (precheck 'dotnet' "Could not find 'dotnet', appending $dotnetPath to PATH.")) {
        $env:PATH += [IO.Path]::PathSeparator + $dotnetPath
    }

    if (-not (precheck 'dotnet' "Still could not find 'dotnet', restoring PATH.")) {
        $env:PATH = $originalPath
    }

    if(precheck 'dotnet' "'dotnet' not found. In order to use or build the 'PowerShell (Core)' Jupyter kernel, you need to install dotnet core.") {
        $dotnetCLIIntalledVersion = (dotnet --version)
        if ( $dotnetCLIIntalledVersion -ne $dotnetCLIRequiredVersion ) {
            Write-Warning "
            The currently installed 'dotnet' (.NET Command Line Tools) is not the expected version.

            Installed version: $dotnetCLIIntalledVersion
            Expected version: $dotnetCLIRequiredVersion

            If your version is older, you PROBABLY cannot run the 'PowerShell (Core)' kernel.

            Get the latest version from https://www.microsoft.com/net/core/preview
            `n
            "
        }
    }

    if ($IsWindows) {
        $Targets = "PowerShell-Full","PowerShell-Core"
    } else {
        $Targets = "PowerShell-Core"
    }

    foreach($target in $Targets) {

        $kernelFile = Join-Path $kernelFolder "$target\kernel.json"

        Remove-Item (Join-Path $InstallPath "$target\*.pdb")
        $kernelPath = Resolve-Path (Join-Path $InstallPath "$target\PowerShell-Kernel.???")

        if (!(Test-Path $kernelPath)) {
            Write-Warning "
            Can't find the PowerShell-Kernel in $kernelPath

            Expected the $target kernel to be in the same folder with this script.

            If you're running this script from the source code rather than a build:
            - Build the project by running: dotnet restore; dotnet build;
            - Copy this file to the 'Debug' or 'Release' output folder
            - Re-run this file
            `n
            "
        }
        $kernelPath = $kernelPath -replace "\\", "\\"

        $null = mkdir -Path (Split-Path $kernelFile) -Force
$kernelData = @"
{
  "argv": [
    "dotnet",
    "$kernelPath",
    "{connection_file}"
  ],
  "display_name": "$($target -replace '-(.*)',' ($1)')",
  "language": "PowerShell"
}
"@

        if($target -match "Full") { $kernelData = $kernelData -replace '\s*"dotnet",\s*[\r\n]+'}

        Set-Content -Path $kernelFile -Value $kernelData
    }
}


if ($InstallDotnet) {
    Install-Dotnet
}

Install-Kernel
