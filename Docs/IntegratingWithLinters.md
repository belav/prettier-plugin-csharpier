## Integrating with Linters/Formatters
More often than not, linters contain formatting rules that conflict with CSharpier.

### Dotnet Format
[Dotnet Format](https://github.com/dotnet/format) provides command line arguments to run different subsets of rules. Running the following will skip the whitespace formatting rules and avoid conflicts with CSharpier
```bash
# Prior to .Net6
dotnet-format --fix-style info --fix-analyzers info

# .Net6
dotnet format style
dotnet format analyzers
```


### StyleCopAnalyzers
[StyleCopAnalyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) is a set of analyzers that implement StyleCop rules. Some of there will conflict with CSharpier. There is an [open issue](https://github.com/belav/csharpier/issues/13) to determine which rules need to be disabled