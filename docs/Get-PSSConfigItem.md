---
external help file: PSSimpleConfig.dll-Help.xml
Module Name: PSSimpleConfig
online version:
schema: 2.0.0
---

# Get-PSSConfigItem

## SYNOPSIS

Returns the value of a configuration item. When given a path, the value of the item at the path is
returned. If no path is provided, the entire object is returned.

## SYNTAX

```powershell
Get-PSSConfigItem [[-Path] <String>] [-ConfigFile <FileInfo>] [<CommonParameters>]
```

## DESCRIPTION

`Get-PSSConfigItem` takes a path made up of JSON properties separated by a dot (.) and returns the value
of the item at the path. If no path is provided, the entire object is returned. If the path does not
exist, `$null` is returned.

Under the hood, `Get-PSSConfigItem` uses Newtonsoft.Json to parse the JSON file into a `JObject` object,
and supports a subset of the functionality provided by the `SelectToken` method. See the **LINKS** section
for a link to the `SelectToken` documentation.

## EXAMPLES

### Example 1

Given this JSON file:

```json
{
    "my": {
        "config": {
            "item": "value"
        }
    }
}
```

```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

### Example 2

A more advanced example, taken from the Newtonsoft SelectToken documentation.

```json
{
  "Stores": ["Lambton Quay", "Willis Street"],
  "Manufacturers": [
    {
      "Name": "Acme Co",
      "Products": [
        {
          "Name": "Anvil",
          "Price": 50
        }
      ]
    },
    {
      "Name": "Contoso",
      "Products": [
        {
          "Name": "Elbow Grease",
          "Price": 99.95
        },
        {
          "Name": "Headlight Fluid",
          "Price": 4
        }
      ]
    }
  ]
}
```

```powershell
C:\PS> Get-PSSConfigItem -Path 'Manufacturers[0].Name'
Acme Co

C:\PS> Get-PSSConfigItem -Path 'Manufacturers[0].Products[0].Name'
Anvil

C:\PS> Get-PSSConfigItem -Path 'Manufacturers[1].Products[0].Name'
Elbow Grease

C:\PS> Get-PSSConfigItem -Path "`$.Manufacturers[?(@.Name == 'Acme Co')]"
Name    Products
----    --------
Acme Co {@{Name=Anvil; Price=50}}
```

## PARAMETERS

### -Path

The JSONPath to the item to return. If no path is provided, the entire object is returned.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS

- [Newtonsoft.JSon SelectToken](https://www.newtonsoft.com/json/help/html/SelectToken.htm)
- [JSONPath](https://goessner.net/articles/JsonPath/)
