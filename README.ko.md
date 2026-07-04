# OPNX.UI

[English](README.md)

OPNX.UI는 OPNX 클라이언트 애플리케이션을 위한 UI 솔루션 제품군입니다.

OPNX.UI의 목표는 관제 클라이언트, 재생/리뷰 도구, 영상 플랫폼 데스크톱 애플리케이션처럼 운영체제와 클라이언트 환경별로 필요한 영상 클라이언트 프로그램을 만들기 위한 재사용 가능한 .NET 기반 UI 토대를 제공하는 것입니다.

현재 이 저장소에서 제공하는 구현체는 Windows 기반 OPNX 영상 클라이언트를 위한 WPF 컨트롤 라이브러리인 `OPNX.UI.WPF`입니다.

## OPNX.UI를 만든 이유

OPNX 클라이언트 애플리케이션에는 일반적인 데스크톱 UI 컨트롤 이상의 기반이 필요합니다.

실제 VMS, NVR, 관제, 재생, 리뷰 클라이언트를 만들다 보면 다음과 같은 UI 기반 기능이 반복적으로 필요합니다.

- 밀도 높은 다채널 영상 레이아웃
- 렌더링 중심의 이미지 및 미디어 표시 컨트롤
- 재생 타임라인과 녹화 영상 리뷰 워크플로
- 장치, 채널, 사용자, 리소스, 설정을 다루는 계층형 뷰
- 운영 애플리케이션을 위한 내비게이션 UI
- 플랫폼별 윈도우 및 셸 통합
- 영상 운영 화면에 맞는 재사용 가능한 입력 컨트롤
- 드래그 앤 드롭 상호작용 인프라
- OPNX.Lib 기반 네트워크, 미디어, 데이터, 스트리밍 인프라와의 통합

OPNX.UI는 이러한 UI 기반을 OPNX 기반 클라이언트 애플리케이션이나 운영체제 대상마다 반복해서 구현하지 않기 위해 만들어졌습니다.

현재 저장소는 `OPNX.UI.WPF`를 통해 Windows 데스크톱 클라이언트를 위한 WPF 구현에 집중합니다. OPNX 생태계가 확장됨에 따라 다른 .NET UI 스택, AOT 지향 클라이언트, 플랫폼별 클라이언트 애플리케이션을 위한 UI 모듈이 추가될 수 있습니다.

## 현재 구현체

### OPNX.UI.WPF

`OPNX.UI.WPF`는 VMS, NVR, 실시간 관제 클라이언트, 재생/리뷰 도구, Windows 데스크톱 영상 플랫폼 애플리케이션을 만들기 위한 WPF UI 라이브러리입니다.

이 라이브러리는 영상 표시, 멀티 타일 관제 레이아웃, 내비게이션, 재생 타임라인, 계층형 데이터 뷰, 커스텀 윈도우 크롬, OPNX.Lib 기반 애플리케이션 구성을 위한 재사용 가능한 WPF 컨트롤과 UI 인프라를 제공합니다.

## OPNX.UI.WPF가 제공하는 기능

`OPNX.UI.WPF`는 다음 영역을 중심으로 구성됩니다.

- 영상 표시 및 레이아웃  
  멀티 타일 영상 레이아웃과 렌더링 중심의 이미지/미디어 표시 시나리오를 위한 컨트롤을 제공합니다.

- 재생 및 리뷰 워크플로  
  녹화 미디어 탐색과 리뷰를 위한 타임라인 컨트롤과 UI building block을 제공합니다.

- 운영 내비게이션 및 데이터 뷰  
  밀도 높은 관제 및 관리 화면을 위한 내비게이션 컨트롤과 계층형 리스트/트리 컴포넌트를 제공합니다.

- WPF 애플리케이션 구성  
  커스텀 타이틀바, 입력 primitive, 공유 컨트롤 베이스, 드래그 앤 드롭 인프라, UI 유틸리티 헬퍼를 제공합니다.

- OPNX 플랫폼 통합  
  OPNX.Lib 기반 애플리케이션, 미디어, 데이터, 스트리밍 계층과 함께 동작하도록 설계된 UI 컴포넌트를 제공합니다.

## 주요 컴포넌트

- `OPNX.UI.WPF.Controls.OpnxMultiView`  
  VMS/NVR 영상 표시 시나리오를 위한 멀티 타일 레이아웃 컨트롤입니다.

- `OPNX.UI.WPF.Controls.OpnxImage`  
  영상 및 미디어 표시 워크플로를 위한 렌더링 중심 이미지 컨트롤입니다.

- `OPNX.UI.WPF.Controls.OpnxPlaybackTimeline`  
  재생, 녹화 미디어 탐색, 리뷰 워크플로를 위한 타임라인 컨트롤입니다.

- `OPNX.UI.WPF.Controls.OpnxTreeListView`  
  장치, 사용자, 리소스, 설정과 같은 계층형 데이터를 위한 트리 리스트 컨트롤입니다.

- `OPNX.UI.WPF.Controls.OpnxNavigator`  
  가로 또는 세로 아이템 배치를 지원하는 내비게이션 선택 컨트롤입니다.

- `OPNX.UI.WPF.Controls.OpnxTitlebar`  
  WPF 셸 윈도우와 공통 윈도우 액션을 위한 커스텀 타이틀바 컨트롤입니다.

- `OPNX.UI.WPF.Controls.Primitives`  
  커스텀 WPF 컨트롤을 만들기 위한 공유 베이스 컨트롤과 재사용 가능한 building block입니다.

## 설계 방향

OPNX.UI는 여러 플랫폼의 OPNX 클라이언트 애플리케이션을 위한 UI 기반으로 설계되었습니다. `OPNX.UI.WPF`는 그 방향의 현재 Windows/WPF 구현체입니다.

- 솔루션 이름 `OPNX.UI`는 더 넓은 UI 제품군을 의미합니다.
- 시간이 지나면서 플랫폼별 UI 모듈을 OPNX.UI 제품군 아래에 추가할 수 있습니다.
- 컨트롤은 VMS/NVR 클라이언트와 같은 밀도 높은 운영 UI를 고려합니다.
- 컴포넌트는 OPNX.Lib 및 OPNX.V 스타일 애플리케이션과 함께 동작하도록 설계됩니다.
- UI 동작은 하나의 애플리케이션 화면에 묶이지 않고 여러 제품에서 재사용될 수 있어야 합니다.
- 공개 API와 예제는 샘플 애플리케이션이 성숙함에 따라 다듬어질 예정입니다.
- 로깅과 진단은 하위 애플리케이션 스택의 추상화 기반 방식을 따릅니다.
- 네이티브 미디어 및 렌더링 의존성과 OPNX 소유 코드는 라이선스와 배포 책임을 명확히 분리합니다.

## 향후 확장 방향

현재 저장소는 `OPNX.UI.WPF`를 제공합니다. 향후 OPNX.UI 모듈은 다음과 같은 .NET 기반 클라이언트 UI 계층으로 확장될 수 있습니다.

- AOT 지향 클라이언트 UI/runtime 지원
- 크로스 플랫폼 .NET UI 컨트롤
- macOS 중심 클라이언트 UI 계층
- 기타 운영체제별 클라이언트 UI 기반

이러한 방향은 명시적인 프로젝트로 추가되기 전까지 현재 공개 API 범위에 포함되지 않습니다.

## 사용 사례

- Video Management System, VMS
- Network Video Recorder, NVR
- 다채널 관제 클라이언트
- 재생 및 녹화 미디어 리뷰 도구
- `OPNX.UI.WPF`를 통한 Windows WPF 영상 클라이언트
- 향후 다른 운영체제를 위한 .NET 기반 영상 클라이언트
- OPNX.Lib 위에서 동작하는 데스크톱 플랫폼 UI 개발

## 현재 상태

OPNX.UI는 현재 활발히 개발 중입니다.

현재 구현체는 `OPNX.UI.WPF`입니다. 컨트롤 세트는 계속 발전 중이며, 샘플과 API 문서는 프로젝트가 성숙함에 따라 별도로 추가될 예정입니다.

현재 저장소는 production-ready UI SDK라기보다는 평가, 통합 테스트, 연구, 비상업적 실험, 초기 피드백을 위한 preview-quality UI library로 보아야 합니다.

## NuGet 패키지

`OPNX.UI.WPF`는 preview NuGet 패키지로 배포됩니다.

설치:

```powershell
dotnet add package OPNX.UI.WPF --version 0.1.0-preview.20260704.1
```

이 패키지는 preview 평가와 통합 테스트를 위한 버전입니다. 안정 버전이 나오기 전까지 API 호환성, 패키지 구조, 문서는 변경될 수 있습니다.

## 빌드

요구 사항:

- .NET 10 SDK
- WPF 지원 Windows 개발 환경

빌드:

```powershell
dotnet build OPNX.UI.slnx -c Debug
```

## 샘플 및 문서 계획

샘플, API 문서, 통합 가이드는 아직 포함되어 있지 않습니다.

우선적으로 다음 항목에 대한 문서화를 계획하고 있습니다.

- 기본 컨트롤 사용
- 레이아웃 구성
- 영상 표시 및 렌더링 통합
- 재생 타임라인 통합
- 트리/리스트 데이터 바인딩
- 윈도우 타이틀바 통합
- OPNX.Lib 통합

## 의존성

`OPNX.UI.WPF`는 `OPNX.Lib`와 함께 사용되도록 설계되었습니다.

로컬 개발 중에는 로컬 OPNX.Lib 프로젝트를 참조할 수 있으며, 패키지 빌드에서는 설정된 OPNX.Lib 패키지 버전을 참조할 수 있습니다.

## 라이선스

OPNX.UI는 source-available 형태로 공개됩니다. 그러나 permissive open-source software로 라이선스되는 것은 아닙니다.

이 저장소의 OPNX 소유 코드는 학습, 평가, 연구, 테스트 및 기타 비상업적 목적에 한해 사용할 수 있습니다.

상업적 사용, 재배포, OEM 통합, 상업 제품 또는 서비스에의 포함은 OPNX의 사전 서면 허가가 필요합니다.

자세한 내용은 [LICENSE.txt](LICENSE.txt)를 확인하십시오. 한국어 참고 번역은 [LICENSE.ko.txt](LICENSE.ko.txt)에서 확인할 수 있습니다.

## 서드파티 컴포넌트

이 저장소는 각 컴포넌트의 고유 라이선스가 적용되는 서드파티 소프트웨어를 사용합니다.

중요 사항:

- `OPNX.Lib`는 OPNX source-available license에 따라 사용됩니다.
- `FFmpeg.AutoGen`, `SharpDX`, `SharpDX.Direct3D9`는 MIT License에 따라 사용됩니다.
- 네이티브 `FFmpeg` 바이너리는 OPNX 라이선스의 적용 대상이 아닙니다.
- OPNX는 사용자가 네이티브 FFmpeg 바이너리를 별도로 획득하고 구성할 것을 권장합니다.
- 네이티브 FFmpeg 바이너리를 번들링하거나 재배포하는 당사자는 선택한 FFmpeg 빌드에 적용되는 라이선스 조건을 직접 준수해야 합니다.
- OPNX.Lib와 OPNX.Lib가 사용하는 서드파티 의존성은 각각의 라이선스 조건과 고지의 적용을 받습니다.

자세한 내용은 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)를 확인하십시오.

## 관련 프로젝트

- `OPNX.Lib`  
  네트워크, 미디어, 스트리밍, 데이터 인프라를 위한 핵심 SDK입니다.

- `OPNX.V`  
  OPNX.Lib와 OPNX.UI 위에서 구축되는 영상 플랫폼 애플리케이션입니다.

## 상업 라이선스 및 OEM 문의

상업적 사용, OEM 계약, 파트너십 문의는 아래 연락처로 문의하십시오.

- `opnx@opnx.kr`
