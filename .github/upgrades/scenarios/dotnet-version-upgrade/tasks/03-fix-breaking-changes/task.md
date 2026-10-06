# 03-fix-breaking-changes: Address API and build issues introduced by the framework upgrade

Resolve compiler errors and API breaking changes identified by the assessment (e.g., removed APIs, changed overloads). Focus on minimal refactors that restore functionality while preserving behavior. Document any changes that may affect public APIs.

**Done when**: The solution builds without errors after TFMs and package updates; unit tests (if present) run without unexpected failures.

## Research

- Assessment flagged 2 source-incompatible API usages: TimeSpan.FromSeconds in:
  - Group1AndroidProject/Views/MainPage.xaml.cs (GeolocationRequest timeout: TimeSpan.FromSeconds(2))
  - Group1AndroidProject/Views/EntryPage.xaml.cs (GeolocationRequest timeout: TimeSpan.FromSeconds(10))

- Current state: after TFMs and package updates the solution restores and builds successfully. No compiler errors observed related to these calls.

## Decision

- No code changes required at this time since the project builds and the TimeSpan usages are still available and behave as expected under net10.0 on this platform.
- Plan: add targeted runtime validation during task 05 (validation) to exercise geolocation calls and confirm behavior. If runtime incompatibility appears, implement a concrete replacement (e.g., use TimeSpan.FromSeconds alternative or construct via `new TimeSpan(0,0,seconds)`) and document API impact.

## Actions to perform now

1. Run a full solution build (already done) and record results in progress-details.md.
2. Add this research and decision to task.md (done).
3. Proceed to mark the task complete; any runtime issues will be handled in validation or a follow-up task.
