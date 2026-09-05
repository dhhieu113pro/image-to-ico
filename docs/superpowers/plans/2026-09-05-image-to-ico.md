# Image to ICO Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build and publish a .NET 10 `ImageToIco.Dnx` tool that converts common raster images to multi-resolution Windows ICO files, with optional Magick.NET-based background removal and GitHub Actions coverage for CI, NuGet publishing, and dogfood icon generation.

**Architecture:** Keep the CLI thin and place parsing, background removal, conversion, and ICO validation in focused classes. Magick.NET handles decode/orientation/resizing/transparency while the converter emits the final ICO; tests validate behavior and package-level execution through `dnx`.

**Tech Stack:** .NET 10, C#, Magick.NET-Q8-AnyCPU, xUnit, GitHub Actions, NuGet trusted publishing, `dnx`.

**Spec:** `docs/superpowers/specs/2026-09-05-image-to-ico-design.md`

## Global Constraints

- Target `net10.0`.
- Package ID is `ImageToIco.Dnx`.
- Tool command is `image-to-ico`.
- Default sizes are `16,24,32,48,64,128,256`.
- Background removal is color/flood-fill based, not AI segmentation.
- No Python, external ImageMagick installation, or AI model is required.
- Release tags `v*` publish the already-verified package to NuGet.org.
- Consumer workflow must invoke the published tool through `dnx`.

---

### Task 1: Project skeleton and option validation

**Files:**
- Create: `ImageToIco.slnx`
- Create: `global.json`
- Create: `src/ImageToIco/ImageToIco.csproj`
- Create: `src/ImageToIco/Program.cs`
- Create: `src/ImageToIco/CliOptions.cs`
- Create: `src/ImageToIco/IconSizeParser.cs`
- Create: `tests/ImageToIco.Tests/ImageToIco.Tests.csproj`
- Create: `tests/ImageToIco.Tests/IconSizeParserTests.cs`
- Create: `tests/ImageToIco.Tests/CliOptionsTests.cs`

**Interfaces:**
- Produces: `IconSizeParser.Parse(string?) -> IReadOnlyList<int>`
- Produces: `CliOptions.Parse(string[]) -> CliParseResult`

- [ ] **Step 1: Write failing parser tests** for default sizes, custom parsing/deduplication, malformed/zero/negative/>256 values, fuzz range, required positional paths, and overwrite/background flags.
- [ ] **Step 2: Run `dotnet test tests/ImageToIco.Tests/ImageToIco.Tests.csproj` and confirm failure** because parser types do not exist.
- [ ] **Step 3: Implement minimal `IconSizeParser` and `CliOptions`** with deterministic validation and concise errors.
- [ ] **Step 4: Run the parser tests and confirm they pass.**
- [ ] **Step 5: Commit the project skeleton and option validation.**

### Task 2: Background removal

**Files:**
- Create: `src/ImageToIco/BackgroundRemover.cs`
- Create: `tests/ImageToIco.Tests/BackgroundRemoverTests.cs`

**Interfaces:**
- Consumes: parsed `--fuzz` and optional `--background-color` values.
- Produces: `BackgroundRemover.Apply(MagickImage image, double fuzzPercent, string? backgroundColor)`.

- [ ] **Step 1: Write failing image tests** proving connected exterior near-solid pixels become transparent while enclosed similarly colored subject pixels remain opaque, and existing alpha remains transparent.
- [ ] **Step 2: Run only `BackgroundRemoverTests` and confirm red.**
- [ ] **Step 3: Implement corner-based background estimation and edge-connected flood transparency using Magick.NET fuzz.**
- [ ] **Step 4: Run `BackgroundRemoverTests` and confirm green.**
- [ ] **Step 5: Commit background removal.**

### Task 3: Multi-resolution ICO conversion

**Files:**
- Create: `src/ImageToIco/ImageConverter.cs`
- Create: `tests/ImageToIco.Tests/ImageConverterTests.cs`

**Interfaces:**
- Consumes: `CliOptions`-validated paths/options and `BackgroundRemover`.
- Produces: `ImageConverter.Convert(string inputPath, string outputPath, IReadOnlyList<int> sizes, bool removeBackground, double fuzzPercent, string? backgroundColor, bool overwrite)`.

- [ ] **Step 1: Write failing integration tests** for PNG/JPEG conversion, non-square aspect-preserving centering, overwrite behavior, alpha preservation, and ICO frames for each requested size.
- [ ] **Step 2: Run `ImageConverterTests` and confirm red.**
- [ ] **Step 3: Implement load/auto-orient/optional-background-removal/resize/transparent-canvas/ICO-write pipeline.** Use one frame per requested size in ascending order and ensure 256x256 is represented correctly.
- [ ] **Step 4: Run `ImageConverterTests` and confirm green.**
- [ ] **Step 5: Commit conversion support.**

### Task 4: CLI executable behavior

**Files:**
- Modify: `src/ImageToIco/Program.cs`
- Create: `tests/ImageToIco.Tests/ProgramTests.cs`

**Interfaces:**
- Consumes: `CliOptions.Parse` and `ImageConverter.Convert`.
- Produces: CLI exit code `0` on success and non-zero with concise stderr on validation/conversion failure.

- [ ] **Step 1: Write failing CLI-level tests** covering success and representative validation errors.
- [ ] **Step 2: Run `ProgramTests` and confirm red.**
- [ ] **Step 3: Wire program entry point to parser/converter and map failures to stderr/exit codes.**
- [ ] **Step 4: Run all unit/integration tests and confirm green.**
- [ ] **Step 5: Commit CLI behavior.**

### Task 5: NuGet `dnx` packaging and package smoke test

**Files:**
- Modify: `src/ImageToIco/ImageToIco.csproj`
- Create: `README.md`
- Create: `LICENSE`
- Create: `scripts/test-package.py`
- Create: `tests/assets/sample-logo.png` or generate it deterministically in the package test.

**Interfaces:**
- Produces: NuGet package `ImageToIco.Dnx.<version>.nupkg` exposing command `image-to-ico`.
- Package smoke test executes the local `.nupkg` with `dnx ImageToIco.Dnx@<version> --source <dir> --yes -- ...`.

- [ ] **Step 1: Add failing package validation expectations** for package ID, MIT metadata, DotnetTool package type, command name, symbols package, and local `dnx` execution generating a valid multi-size ICO.
- [ ] **Step 2: Pack a CI version and run `scripts/test-package.py`; confirm it fails before tool metadata is complete.**
- [ ] **Step 3: Add `PackAsTool`, `ToolCommandName=image-to-ico`, package metadata, README/LICENSE packing, symbol package generation, and `RollForward=Major`.**
- [ ] **Step 4: Re-pack and run the package smoke test through `dnx`; confirm success.**
- [ ] **Step 5: Commit packaging and documentation.**

### Task 6: CI, trusted NuGet publishing, and dogfood workflow

**Files:**
- Create: `.github/workflows/ci.yml`
- Create: `.github/workflows/generate-icon.yml`
- Modify: `README.md`

**Interfaces:**
- CI verifies tests on Ubuntu/Windows/macOS, packs once, smoke-tests through local `dnx`, uploads package artifacts, and publishes tagged versions using NuGet trusted publishing.
- Dogfood workflow invokes `dnx ImageToIco.Dnx --yes -- ... --remove-background` and uploads generated ICO.

- [ ] **Step 1: Add CI workflow modeled on `roslyn-mcp`** with matrix verification, semantic version derivation, package smoke test, artifact upload, tag/main ancestry check, and OIDC NuGet publishing.
- [ ] **Step 2: Add separate dogfood workflow** with `workflow_dispatch`, .NET 10 setup, deterministic logo source, published `dnx` invocation, and generated ICO artifact upload.
- [ ] **Step 3: Add README GitHub Actions example matching the dogfood command.**
- [ ] **Step 4: Validate YAML structure and inspect workflow diff for exact package/artifact names.**
- [ ] **Step 5: Commit CI workflows.**

### Task 7: Final verification and PR

**Files:**
- Review all changed files.

**Interfaces:**
- Produces: reviewed feature branch and PR to `main`.

- [ ] **Step 1: Run full `dotnet test ImageToIco.slnx --configuration Release`.**
- [ ] **Step 2: Run `dotnet pack src/ImageToIco/ImageToIco.csproj --configuration Release --output artifacts/packages -p:Version=0.0.0-ci.1`.**
- [ ] **Step 3: Run `python scripts/test-package.py artifacts/packages 0.0.0-ci.1`.**
- [ ] **Step 4: Re-read the approved spec and verify every success criterion against implementation/tests/workflows.**
- [ ] **Step 5: Open a PR summarizing CLI behavior, Magick.NET background removal, `dnx` packaging, CI, and dogfood workflow.**
