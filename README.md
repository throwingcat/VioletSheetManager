Violet SheetManager

게임 내 데이터 테이블의 다운로드와

클래스 생성 및 데이터 로드 후 가져오기를 지원합니다.

*현재 CSV 형식만 지원합니다. 

---
[사용방법]
## Project Settings 옵션 설정
Project Setting > Player > Other Settings > Configuration

Allow downloads over HTTP* 항목을 Always allowed로 설정합니다.

## Config 생성
프로젝트탭에서 Create/Violet/SheetManager/Config 메뉴를 통해 Config 파일을 생성합니다.

## Config 설정

1. 클래스가 생성될 폴더를 연결합니다
2. Sheet추가하고 Sheet의 이름과 다운로드 URL을 작성합니다.
3. 다운로드시 주소는 BaseURL + URL로 연결합니다.

---

## Manager 생성

1. Unity 상단 메뉴 Violet/Sheet/Generate 메뉴를 통해 매니저 스크립트를 생성합니다.
2. Unity 상단 메뉴 Violet/Sheet/Create Manager 메뉴를 통해 매니저 오브젝트를 생성합니다.

## Manager 설정

1. 생성된 Manager를 클릭하여 Insepctor에서 SheetManager Component가 있는지 확인 합니다.
2. SheetManager Component에서 Config 필드에 생성한 Config 파일을 연결 합니다.

---




