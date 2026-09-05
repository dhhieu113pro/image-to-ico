# Image to ICO

Convert a large PNG/JPEG/WebP/BMP/TIFF image into a multi-resolution Windows `.ico` file from .NET 10.

The tool is published as the NuGet .NET tool package `ImageToIco.Dnx`, so consumers can run it directly with `dnx` without a permanent global install.

## Requirements

- .NET 10 SDK

## Quick start

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico
```

By default the generated ICO contains these sizes:

```text
16,24,32,48,64,128,256
```

## Remove a solid or near-solid background

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico --remove-background
```

The background-removal mode is intended for logos and app artwork with a solid or nearly solid exterior background. It is not AI/semantic segmentation.

## Custom sizes

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico --sizes 16,32,48,64,128,256
```

## Tune background tolerance

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico \
  --remove-background \
  --fuzz 12
```

`--fuzz` accepts a percentage from `0` to `100`. The default is `8`.

## Explicit background color

```bash
dnx ImageToIco.Dnx --yes -- logo.jpg app.ico \
  --remove-background \
  --background-color "#ffffff"
```

## Overwrite an existing ICO

```bash
dnx ImageToIco.Dnx --yes -- logo.png app.ico --overwrite
```

## GitHub Actions

A repository can generate its Windows icon from a large source logo during CI with only the .NET 10 SDK:

```yaml
- uses: actions/setup-dotnet@v5
  with:
    dotnet-version: 10.0.x

- name: Generate Windows icon
  run: >-
    dnx ImageToIco.Dnx --yes --
    assets/logo.png
    assets/app.ico
    --remove-background
    --overwrite
```

The generated `assets/app.ico` can then be consumed by your .NET, Avalonia, WinUI, Electron, installer, or packaging workflow.

## CLI

```text
image-to-ico <input> <output> [options]

Options:
  --remove-background          Remove the connected exterior background
  --sizes <csv>                ICO sizes; default: 16,24,32,48,64,128,256
  --fuzz <percent>             Background tolerance; default: 8
  --background-color <color>   Explicit background color such as #ffffff
  --overwrite                  Replace an existing output file
```

## Supported inputs

The tool uses Magick.NET and is designed for common raster formats such as PNG, JPEG, WebP, GIF, BMP, and TIFF. Existing PNG alpha transparency is preserved.

## Development

Run the complete test suite with the same hard coverage gate used by CI:

```bash
python scripts/test-all.py
```

The gate requires **100% line, branch, and method coverage** for production code.

## Packing locally

```bash
dotnet pack src/ImageToIco/ImageToIco.csproj \
  --configuration Release \
  --output artifacts/packages \
  -p:Version=0.0.0-local
```

After the package is configured as `ImageToIco.Dnx`, it can be exercised from the local NuGet source using:

```bash
dnx ImageToIco.Dnx@0.0.0-local \
  --source artifacts/packages \
  --yes -- \
  logo.png app.ico
```

## Publishing

Releases are published from GitHub Actions rather than manually from a developer machine. A tag such as:

```bash
git tag v1.0.0
git push origin v1.0.0
```

maps to NuGet package version `1.0.0`. The release workflow must first run tests, enforce the 100% coverage gate, pack the tool, and smoke-test the exact local `.nupkg` through `dnx`; only that verified package is eligible for NuGet.org publishing through trusted publishing/OIDC.

## License

MIT
