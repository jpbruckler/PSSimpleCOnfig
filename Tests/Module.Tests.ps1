BeforeAll {
    $moduleName = 'PSSimpleConfig'
    $moduleRoot = (Resolve-Path (Join-Path -Path $PSScriptRoot -ChildPath "..\Output\$moduleName")).Path
    $buildData  = Get-Content "$PSScriptRoot\..\build.dat" -Raw | ConvertFrom-Json
    $cwd = Get-Location
    Set-Location -Path $moduleRoot
    $manifest = Import-PowerShellDataFile ".\$moduleName.psd1"
    $script:ManifestEval = $null
}

Describe 'Module Tests' -Tag Unit {
    Context "Module Tests" {
        It 'Passes Test-ModuleManifest' {
            { $script:ManifestEval = Test-ModuleManifest -Path ".\$moduleName.psd1" } | Should -Not -Throw
            $? | Should -BeTrue
        }

        It 'Should have a matching module name in the manifest' {
            $script:manifestEval.Name | Should -BeExactly $moduleName
        }

        It 'Should have a valid description in the manifest' {
            $script:manifestEval.Description | Should -Not -BeNullOrEmpty
        }

        It 'Should have a valid author in the manifest' {
            $script:manifestEval.Author | Should -Not -BeNullOrEmpty
        }

        It 'Should have a valid version in the manifest' {
            $script:manifestEval.Version -as [Version] | Should -Not -BeNullOrEmpty
        }

        It 'Should have a valid guid in the manifest' {
            { [guid]::Parse($script:manifestEval.Guid) } | Should -Not -Throw
        }

        It 'Should not have any spaces in the tags' {
            foreach ($tag in $script:manifestEval.Tags) {
                $tag | Should -Not -Match '\s'
            }
        }

        It 'Should have a valid project Uri' {
            $script:manifestEval.ProjectUri | Should -Not -BeNullOrEmpty
        }

        It 'Has the correct License URI' {
            $manifest.PrivateData.PSData.LicenseUri | Should -BeExactly 'https://choosealicense.com/licenses/mit/'
        }

        It 'Has the correct Project URI' {
            $manifest.PrivateData.PSData.ProjectUri | Should -BeExactly 'https://github.com/jpbruckler/PSSimpleConfig'
        }

        It 'Correctly sets the module version' {
            $manifest.ModuleVersion -as [Version] | Should -Be ([Version]::Parse($buildData.LastVersion))
        }

        It "Has a valid Root Module [$moduleName.psm1]" {
            $manifest.RootModule | Should -Be "$moduleName.psm1"
        }
    }
}
AfterAll {
    Set-Location -Path $cwd
}
