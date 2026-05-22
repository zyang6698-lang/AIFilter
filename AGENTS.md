# AGENTS.md — DeepSightAI

## Build & Verify

```bash
# Build the entire solution
dotnet build DeepSightAI.sln

# Verify build: check for compiler errors only (no warnings)
dotnet build DeepSightAI.sln --no-restore 2>&1 | Select-String "error CS"
```

- This is a **.NET Framework 4.7.2** solution. `dotnet run` will not work for the EXE project — it's a WinForms app, not .NET Core.
- The .vscode `launch.json` is a **non-functional placeholder** (C# extension could not decode the .NET Framework projects). Use Visual Studio for debugging.
- `DeepSightDB` and `DeepSightTool` are SDK-style projects that **multi-target `net472;net8.0`**. All other projects are .NET Framework 4.7.2 only.
- NuGet package management is mixed: some projects use `packages.config` (DeepSightAI, DeepSightCommunication, DeepSightModel, DeepSightWorkLib), others use SDK-style `<PackageReference>` (DeepSightDB, DeepSightTool, DeepSightWorkLib.Tests).

## Test

```bash
dotnet test DeepSightWorkLib.Tests/DeepSightWorkLib.Tests.csproj
```

- Test framework: **MSTest 4.0.2 + Moq 4.20.72**
- Tests target `net472` only
- Method-level parallelization is enabled (`MSTestSettings.cs`)
- **Do not add test projects for other assemblies** unless explicitly asked

## Solution Layout

| Project | Type | Role |
|---|---|---|
| `DeepSightAI` | WinExe | Main WinForms app (forms, controls, dialogs, entry point) |
| `DeepSightWorkLib` | Library | Core business logic (`BusinessClass` facade, TPL Dataflow pipeline, services) |
| `DeepSightModel` | Library | Data models, config classes, DTOs |
| `DeepSightDB` | Library | PostgreSQL access via Npgsql, CSV helpers |
| `DeepSightTool` | Library | Foundation: Serilog logging (`LogTextHelper`), math utilities |
| `DeepSightCommunication` | Library | MinIO client, LevelDB HTTP client |
| `DeepSightDisplay` | Library | OpenCV image display controls, heat map rendering |
| `DeepSightEvent` | Library | Alarm service, system events/delegates |
| `DeepSightWorkLib.Tests` | Library | Unit tests for core business logic |

- Output: `Bin/` at solution root (configured in each `.csproj`)
- The `DeepSightHeatMap/` directory is **stale** — not in the `.sln`, no `.csproj`. Its functionality migrated into `DeepSightDisplay/HeatMap/`.

## Key Architecture

- **BusinessClass** (`DeepSightWorkLib/BusinessClass.cs`) is the central facade. It initializes and coordinates all services (database, queues, workers, AVI reading, image loading, defect processing, result writing, post-processing, validation testing). The `Machine.cs` singleton holds a reference: `Machine.master`.
- **Service classes** follow `XxxService` naming and live in each project's `Services/` folder. 16 services in `DeepSightWorkLib/Services/`.
- **TPL Dataflow pipeline** in `DeepSightWorkLib/Services/Pipeline/` processes panels through stages: JSON parse → Image load → Inference → Result write → Post-process. Pipeline stages are individually composable.
- **Logging**: Always use `LogTextHelper` (`DeepSightTool/LogTextHelper.cs`). It wraps Serilog with three loggers (Info/Warn/Error), async file sinks with hourly rolling, 100MB limits, and UI callback with cooldown. Logs to `Log/` directory.
- **OpenCvSharp**: Runtime native DLLs come from NuGet (`OpenCvSharp4.runtime.win`). Managed assemblies (`OpenCvSharp.dll`, `OpenCvSharp.Extensions.dll`, etc.) are referenced from `..\Bin\` — **not from NuGet**. Do not add OpenCvSharp NuGet package references to projects that reference these Bin assemblies.
- **Database**: PostgreSQL via Npgsql 4.1.14. MinIO 6.0.4 for object storage.
- **UI**: Windows Forms + SunnyUI 3.9.6.

## Version Management

- `Directory.Build.targets` auto-syncs version from Git tags during **Release builds only** (not Debug).
- Version bump workflow:
  ```powershell
  .\scripts\UpdateVersion.ps1 -Bump Patch  # v1.1.1 → v1.1.2 + git tag
  .\scripts\UpdateVersion.ps1 -Bump Minor
  .\scripts\UpdateVersion.ps1 -Bump Major
  ```
- The script updates all `AssemblyInfo.cs` files and `installer/DeepSightAI.iss`. Push the tag with `git push origin vX.Y.Z`.

## Configuration & Environment

- **Shell PATH 受限**：当前环境 `PATH` 仅包含 `/home/zhangyang/.local/bin`，缺少 `/usr/bin` 和 `/bin`。使用 `git`、`dotnet` 等命令时需加完整路径（如 `/usr/bin/git`、`/usr/bin/dotnet`），或在命令前先执行：
  ```bash
  export PATH="/usr/bin:/bin:$PATH"
  ```
- Solution configurations: `Debug`, `DebugRemote`, `Release` (each with x64/x86/Any CPU).
- Installer is **Inno Setup** (`installer/DeepSightAI.iss`). Bundles PostgreSQL 18.1, CUDA 11.8, TensorRT 8.5.3.1, Qt, Python 2.7. Full installer vs. update installer (`DeepSightAI_Update.iss`).
- No CI/CD pipeline is configured in the repo. `UpdateVersion.ps1` has GITHUB_OUTPUT support if one is added later.
- The `.vscode/settings.json` defaults **all files to read-only**. If editing via VS Code, remove or adjust `files.readonlyInclude`.

## Conventions

- Responses and Git commit messages: **简体中文 (Simplified Chinese)**
- Do NOT add comments to code unless explicitly asked
- Service classes go in `Services/` folders, named `XxxService`
