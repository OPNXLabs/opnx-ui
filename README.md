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
- Tree and flat-list surfaces with viewport-aware row density for operational data
- MVVM paging navigation with finely themeable page and movement states
- Determinate and indeterminate radial progress for wait and loading experiences
- Operational navigation, custom title bars, input primitives, and drag-and-drop
- Integration with OPNX.Lib-based media, networking, streaming, and data layers

## Core Video UI

### Direct3D Video Presentation

`OpnxImage` does not depend on repeatedly copying bitmaps into a standard WPF `Image`. It connects a Direct3D 9 surface to WPF through `D3DImage`, separating the continuously updated video surface from WPF layout in multi-channel live views. A double-buffered `WriteableBitmap` path is available for environments such as remote sessions where the Direct3D path cannot be used.

Frame decoding, conversion, and ownership remain in the OPNX.Lib media layer, while OPNX.UI focuses on presentation surfaces and WPF composition. The same rendering control can therefore be reused for live, playback, thumbnail, and analytics-result views.

### Operational MultiView

`OpnxMultiView` is not a simple uniform grid. It manages the video-cell layout behavior expected from VMS/NVR operations screens.

- Regular grids and irregular cell layouts
- Cell split/merge behavior and layout-state save/restore
- Single/multiple cell selection and selection highlighting
- Cell zoom, full-screen flow, and synchronized zoom state
- Cell and layout synchronization across multiple MultiView instances
- Thumbnail generation and drag-and-drop video placement
- Per-cell entity association and change events

Layout and cell state remain in dedicated models, allowing applications to change views or persist user layouts without rebuilding the screen. The control can serve live monitoring, playback review, event walls, and multi-output workspaces.

### Multi-Channel Playback Timeline

`OpnxPlaybackTimeline` is a review control for navigating recording ranges and events from multiple channels on one time axis rather than a simple slider.

- Per-channel recording start/end ranges
- Event type, color, description, and merge information
- Navigation based on center time and visible time range
- Range-change and visible-data request events
- Recording/event hit testing and selection results
- Themeable selected channels, row heights, recording bars, ticks, and separators
- Optional channel panel and entity-name presentation

The control does not need to own an entire recording history. It can request data for the currently visible time range, making it suitable for long-duration archives and multi-channel review workflows.

## Main Components

| Component | Purpose |
| --- | --- |
| `OpnxMultiView` | Configurable video-cell layouts, selection, zoom, synchronization, thumbnails, and Drag-and-drop |
| `OpnxImage` | Direct3D/D3DImage video surfaces with a remote-session-compatible buffered path |
| `OpnxImageViewer` | Video presentation and interaction built around `OpnxImage` |
| `OpnxPlaybackTimeline` | Multi-channel recording ranges, events, selection, time navigation, styling, and playback review |

## Supporting Operational Controls

OPNX.UI.WPF is more than a collection of video surfaces. It provides a themed control set for login, device and user management, configuration, log search, playback control, and runtime status. A typical NVR/VMS client can be built primarily with OPNX.UI.WPF without requiring a separate commercial WPF control suite.

| Area | Controls |
| --- | --- |
| Video presentation | `OpnxImage`, `OpnxImageViewer`, `OpnxMultiView`, `OpnxPlaybackTimeline` |
| Data presentation | `OpnxTreeListView`, `OpnxPagingControl` |
| Text and value input | `OpnxTextBox`, `OpnxPasswordBox`, `OpnxNumericBox`, `OpnxIpTextBox` |
| Dates and selection | `OpnxComboBox`, `OpnxCheckBox`, `OpnxDatePicker`, `OpnxDateRangeSelector`, `OpnxStepSelector` |
| Commands | `OpnxButton`, `OpnxToggleButton`, `OpnxArcButton`, `OpnxRoundRectButton` |
| Application structure | `OpnxNavigator`, `OpnxTabControl`, `OpnxTabItem`, `OpnxTitlebar` |
| Progress and status | `OpnxRadialProgressBar` |
| Shared foundations | `Controls.Primitives`, drag-and-drop, and selection building blocks |

`OpnxTreeListView` supports parent/child tree presentation and a flat-list mode. With `RowSizingMode="FitViewport"` and `FitRowCount`, it can calculate row height so a fixed page size fills the current viewport, reducing excessive empty space on large displays and avoidable scrolling on smaller layouts.

```xml
<opnx:OpnxTreeListView ItemsSource="{Binding Items}"
                       ViewMode="Flat"
                       RowSizingMode="FitViewport"
                       FitRowCount="20" />
```

`OpnxPagingControl` preserves the external view model for `SelectedPageNumber` and `MaxPageNumber` while managing page items and Next/Previous state. Normal, selected, and disabled pages plus movement-button backgrounds, foregrounds, and mouse-over/pressed opacity can be themed independently.

`OpnxComboBox` separates the main control and dropdown-panel backgrounds, borders, and interaction colors, making it suitable for dense checkable-filter interfaces. `OpnxRadialProgressBar` provides determinate and indeterminate progress without application-specific behavior and can be used in splash screens, wait overlays, and long-running task indicators.

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

---

> **“Have not I commanded thee? Be strong and of a good courage; be not afraid, neither be thou dismayed: for the LORD thy God is with thee whithersoever thou goest.”**
>
> — Joshua 1:9, King James Version (KJV)
