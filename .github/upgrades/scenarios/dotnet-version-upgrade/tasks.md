# .NET Version Upgrade Progress

## Overview

Upgrade the Group1AndroidProject solution to net10.0 using a Bottom-Up strategy: update library and shared projects first, then the MAUI application. The plan focuses on updating TFMs, applying package updates, addressing breaking changes, and validating the MAUI app.

**Progress**: 3/6 tasks complete <progress value="50" max="100"></progress> 50%

## Tasks

- ✅ 01-update-project-tfms: Update project target frameworks ([Content](tasks/01-update-project-tfms/task.md), [Progress](tasks/01-update-project-tfms/progress-details.md))
- ✅ 02-update-packages: Update NuGet packages to compatible stable versions ([Content](tasks/02-update-packages/task.md), [Progress](tasks/02-update-packages/progress-details.md))
- 🔄 03-fix-breaking-changes: Address API and build issues introduced by the framework upgrade ([Content](tasks/03-fix-breaking-changes/task.md))
- ✅ 04-maui-adjustments: Apply .NET MAUI-specific compatibility fixes ([Content](tasks/04-maui-adjustments/task.md), [Progress](tasks/04-maui-adjustments/progress-details.md))
- 🔲 05-validation: Full build, test run, and final verification
- 🔲 06-cleanup-and-merge: Final cleanup and prepare for merge
