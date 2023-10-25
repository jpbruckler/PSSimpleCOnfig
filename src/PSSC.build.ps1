#requires -modules InvokeBuild
<#
.SYNOPSIS
    Build script for PSSimpleConfig module.
.NOTES
    $BuildRoot is a global variable that is set by the build script and points
    to the root of the build directory, in this case .\src
#>

param(
    $Configuration = 'Debug'
)


Add-BuildTask -Name TestLocal -Jobs Clean

Enter-Build {
    $script:ModuleName          = 'PSSimpleConfig'
    $script:ProjectRoot         = (Resolve-Path "$BuildRoot\..\").Path
    $script:DocsRoot            = Join-Path $ProjectRoot -ChildPath 'docs'
    $script:TestsRoot           = Join-Path $BuildRoot -ChildPath 'tests'
    $script:ArtifactsPath       = Join-Path $BuildRoot -ChildPath 'artifacts'
    $script:ArchivePath         = Join-Path $BuildRoot -ChildPath 'archive'
    $script:BuildOutputPath     = Join-Path $ProjectRoot -ChildPath 'Output'
    $script:ModuleManifestFile  = (Resolve-Path (Join-Path $BuildRoot "$($script:ModuleName)\*.psd1")).Path
    
    $manifestInfo               = Import-PowerShellDataFile $script:ModuleManifestFile
    
    if ($Configuration -eq 'Release') {
        $script:Version        = (Get-Content "$BuildRoot\..\build.dat" -Raw | ConvertFrom-Json).ReleaseVersion
    }
    else {
        $script:Version        = $manifestInfo.ModuleVersion
    }

    
}

Set-BuildHeader {
    param($Path)

    Write-Build DarkMagenta ("=" * 79)
    Write-Build DarkMagenta "    Module.........: $ModuleName"
    Write-Build DarkMagenta "    Version........: $Version"
    Write-Build DarkMagenta "    Configuration..: $Configuration"
    Write-Build Yellow      "    Project Root...: $script:ProjectRoot"
    Write-Build Yellow      "    Manifest File..: $script:ModuleManifestFile"
    Write-Build DarkMagenta ("=" * 79)

    Write-Build DarkGray "Task $Path : $(Get-BuildSynopsis $Task)"
    # task location in a script
    Write-Build DarkGray "    At $($Task.InvocationInfo.ScriptName):$($Task.InvocationInfo.ScriptLineNumber)"
} #Set-BuildHeader

# Define footers similar to default but change the color to DarkGray.
Set-BuildFooter {
    param($Path)
    Write-Build DarkGray "Done $Path, $($Task.Elapsed)"
    # # separator line
    # Write-Build Gray ('=' * 79)
} #Set-BuildFooter


task . {
    Write-Build Yellow "    Building module $ModuleName version $Version..."
}

task Clean {
    Write-Build Green "    Performing cleanup tasks..."
    Write-Build White "        Removing children of $BuildOutputPath..."
    Remove-Item -Path $BuildOutputPath\* -Recurse -Force -ErrorAction SilentlyContinue

    Write-Build White "        Removing children of $ArtifactsPath..."
    Remove-Item -Path $ArtifactsPath\* -Recurse -Force -ErrorAction SilentlyContinue

    Write-Build White "        Removing children of $ArchivePath..."
    Remove-Item -Path $ArchivePath\* -Recurse -Force -ErrorAction SilentlyContinue

    Write-Build Green "    Cleanup complete."
}

