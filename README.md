# Liminality

Powerful idiomatic state machines for .NET.



## Cloning and Building

The following will give you a project that is set up and ready to use. 

> Make sure not use a shallow clone so that [GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) can be applied.

```bash
git clone https://github.com/psibr/liminality.git
cd liminality
dotnet build
```

## Running the sample API

Once you've cloned the repo, from the repo root folder, run:

```
dotnet run --project examples/AspNetCoreExample
```

You can now browse to `https://localhost:5001/swagger` to interact with the samples.

## Debugging the samples or liminality

This repo has launch.json and task.json files added to build, run, debug, and navigate to the samples swagger (OpenAPI) in [Visual Studio Code](https://code.visualstudio.com/) using the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).

## Running the unit tests

Once you've cloned the repo, from the repo root folder, run:

```
dotnet test
```

## Contributions

Maintained by .NET OSS organization [卩丂丨乃尺](https://www.psibr.net).

## OSS projects used
[.NET Core](https://dot.net)
[Visual Studio Code](https://code.visualstudio.com/)
[Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
[Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)