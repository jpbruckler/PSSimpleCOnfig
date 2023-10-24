---
Module Name: PSSimpleConfig
Module Guid: ab5d284c-7e80-48de-9a0c-58fc81c970b5
Download Help Link: NA
Help Version: 0.1.0
Locale: en-US
---

# PSSimpleConfig Module

## Description

PSSimpleConfig provides a simple way to get, set, and manage configuration data using a JSON backend.
Configuration data is set and returned by providing a dot notation path (my.config.item).

The intent of this module is to provide a simple way to manage configuration data for a script or
module. It is not intended to be a replacement for a full configuration management system.

## PSSimpleConfig Cmdlets

### [Get-PSSConfigPath](Get-PSSConfigPath.md)

Returns a System.IO.FileInfo object representing the path to the active PSSimpleConfig configuration
file.

### [Set-PSSConfigPath](Set-PSSConfigPath.md)

Sets the path to the active PSSimpleConfig configuration file. The path must be to an existing file.
The active file is normally set via `Import-PSSConfig`, but this cmdlet can be used to change the
target file.

### [Get-PSSConfigItem](Get-PSSConfigItem.md)

Returns the value of a configuration item. The item is specified by providing a dot notation path, e.g.
`my.config.item`. If the item does not exist, `$null` is returned.

### [Import-PSSConfig](Import-PSSConfig.md)

Imports a JSON configuration file and sets it as the active configuration file. The path to the
configuration can either be explicitly provided via the `-Path` parameter, or it can be determined
by the `-ModuleName` parameter. If the `-ModuleName` parameter is provided, the current available
modules are searched (via `Get-Module`), if a matching module is found, a new folder named `config`
is created in the module's root directory, and a file named `config.json` is created in the `config`
folder. The path to the new file is then set as the active configuration file.

### [Set-PSSConfigItem](Set-PSSConfigItem.md)

Sets the value of a configuration item. The item is specified by providing a dot notation path, e.g.
`my.config.item`. If the item does not exist, it is created. If the item already exists, it is
overwritten. If `$null` is provided as the value, the item is deleted.
