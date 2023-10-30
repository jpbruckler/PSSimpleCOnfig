function Set-PSSConfigPath {
    <#
    .SYNOPSIS
        Sets the path to the active PSSimpleConfig configuration file.
    .DESCRIPTION
        Sets the path to the active PSSimpleConfig configuration file. The path
        must be to an existing file.
    .PARAMETER Path
        The path to the JSON file to use with PSSimpleConfig.
    .INPUTS
        System.IO.FileInfo
    .OUTPUTS
        None
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [ValidateScript({
            if (Test-Path -PathType Leaf $_) {
                return $true
            } else {
                throw "Path must be a valid file path."
            }
        })]
        [System.IO.FileInfo]$Path
    )

    process {
        [PSSimpleConfig.Utilities.PSSC]::Instance.UpdateConfigPath($Path)
    }
}

function Get-PSSConfigPath {
    <#
    .SYNOPSIS
        Returns the path to the active PSSimpleConfig configuration file.
    .DESCRIPTION
        Returns a System.IO.FileInfo object representing the path to the active
        PSSimpleConfig configuration file.
    .EXAMPLE
        PS C:\> Get-PSSConfigPath

        Mode                 LastWriteTime         Length Name
        ----                 -------------         ------ ----
        -a---          10/23/2023  8:24 PM             46 config.json
    .INPUTS
        None
    .OUTPUTS
        System.IO.FileInfo
    #>
    [CmdletBinding()]
    param()

    process {
        [PSSimpleConfig.Utilities.PSSC]::Instance.ConfigPath
    }
}