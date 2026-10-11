$ErrorActionPreference = 'Stop'
dotnet publish "$PSScriptRoot\ScreenQuarters\ScreenQuarters.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "$PSScriptRoot\publish"
if ($LASTEXITCODE -ne 0) { throw 'Publish başarısız.' }
Write-Host "Hazır: $PSScriptRoot\publish\ScreenQuarters.exe"
