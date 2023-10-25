BeforeAll {
    $moduleName = 'PSSimpleConfig'
    $moduleRoot = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath "..\Output\$moduleName")).Path
    $buildData  = Get-Content "$PSScriptRoot\..\build.dat" -Raw | ConvertFrom-Json
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
Describe 'Module Export Tests' -Tag Unit {
    Context 'Exported Commands' {
        It 'Public command count matches the number of commands in the manifest' {
            $exportCount = [int]$script:manifest.FunctionsToExport.Count + [int]$script:manifest.CmdletsToExport.Count
            $exportCount | Should -BeExactly $script:moduleExports.Count
        }
    }
}
AfterAll {
    Set-Location -Path $cwd
}