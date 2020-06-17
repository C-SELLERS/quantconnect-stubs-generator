# Lean Python Generator

[![Build Status](https://github.com/jmerle/lean-python-generator/workflows/Build/badge.svg)](https://github.com/jmerle/lean-python-generator/actions?query=workflow%3ABuild)  

After seeing QuantConnect create the [QuantConnect/quantconnect-lean](https://github.com/QuantConnect/quantconnect-lean) project, I challenged myself to do the same thing (generating Python type hints based on [QuantConnect/Lean's](https://github.com/QuantConnect/Lean) C# codebase) for the heck of it. The resulting project is capable of converting C# classes/structs/enums/interfaces/fields/properties/methods to Python classes/members with the correct type conversions, decorators, generics and documentation.

## Installation

Although at the time of writing this [QuantConnect/quantconnect-lean](https://github.com/QuantConnect/quantconnect-lean) is no more than a simple setup, to prevent against disambiguity issues in the future types generated by this project will not be published to PyPi. To install the types manually follow the following steps:
1. Download the latest lean-types.zip artifact from the latest build listed [here](https://github.com/jmerle/lean-python-generator/actions?query=workflow:Build)
2. Unzip the downloaded zip and `cd` into the extracted directory you extracted the zip's contents to
3. Run `python setup.py install`

## Development

To further develop the project, clone the repository, `cd` into the LeanPythonGenerator project and run `dotnet run <Lean path> <Output directory>`.
