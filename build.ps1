param(
    $Prefix = "-beta-8",
    ${Configuration} = "Release"
)

Push-Location $PSScriptRoot

## Clean ##
Remove-Item (Join-Path $PSScriptRoot "Output\${Configuration}") -recurse

## Build ##
dotnet restore

dotnet publish -f netcoreapp2.0 -c ${Configuration} -r win7-x64 --version-suffix $Prefix
Move-Item "Output\${Configuration}\netcoreapp2.0\win7-x64\publish" "Output\${Configuration}\Windows"
Set-Content "Output\${Configuration}\Windows\powershell.config.json" '{"Microsoft.PowerShell:ExecutionPolicy":"RemoteSigned"}' -Encoding UTF8

dotnet publish -f netcoreapp2.0 -c ${Configuration} -r linux-x64 --version-suffix $Prefix
Move-Item "Output\${Configuration}\netcoreapp2.0\linux-x64\publish" "Output\${Configuration}\Linux"
Set-Content "Output\${Configuration}\Linux\powershell.config.json" '{"Microsoft.PowerShell:ExecutionPolicy":"RemoteSigned"}' -Encoding UTF8

dotnet publish -f netcoreapp2.0 -c ${Configuration} -r osx.10.12-x64 --version-suffix $Prefix
Move-Item "Output\${Configuration}\netcoreapp2.0\osx.10.12-x64\publish" "Output\${Configuration}\Mac"
Set-Content "Output\${Configuration}\Mac\powershell.config.json" '{"Microsoft.PowerShell:ExecutionPolicy":"RemoteSigned"}' -Encoding UTF8

# dotnet publish -f net462 -c ${Configuration} -r win7-x64 --version-suffix $Prefix
# Move-Item "Output\${Configuration}\net462\win7-x64\publish" "Output\${Configuration}\WindowsPowerShell"


## pack ##
# Clean up the extra build outputs so they don't get packaged
Remove-Item "Output\${Configuration}\net462" -Recurse -ErrorAction SilentlyContinue
Remove-Item "Output\${Configuration}\netcoreapp2.0" -Recurse

# Bring in the chocolatey scripts
Copy-Item ".\tools" "Output\${Configuration}" -Recurse

# Create a catalog and validation
New-FileCatalog -CatalogFilePath "Output\${Configuration}\tools\Jupyter-PowerShell.cat" -Path Output\${Configuration}\
if(Get-Module Authenticode -List) {
    Authenticode\Set-AuthenticodeSignature "Output\${Configuration}\tools\Jupyter-PowerShell.cat"
}

C:\ProgramData\chocolatey\choco.exe pack --outputdirectory Output\${Configuration}

# C:\ProgramData\chocolatey\choco.exe push .\Output\${Configuration}\jupyter-powershell.1.0.0-$($Prefix).nupkg --api-key 8980d6ca-fc5a-4308-a321-8ff21f6a1321

