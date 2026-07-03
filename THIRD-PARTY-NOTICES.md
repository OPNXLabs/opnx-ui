# Third-Party Notices

This document describes third-party software used by `OPNX.UI.WPF` that is subject to separate license terms.

The OPNX source-available license applies only to OPNX-owned code in this repository. Third-party packages, native runtimes, notices, and attribution materials included in or used by this repository remain subject to their own license terms and attribution requirements.

## Scope

This notice covers the third-party components currently referenced or used by this repository, including:

- direct NuGet package references used by OPNX.UI.WPF
- OPNX.Lib, which is used as the OPNX platform infrastructure dependency
- selected native runtime components that OPNX.UI.WPF can interoperate with, but does not license or provide under the OPNX license

Users who redistribute, package, or deploy software based on OPNX.UI.WPF are responsible for reviewing the exact dependency graph, OPNX.Lib version, and native binaries they ship.

## OPNX Platform Dependency

## OPNX.Lib

- Component: `OPNX.Lib`
- Version: `0.1.0-preview.20260410.2`
- Project: [https://github.com/OPNXLabs](https://github.com/OPNXLabs)
- License: OPNX source-available license for learning, evaluation, research, testing, and non-commercial use
- Usage: common platform, networking, media, data, and streaming infrastructure used by `OPNX.UI.WPF`

Important notes:

- During local development, `OPNX.UI.WPF` may reference local OPNX.Lib projects.
- Package builds may reference the configured OPNX.Lib package version.
- OPNX.Lib has its own license, third-party notices, and transitive dependencies.
- Users who redistribute or deploy OPNX.UI.WPF together with OPNX.Lib are responsible for complying with the OPNX.Lib license and its third-party notices.

## Direct NuGet Package References

The following packages are directly referenced by `OPNX.UI.WPF`.

## FFmpeg.AutoGen

- Component: `FFmpeg.AutoGen`
- Version: `8.1.0`
- Project: [https://www.nuget.org/packages/FFmpeg.AutoGen](https://www.nuget.org/packages/FFmpeg.AutoGen)
- Upstream source: [https://github.com/Ruslan-B/FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen)
- License: `MIT`
- Used by: `OPNX.UI.WPF`
- Usage: .NET bindings used by rendering and media-oriented WPF controls to interoperate with FFmpeg native libraries

The full MIT license text is provided in:

- [third_party_licenses/MIT.txt](third_party_licenses/MIT.txt)

## SharpDX

- Components:
  - `SharpDX` (`4.2.0`)
  - `SharpDX.Direct3D9` (`4.2.0`)
- Project: [https://github.com/sharpdx/SharpDX](https://github.com/sharpdx/SharpDX)
- Package references:
  - [https://www.nuget.org/packages/SharpDX](https://www.nuget.org/packages/SharpDX)
  - [https://www.nuget.org/packages/SharpDX.Direct3D9](https://www.nuget.org/packages/SharpDX.Direct3D9)
- License: `MIT`
- Used by: `OPNX.UI.WPF`
- Usage: Direct3D9 interop and rendering support used by `OpnxImage` and related WPF rendering infrastructure

The full MIT license text is provided in:

- [third_party_licenses/MIT.txt](third_party_licenses/MIT.txt)

## Native Runtime Components

## FFmpeg Native Libraries

- Component: `FFmpeg` native libraries
- Typical libraries: `avcodec`, `avformat`, `avutil`, `swresample`, `swscale`, `avfilter`, `avdevice`
- Project: [https://ffmpeg.org/](https://ffmpeg.org/)
- License: depends on the selected build and enabled components

Important notes:

- `OPNX.UI.WPF` references `FFmpeg.AutoGen`, but native FFmpeg libraries are separate from `FFmpeg.AutoGen` and from OPNX-owned code.
- OPNX does not grant any rights to FFmpeg native binaries under the OPNX source-available license.
- OPNX recommends that users obtain and configure FFmpeg native binaries separately.
- If you bundle, redistribute, or otherwise provide FFmpeg native binaries with your product or service, you are responsible for complying with the applicable FFmpeg license terms for the specific build you use.
- FFmpeg is generally available under `LGPL-2.1-or-later`, but some builds or enabled components may cause `GPL` terms to apply.

## Additional OPNX.Lib Third-Party Notices

Because `OPNX.UI.WPF` is designed to work with `OPNX.Lib`, users should also review the third-party notices provided by the OPNX.Lib version they use.

Those notices may include, among others:

- ZstdSharp.Port
- Npgsql
- MySqlConnector
- OpenCvSharp4
- SkiaSharp
- SIPSorcery
- DataChannelDotnet
- SharpRTSP-derived source portions
- FFmpeg native library reference materials
