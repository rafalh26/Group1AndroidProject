# 02-update-packages: Update NuGet packages to compatible stable versions

Upgrade NuGet package references to the latest stable versions compatible with net10.0. Prioritize security fixes and packages flagged in the assessment. Record any packages that have no compatible version and mark them as blockers for follow-up.

**Done when**: All packages that have compatible updates are updated and package restore succeeds for the solution.

## Research

- Project: Group1AndroidProject/Group1AndroidProject.csproj
  - Packages identified in assessment:
    - Microsoft.Extensions.Logging.Debug: current 8.0.1 → suggested 10.0.12 (update recommended)
    - Npgsql: current 9.0.2 → compatible
    - Microsoft.Maui packages: use $(MauiVersion) (set to 10.0.0 in Directory.Build.props)

## Actions

1. Update Microsoft.Extensions.Logging.Debug to 10.0.12 in project file.
2. Run `dotnet restore` and `dotnet build`.
3. If additional package updates are suggested by restore warnings or assessment, apply them and re-test.
4. Commit package updates per commit strategy.
