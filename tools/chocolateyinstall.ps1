[CmdletBinding()]
param(
    # The path to put our kernel.json folders in
    $KernelFolder,

    # The path where the kernel executables are (should contain the 'net461' and 'netcoreapp2.0' folders)
    $InstallPath = $(Split-Path $PSScriptRoot)
)

Write-Warning "The current preview of Jupyter-PowerShell requires a preview release of dotnet $dotnetCLIRequiredVersion"

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

$dotnetCLIChannel = "preview"
$dotnetCLIRequiredVersion = "2.0.0-preview2-006502"

function script:Test-Command([string]$command, [string]$NotFoundMessage) {
    if (Get-Command $command -ErrorAction SilentlyContinue) {
        return $true
    } else {
        if ($NotFoundMessage -ne $null) {
            Write-Warning $NotFoundMessage
        }
        return $false
    }
}

# Try to fix the Path until we can find dotnet.exe
$dotnetPath = if ($IsWindows) {
    "$env:ProgramFiles\dotnet" + ';' + "$env:LocalAppData\Microsoft\dotnet"
} else {
    "$env:HOME/.dotnet"
}

$Targets = @()

if ($IsWindows) {
    $Targets += "PowerShell-Full"
    if(!$KernelFolder) {
        $KernelFolder = Join-Path $Env:AppData "jupyter\kernels\"
    }
} else {
    if(!$KernelFolder) {
        if($IsLinux) {
            $KernelFolder = "~/.local/share/jupyter/kernels"
        } else {
            $KernelFolder = "~/Library/Jupyter/kernels"
        }
    }
}

if (-not (Test-Command 'dotnet')) {
    $originalPath = $env:PATH
    $env:PATH += [IO.Path]::PathSeparator + $dotnetPath
    if (-not (Test-Command 'dotnet')) {
        $env:PATH = $originalPath
    }
}


if(Test-Command 'dotnet' "'dotnet' not found. In order to use the 'PowerShell (Core)' Jupyter kernel, you need to install dotnet core.") {
    if (($dotnetCLIIntalledVersion = dotnet --version) -eq $dotnetCLIRequiredVersion) {
        $Targets += "PowerShell-Core"
    } else {
        Write-Warning "
        The currently installed 'dotnet' (.NET Command Line Tools) is not the expected version.

        Installed version: $dotnetCLIIntalledVersion
        Expected version: $dotnetCLIRequiredVersion

        You can get the latest version from https://microsoft.com/net/core/preview or by downloading and running:

        https://raw.githubusercontent.com/dotnet/cli/master/scripts/obtain/dotnet-install.$( if($IsWindows){ "ps1" } else { "sh" } )
        `n
        "
        if(!$IsWindows) {
            Write-Error "No PowerShell kernel installed. Install dotnet $dotnetCLIRequiredVersion to your Path and run '$PSCommandPath' again."
        }
    }
}

foreach($target in $Targets) {
    $kernelFile = Join-Path $kernelFolder "$target\kernel.json"

    Remove-Item (Join-Path $InstallPath "$target\*.pdb")
    $kernelPath = Resolve-Path (Join-Path $InstallPath "$target\PowerShell-Kernel.???")

    if (!(Test-Path $kernelPath)) {
        Write-Warning "
        Can't find the PowerShell kernel file in $kernelPath

        Expected the $target kernel to be in the same folder with this script.

        If you're running this script from the source code rather than a build:
        - Build the project by running: dotnet restore; dotnet build;
        - Copy this file to the 'Debug' or 'Release' output folder
        - Re-run this file
        `n
        "
    }
    # Necessary for windows only:
    $kernelPath = $kernelPath -replace "\\", "\\"

    $null = New-Item -Path (Split-Path $kernelFile) -Force -ItemType Directory

    $kernelData = @(
        "{"
        "  ""argv"": ["
        "    ""dotnet"","
        "    ""$kernelPath"","
        "    ""{connection_file}"""
        "  ],"
        "  ""display_name"": ""$($target -replace '-(.*)',' ($1)')"","
        "  ""language"": ""PowerShell"""
        "}"
    )

    if($target -match "Full") { $kernelData = $kernelData -notmatch "dotnet" }

    Set-Content -Path $kernelFile -Value ($kernelData -join "`n")
}
