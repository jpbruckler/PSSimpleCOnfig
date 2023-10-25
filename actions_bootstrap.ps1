
# Install ModuleFast and required modules
& ([scriptblock]::Create((iwr 'bit.ly/modulefast')))
Install-ModuleFast @{ModuleName='Invoke-Build';RequiredVersion='0.1.3'}, @{ModuleName='Pester';RequiredVersion='5.5.0'}, @{ModuleName='PSScriptAnalyzer';RequiredVersion='1.21.1'}