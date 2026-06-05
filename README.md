# OPNX.UI.WPF

OPNX.UI.WPF is a WPF UI library for building stateful video applications such as VMS, NVR, live monitoring clients, and playback/review tools.

It provides reusable controls for video display, multi-tile layouts, navigation, playback timelines, hierarchical data views, and WPF application composition on top of OPNX.Lib.

## Status

OPNX.UI.WPF is under active development.

The control set is still evolving, and samples/API documentation will be added separately. The current README describes the intended component surface and project direction without treating every control as final.

## Features

- `OpnxMultiView` for grid-based video tile layouts
- `OpnxImage` for DirectX-backed image and rendering scenarios
- `OpnxPlaybackTimeline` for playback timeline and recorded media navigation workflows
- `OpnxTreeListView` for hierarchical list/tree management screens
- `OpnxNavigator` for selectable application navigation surfaces
- `OpnxTitlebar` for custom WPF window chrome scenarios
- `OpnxButton`, `OpnxToggleButton`, `OpnxCheckBox`, `OpnxTextBox`, and `OpnxPasswordBox` for reusable input primitives
- Drag-and-drop interaction infrastructure
- Shared WPF primitives for building custom controls
- Rendering and utility helpers for UI composition

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

OPNX.UI.WPF is intended to provide application-grade controls rather than isolated visual samples.

- Controls are designed for dense operational UIs such as VMS/NVR clients.
- Components are expected to work together with OPNX.Lib and OPNX.V-style applications.
- Public APIs and examples will be refined as the sample applications mature.
- Logging and diagnostics should remain abstraction-based through the underlying application stack.

## Use Cases

- Video Management Systems (VMS / NVR)
- Multi-channel monitoring clients
- Playback and recorded media review tools
- WPF-based media and surveillance applications
- Platform UI development on top of `OPNX.Lib`

## Build

Requirements:

- .NET 10 SDK
- Windows development environment with WPF support

Build:

```powershell
dotnet build OPNX.UI.slnx -c Debug
```

## Samples And Documentation

Samples and API documentation are planned but are not included yet.

Planned documentation topics include:

- basic control usage
- layout composition
- playback timeline integration
- tree/list data binding
- window title bar integration

## Dependency

`OPNX.UI.WPF` is designed to be used together with `OPNX.Lib`.

## License

This repository is source-available for learning, evaluation, research, testing, and other non-commercial use.

Commercial use, redistribution, OEM integration, or inclusion in commercial products or services requires prior written permission from OPNX.

See [LICENSE.txt](LICENSE.txt) for full terms.

## Third-Party Components

This repository uses third-party software components under their respective licenses.

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) for details.

## Related Projects

- `OPNX.Lib` - core SDK for networking, media, streaming, and data infrastructure.
- `OPNX.V` - video platform applications built on top of OPNX.Lib and OPNX.UI.

## Commercial And OEM Inquiries

For commercial licensing, OEM agreements, or partnership inquiries:

- `opnx@opnx.kr`
