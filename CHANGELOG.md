# Changelog

## [1.1.0] - 2026-04-09

### Added
- **DSingleton\<T\>**: MonoBehaviour 기반 제네릭 싱글톤 베이스 클래스
  - 자동 생성, 중복 파괴, 종료 시 null 반환 처리
- **MD_Setting**: JSON 파일 기반 설정 관리 모듈
  - 기본 필드: `debugMode`, `timeout`
  - `LoadAs<T>()`로 프로젝트별 커스텀 설정 확장 지원
  - 설정 변경 이벤트 (`OnSettingsChanged`)

---

## [1.0.0] - 2026-04-09

### Added
- **Dori Core**: 서비스 로케이터 기반 모듈 프레임워크
  - MD_Timer, MD_Sound, MD_Pool, MD_Resource, MD_Error, MD_WebNetwork
  - MD_IdleReturn (사용자 비활동 감지 → 첫 화면 복귀 이벤트)
- **Dori Toolkit**: 독립 컴포넌트 모음
  - Custom Dotween System (7종 애니메이션 컴포넌트)
  - Touch System (터치 파티클 이펙트)
  - UI Timer System (카운트다운 + 준비/출발 연출)
  - ChildFirstLayoutGroup
  - AutoTMPFontCreator, CustomTMPGeneratorWindow, PSDLayerImporter (Editor)
- UPM 패키지 구조 (package.json, assembly definitions)
- Core / Toolkit 개별 임포트 지원 (`?path=` 파라미터)
