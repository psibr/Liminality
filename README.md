# PSIBR.Liminality

>Transition library by a transfem so you know it's good

PSIBR.Liminality is a basic state machine modelling library that handles a lot of organization descisions for you, but doesn't try to boil the ocean. 

Bring your own state storage and fancy behaviors.


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
dotnet run --project samples
```

You can now browse to https://localhost:5001/swagger to interact with the samples.

## Debugging the samples or Liminality

This repo has launch.json and task.json files added to build, run, debug, and navigate to the samples swagger (OpenAPI) in [Visual Studio Code](https://code.visualstudio.com/) using the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).

## Running the unit tests

Once you've cloned the repo, from the repo root folder, run:

```
dotnet test
```

## Special thanks to contributors

- [zoeysaurusrex](https://github.com/zoeysaurusrex)

## Libraries and tools we use

These are the tools used to build Liminality 🥂

- [.NET Core](https://dot.net)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
