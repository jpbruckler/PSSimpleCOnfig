BeforeAll {
    $moduleName = 'PSSimpleConfig'
    $script:moduleRoot = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath "..\..\..\Output\$moduleName")).Path
    $cwd = Get-Location
    Set-Location -Path $script:moduleRoot

    if (Get-Module -Name $moduleName -ErrorAction 'SilentlyContinue') {
        #if the module is already in memory, remove it
        Remove-Module -Name $moduleName -Force
    }
    Import-Module ".\$moduleName.psd1" -Force

    $script:moduleExports = Get-Command -Module $ModuleName | Select-Object -ExpandProperty Name
    $script:manifest = Import-PowerShellDataFile ".\$moduleName.psd1"
}

Describe 'PSSimpleConfig: Import-PSSConfig' {
    BeforeAll {
        $script:PathOverride = "$TestDrive\$moduleName"
    }
    AfterEach {
        Remove-Item -Path $script:PathOverride\config -Recurse -Force -ErrorAction SilentlyContinue
    }

    It "Creates config\config.json in the current directory without -PathOverride" {

        Import-PSSConfig
        $configPath = Join-Path -Path ([System.IO.Directory]::GetCurrentDirectory()) -ChildPath 'config\config.json'
        Write-Information ([System.IO.Directory]::GetCurrentDirectory()) -InformationAction Continue
        $configPath | Should -Exist
        Remove-Item -Path $configPath -Force
    }

    It "Creates config\config.json at a given path when -PathOverride is specified" {
        Set-Location $pwd.Path
        Import-PSSConfig -PathOverride $script:PathOverride
        $configPath = Join-Path -Path $script:PathOverride -ChildPath 'config\config.json'
        $configPath | Should -Exist
    }

    It "Creates config\config.json with default values" {
        Import-PSSConfig -PathOverride $script:PathOverride
        $configPath = Join-Path -Path $script:PathOverride -ChildPath 'config\config.json'
        $config = Get-Content -Path $configPath -Raw | ConvertFrom-Json
        $config | Should -BeNullOrEmpty
        $config | Should -BeOfType [System.Management.Automation.PSCustomObject]
    }

    It "Doesn't overwrite exiting config files" {
        $configPath = Join-Path -Path $script:PathOverride -ChildPath 'config\config.json'
        New-Item $script:PathOverride\config -ItemType Directory -Force | Out-Null
        $config = @{
            'test' = 'value'
        }
        $config | ConvertTo-Json | Out-File -FilePath $configPath -Force
        Import-PSSConfig -PathOverride $script:PathOverride
        $config = Get-Content -Path $configPath -Raw | ConvertFrom-Json
        $config | Should -Not -BeNullOrEmpty
        $config | Should -BeOfType [System.Management.Automation.PSCustomObject]
        $config.test | Should -Be 'value'
    }
}

AfterAll {
    Set-Location -Path $cwd
}
