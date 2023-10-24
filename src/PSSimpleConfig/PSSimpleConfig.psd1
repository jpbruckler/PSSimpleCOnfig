@{
    RootModule         = 'PSSimpleConfig.psm1'
    NestedModules      = @('bin\PSSimpleConfig.dll')

    ModuleVersion      = '0.0.8696.016'
    GUID               = 'ab5d284c-7e80-48de-9a0c-58fc81c970b5'
    Author             = 'John Bruckler'
    CompanyName        = 'John Bruckler'
    Copyright          = '(c) John Bruckler. All rights reserved.'

    Description        = 'PSSimpleConfig provides a simple way to get, set, and manage configuration
                        data using a JSON backend. Configuration data is set and returned by
                        providing a dot notation path (my.config.item, or my.config.item = "value"
                        to set).'

    PowerShellVersion  = '7.3'

    RequiredAssemblies = @(
        'bin\Newtonsoft.Json.dll'
    )

    FunctionsToExport  = @(
        'Get-PSSConfigPath',
        'Set-PSSConfigPath'
    )

    CmdletsToExport    = @(
        'Get-PSSConfigitem',
        'Get-PSSCModuleInfo',
        'Set-PSSConfigitem',
        'Import-PSSConfig'
    )

    FileList           = @(
        'bin\Newtonsoft.Json.dll',
        'bin\PSSimpleConfig.dll',
        'PSSimpleConfig.psm1',
        'PSSimpleConfig.psd1'
    )

    PrivateData        = @{
        PSData = @{
            Tags                     = @('Config', 'Configuration')
            LicenseUri               = 'https://choosealicense.com/licenses/mit/'
            ProjectUri               = 'https://github.com/jpbruckler/PSSimpleConfig'
            # IconUri = ''
            # ReleaseNotes = ''
            RequireLicenseAcceptance = $false
        }

    }

    # HelpInfoURI = ''
}

