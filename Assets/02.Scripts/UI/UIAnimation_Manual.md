# UI 애니메이션 시스템 매뉴얼

## 목차
1. [시스템 개요](#시스템-개요)
2. [프리셋 생성 방법](#프리셋-생성-방법)
3. [UI에 애니메이션 적용 방법](#ui에-애니메이션-적용-방법)
4. [애니메이션 타입 가이드](#애니메이션-타입-가이드)
5. [실전 예제](#실전-예제)
6. [트러블슈팅](#트러블슈팅)

---

## 시스템 개요

### 무엇을 할 수 있나요?
- **코드 수정 없이** Inspector에서 모든 UI 애니메이션 설정 가능
- 재사용 가능한 **애니메이션 프리셋** 생성 및 관리
- UI Open/Close 애니메이션 + 이벤트별 애니메이션 (점수 증가, 콤보, 히트 피드백 등) 모두 설정 가능

### 핵심 컴포넌트
| 컴포넌트 | 설명 | 용도 |
|---------|------|------|
| **UIAnimationPreset** | ScriptableObject 기반 애니메이션 설정 파일 | 재사용 가능한 애니메이션 프리셋 |
| **UIAnimationPlayer** | MonoBehaviour 컴포넌트 | UI 오브젝트에 부착해서 애니메이션 재생 |
| **EUIAnimationType** | Enum | 애니메이션 종류 (FadeIn, ScaleOut, Punch 등) |
| **EUIAnimationTrigger** | Enum | 애니메이션 트리거 (OnOpen, OnScoreUpdate 등) |

---

## 프리셋 생성 방법

### Step 1: 프리셋 에셋 생성

1. **Project 창**에서 프리셋을 저장할 폴더로 이동 (예: `Assets/Resources/Animations/`)
2. 우클릭 → **Create → Game → UI Animation Preset**
3. 생성된 파일 이름 설정 (예: `ScalePunch_Score`, `FadeIn_Standard`)

### Step 2: 프리셋 설정

프리셋을 선택하면 **Inspector**에 다음 항목들이 표시됩니다:

#### 📌 Animation Info
- **Preset Name**: 프리셋 식별 이름 (예: "점수 증가 펀치 애니메이션")
- **Animation Type**: 애니메이션 종류 선택 (드롭다운)

#### 📌 Common Settings
- **Duration**: 애니메이션 지속 시간 (초 단위, 예: 0.3)
- **Ease Type**: 이징 타입 (OutQuad, OutBack, InOutQuad 등)
- **Delay**: 애니메이션 시작 전 대기 시간 (초)

#### 📌 Type-Specific Settings
애니메이션 타입에 따라 추가 설정이 표시됩니다:

**Scale Settings** (ScaleIn, ScaleOut 등)
- **Start Scale**: 시작 크기 (예: 0, 0, 0)
- **End Scale**: 종료 크기 (예: 1, 1, 1)

**Slide Settings** (SlideIn/Out 등)
- **Slide Distance**: 슬라이드 거리 (픽셀, 예: 100)

**Punch Settings** (PunchScale, PunchPosition 등)
- **Punch Scale**: 펀치 스케일 강도 (예: 0.1, 0.1, 0.1)
- **Punch Position**: 펀치 위치 강도 (픽셀, 예: 10, 10)
- **Punch Rotation**: 펀치 회전 강도 (각도, 예: 0, 0, 10)
- **Vibrato**: 진동 횟수 (1~20, 기본: 10)
- **Elasticity**: 탄성 (0~1, 0=부드러움, 1=강함)

**Shake Settings** (ShakePosition, ShakeRotation 등)
- **Strength**: Position Shake 강도 (픽셀)
- **Rotation Strength**: Rotation Shake 강도 (각도)
- **Vibrato**: 진동 횟수 (1~20)
- **Randomness**: 무작위성 (0~180, 0=일정, 180=랜덤)

#### 📌 Callbacks
- **Use On Complete**: 애니메이션 완료 시 콜백 호출 여부

---

## UI에 애니메이션 적용 방법

### 방법 1: Open/Close 애니메이션 (BaseUI 상속 클래스)

모든 UI가 열리고 닫힐 때 자동으로 재생되는 애니메이션을 설정합니다.

#### Step 1: UI 오브젝트 선택
Hierarchy 창에서 UI 오브젝트 선택 (예: Score UI, Menu UI 등)

#### Step 2: Inspector에서 Config 설정
1. **Config** 섹션 펼치기
2. **동작 설정 → Use Transition** 체크박스 활성화
3. **Animation Settings** 섹션이 나타남
4. **Open Animation Preset**: UI가 열릴 때 재생할 프리셋 할당
5. **Close Animation Preset**: UI가 닫힐 때 재생할 프리셋 할당

#### 예시
- Open Animation: `FadeScaleIn_Standard` (서서히 나타나면서 커짐)
- Close Animation: `FadeOut_Standard` (서서히 사라짐)

### 방법 2: 이벤트 애니메이션 (특정 이벤트 발생 시)

점수 증가, 콤보 증가, 히트 피드백 등 특정 이벤트가 발생했을 때 재생되는 애니메이션을 설정합니다.

#### Step 1: UIAnimationPlayer 컴포넌트 추가
1. Hierarchy 창에서 UI 오브젝트 선택
2. **Inspector → Add Component → UIAnimationPlayer**

#### Step 2: 애니메이션 매핑 설정
1. **UIAnimationPlayer** 컴포넌트의 **Animation Mappings** 배열 펼치기
2. **Size** 값을 증가시켜 새로운 항목 추가 (예: 1)
3. 각 항목 설정:
   - **Trigger**: 애니메이션을 실행할 트리거 선택 (드롭다운)
     - `OnScoreUpdate`: 점수 업데이트 시
     - `OnComboUpdate`: 콤보 업데이트 시
     - `OnHit`: 타격 피드백
     - `OnGaugeUpdate`: 게이지 업데이트 시
   - **Animations**: 여러 애니메이션을 배열로 추가 (동시 실행)

#### Step 3: 애니메이션 추가 (2가지 방법)

**방법 A: 프리셋 사용 (재사용하는 애니메이션)**
1. Animations 배열의 **Size** 증가
2. **Use Preset** 체크박스 활성화 ✓
3. **Preset** 필드에 프리셋 할당

**방법 B: 인라인 설정 (이 UI에만 사용하는 고유 애니메이션)**
1. Animations 배열의 **Size** 증가
2. **Use Preset** 체크박스 비활성화 ✗
3. **Inline Settings** 섹션에서 직접 설정:
   - Animation Type 선택
   - Duration, Ease Type, Delay 설정
   - Type-Specific Settings 조정

#### Step 4: 기본 타겟 설정 (선택사항)
- **Default Target**: 애니메이션이 적용될 기본 RectTransform 설정
- 설정하지 않으면 자동으로 현재 오브젝트의 RectTransform 사용

---

### 🎯 하이브리드 방식의 장점

**ScriptableObject 프리셋**:
- ✅ 여러 UI에서 재사용 가능
- ✅ 한 번 수정하면 모든 곳에 반영
- ✅ 일관된 애니메이션 유지

**Inspector 인라인 설정**:
- ✅ 파일 생성 불필요
- ✅ 빠른 프로토타이핑
- ✅ UI마다 독립적인 설정

**언제 무엇을 사용할까?**
- 자주 쓰는 애니메이션 → **프리셋**
- 특수한 일회성 애니메이션 → **인라인 설정**
- 여러 애니메이션 조합 → **배열에 추가**

---

### 예시: 점수 UI 설정 (여러 애니메이션 조합)

1. `UI_Score` 오브젝트에 UIAnimationPlayer 추가
2. Animation Mappings:
   ```
   ├─ Element 0
   │  ├─ Trigger: OnScoreUpdate
   │  └─ Animations (Size: 3)
   │     ├─ [0] Use Preset ✓
   │     │   └─ Preset: ScalePunch_Score (재사용 프리셋)
   │     ├─ [1] Use Preset ✗ (인라인 설정)
   │     │   ├─ Animation Type: ShakePosition
   │     │   ├─ Duration: 0.15
   │     │   ├─ Ease Type: Linear
   │     │   └─ Shake Settings
   │     │       └─ Strength: 3
   │     └─ [2] Use Preset ✓
   │         └─ Preset: FadeIn_Fast (재사용 프리셋)
   ```

**결과**: 점수 증가 시 3가지 효과가 **동시에** 재생됩니다!
- 크기가 통통 튀고 (PunchScale)
- 살짝 흔들리며 (ShakePosition)
- 페이드 인 효과 (FadeIn)

---

## 애니메이션 타입 가이드

### 기본 애니메이션

#### FadeIn / FadeOut
UI가 서서히 나타나거나 사라지는 효과
- **사용 예**: 메뉴 오픈/클로즈, 알림 표시
- **추천 설정**: Duration 0.3, Ease OutQuad

#### ScaleIn / ScaleOut
UI가 크기가 변하면서 나타나거나 사라지는 효과
- **사용 예**: 버튼 등장, 팝업 표시
- **추천 설정**:
  - ScaleIn: Start Scale (0,0,0), End Scale (1,1,1), Duration 0.3, Ease OutBack
  - ScaleOut: End Scale (0,0,0), Duration 0.2, Ease InBack

### Slide 애니메이션

#### SlideInFrom... / SlideOutTo...
UI가 특정 방향에서 슬라이드해서 들어오거나 나가는 효과
- **방향**: Top, Bottom, Left, Right
- **사용 예**: 사이드 메뉴, 알림 배너
- **추천 설정**: Slide Distance 100~200, Duration 0.4, Ease OutBack

### 피드백 애니메이션

#### PunchScale
UI가 짧게 커졌다 원래 크기로 돌아오는 효과
- **사용 예**: 점수 증가, 버튼 클릭, 아이템 획득
- **추천 설정**:
  - Punch Scale (0.1, 0.1, 0.1)
  - Duration 0.2
  - Vibrato 10
  - Elasticity 0.2
  - Ease OutBack

#### PunchPosition
UI가 짧게 흔들리는 효과 (위치)
- **사용 예**: 오류 알림, 충격 피드백
- **추천 설정**:
  - Punch Position (10, 10)
  - Duration 0.3
  - Vibrato 8
  - Ease OutQuad

#### PunchRotation
UI가 짧게 회전하는 효과
- **사용 예**: 특수 효과, 강조
- **추천 설정**:
  - Punch Rotation (0, 0, 10)
  - Duration 0.3
  - Vibrato 6

#### ShakePosition / ShakeRotation
UI가 불규칙하게 흔들리는 효과
- **사용 예**: 경고, 폭발 효과, 카메라 쉐이크
- **추천 설정**:
  - Strength 10~20
  - Duration 0.5
  - Vibrato 10
  - Randomness 90

### 조합 애니메이션

#### FadeScaleIn / FadeScaleOut
Fade와 Scale 효과가 동시에 적용
- **사용 예**: 부드러운 UI 전환, 고급 느낌의 팝업
- **추천 설정**: Duration 0.4, Ease OutBack

---

## 실전 예제

### 예제 1: 점수 UI 복합 효과 (프리셋 + 인라인 조합)

**목표**: 점수가 증가할 때 튀면서 살짝 흔들리는 효과

**프리셋 생성** (재사용할 것만):
1. `Assets/Resources/Animations/Score/` 폴더 생성
2. 프리셋 생성: `ScalePunch_Score`
3. 설정:
   - Animation Type: `PunchScale`
   - Duration: `0.2`
   - Ease Type: `OutBack`
   - Punch Scale: `(0.15, 0.15, 0.15)`
   - Vibrato: `10`
   - Elasticity: `0.2`

**UI 적용** (프리셋 + 인라인 조합):
1. Hierarchy → `UI_Score` 오브젝트 선택
2. Add Component → `UIAnimationPlayer`
3. Animation Mappings:
   - Size: `1`
   - Element 0:
     - Trigger: `OnScoreUpdate`
     - Animations (Size: `2`):
       - **[0] Use Preset ✓**
         - Preset: `ScalePunch_Score` (재사용 프리셋)
       - **[1] Use Preset ✗** (인라인 설정)
         - Animation Type: `ShakePosition`
         - Duration: `0.15`
         - Ease Type: `Linear`
         - Shake Settings:
           - Strength: `3`
           - Vibrato: `8`

**결과**: 점수가 증가할 때마다 **숫자가 통통 튀면서 살짝 흔들립니다**!
- 프리셋은 재사용 가능 (다른 UI에서도 사용)
- 인라인 설정은 이 UI에만 사용 (파일 생성 불필요)

---

### 예제 2: 콤보 UI 애니메이션

**목표**: 콤보 증가 시 텍스트가 커지면서 나타나는 효과

**프리셋 생성**:
1. 프리셋: `FadeScaleIn_Combo`
2. 설정:
   - Animation Type: `FadeScaleIn`
   - Duration: `0.3`
   - Ease Type: `OutBack`
   - Start Scale: `(0.5, 0.5, 0.5)`
   - End Scale: `(1, 1, 1)`

**UI 적용**:
1. `UI_Combo` 오브젝트 선택
2. UIAnimationPlayer 컴포넌트의 Animation Mappings:
   - Trigger: `OnComboUpdate`
   - Preset: `FadeScaleIn_Combo`

---

### 예제 3: 메뉴 UI Open/Close 애니메이션

**목표**: 메뉴가 부드럽게 나타나고 사라지는 효과

**프리셋 생성**:

**오픈 애니메이션** - `FadeScaleIn_Menu`:
- Animation Type: `FadeScaleIn`
- Duration: `0.4`
- Ease Type: `OutBack`
- Start Scale: `(0.8, 0.8, 0.8)`
- End Scale: `(1, 1, 1)`

**클로즈 애니메이션** - `FadeScaleOut_Menu`:
- Animation Type: `FadeScaleOut`
- Duration: `0.3`
- Ease Type: `InQuad`
- End Scale: `(0.8, 0.8, 0.8)`

**UI 적용**:
1. Menu UI 오브젝트 선택
2. Config → Use Transition 체크
3. Animation Settings:
   - Open Animation Preset: `FadeScaleIn_Menu`
   - Close Animation Preset: `FadeScaleOut_Menu`

---

### 예제 4: 게이지 UI 피드백

**목표**: 게이지 값이 변할 때 살짝 흔들리는 효과

**프리셋 생성**:
1. 프리셋: `ShakePosition_Gauge`
2. 설정:
   - Animation Type: `ShakePosition`
   - Duration: `0.2`
   - Ease Type: `Linear`
   - Strength: `5`
   - Vibrato: `10`
   - Randomness: `90`

**UI 적용**:
1. `UI_CrescentGauge` 오브젝트 선택
2. UIAnimationPlayer 추가
3. Animation Mappings:
   - Trigger: `OnGaugeUpdate`
   - Preset: `ShakePosition_Gauge`

---

## 트러블슈팅

### Q: 애니메이션이 재생되지 않아요!

**해결 방법**:
1. **UIAnimationPlayer 컴포넌트 확인**: UI 오브젝트에 제대로 부착되어 있는지 확인
2. **프리셋 할당 확인**: Animation Mappings에 프리셋이 제대로 할당되었는지 확인
3. **트리거 확인**: 올바른 트리거가 선택되었는지 확인
4. **Console 로그 확인**: 경고 또는 에러 메시지 확인

### Q: 애니메이션이 겹쳐서 이상해요!

**원인**: 같은 타겟에 여러 애니메이션이 동시에 재생되고 있을 수 있습니다.

**해결 방법**:
- UIAnimationPreset은 자동으로 기존 애니메이션을 처리합니다:
  - **Punch/Shake 애니메이션**: `DOComplete()`로 완료 후 원본 크기/위치로 복원
  - **다른 애니메이션**: `DOKill()`로 즉시 제거
- UIAnimationPlayer를 사용하면 자동으로 원본 크기와 위치를 추적하므로 더 안전합니다.
- 만약 문제가 계속된다면, 애니메이션 타이밍을 조정하거나 Delay를 사용하세요.

### Q: Punch 애니메이션이 진행 중일 때 새로 실행하면 크기가 이상해져요!

**원인**: Punch 애니메이션은 상대적 애니메이션이므로 원본 크기에서 시작해야 합니다.

**해결 방법**:
- **UIAnimationPlayer 사용 (권장)**: 자동으로 원본 크기/위치를 추적하고 복원합니다.
  ```csharp
  // UIAnimationPlayer가 자동으로 원본 크기 관리
  _animationPlayer.Play(EUIAnimationTrigger.OnScoreUpdate, targetTransform);
  ```
- **직접 프리셋 호출**: UIAnimationPlayer 없이 직접 호출 시에는 `DOComplete()`로 이전 애니메이션을 완료시킵니다.
  - 가능하면 UIAnimationPlayer를 사용하는 것을 권장합니다!

**작동 원리**:
1. UIAnimationPlayer가 최초 1회 원본 Transform 정보 저장
2. Punch 애니메이션 실행 전에 원본 크기/위치로 복원
3. 새 Punch 애니메이션 시작
4. 항상 정확한 크기에서 Punch 효과 발생!

### Q: Fade 애니메이션이 작동하지 않아요!

**원인**: Fade 애니메이션은 CanvasGroup 컴포넌트가 필요합니다.

**해결 방법**:
- UIAnimationPreset이 자동으로 CanvasGroup을 추가합니다.
- 수동으로 추가하려면: Add Component → Canvas Group

### Q: UI가 슬라이드 후 원래 위치로 돌아오지 않아요!

**원인**: Slide 애니메이션은 UI의 원래 위치를 기준으로 작동합니다.

**해결 방법**:
- UI의 앵커 위치(Anchored Position)가 올바르게 설정되어 있는지 확인하세요.
- Slide Out 애니메이션은 UI를 숨긴 후 원래 위치로 되돌려야 할 수 있습니다.

### Q: 프리셋을 여러 UI에서 공유하고 싶어요!

**방법**:
1. `Assets/Resources/Animations/Common/` 폴더에 공통 프리셋 생성
2. 여러 UI의 UIAnimationPlayer에서 같은 프리셋 참조

**예시**:
- `FadeIn_Standard`: 모든 UI Open 애니메이션에 사용
- `FadeOut_Standard`: 모든 UI Close 애니메이션에 사용
- `ScalePunch_Generic`: 범용 피드백 효과

### Q: 새로운 트리거를 추가하고 싶어요!

**방법**:
1. `Assets/02.Scripts/System/Enum/EUIAnimationTrigger.cs` 파일 열기
2. Enum에 새로운 트리거 추가 (예: `OnItemPickup`)
3. 해당 UI 스크립트에서 새 트리거 사용

```csharp
// 예시
if (_animationPlayer != null)
{
    _animationPlayer.Play(EUIAnimationTrigger.OnItemPickup, targetTransform);
}
```

---

## 팁 & 권장사항

### 애니메이션 Duration 가이드
- **짧고 빠른 피드백** (버튼 클릭, 점수 증가): 0.1~0.2초
- **일반 전환** (UI 등장/사라짐): 0.3~0.4초
- **부드러운 전환** (메뉴 오픈): 0.4~0.6초
- **느린 연출** (특수 효과): 0.6초 이상

### Ease Type 선택 가이드
- **OutQuad**: 부드럽고 자연스러운 감속 (범용)
- **OutBack**: 통통 튀는 느낌 (버튼, 점수 증가)
- **InOutQuad**: 시작과 끝이 부드러운 전환
- **Linear**: 일정한 속도 (Shake 효과)
- **OutElastic**: 탄성 있는 느낌 (특수 효과)

### 프리셋 명명 규칙
- **[타입]_[용도]**: `ScalePunch_Score`, `FadeIn_Menu`
- **[타입]_[속도]**: `FadeIn_Fast`, `SlideIn_Slow`
- **[타입]_Standard**: 범용 프리셋

### 폴더 구조 권장
```
Assets/Resources/Animations/
├── Common/              # 공통 프리셋
│   ├── FadeIn_Standard
│   ├── FadeOut_Standard
│   └── ScaleIn_Standard
├── Score/               # 점수 관련
│   └── ScalePunch_Score
├── Combo/               # 콤보 관련
│   └── FadeScaleIn_Combo
└── Feedback/            # 피드백 효과
    ├── Punch_Generic
    └── Shake_Hit
```

---

## 지원 및 문의

추가 질문이나 문제가 있으시면 프로그래머에게 문의하세요!

**시스템 버전**: 1.0
**최종 업데이트**: 2026-01-07
