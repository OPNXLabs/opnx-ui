# OPNX.UI

[Korean](README.ko.md)

> **License notice:** OPNX.UI is source-available software, not open-source software. Commercial use and redistribution require prior written permission from OPNX. See [LICENSE.txt](LICENSE.txt).

OPNX.UI is the UI solution family for OPNX client applications.

The goal of OPNX.UI is to provide reusable .NET-based UI foundations for building video client applications across operating systems and client environments, including monitoring clients, playback/review tools, and video-platform desktop applications.

The current implementation in this repository is `OPNX.UI.WPF`, a WPF control library for Windows-based OPNX video clients.

## Why OPNX.UI Exists

OPNX client applications need more than ordinary desktop UI controls.

Real VMS, NVR, monitoring, playback, and review clients repeatedly need the same difficult UI foundations:

- dense multi-channel video layouts
- rendering-oriented image and media display controls
- playback timelines and recorded media review workflows
- hierarchical device, channel, user, resource, and configuration views
- navigation surfaces for operational applications
- platform-specific window and shell integration
- reusable input controls that fit video-operation screens
- drag-and-drop interaction infrastructure
- integration with OPNX.Lib-based networking, media, data, and streaming infrastructure

OPNX.UI exists so these UI foundations do not have to be rebuilt separately for every OPNX-based client application or operating system target.

The repository currently focuses on WPF for Windows desktop clients through `OPNX.UI.WPF`. Future UI modules may target other .NET UI stacks, AOT-oriented clients, or platform-specific client applications as the OPNX ecosystem grows.

## Current Implementation

### OPNX.UI.WPF

`OPNX.UI.WPF` is a WPF UI library for building stateful video applications, including VMS, NVR, live monitoring clients, playback/review tools, and Windows desktop video-platform applications.

It provides reusable WPF controls and UI infrastructure for video display, multi-tile monitoring layouts, navigation, playback timelines, hierarchical data views, custom window chrome, and application composition on top of OPNX.Lib.

## What OPNX.UI.WPF Provides

`OPNX.UI.WPF` is organized around the following areas.

- Video display and layout  
  Controls for multi-tile video layouts and rendering-oriented image/media display scenarios.

- Playback and review workflows  
  Timeline controls and UI building blocks for recorded media navigation and review.

- Operational navigation and data views  
  Navigation controls and hierarchical list/tree components for dense monitoring and management screens.

- WPF application composition  
  Custom title bar, input primitives, shared control bases, drag-and-drop infrastructure, and UI utility helpers.

- OPNX platform integration  
  UI components intended to work with OPNX.Lib-based application, media, data, and streaming layers.

## Main Components

- `OPNX.UI.WPF.Controls.OpnxMultiView`  
  Multi-tile layout control for VMS/NVR video display scenarios.

- `OPNX.UI.WPF.Controls.OpnxImage`  
  Rendering-oriented image control intended for video and media display workflows.

- `OPNX.UI.WPF.Controls.OpnxPlaybackTimeline`  
  Timeline control for playback, recorded media browsing, and review workflows.

- `OPNX.UI.WPF.Controls.OpnxTreeListView`  
  Tree list control for hierarchical device, user, resource, or configuration views.

- `OPNX.UI.WPF.Controls.OpnxNavigator`  
  Navigation selector control with horizontal or vertical item placement support.

- `OPNX.UI.WPF.Controls.OpnxTitlebar`  
  Custom title bar control for WPF shell windows and common window actions.

- `OPNX.UI.WPF.Controls.Primitives`  
  Shared base controls and reusable building blocks for custom WPF controls.

## Design Direction

OPNX.UI is designed as a UI foundation for OPNX client applications across platforms. `OPNX.UI.WPF` is the current Windows/WPF implementation of that direction.

- The solution name `OPNX.UI` represents the broader UI family.
- Platform-specific UI modules can be added under the OPNX.UI family over time.
- Controls are designed for dense operational UIs such as VMS/NVR clients.
- Components are expected to work together with OPNX.Lib and OPNX.V-style applications.
- UI behavior should remain reusable across products instead of being tied to one application screen.
- Public APIs and examples will be refined as the sample applications mature.
- Logging and diagnostics should remain abstraction-based through the underlying application stack.
- Native media and rendering dependencies are kept separate from OPNX-owned code in both licensing and distribution responsibility.

## Potential Future Directions

The repository currently provides `OPNX.UI.WPF`. Future OPNX.UI modules may include additional .NET-based client UI layers, such as:

- AOT-oriented client UI/runtime support
- cross-platform .NET UI controls
- macOS-focused client UI layers
- other operating-system-specific client UI foundations

These future directions are not part of the current public API surface unless they are added as explicit projects.

## Use Cases

- Video Management Systems, VMS
- Network Video Recorders, NVR
- Multi-channel monitoring clients
- Playback and recorded media review tools
- Windows WPF video clients through `OPNX.UI.WPF`
- Future .NET-based video clients for other operating systems
- Desktop platform UI development on top of OPNX.Lib

## Current Status

OPNX.UI is under active development.

The current implementation is `OPNX.UI.WPF`. Its control set is still evolving. Runnable examples are maintained in the separate [OPNX Samples repository](https://github.com/OPNXLabs/opnx-samples), while API and integration documentation will continue to evolve as the project matures.

The current repository should be treated as a preview-quality UI library for evaluation, integration testing, research, non-commercial experimentation, and early feedback rather than as a production-ready UI SDK.

## NuGet Package

`OPNX.UI.WPF` is published as a preview NuGet package.

Install:

```powershell
dotnet add package OPNX.UI.WPF --prerelease
```

This package is intended for preview evaluation and integration testing. API compatibility, package structure, and documentation may change before a stable release.

## Build

Requirements:

- .NET 10 SDK
- Windows development environment with WPF support

Build with the published OPNX.Lib package:

```powershell
dotnet build OPNX.UI.slnx -c Debug
```

Create the NuGet package explicitly from the package-backed configuration:

```powershell
dotnet pack .\src\OPNX.UI.WPF\OPNX.UI.WPF.csproj -c Release -p:Platform=x64
```

## Samples And Documentation

Runnable sample applications are available in [OPNXLabs/opnx-samples](https://github.com/OPNXLabs/opnx-samples):

- `OPNX.Samples.PlaybackTimeline` — playback timeline layout, styling, selection, and record/event visualization
- `OPNX.Samples.RtspMultiLiveViewer` — multi-view layout, video presentation, navigation, tree/list controls, and title bar integration
- `OPNX.Samples.EntityStore` — OPNX.Lib entity store integration used by stateful UI applications
- `OPNX.Samples.TcpChat` — OPNX.Lib networking integration used by client applications

The samples are preview-quality examples and follow the package versions documented by the samples repository. Planned documentation topics include:

- basic control usage
- layout composition
- video display and rendering integration
- playback timeline integration
- tree/list data binding
- window title bar integration
- OPNX.Lib integration

## Dependency

`OPNX.UI.WPF` is designed to be used together with `OPNX.Lib`.

- `OPNX.UI.slnx` and direct project builds use the configured OPNX.Lib NuGet package.
- `dotnet pack` uses the configured OPNX.Lib package version so the resulting package remains independently restorable.

## License

OPNX.UI is source-available, but it is not licensed as permissive open-source software.

OPNX-owned code in this repository may be used for learning, evaluation, research, testing, and other non-commercial purposes.

Commercial use, redistribution, OEM integration, or inclusion in commercial products or services requires prior written permission from OPNX.

See [LICENSE.txt](LICENSE.txt) for full terms. A Korean reference translation is available at [LICENSE.ko.txt](LICENSE.ko.txt).

## Third-Party Components

This repository uses third-party software components under their respective licenses.

Important notes:

- `OPNX.Lib` is used under the OPNX source-available license.
- `FFmpeg.AutoGen`, `SharpDX`, and `SharpDX.Direct3D9` are used under the MIT License.
- Native `FFmpeg` binaries are not covered by the OPNX license.
- OPNX recommends that users obtain and configure native FFmpeg binaries separately.
- Any party that bundles or redistributes native FFmpeg binaries is responsible for complying with the license terms that apply to the selected FFmpeg build.
- OPNX.Lib and its own third-party dependencies remain subject to their respective license terms and notices.

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for details.

## Related Projects

- [`OPNX Samples`](https://github.com/OPNXLabs/opnx-samples)
  Runnable examples for OPNX.Lib and OPNX.UI.

- `OPNX.Lib`  
  Core SDK for networking, media, streaming, and data infrastructure.

- `OPNX.V`  
  Video platform applications built on top of OPNX.Lib and OPNX.UI.

## Commercial And OEM Inquiries

OPNX.UI is developed and distributed by 오픈엑스 (OPNX), a business registered in the Republic of Korea.

For commercial licensing, OEM agreements, or partnership inquiries, contact:

- [https://www.opnx.kr/](https://www.opnx.kr/)
- `opnx@opnx.kr`

## Security And Contributions

- Report security issues privately as described in [SECURITY.md](https://github.com/OPNXLabs/opnx-ui/blob/master/SECURITY.md).
- Review [CONTRIBUTING.md](https://github.com/OPNXLabs/opnx-ui/blob/master/CONTRIBUTING.md) before opening an issue or proposing a change.
