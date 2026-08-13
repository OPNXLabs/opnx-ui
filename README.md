# OPNX.UI

[한국어](README.ko.md)

> **License notice:** OPNX.UI is source-available software, not open-source software. Commercial use and redistribution require prior written permission from OPNX. See [LICENSE.txt](LICENSE.txt).

OPNX.UI is the reusable .NET UI foundation for OPNX video clients. The current implementation is `OPNX.UI.WPF`, a Windows WPF control library for VMS, NVR, monitoring, playback, and review applications.

## Video UI Components That Are Usually Product Code

OPNX.UI packages difficult, domain-specific controls that are commonly rebuilt inside each VMS or NVR product:

- **Direct3D video presentation for WPF** — `OpnxImage` integrates Direct3D 9 surfaces through `D3DImage` for hardware-oriented frame presentation, with a double-buffered `WriteableBitmap` path for environments such as remote sessions.
- **A real playback timeline** — `OpnxPlaybackTimeline` presents recording ranges and event data across multiple channels, supports time-range navigation and selection, and exposes styling and layout controls for review applications.
- **Operational multi-view, not a uniform grid** — `OpnxMultiView` supports configurable and irregular cell layouts, split/merge-style layout operations, cell selection, zoom/full-screen flows, synchronized layout state, thumbnails, and Drag-and-drop content placement.
- **Controls for dense operator workflows** — hierarchical TreeList, paging, navigation, custom window chrome, selection primitives, and shared Drag-and-drop infrastructure complete the screens around the video itself.

These controls are reusable library components rather than screenshots or application-specific prototypes. They are designed to be composed with OPNX.Lib media, streaming, networking, and entity infrastructure.

## Why OPNX.UI Exists

Video clients repeatedly need dense multi-channel layouts, rendering-oriented image controls, playback timelines, hierarchical operational views, paged data navigation, custom window integration, and drag-and-drop infrastructure. OPNX.UI keeps those capabilities reusable instead of tying them to one product screen.

## Current Implementation

### OPNX.UI.WPF

`OPNX.UI.WPF` provides controls and infrastructure for:

- Multi-tile video display and rendering-oriented media presentation
- Recorded-media timelines, event ranges, selection, navigation, and review workflows
- Hierarchical device, user, resource, and configuration views
- Paged list navigation and compact page-number layouts
- Operational navigation, custom title bars, input primitives, and drag-and-drop
- Integration with OPNX.Lib-based media, networking, streaming, and data layers

## Main Components

| Component | Purpose |
| --- | --- |
| `OpnxMultiView` | Configurable video-cell layouts, selection, zoom, synchronization, thumbnails, and Drag-and-drop |
| `OpnxImage` | Direct3D/D3DImage video surfaces with a remote-session-compatible buffered path |
| `OpnxPlaybackTimeline` | Multi-channel recording ranges, events, selection, time navigation, styling, and playback review |
| `OpnxTreeListView` | Hierarchical operational and configuration data |
| `OpnxPagingControl` | Page selection, navigation, and compact page-number presentation |
| `OpnxNavigator` | Horizontal or vertical application navigation |
| `OpnxTitlebar` | Custom WPF window title bar and common window actions |
| `Controls.Primitives` | Shared control bases and reusable building blocks |

## Design Direction

- Controls target dense operational interfaces such as VMS/NVR clients.
- Public behavior remains reusable across products rather than one application screen.
- OPNX.UI works with OPNX.Lib without duplicating application-specific logic.
- Additional .NET UI stacks may be added later, but only `OPNX.UI.WPF` is currently implemented.
- Public APIs remain preview quality while samples and integration contracts mature.

## Current Status

OPNX.UI is under active development. The current package is intended for preview evaluation, integration testing, research, non-commercial experimentation, and early feedback rather than as a stable production UI SDK.

## NuGet Package And Build

```powershell
dotnet add package OPNX.UI.WPF --prerelease
dotnet build OPNX.UI.slnx -c Debug
dotnet pack .\src\OPNX.UI.WPF\OPNX.UI.WPF.csproj -c Release -p:Platform=x64
```

Requirements: .NET 10 SDK and a Windows development environment with WPF support.

- `OPNX.UI.slnx` uses the configured published OPNX.Lib package.
- `OPNX.UI.Dev.slnx` uses sibling local OPNX.Lib source projects for cross-repository development.

## Samples And Documentation

Runnable examples are maintained in [OPNX Samples](https://github.com/OPNXLabs/opnx-samples).

- `OPNX.Samples.PlaybackTimeline` — timeline layout, recorded/event ranges, styling, selection, and navigation
- `OPNX.Samples.RtspMultiLiveViewer` — multi-view video, navigation, tree/list controls, and title-bar integration
- `OPNX.Samples.EntityStore` — OPNX.Lib EntityStore integration
- `OPNX.Samples.TcpChat` — OPNX.Lib networking integration

## License And Support

OPNX.UI is source-available but is not permissively licensed open-source software. Commercial use, redistribution, OEM integration, or inclusion in commercial products requires prior written permission from OPNX. See [LICENSE.txt](LICENSE.txt), [LICENSE.ko.txt](LICENSE.ko.txt), and [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

- Website: [https://www.opnx.kr/](https://www.opnx.kr/)
- Contact: `opnx@opnx.kr`
- Security: [SECURITY.md](SECURITY.md)
- Contributions: [CONTRIBUTING.md](CONTRIBUTING.md)

## Related Projects

- [OPNX Samples](https://github.com/OPNXLabs/opnx-samples) — runnable OPNX.Lib and OPNX.UI examples
- `OPNX.Lib` — networking, media, streaming, data, and system infrastructure
- `OPNX.V` — video-platform applications built on OPNX.Lib and OPNX.UI
