## CSharpier
CSharpier is an opinionated code formatter for c#. It uses Roslyn to parse your code and re-prints it using its own rules. 
The printing process was ported from [prettier](https://github.com/prettier/prettier) but has evolved over time.  
CSharpier provides a few basic options that affect formatting and has no plans to add more. It follows the [Option Philosophy](https://prettier.io/docs/en/option-philosophy.html) of prettier.

### Work In Progress
CSharpier is still in active development.
- The rules it uses to format code are not yet finalized and some of the results are a little ugly.
- The rules it uses to format your code will change over time.
- It has been tested against a large number of public repositories to validate that the changes it makes do not lose any syntax but there is a possibility it will break your code.
    - This process has been automated and runs each time code is pushed.


I encourage you to try it out on your own code and report any bugs, code that doesn't format well, or opinions on how you think the formatting should change. If you can live with the fact that the formatting will be changing over time, it is reasonably safe to use. In addition to a steadily growing set of unit tests; csharpier is tested against ~60k c# files from a range of public repositories to validate it does not result in the lose of any source code. 

### Features
  - It is fast. It can format a solution of ~11,000 files in ~30 seconds.
  - It supports validating the syntax of the code it produces to ensure the only changes made were whitespace and line breaks.
  - It formats c# <= 9.0

Try it out at [Playground](https://csharpier.com)

### Before
```c#
public class ClassName {
    public string ShortPropertyName {
        get;
        set; 
    }

    public void LongUglyMethod(string longParameter1, string longParameter2, string longParameter3) { 
        this.LongUglyMethod("1234567890", "abcdefghijklmnopqrstuvwxyz", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
    }
}
```

### After
```c#
public class ClassName
{
    public string ShortPropertyName { get; set; }

    public void LongUglyMethod(
        string longParameter1,
        string longParameter2,
        string longParameter3
    ) {
        this.LongUglyMethod(
            "1234567890",
            "abcdefghijklmnopqrstuvwxyz",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        );
    }
}
```

## Installation

```console
dotnet tool install -g csharpier
```

### Basic Usage
Run csharpier from the directory you wish to format to recursively format all .cs files
```console
dotnet csharpier .
```

Run csharpier on a specific file
```console
dotnet csharpier /src/UglyUnformattedFile.cs
```

### MsBuild Package
If you prefer to have csharpier run when a project is built, you can use the CSharpier.MSBuild nuget package
```console
Install-Package CSharpier.MSBuild
```

## Documentation
[Command Line Interface](Docs/CLI.md)  
[Configuration File](Docs/Configuration.md)  
[Editors and CI/CD](Docs/EditorsAndCICD.md)  
[Integrating with Linters/Formatters](Docs/IntegratingWithLinters.md)  
[Ignoring Files](Docs/Ignore.md)  
[ChangeLog](CHANGELOG.md)  
[MSBuild Package](Docs/MSBuild.md)


## Contributing
See [Development Readme](CONTRIBUTING.md)  

Join Us [![Discord](https://img.shields.io/badge/Discord-chat?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/HfAKGEZQcX)
