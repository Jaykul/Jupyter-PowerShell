param(
    ${Prefix},

    ${Configuration} = "Release",

    [ValidateSet("Linux", "Windows", "OSx")]
    [string[]]${Platform} = @("Linux","Windows","OSx"),

    [switch]$Package,

    $ChocoApiKey
)

$OutputPath = Join-Path $PSScriptRoot "Output/${Configuration}"
Push-Location $PSScriptRoot

## Clean ##
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -ErrorAction Stop
}
$null = New-Item Output/${Configuration} -Force -ItemType Directory

## Build ##
Push-Location ./Source
dotnet restore
Pop-Location
if ($Platform -contains "Windows") {
    Push-Location ./Source
    dotnet publish -f netcoreapp2.0 -c ${Configuration} -r win7-x64 #--version-suffix $Prefix
    Pop-Location
    Move-Item "Source/Output/${Configuration}/netcoreapp2.0/win7-x64/publish" "Output/${Configuration}/Windows"
    Get-ChildItem $PSHome -Directory | Copy-Item -Destination "Output/${Configuration}/Windows" -Recurse
}
if ($Platform -contains "Linux") {
    Push-Location ./Source
    dotnet publish -f netcoreapp2.0 -c ${Configuration} -r linux-x64 #--version-suffix $Prefix
    Pop-Location
    Move-Item "Source/Output/${Configuration}/netcoreapp2.0/linux-x64/publish" "Output/${Configuration}/Linux"
    Write-Host $PSHome
    Get-ChildItem $PSHome -Directory | Copy-Item -Destination "Output/${Configuration}/Linux" -Recurse
}
if ($Platform -contains "OSx") {
    Push-Location ./Source
    dotnet publish -f netcoreapp2.0 -c ${Configuration} -r osx.10.12-x64 #--version-suffix $Prefix
    Pop-Location
    Move-Item "Source/Output/${Configuration}/netcoreapp2.0/osx.10.12-x64/publish" "Output/${Configuration}/Mac"
    Get-ChildItem $PSHome -Directory | Copy-Item -Destination "Output/${Configuration}/Mac" -Recurse
}

# dotnet publish -f net462 -c ${Configuration} -r win7-x64 --version-suffix $Prefix
# Move-Item "Output/${Configuration}/net462/win7-x64/publish" "../Output/${Configuration}/WindowsPowerShell"

## pack ##
# Bring in the chocolatey scripts
Copy-Item "./Source/tools" "./Output/${Configuration}" -Recurse

if($Package) {
    if($Prefix) {
        $Prefix = "-" + $Prefix
    }
    # Create a catalog and validation
    New-FileCatalog -CatalogFilePath "./Output/${Configuration}/tools/Jupyter-PowerShell.cat" -Path Output/${Configuration}/
    if(Get-Module Authenticode -List) {
        Authenticode\Set-AuthenticodeSignature "./Output/${Configuration}/tools/Jupyter-PowerShell.cat"
    }

    if(Get-Command choco -ErrorAction SilentlyContinue) {
        choco pack --outputdirectory ./Output/${Configuration}

        if ($ChocoApiKey) {
            choco push ./Output/${Configuration}/jupyter-powershell.1.0.0$($Prefix).nupkg --api-key $ChocoApiKey
        }
    } else {
        Write-Warning "Could not find choco command.
        To package, run: choco pack --outputdirectory $(Resolve-Path Output/${Configuration})
        To publish, run: choco push $(Join-Path (Resolve-Path Output/${Configuration}) jupyter-powershell.1.0.0$($Prefix).nupkg)"
    }
}
