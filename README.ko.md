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
- 장치 계층과 운영 목록을 함께 지원하는 TreeList와 Viewport 맞춤 Row 밀도
- MVVM 페이징 탐색과 화면 상태에 맞게 세밀하게 조정 가능한 Control Theme
- Determinate·Indeterminate 원형 진행 표시와 Wait UI 구성요소
- 운영 Navigation, 커스텀 Titlebar, 입력 Primitive 및 Drag-and-drop
- OPNX.Lib 기반 Media, Network, Streaming 및 Data 계층 통합

## 핵심 영상 UI

### Direct3D 기반 고속 영상 표시

`OpnxImage`는 일반 WPF `Image`에 Bitmap을 반복 복사하는 표시 방식이 아니라 Direct3D 9 Surface를 `D3DImage`와 연결하는 영상 렌더링 경로를 제공합니다. 다채널 Live View처럼 지속적으로 Frame이 갱신되는 화면에서 WPF Layout과 영상 Surface를 분리하고, 원격 세션처럼 Direct3D 경로를 사용할 수 없는 환경에서는 `WriteableBitmap` 이중 Buffer 경로로 전환할 수 있습니다.

영상 Frame의 Decode·변환·수명주기는 OPNX.Lib Media 계층에 두고 OPNX.UI는 표시 Surface와 UI 합성에 집중하므로, 동일한 렌더링 Control을 Live, Playback, Thumbnail 및 분석 결과 화면에 재사용할 수 있습니다.

### 운영용 MultiView

`OpnxMultiView`는 단순한 UniformGrid가 아니라 VMS/NVR 운영 화면의 영상 Cell Layout을 관리합니다.

- 정형 Grid와 비정형 Cell Layout
- Cell 분할·병합과 Layout 상태 저장·복원
- 단일·다중 Cell 선택과 선택 강조
- Cell 확대, 전체 화면 및 Zoom 상태 동기화
- 여러 MultiView 사이의 Cell·Layout 동기화
- Thumbnail 생성과 Drag-and-drop 영상 배치
- Cell별 Entity 연결과 변경 이벤트

Layout과 Cell 상태가 별도 모델로 유지되므로 화면을 다시 만들지 않고 View 구성을 변경하거나 사용자 Layout을 저장할 수 있습니다. Live Monitor뿐 아니라 Playback Review, Event Wall 및 다중 출력 화면의 기반으로 사용할 수 있습니다.

### 다채널 Playback Timeline

`OpnxPlaybackTimeline`은 단순 Slider가 아니라 여러 채널의 녹화 구간과 Event를 하나의 시간 축에서 탐색하기 위한 Review Control입니다.

- 채널별 녹화 시작·종료 구간 표시
- Event 종류, 색상, 설명 및 병합 정보 표시
- Center Time과 Visible Time Range 기반 탐색
- 시간 범위 변경과 필요한 데이터 요청 Event
- 녹화 구간·Event Hit Test와 선택 결과
- 선택 채널, Row 높이, 녹화 Bar, Tick 및 구분선 Theme
- 좌측 채널 Panel과 Entity 이름 표시 선택

Control이 모든 녹화 데이터를 소유하지 않고 현재 보이는 시간 범위를 기준으로 데이터를 요청할 수 있어, 장기간 녹화 기록과 다채널 Review 화면에도 적용할 수 있습니다.

## 주요 컴포넌트

| 컴포넌트 | 역할 |
| --- | --- |
| `OpnxMultiView` | 영상 Cell Layout, 선택, 확대, 동기화, Thumbnail 및 Drag-and-drop |
| `OpnxImage` | Direct3D/D3DImage 영상 Surface와 원격 세션 호환 Buffer 경로 |
| `OpnxImageViewer` | `OpnxImage`를 사용하는 영상 표시·상호작용 Viewer |
| `OpnxPlaybackTimeline` | 다채널 녹화 구간, Event, 선택, 시간 이동, Style 및 Playback Review |

## 보조 운영 컨트롤

OPNX.UI.WPF는 영상 Surface만 제공하는 Library가 아닙니다. 로그인, 장치·사용자 관리, 설정, 로그 검색, 재생 제어와 상태 표시까지 동일한 Theme 계약으로 구성할 수 있는 Control Set을 함께 제공합니다. 기본적인 NVR/VMS 클라이언트는 별도의 상용 WPF Control 제품군 없이 OPNX.UI.WPF를 중심으로 구성할 수 있습니다.

| 영역 | 제공 컨트롤 |
| --- | --- |
| 영상 표시 | `OpnxImage`, `OpnxImageViewer`, `OpnxMultiView`, `OpnxPlaybackTimeline` |
| 데이터 표시 | `OpnxTreeListView`, `OpnxPagingControl` |
| 텍스트·값 입력 | `OpnxTextBox`, `OpnxPasswordBox`, `OpnxNumericBox`, `OpnxIpTextBox` |
| 날짜·선택 | `OpnxComboBox`, `OpnxCheckBox`, `OpnxDatePicker`, `OpnxDateRangeSelector`, `OpnxStepSelector` |
| 명령 | `OpnxButton`, `OpnxToggleButton`, `OpnxArcButton`, `OpnxRoundRectButton` |
| 화면 구성 | `OpnxNavigator`, `OpnxTabControl`, `OpnxTabItem`, `OpnxTitlebar` |
| 상태 표시 | `OpnxRadialProgressBar` |
| 공통 기반 | `Controls.Primitives`, Drag-and-drop 및 Selection Building Block |

`OpnxTreeListView`는 부모·자식 관계를 표현하는 Tree 모드와 일반 목록을 위한 Flat 모드를 제공하며, `RowSizingMode="FitViewport"`와 `FitRowCount`를 사용하면 고정된 페이지 항목 수가 현재 Viewport를 채우도록 Row 높이를 계산할 수 있습니다. 해상도가 커질 때 생기는 과도한 공백과 작은 화면에서 불필요하게 나타나는 Scroll을 줄이는 데 사용할 수 있습니다.

```xml
<opnx:OpnxTreeListView ItemsSource="{Binding Items}"
                       ViewMode="Flat"
                       RowSizingMode="FitViewport"
                       FitRowCount="20" />
```

`OpnxPagingControl`은 외부 ViewModel의 `SelectedPageNumber`와 `MaxPageNumber`를 유지하면서 페이지 번호와 Next·Prev 상태를 관리합니다. 일반·선택·비활성 페이지와 이동 버튼의 Background, Foreground 및 MouseOver·Pressed Opacity를 각각 Theme으로 지정할 수 있습니다.

`OpnxComboBox`는 본문과 DropDown Panel의 Background·Border 및 상태 색상을 독립적으로 설정할 수 있어 Checkable Filter 같은 밀도 높은 검색 UI에 적용할 수 있습니다. `OpnxRadialProgressBar`는 별도 애플리케이션 로직 없이 Determinate와 Indeterminate 진행 상태를 표현하며 Splash, Wait Overlay 및 장시간 작업 표시에 사용할 수 있습니다.

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

---

> **“강하고 담대하라. 두려워하지 말며 놀라지 말라.”**
>
> — 여호수아 1:9
