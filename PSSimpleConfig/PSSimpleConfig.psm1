function Get-PSSCSessionVariable {
    if ($null -ne $script:PSSimpleConfig) {
        return $script:PSSimpleConfig
    }
    else {
        Write-Error "Script-scoped variable not found."
    }
}