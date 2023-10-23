# PSSimpleConfig

## Synopsis

<!-- Enter a synopsis -->

## Description

Intended to be a simple PowerShell module written in C# with 2 primary goals:

1. Provide myself an opportunity to write a relatively simple binary module.
2. Provide a module that can be used to ease interaction with using JSON files for module configuration, something I tend to use a lot in modules I write.

PSSimpleConfig is a convention over configuration module, meaning it's intended to be used without it's own special configuration. The primary use case is for this to be a 'helper module' for other PowerShell modules. By default it will create a `$ModuleRoot\conf\config.json` file, where `$ModuleRoot` is the root of the module whose configuration is being managed. This is determined by running `(Get-Module -ListAvailable -Name "modulename").ModuleBase`. *Edit: currently it uses `[System.IO.Directory]::GetCurrentDirectory()` to determine the config root, but this will be changed to the former method in the future.*

## Why

The whole point is to make getting and setting variables in JSON-backed scenarios easier. PSSimpleConfig supports dot-notation navigation of JSON data structures, e.g; `Get-PSSConfigItem MyModule.ActiveDirectory.Domain.Name`. Setting the values is done the same way. Deleting nodes in the JSON is done by setting the value to `$null`.

## Getting Started

### Prerequisites

<!-- list any prerequisites -->

### Installation

```powershell
# how to install PSSimpleConfig

```

### Quick start

#### Example1

### Example Usage

We have a fictional module called `MyCoolModule`, that has a directory structure like below:

```cmd
\MyCoolModule
 |   MyCoolModule.psd1
 |   MyCoolModule.psm1
 |
 +---Private
 |       Set-SomethingCool.ps1
 |
 \---Public
         Get-SomethingCool.ps1
```

Adding PSSimpleConfig to your module and calling Import-PSSConfig will looking in the current directory for `conf\config.json` and if it doesn't exist, will create it.

On initial creation the file will be an empty json object:

```json
{

}
```

As you add items to the configuration, the file will be updated with additional data.

Running the command below from the module .psm1 file...

```powershell
Set-PSSConfigItem -Path ModuleRoot -Value $PSScriptRoot
```

...will update the `config.json` file to look like:

```json
{
 "ModuleRoot": "C:\\Program Files\\PowerShell\\7\\Modules\\MyCoolModule"
}
```

Then from any other cmdlet or function in your module you can execute

```powerhshell
Get-PSSconfig -Path ModuleRoot
```

And it will return `C:\Program Files\PowerShell\7\Modules\MyCoolModule`.

Setting and retrieving nested configuration items is just as simple.

```powershell
Set-PSSConfigItem -Path AD.Domain -Value @{ Name = 'contoso.com', DnsRoot = 'contoso.com'}
```

Will update the config file to look like:

```json
{
 "ModuleRoot": "C:\\Program Files\\PowerShell\\7\\Modules\\MyCoolModule"
 "AD": {
  "Domain": {
   "Name": "contoso.com",
   "DnsRoot": "contoso.com"
  }
 }
}
```

Giving `Get-PSSConfigItem` a path of `AD.Domain` will return a PSObject containing the note properties Name and DnsRoot.

## Author

John Bruckler
