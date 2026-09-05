# Image to ICO Design

## Goal

Build a .NET 10 command-line tool that converts a large source image into a multi-resolution Windows `.ico` file, with optional automatic transparent-background removal using Magick.NET. Package the tool as a NuGet .NET tool so it can be executed directly with `dnx`, and provide GitHub Actions workflows that validate, publish, and dogfood the tool.

## Scope

The first version supports one input image and one output `.ico` file. It accepts common image formats supported by Magick.NET, including PNG, JPEG, WebP, GIF, BMP, and TIFF. It emits a single ICO containing multiple square renditions.

Default icon sizes are: `16,24,32,48,64,128,256`.

The first release does not include AI/semantic background removal. Background removal is intended for solid or near-solid backgrounds around logos and app artwork.

## CLI

Package ID: `ImageToIco.Dnx`

Tool command: `image-to-ico`

Primary usage:

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico
```

Background removal:

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico --remove-background
```

Custom sizes and fuzz tolerance:

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico \
  --remove-background \
  --sizes 16,24,32,48,64,128,256 \
  --fuzz 8
```

Arguments and options:

- positional `input`: required source image path
- positional `output`: required destination `.ico` path
- `--remove-background`: enable automatic background transparency
- `--sizes <csv>`: comma-separated square icon sizes; defaults to `16,24,32,48,64,128,256`
- `--fuzz <percent>`: background color tolerance percentage; defaults to `8`
- `--background-color <color>`: optional explicit color override, for example `#ffffff`; when omitted, background color is inferred from image corners
- `--overwrite`: allow replacing an existing output file

Invalid sizes, unsupported files, missing files, invalid fuzz values, and output collisions return a non-zero exit code with a concise error message on stderr.

## Image Processing Pipeline

1. Load the input image with Magick.NET.
2. Auto-orient the image from EXIF metadata.
3. Preserve existing alpha transparency.
4. If `--remove-background` is enabled:
   - if `--background-color` is supplied, use it;
   - otherwise sample the four image corners and choose the dominant/closest corner color as the background estimate;
   - set Magick.NET fuzz tolerance from `--fuzz`;
   - remove only background regions connected to the exterior/corners, so similarly colored pixels enclosed inside the logo are not globally erased;
   - retain anti-aliased edge alpha.
5. Resize the normalized source into each requested square size using high-quality filtering while preserving aspect ratio.
6. Center each rendition on a transparent square canvas so non-square artwork is not distorted.
7. Encode the renditions into one ICO file in ascending size order.

The 256×256 rendition should be PNG-compressed inside the ICO. Smaller renditions may also be PNG-backed if Magick.NET's ICO encoder produces standards-compatible results; tests must verify Windows-compatible multi-image output.

## Architecture

The repository will remain small and separated by responsibility:

- `src/ImageToIco/Program.cs`: CLI entry point and exit-code handling
- `src/ImageToIco/CliOptions.cs`: parsed/validated command-line options
- `src/ImageToIco/ImageConverter.cs`: orchestration of image normalization, background removal, resize, and ICO writing
- `src/ImageToIco/BackgroundRemover.cs`: background detection and connected-edge transparency logic
- `src/ImageToIco/IconSizeParser.cs`: size-list validation and normalization
- `tests/ImageToIco.Tests/`: unit/integration tests for parsing and generated ICO structure

Use built-in/simple argument parsing unless a small maintained CLI package materially reduces code. Avoid adding a large command framework for this utility.

## Dependencies

- .NET 10 (`net10.0`)
- `Magick.NET-Q8-AnyCPU`
- xUnit test project

No Python, native ImageMagick installation, or AI model is required.

## NuGet and dnx Packaging

Follow the established `roslyn-mcp` packaging pattern where applicable:

- `<PackAsTool>true</PackAsTool>`
- `<ToolCommandName>image-to-ico</ToolCommandName>`
- package ID `ImageToIco.Dnx`
- MIT license metadata
- repository/readme metadata
- symbol package generation
- package on release tags only

Unlike `RoslynMcp.Dnx`, this package is a normal `DotnetTool` only and does not include the MCP server manifest or `McpServer` package type.

CI must pack the tool and run the resulting local package through `dnx` before publication.

## CI / Release Workflow

`.github/workflows/ci.yml` will:

1. run tests on Ubuntu, Windows, and macOS with .NET 10;
2. pack `ImageToIco.Dnx`;
3. create a test source image during CI;
4. execute the packed package using `dnx ... --source <local-package-dir> --yes -- ...`;
5. verify the produced ICO exists and contains the expected requested sizes;
6. upload the NuGet package artifact;
7. on `v*` tags, publish the already-verified package to NuGet.org using the same trusted-publishing/OIDC approach as `roslyn-mcp`.

Tag `v1.0.0` maps to package version `1.0.0`; non-tag CI runs use an internal CI version such as `0.0.0-ci.<run-number>` and are not pushed to NuGet.org.

## Dogfood GitHub Action

A separate `.github/workflows/generate-icon.yml` demonstrates consumer usage. It is intentionally distinct from package/release CI.

The workflow:

1. checks out the repository;
2. installs .NET 10;
3. invokes the published tool through `dnx ImageToIco.Dnx --yes -- <large-logo> <output.ico> --remove-background`;
4. uploads the generated ICO as a workflow artifact.

When the repository contains a canonical large logo, the workflow uses that file. Until then, the implementation can include a small deterministic sample/test logo specifically for CI/dogfooding rather than depending on an external download.

This workflow proves that another repository can generate a Windows icon with only .NET 10 plus one `dnx` command.

## Testing

Tests cover:

- default size parsing
- custom size parsing and deduplication
- rejection of zero, negative, malformed, and sizes greater than 256
- fuzz validation (`0..100`)
- output collision behavior with and without `--overwrite`
- conversion from at least PNG and JPEG
- existing alpha preservation
- background removal of a near-solid connected exterior background
- preservation of similarly colored pixels enclosed inside the subject
- non-square input is centered without distortion
- generated ICO contains each requested resolution
- `dnx` smoke test uses the packed local `.nupkg`, not project output

Tests should avoid pixel-perfect comparisons where encoder differences can make them brittle; verify dimensions, alpha behavior, and representative pixels instead.

## Documentation

README will include:

- what the tool does
- installation-free `dnx` usage
- basic conversion example
- transparent-background example
- custom sizes and fuzz example
- GitHub Actions snippet for generating an app icon from a large logo
- supported input formats
- note that background removal is color/flood-fill based, not AI segmentation
- NuGet package badge/link once published

## Non-Goals for v1

- AI/semantic background removal
- GUI application
- batch directory conversion
- macOS `.icns` output
- SVG rasterization guarantees beyond what the selected Magick.NET build supports
- arbitrary per-size source artwork

## Success Criteria

The feature is complete when a clean machine with .NET 10 can run:

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico --remove-background
```

and obtain a Windows-compatible `.ico` containing the default seven sizes, while CI independently verifies the packed NuGet tool through `dnx` before any tagged release is published.