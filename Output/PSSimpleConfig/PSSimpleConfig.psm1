function Get-PSSCSessionVariable {
    if ($null -ne $script:PSSC) {
        return $script:PSSC
    }
    else {
        Write-Error "Script-scoped variable not found."
    }
}