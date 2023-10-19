$moduleName = 'PSSimpleConfig'
$psd1File   = "$PSScriptRoot\$moduleName\$moduleName.psd1"
Push-Location $PSScriptRoot

# Auto increment build number
# Major.Minor.Build.Revision
# Build = Days since 1/1/2000
# Revision = Number of builds on the same day padded to 3 digits

$buildData = Get-Content .\build.dat -Raw | ConvertFrom-Json
$manifest = Import-PowerShellDataFile $psd1File
$verParts = $manifest.ModuleVersion.Split('.')
if ([string]::IsNullOrEmpty($buildData.LastBuildDate)) {
    $buildData.LastBuildDate = Get-Date -Format O
}
else {
    $lastBuild  = Get-Date $buildData.LastBuildDate -Format 'yyyy-MM-dd'
    $today      = Get-Date -Format 'yyyy-MM-dd'
    $newDay     = $lastBuild -ne $today
}
$buildData.LastBuildDate = Get-Date -Format O
$buildData | ConvertTo-Json | Out-File .\build.dat -Force

if ($newDay) {
    # It's been more than a day since the last build
    $revision = '1'.PadLeft(3, '0')
}
else {
    $revision = ([string]([int]$verParts[-1] + 1)).PadLeft(3,'0')
}
$newBuild = ((get-Date) - (get-date '1/1/2000')).Days
$newVersion = ("{0}.{1}.{2}.{3}" -f $verParts[0], $verParts[1], $newBuild, $revision)


$build = & dotnet build $PSScriptRoot\src -o $PSScriptRoot\Output\$moduleName\bin
if ($build -match 'Build succeeded') {
    (Get-Content $psd1File) -replace "ModuleVersion = '(.*)'", "ModuleVersion = '$newVersion'" | Set-Content $psd1File
    Copy-Item "$PSScriptRoot\$moduleName\*" "$PSScriptRoot\output\$moduleName" -Recurse -Force
}
$build[-5..-1]