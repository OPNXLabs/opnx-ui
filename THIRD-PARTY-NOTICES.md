# Third-Party Notices

This document describes third-party software used by `OPNX.UI.WPF` that is subject to separate license terms.

The OPNX source-available license applies only to OPNX-owned code. Third-party components remain subject to their own licenses and notices.

## OPNX.Lib

- Component: `OPNX.Lib`
- Version: `0.1.0-preview.20260410.2`
- Project: [https://github.com/OPNXLabs](https://github.com/OPNXLabs)
- License: OPNX source-available license for learning, evaluation, research, and non-commercial use
- Usage: common platform, networking, media, data, and streaming infrastructure used by `OPNX.UI.WPF`

## FFmpeg.AutoGen

- Component: `FFmpeg.AutoGen`
- Version: `8.1.0`
- Project: [https://www.nuget.org/packages/FFmpeg.AutoGen](https://www.nuget.org/packages/FFmpeg.AutoGen)
- Upstream source: [https://github.com/Ruslan-B/FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen)
- License: `MIT`
- Usage: .NET bindings used by rendering and media-oriented WPF controls

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
- Usage: Direct3D9 interop and rendering support used by `OpnxImage` and related WPF rendering infrastructure

The full MIT license text is provided in:

- [third_party_licenses/MIT.txt](third_party_licenses/MIT.txt)

## FFmpeg Native Libraries

- Component: `FFmpeg` native libraries
- Typical libraries: `avcodec`, `avformat`, `avutil`, `swresample`, `swscale`, `avfilter`, `avdevice`
- Project: [https://ffmpeg.org/](https://ffmpeg.org/)
- License: depends on the selected build and enabled components

Important notes:

- `OPNX.UI.WPF` references `FFmpeg.AutoGen`, but native FFmpeg binaries are separate from `FFmpeg.AutoGen`.
- OPNX does not grant any rights to FFmpeg native binaries under the OPNX source-available license.
- OPNX recommends that users obtain and configure FFmpeg native binaries separately.
- If you bundle, redistribute, or otherwise provide FFmpeg native binaries with your product or service, you are responsible for complying with the applicable FFmpeg license terms for the specific build you use.
