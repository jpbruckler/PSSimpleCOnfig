<#
.SYNOPSIS
    An Invoke-Build Build file.
.DESCRIPTION
    Build steps can include:
        - ValidateRequirements
        - ImportModuleManifest
        - Clean
        - Analyze
        - FormattingCheck
        - Test
        - DevCC
        - CreateHelpStart
        - Build
        - IntegrationTest
        - Archive
.EXAMPLE
    Invoke-Build

    This will perform the default build Add-BuildTasks: see below for the default Add-BuildTask execution
.EXAMPLE
    Invoke-Build -Add-BuildTask Analyze,Test

    This will perform only the Analyze and Test Add-BuildTasks.
.NOTES
    This build file by Catesta will pull in configurations from the "<module>.Settings.ps1" file as well, where users can more easily customize the build process if required.
    https://github.com/nightroman/Invoke-Build
    https://github.com/nightroman/Invoke-Build/wiki/Build-Scripts-Guidelines
    If using VSCode you can use the generated tasks.json to execute the various tasks in this build file.
        Ctrl + P | then type task (add space) - you will then be presented with a list of available tasks to run
    The 'InstallDependencies' Add-BuildTask isn't present here.
        Module dependencies are installed at a previous step in the pipeline.
        If your manifest has module dependencies include all required modules in your CI/CD bootstrap file:
            AWS - install_modules.ps1
            Azure - actions_bootstrap.ps1
            GitHub Actions - actions_bootstrap.ps1
            AppVeyor  - actions_bootstrap.ps1
#>

#Include: Settings
$ModuleName = (Split-Path -Path $BuildFile -Leaf).Split('.')[0]
. "./$ModuleName.Settings.ps1"

function Test-ManifestBool ($Path) {
    Get-ChildItem $Path | Test-ModuleManifest -ErrorAction SilentlyContinue | Out-Null; $?
}

#Default Build
$str = @()
$str = 'Clean', 'ValidateRequirements', 'ImportModuleManifest'
$str += 'FormattingCheck'
$str += 'Analyze', 'Test'
$str += 'CreateHelpStart'
$str2 = $str
$str2 += 'Build', 'Archive'
$str += 'Build', 'IntegrationTest', 'Archive'
Add-BuildTask -Name . -Jobs $str

#Local testing build process
Add-BuildTask TestLocal Clean, ImportModuleManifest, Analyze, Test

#Local help file creation process
Add-BuildTask HelpLocal Clean, ImportModuleManifest, CreateHelpStart

#Full build sans integration tests
Add-BuildTask BuildNoIntegration -Jobs $str2

# Pre-build variables to be used by other portions of the script
Enter-Build {
    $script:ModuleName = (Split-Path -Path $BuildFile -Leaf).Split('.')[0]

    # Identify other required paths
    $script:ModuleSourcePath = Join-Path -Path $BuildRoot -ChildPath
    $script:ModuleFiles = Join-Path -Path $script:ModuleSourcePath -ChildPath '*'

    $script:ModuleManifestFile = Join-Path -Path $script:ModuleSourcePath -ChildPath "$($script:ModuleName).psd1"

    $manifestInfo = Import-PowerShellDataFile -Path $script:ModuleManifestFile
    $script:ModuleVersion = $manifestInfo.ModuleVersion
    $script:ModuleDescription = $manifestInfo.Description
    $script:FunctionsToExport = $manifestInfo.FunctionsToExport

    $script:TestsPath = Join-Path -Path $BuildRoot -ChildPath 'Tests'
    $script:UnitTestsPath = Join-Path -Path $script:TestsPath -ChildPath 'Unit'
    $script:IntegrationTestsPath = Join-Path -Path $script:TestsPath -ChildPath 'Integration'

    $script:ArtifactsPath = Join-Path -Path $BuildRoot -ChildPath 'Artifacts'
    $script:ArchivePath = Join-Path -Path $BuildRoot -ChildPath 'Archive'

    $script:BuildModuleRootFile = Join-Path -Path $script:ArtifactsPath -ChildPath "$($script:ModuleName).psm1"

    # Ensure our builds fail until if below a minimum defined code test coverage threshold
    $script:coverageThreshold = 30

    [version]$script:MinPesterVersion = '5.2.2'
    [version]$script:MaxPesterVersion = '5.99.99'
    $script:testOutputFormat = 'NUnitXML'
} #Enter-Build

# Define headers as separator, task path, synopsis, and location, e.g. for Ctrl+Click in VSCode.
# Also change the default color to Green. If you need task start times, use `$Task.Started`.
Set-BuildHeader {
    param($Path)
    # separator line
    Write-Build DarkMagenta ('=' * 79)
    # default header + synopsis
    Write-Build DarkGray "Task $Path : $(Get-BuildSynopsis $Task)"
    # task location in a script
    Write-Build DarkGray "At $($Task.InvocationInfo.ScriptName):$($Task.InvocationInfo.ScriptLineNumber)"
    Write-Build Yellow "Manifest File: $script:ModuleManifestFile"
    Write-Build Yellow "Manifest Version: $($manifestInfo.ModuleVersion)"
} #Set-BuildHeader

# Define footers similar to default but change the color to DarkGray.
Set-BuildFooter {
    param($Path)
    Write-Build DarkGray "Done $Path, $($Task.Elapsed)"
    # # separator line
    # Write-Build Gray ('=' * 79)
} #Set-BuildFooter

# Synopsis: Build help for module
Add-BuildTask CreateHelpStart {
    Write-Build White '      Performing all help related actions.'

    Write-Build Gray '           Importing platyPS ...'
    Import-Module platyPS -ErrorAction Stop
    Write-Build Gray '           ...platyPS imported successfully.'
} #CreateHelpStart

# Synopsis: Build markdown help files for module and fail if help information is missing
Add-BuildTask CreateMarkdownHelp -After CreateHelpStart {
    $ModulePage = "$script:ArtifactsPath\docs\$($ModuleName).md"

    $markdownParams = @{
        Module         = $ModuleName
        OutputFolder   = "..\docs\"
        Force          = $true
        WithModulePage = $true
        Locale         = 'en-US'
        FwLink         = "NA"
        HelpVersion    = '0.1.0'
    }

    Write-Build Gray '           Generating markdown files...'
    $null = New-MarkdownHelp @markdownParams
    Write-Build Gray '           ...Markdown generation completed.'

    Write-Build Gray '           Replacing markdown elements...'
    # Replace multi-line EXAMPLES
    $OutputDir = "$script:ArtifactsPath\docs\"
    $OutputDir | Get-ChildItem -File | ForEach-Object {
        # fix formatting in multiline examples
        $content = Get-Content $_.FullName -Raw
        $newContent = $content -replace '(## EXAMPLE [^`]+?```\r\n[^`\r\n]+?\r\n)(```\r\n\r\n)([^#]+?\r\n)(\r\n)([^#]+)(#)', '$1$3$2$4$5$6'
        if ($newContent -ne $content) {
            Set-Content -Path $_.FullName -Value $newContent -Force
        }
    }
    # Replace each missing element we need for a proper generic module page .md file
    $ModulePageFileContent = Get-Content -Raw $ModulePage
    $ModulePageFileContent = $ModulePageFileContent -replace '{{Manually Enter Description Here}}', $script:ModuleDescription
    $script:FunctionsToExport | ForEach-Object {
        Write-Build DarkGray "             Updating definition for the following function: $($_)"
        $TextToReplace = "{{Manually Enter $($_) Description Here}}"
        $ReplacementText = (Get-Help -Detailed $_).Synopsis
        $ModulePageFileContent = $ModulePageFileContent -replace $TextToReplace, $ReplacementText
    }

    $ModulePageFileContent | Out-File $ModulePage -Force -Encoding:utf8
    Write-Build Gray '           ...Markdown replacements complete.'

    Write-Build Gray '           Verifying GUID...'
    $MissingGUID = Select-String -Path "$script:ArtifactsPath\docs\*.md" -Pattern "(00000000-0000-0000-0000-000000000000)"
    if ($MissingGUID.Count -gt 0) {
        Write-Build Yellow '             The documentation that got generated resulted in a generic GUID. Check the GUID entry of your module manifest.'
        throw 'Missing GUID. Please review and rebuild.'
    }

    Write-Build Gray '           Checking for missing documentation in md files...'
    $MissingDocumentation = Select-String -Path "$script:ArtifactsPath\docs\*.md" -Pattern "({{.*}})"
    if ($MissingDocumentation.Count -gt 0) {
        Write-Build Yellow '             The documentation that got generated resulted in missing sections which should be filled out.'
        Write-Build Yellow '             Please review the following sections in your comment based help, fill out missing information and rerun this build:'
        Write-Build Yellow '             (Note: This can happen if the .EXTERNALHELP CBH is defined for a function before running this build.)'
        Write-Build Yellow "             Path of files with issues: $script:ArtifactsPath\docs\"
        $MissingDocumentation | Select-Object FileName, LineNumber, Line | Format-Table -AutoSize
        throw 'Missing documentation. Please review and rebuild.'
    }

    Write-Build Gray '           Checking for missing SYNOPSIS in md files...'
    $fSynopsisOutput = @()
    $synopsisEval = Select-String -Path "$script:ArtifactsPath\docs\*.md" -Pattern "^## SYNOPSIS$" -Context 0, 1
    $synopsisEval | ForEach-Object {
        $chAC = $_.Context.DisplayPostContext.ToCharArray()
        if ($null -eq $chAC) {
            $fSynopsisOutput += $_.FileName
        }
    }
    if ($fSynopsisOutput) {
        Write-Build Yellow "             The following files are missing SYNOPSIS:"
        $fSynopsisOutput
        throw 'SYNOPSIS information missing. Please review.'
    }

    Write-Build Gray '           ...Markdown generation complete.'
} #CreateMarkdownHelp

# Synopsis: Build the external xml help file from markdown help files with PlatyPS
Add-BuildTask CreateExternalHelp -After CreateMarkdownHelp {
    Write-Build Gray '           Creating external xml help file...'
    $null = New-ExternalHelp "$script:ArtifactsPath\docs" -OutputPath "$script:ArtifactsPath\en-US\" -Force
    Write-Build Gray '           ...External xml help file created!'
} #CreateExternalHelp

Add-BuildTask CreateHelpComplete -After CreateExternalHelp {
    Write-Build Green '      ...CreateHelp Complete!'
} #CreateHelpStart

# Synopsis: Replace comment based help (CBH) with external help in all public functions for this project
Add-BuildTask UpdateCBH -After AssetCopy {
    $ExternalHelp = @"
<#
.EXTERNALHELP $($ModuleName)-help.xml
#>
"@

    $CBHPattern = "(?ms)(\<#.*\.SYNOPSIS.*?#>)"
    Get-ChildItem -Path "$script:ArtifactsPath\Public\*.ps1" -File | ForEach-Object {
        $FormattedOutFile = $_.FullName
        Write-Output "      Replacing CBH in file: $($FormattedOutFile)"
        $UpdatedFile = (Get-Content  $FormattedOutFile -raw) -replace $CBHPattern, $ExternalHelp
        $UpdatedFile | Out-File -FilePath $FormattedOutFile -force -Encoding:utf8
    }
} #UpdateCBH
