# Overcome

Unity 3D FPS 프로젝트

## 🚀 프로젝트 설정 가이드

### 1. 저장소 클론
```bash
git clone [저장소 URL]
```

### 2. 외부 에셋 설치 (필수)
아래 에셋들은 용량 문제로 Git에 포함되지 않습니다.
**반드시 다운로드 후 지정된 경로에 배치하세요.**

| 에셋명 | 다운로드 링크 | 배치 경로 |
|--------|--------------|-----------|
| _DLNK | [Google Drive](https://drive.google.com/file/d/1uUplZ7xWtO6lnFHCYR3lMLjP13AUoWHO/view?usp=sharing) | `Assets/_DLNK/` |

### 3. Unity 버전
- **Unity 6000.3.2f1**
- 다른 버전 사용 시 호환성 문제가 발생할 수 있습니다.

### 4. 프로젝트 열기
1. Unity Hub에서 프로젝트 폴더 선택
2. 첫 실행 시 임포트 시간이 걸릴 수 있습니다.

## ⚠️ 주의사항
- 에셋 폴더명과 경로를 정확히 맞춰주세요
- `_DLNK` 폴더는 `.gitignore`에 포함되어 있으므로 커밋하지 마세요
- 에셋 폴더 구조가 `Assets/_DLNK/_DLNK/...`처럼 중첩되지 않도록 주의

## 📁 프로젝트 구조
```
Assets/
├── 01.Scenes/          # 씬 파일
├── 02.Scripts/         # 스크립트
├── 03.Prefabs/         # 프리팹
├── 04.Images/          # 이미지
├── 08.Materials/       # 머티리얼
├── 10.ScriptableObjects/  # SO 데이터
└── _DLNK/              # 외부 에셋 (Git 제외)
```
