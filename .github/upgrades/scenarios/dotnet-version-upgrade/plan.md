# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade Group1AndroidProject solution to net10.0
**Scope**: Single MAUI application project (Group1AndroidProject) and its referenced libraries; small solution

## Tasks

### 01-update-project-tfms: Update project target frameworks

Update the project files to target net10.0. For multi-targeted libraries, prefer adding net10.0 where appropriate. Ensure SDK-style csproj format is preserved. This task includes updating the TargetFramework/TargetFrameworks properties across affected projects and verifying project files for deprecated elements that must be removed or replaced.

**Done when**: All project files list net10.0 as their target framework (or include net10.0 in TargetFrameworks), and the solution loads in IDE without project file parse errors.

---

### 02-update-packages: Update NuGet packages to compatible stable versions

Upgrade NuGet package references to the latest stable versions compatible with net10.0. Prioritize security fixes and packages flagged in the assessment. Record any packages that have no compatible version and mark them as blockers for follow-up.

**Done when**: All packages that have compatible updates are updated and package restore succeeds for the solution.

---

### 03-fix-breaking-changes: Address API and build issues introduced by the framework upgrade

Resolve compiler errors and API breaking changes identified by the assessment (e.g., removed APIs, changed overloads). Focus on minimal refactors that restore functionality while preserving behavior. Document any changes that may affect public APIs.

**Done when**: The solution builds without errors after TFMs and package updates; unit tests (if present) run without unexpected failures.

---

### 04-maui-adjustments: Apply .NET MAUI-specific compatibility fixes

Address MAUI-specific migration needs such as updated handlers, lifecycle changes, or platform-specific manifest updates. Verify that Android/iOS launch configurations and platform assets are compatible with net10.0 and the newer SDK.

**Done when**: The MAUI app starts and the platform projects build successfully.

---

### 05-validation: Full build, test run, and final verification

Run a full solution build and any available unit/integration tests. Perform smoke test of the MAUI app on a device/emulator if feasible.

**Done when**: Full solution build succeeds, tests pass, and a basic smoke test verifies app startup.

---

### 06-cleanup-and-merge: Final cleanup and prepare for merge

Review changes, tidy project files, update scenario documentation, and create a PR-ready branch. Ensure commit strategy (After Each Task) was applied and that commits are well-scoped.

**Done when**: Code is committed on the working branch with clear commit messages and the branch is ready for PR.
