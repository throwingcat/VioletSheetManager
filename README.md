Violet SheetManager

테이블 데이터의 클래스 생성 및 읽기를 지원합니다.

---
[사용방법]
## Project Settings 옵션 설정
Project Setting > Player > Other Settings > Configuration

Allow downloads over HTTP* 항목을 Always allowed로 설정합니다.


## Config 생성
프로젝트탭에서 Create/Violet/SheetManager/Config 메뉴를 통해 Config 파일을 생성합니다.

## Config 설정
1. Config - Local 설정
2. Sheet Local Directory 항목에 .xlsx 파일이 있는 폴더를 연결합니다.
3. Class Generate Folder 항목에 클래스가 생성될 폴더를 연결합니다
4. 톱니바퀴 아이콘을 눌러 Excel Reader Setting Window를 활성화 합니다.
5. Variable Type Line - 변수 타입이 지정된 라인을 입력합니다.<br/>Window 하단 [From ▶ To] List를 통해 Excel에 지정된 변수 타입과 class에 생성될 변수 타입을 지정할 수 있습니다.
7. Variable Name Line - 변수 이름이 지정된 라인을 입력합니다.
8. Value Start Line - 값이 시작되는 라인을 지정합니다.

<br/>![image](https://github.com/throwingcat/VioletSheetManager/assets/7219435/4147e0e8-d7be-4bfd-85af-01437819f6b7)

---

## Manager 생성

1. Unity 상단 메뉴 Violet/Sheet/Generate 메뉴를 통해 매니저 스크립트를 생성합니다.
2. Unity 상단 메뉴 Violet/Sheet/Create Manager 메뉴를 통해 매니저 오브젝트를 생성합니다.

## Manager 설정

1. 생성된 Manager를 클릭하여 Insepctor에서 SheetManager Component가 있는지 확인 합니다.
2. SheetManager Component에서 Config 필드에 생성한 Config 파일을 연결 합니다.

---




