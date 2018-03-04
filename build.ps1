$Prefix = "-beta-8"
Push-Location $PSScriptRoot

## Clean ##
Remove-Item (Join-Path $PSScriptRoot Output\Release) -recurse

## Build ##
dotnet restore

dotnet publish -f netcoreapp2.0 -c Release -r win7-x64 --version-suffix $Prefix
Move-Item "Output\Release\netcoreapp2.0\win7-x64\publish" "Output\Release\Windows"

dotnet publish -f netcoreapp2.0 -c Release -r linux-x64 --version-suffix $Prefix
Move-Item "Output\Release\netcoreapp2.0\linux-x64\publish" "Output\Release\Linux"

dotnet publish -f netcoreapp2.0 -c Release -r osx.10.12-x64 --version-suffix $Prefix
Move-Item "Output\Release\netcoreapp2.0\osx.10.12-x64\publish" "Output\Release\Mac"

# dotnet publish -f net462 -c Release -r win7-x64 --version-suffix $Prefix
# Move-Item "Output\Release\net462\win7-x64\publish" "Output\Release\WindowsPowerShell"

## pack ##
# Clean up the extra build outputs so they don't get packaged
Remove-Item Output\Release\net462 -Recurse
Remove-Item Output\Release\netcoreapp2.0 -Recurse

# Bring in the chocolatey scripts
Copy-Item tools Output\Release -Recurse

# Create a catalog and validation
New-FileCatalog -CatalogFilePath Output\Release\tools\Jupyter-PowerShell.cat -Path Output\Release\
if(Get-Module Authenticode -List) {
    Authenticode\Set-AuthenticodeSignature Output\Release\tools\Jupyter-PowerShell.cat
}

C:\ProgramData\chocolatey\choco.exe pack --outputdirectory Output\Release

# C:\ProgramData\chocolatey\choco.exe push .\Output\Release\jupyter-powershell.1.0.0-$($Prefix).nupkg --api-key 8980d6ca-fc5a-4308-a321-8ff21f6a1321

