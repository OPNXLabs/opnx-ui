# OPNX.UI

[English](README.md)

> **라이선스 안내:** OPNX.UI는 오픈 소스 소프트웨어가 아닌 source-available 소프트웨어입니다. 상업적 사용과 재배포에는 OPNX의 사전 서면 허가가 필요합니다. 자세한 내용은 [LICENSE.txt](LICENSE.txt)를 확인하십시오.

OPNX.UI는 OPNX 영상 클라이언트를 위한 재사용 가능한 .NET UI 기반입니다. 현재 구현체는 VMS, NVR, 모니터링, 재생 및 리뷰 애플리케이션을 위한 Windows WPF 컨트롤 라이브러리인 `OPNX.UI.WPF`입니다.

## 보통 제품 내부에만 존재하는 영상 UI 컴포넌트

OPNX.UI는 VMS나 NVR 제품마다 반복해서 개발되는 어렵고 도메인 특화된 Control을 재사용 가능한 형태로 제공합니다.

- **WPF를 위한 Direct3D 영상 표시** — `OpnxImage`는 `D3DImage`를 통해 Direct3D 9 Surface를 WPF에 통합하며, 원격 세션 같은 환경을 위한 `WriteableBitmap` 이중 Buffer 경로도 제공합니다.
- **실제 운영을 위한 Playback Timeline** — `OpnxPlaybackTimeline`은 여러 채널의 녹화 구간과 Event 데이터를 표시하고, 시간 범위 이동·선택과 Review 화면에 필요한 Style·Layout 설정을 제공합니다.
- **단순 균등 Grid가 아닌 운영용 MultiView** — `OpnxMultiView`는 정형·비정형 Cell Layout, 분할·병합형 Layout 동작, Cell 선택, 확대·전체 화면 흐름, Layout 상태 동기화, Thumbnail 및 Drag-and-drop 배치를 지원합니다.
- **밀도 높은 운영 화면용 Control** — 계층형 TreeList, Paging, Navigation, Custom Window Chrome, Selection Primitive 및 공통 Drag-and-drop 인프라가 영상 주변의 실제 운영 화면을 구성합니다.

이 기능들은 화면 시안이나 특정 애플리케이션 전용 Prototype이 아니라 재사용 가능한 Library Component입니다. OPNX.Lib의 Media, Streaming, Network 및 Entity 인프라와 조합하도록 설계되어 있습니다.

## OPNX.UI를 만든 이유

영상 클라이언트에는 다중 채널 레이아웃, 렌더링 중심 이미지 컨트롤, 재생 타임라인, 계층형 운영 화면, 페이징, 커스텀 윈도우 및 Drag-and-drop 인프라가 반복적으로 필요합니다. OPNX.UI는 이런 기반이 특정 제품 화면에 종속되지 않고 재사용되도록 합니다.

## 현재 구현체

### OPNX.UI.WPF

`OPNX.UI.WPF`는 다음 기능을 제공합니다.

- 다중 타일 영상 표시와 렌더링 중심 미디어 표현
- 녹화·이벤트 구간, 선택, 시간 이동 및 리뷰를 위한 재생 타임라인
- 장치, 사용자, 리소스 및 설정을 위한 계층형 데이터 화면
- 페이징 목록 탐색과 간결한 페이지 번호 배치
- 운영 Navigation, 커스텀 Titlebar, 입력 Primitive 및 Drag-and-drop
- OPNX.Lib 기반 Media, Network, Streaming 및 Data 계층 통합

## 주요 컴포넌트

| 컴포넌트 | 역할 |
| --- | --- |
| `OpnxMultiView` | 영상 Cell Layout, 선택, 확대, 동기화, Thumbnail 및 Drag-and-drop |
| `OpnxImage` | Direct3D/D3DImage 영상 Surface와 원격 세션 호환 Buffer 경로 |
| `OpnxPlaybackTimeline` | 다채널 녹화 구간, Event, 선택, 시간 이동, Style 및 Playback Review |
| `OpnxTreeListView` | 계층형 운영·설정 데이터 표시 |
| `OpnxPagingControl` | 페이지 선택, 이동 및 페이지 번호 표시 |
| `OpnxNavigator` | 가로·세로 애플리케이션 Navigation |
| `OpnxTitlebar` | WPF 커스텀 Titlebar와 공통 윈도우 동작 |
| `Controls.Primitives` | 공통 Control Base와 재사용 Building Block |

## 설계 방향

- VMS/NVR 같은 밀도 높은 운영 UI를 대상으로 합니다.
- 공개 동작은 특정 화면이 아니라 여러 제품에서 재사용할 수 있어야 합니다.
- 애플리케이션 고유 로직을 중복하지 않고 OPNX.Lib와 통합합니다.
- 향후 다른 .NET UI 계층을 추가할 수 있지만 현재 구현체는 `OPNX.UI.WPF`뿐입니다.
- 샘플과 통합 계약이 성숙하는 동안 공개 API는 Preview 단계로 유지됩니다.

## 현재 상태

OPNX.UI는 활발히 개발 중입니다. 현재 패키지는 안정된 Production UI SDK가 아니라 평가, 통합 테스트, 연구, 비상업적 실험 및 초기 피드백을 위한 Preview 라이브러리입니다.

## NuGet 패키지 및 빌드

```powershell
dotnet add package OPNX.UI.WPF --prerelease
dotnet build OPNX.UI.slnx -c Debug
dotnet pack .\src\OPNX.UI.WPF\OPNX.UI.WPF.csproj -c Release -p:Platform=x64
```

요구 사항은 .NET 10 SDK와 WPF를 지원하는 Windows 개발 환경입니다.

- `OPNX.UI.slnx`는 설정된 OPNX.Lib NuGet 패키지를 사용합니다.
- `OPNX.UI.Dev.slnx`는 여러 저장소를 함께 개발할 때 형제 OPNX.Lib 소스 프로젝트를 사용합니다.

## 샘플 및 문서

실행 가능한 예제는 [OPNX Samples](https://github.com/OPNXLabs/opnx-samples)에서 관리합니다.

- `OPNX.Samples.PlaybackTimeline` — 타임라인 배치, 녹화·이벤트 구간, 스타일, 선택 및 이동
- `OPNX.Samples.RtspMultiLiveViewer` — MultiView 영상, Navigation, Tree/List 및 Titlebar 통합
- `OPNX.Samples.EntityStore` — OPNX.Lib EntityStore 통합
- `OPNX.Samples.TcpChat` — OPNX.Lib Network 통합

## 라이선스 및 지원

OPNX.UI는 source-available이지만 permissive 오픈 소스 라이선스가 아닙니다. 상업적 사용, 재배포, OEM 통합 또는 상용 제품 포함에는 OPNX의 사전 서면 허가가 필요합니다. [LICENSE.txt](LICENSE.txt), [LICENSE.ko.txt](LICENSE.ko.txt), [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)를 확인하십시오.

- 웹사이트: [https://www.opnx.kr/](https://www.opnx.kr/)
- 문의: `opnx@opnx.kr`
- 보안: [SECURITY.ko.md](SECURITY.ko.md)
- 기여: [CONTRIBUTING.ko.md](CONTRIBUTING.ko.md)

## 관련 프로젝트

- [OPNX Samples](https://github.com/OPNXLabs/opnx-samples) — OPNX.Lib 및 OPNX.UI 실행 예제
- `OPNX.Lib` — Network, Media, Streaming, Data 및 System 인프라
- `OPNX.V` — OPNX.Lib와 OPNX.UI 기반 영상 플랫폼 애플리케이션
