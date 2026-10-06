# 01-update-project-tfms: Update project target frameworks

Update the project files to target net10.0. For multi-targeted libraries, prefer adding net10.0 where appropriate. Ensure SDK-style csproj format is preserved. This task includes updating the TargetFramework/TargetFrameworks properties across affected projects and verifying project files for deprecated elements that must be removed or replaced.

**Done when**: All project files list net10.0 as their target framework (or include net10.0 in TargetFrameworks), and the solution loads in IDE without project file parse errors.

## Research

- Project: Group1AndroidProject/Group1AndroidProject.csproj
  - Current Target Frameworks: net8.0-android;net8.0-ios;net8.0-maccatalyst;net8.0-windows10.0.19041.0
  - Proposed Target Frameworks (from assessment): net8.0-android;net8.0-ios;net8.0-maccatalyst;net8.0-windows10.0.19041.0;net10.0-windows
  - SDK-style: True
  - Package requiring update: Microsoft.Extensions.Logging.Debug (8.0.1 → suggested 10.0.12)
  - API issues flagged: 2 source-incompatible API issues (e.g., TimeSpan.FromSeconds usages)

## Research actions to perform before code changes

- Backup project files by creating a commit (following Commit Strategy).
- Update TargetFramework/TargetFrameworks to include net10.0-windows where appropriate.
- Plan to update Microsoft.Extensions.Logging.Debug to the suggested version after TFMs are updated.
- Record any additional API errors found during build and address them in task 03.
