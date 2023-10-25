BeforeAll {
    $moduleName = 'PSSimpleConfig'
    $moduleRoot = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath "..\..\..\Output\$moduleName")).Path
    $cwd = Get-Location
    Set-Location -Path $moduleRoot

    if (Get-Module -Name $moduleName -ErrorAction 'SilentlyContinue') {
        #if the module is already in memory, remove it
        Remove-Module -Name $moduleName -Force
    }
    Import-Module ".\$moduleName.psd1" -Force

    $script:moduleExports = Get-Command -Module $ModuleName | Select-Object -ExpandProperty Name
    $script:manifest = Import-PowerShellDataFile ".\$moduleName.psd1"
}

Describe "PSSimpleConfig: Set-PSConfigItem" {

}

AfterAll {
    Set-Location -Path $cwd
}