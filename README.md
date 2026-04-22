# OPNX.UI.WPF

OPNX.UI.WPF is a WPF UI library for building stateful video applications such as VMS, NVR, and real-time monitoring platforms on top of OPNX.Lib.

It provides reusable controls for video display, multi-tile layouts, interaction handling, and WPF-based application composition.

## Features

- `OpnxMultiView` for grid-based video tile layouts
- `OpnxImage` for DirectX-backed image and rendering scenarios
- Reusable WPF primitives for building custom controls
- Drag-and-drop interaction infrastructure
- Utility and rendering helpers for UI composition
- Designed to work together with `OPNX.Lib`

## Main Components

- `OPNX.UI.WPF.Controls.OpnxMultiView`  
  Core multi-tile layout control for VMS/NVR video display scenarios.

- `OPNX.UI.WPF.Controls.OpnxImage`  
  Image and rendering-oriented control intended for video and media display workflows.

- `OPNX.UI.WPF.Controls.Primitives`  
  Shared base controls and reusable building blocks for custom WPF controls.

- `OPNX.UI.WPF.Interactivity`  
  Interaction components such as drag-and-drop support.

- `OPNX.UI.WPF.Infrastructure.Rendering`  
  Internal rendering support components used by UI controls.

## Use Cases

- Video Management Systems (VMS / NVR)
- Multi-channel monitoring applications
- Real-time camera and stream viewer applications
- WPF-based media and surveillance clients
- Platform UI development on top of `OPNX.Lib`

## Dependency

`OPNX.UI.WPF` is designed to be used together with `OPNX.Lib`.

## License

This repository is source-available for learning, evaluation, research, testing, and other non-commercial use.

Commercial use, redistribution, OEM integration, or inclusion in commercial products or services requires prior written permission from OPNX.

See [LICENSE.txt](LICENSE.txt) for full terms.

## Commercial & OEM Inquiries

For commercial licensing, OEM agreements, or partnership inquiries:

- `opnx@opnx.kr`
