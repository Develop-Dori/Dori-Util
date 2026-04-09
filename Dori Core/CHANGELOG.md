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
- 서비스 로케이터 기반 모듈 프레임워크 (`DCore`)
- MD_Timer, MD_Sound, MD_Pool, MD_Resource, MD_Error, MD_WebNetwork
- MD_IdleReturn (사용자 비활동 감지 → 첫 화면 복귀 이벤트)
- DCore_Object (MonoBehaviour 라이프사이클 콜백 래퍼)
- SOD_Sound (사운드 에셋 ScriptableObject)
