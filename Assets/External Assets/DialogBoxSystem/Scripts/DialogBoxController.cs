using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

namespace DialogBox
{
    #region 열거형 옵션

    /// <summary>
    /// 정렬 옵션
    /// </summary>
    public enum Align
    {
        /// <summary>
        /// 왼쪽
        /// </summary>
        LEFT,

        /// <summary>
        /// 오른쪽
        /// </summary>
        RIGHT,

        /// <summary>
        /// 가운데 (크기는 변하지 않음)
        /// </summary>
        CENTER,

        /// <summary>
        /// 가운데 확장 (크기를 좌우로 최대로 늘림)
        /// </summary>
        EXPAND,
    }

    /// <summary>
    /// 입력필드 이벤트
    /// </summary>
    public enum InputFieldEvent
    {
        /// <summary>
        /// 입력값이 변경될 때
        /// </summary>
        OnValueChanged,

        /// <summary>
        /// 변경이 끝난경우 (포커스 해제)
        /// </summary>
        OnEndEdit,

        /// <summary>
        /// 선택될 때
        /// </summary>
        OnSelect,

        /// <summary>
        /// 선택이 끝날 때
        /// </summary>
        OnDeselct,
    }

    #endregion

    public class DialogBoxController : MonoBehaviour
    {
        private const int BORDER_GAP = 20; //보더에 의해 콘텐츠 영역에서 차지하는 크기

        public const string RESERVED_EVENT_CLOSE = "RESERVED_EVENT_CLOSE";

        [Space(50)]
        [Header("다이얼로그 박스 루트(캔버스 자식)")]
        [SerializeField] private GameObject mDialogBoxRoot;

        [Space(50)]
        [Header("타이틀 라벨")]
        [SerializeField] private TextMeshProUGUI mTitleLabel;

        [Space(50)]
        [Header("추가할 텍스트 라벨 프리팹")]
        [SerializeField] private GameObject mTextLabelPrefab;

        [Header("추가할 버튼 프리팹")]
        [SerializeField] private GameObject mButtonPrefab;

        [Header("추가할 보더(이미지) 프리팹")]
        [SerializeField] private GameObject mBorderPrefab;

        [Header("추가할 입력필드 프리팹")]
        [SerializeField] private GameObject mInputFieldPrefab;

        [Space(50)]
        [Header("상단(타이틀) 영에 추가될 오브젝트들의 부모 트랜스폼")]
        [SerializeField] private RectTransform mTopContentsParent;

        [Header("콘텐츠 영역에 추가될 오브젝트들의 부모 트랜스폼")]
        [SerializeField] private RectTransform mCenterContentsParent;

        [Header("하단 상호작용 영역에 추가될 오브젝트들의 부모 트랜스폼")]
        [SerializeField] private RectTransform mBottomContentsParent;

        [Space(50)]
        [Header("콘텐츠 영역의 최상위 부모 트랜스폼")]
        [SerializeField] private RectTransform mContentsScrollViewRoot;

        private Vector2Int mDialogBoxRootSize; //활성화 된 다이얼로그박스의 사이즈
        private Action<DialogBoxController, string> mEventAction; //이 다이얼로그박스가 호출할 이벤트
        private Dictionary<string, DialogBoxController> mDestroyCallEvents = new Dictionary<string, DialogBoxController>(); //다이얼로그박스가 파괴될때 호출할 이벤트
        private Dictionary<string, DialogBoxController> mReferenceDialogBoxes = new Dictionary<string, DialogBoxController>(); //참조할 다이얼로그 박스

        #region 인스턴스된 오브젝트
        private Dictionary<string, GameObject> mInstantiatedObjects = new Dictionary<string, GameObject>(); //인스턴스된 입력필드들
        #endregion

        /// <summary>
        /// 다이얼로그박스를 초기화합니다.
        /// </summary>
        /// <param name="boxWidth">가로크기 (0 ~ 800)</param>
        /// <param name="boxHeight">세로크기 (0 ~ 600)</param>
        /// <param name="eventAction">다이얼로그박스 요소들이 이벤트를 호출할경우 전달될 대리자</param>
        public void InitDialogBox(int boxWidth, int boxHeight, Action<DialogBoxController, string> eventAction = null)
        {
            //크기 설정
            RectTransform rootRectTransform = mDialogBoxRoot.GetComponent<RectTransform>();
            rootRectTransform.sizeDelta = mDialogBoxRootSize = new Vector2Int(boxWidth, boxHeight);
            RefreshBoxSize();
            ToggleDialogBox(true);

            //이벤트 버튼 설정 (있는경우)
            if (eventAction != null)
            {
                mEventAction += eventAction; //이벤트 등록
            }
        }

        #region 요소 추가
        /// <summary>
        /// 텍스트 레이블 추가
        /// </summary>
        /// <param name="key">레퍼런스를 위한 키</param>
        /// <param name="isEnableWhenStart">다이얼로그박스가 활성화될때 같이 활성화되는가?</param>
        /// <param name="text">입력할 텍스트</param>
        /// <param name="fontSize">폰트 크기</param>
        /// <param name="alignOption">텍스트 정렬 옵션</param>
        /// <returns></returns>
        public TextMeshProUGUI AddText(string key, bool isEnableWhenStart, string text, float fontSize, TextAlignmentOptions alignOption = TextAlignmentOptions.Left)
        {
            GameObject newText = Instantiate(mTextLabelPrefab, Vector3.zero, Quaternion.identity, mCenterContentsParent);
            TextMeshProUGUI currentText = newText.GetComponent<TextMeshProUGUI>();

            //텍스트 설정
            currentText.text = text;
            currentText.fontSize = fontSize / 1.45f; //1.45는 평균적인 텍스트의 크기보정
            currentText.alignment = alignOption;

            //텍스트의 크기를 현재 콘텐츠영역 크기에 맞춤
            newText.GetComponent<RectTransform>().sizeDelta = new Vector2Int(mDialogBoxRootSize.x - BORDER_GAP, 0);

            //딕셔너리에 삽입
            AddInstantiatedObject(key, newText);

            //텍스트 활성화
            newText.SetActive(isEnableWhenStart);
            return currentText;
        }

        /// <summary>
        /// 버튼을 추가
        /// </summary>
        /// <param name="key">레퍼런스를 위한 키</param>
        /// <param name="isEnableWhenStart">다이얼로그박스가 활성화될때 같이 활성화되는가?</param>
        /// <param name="text">버튼에 쓰여질 텍스트</param>
        /// <param name="eventID">버튼이 눌리면 호출될 이벤트 ID</param>
        /// <param name="targetTransform">어떤 트랜스폼의 자식으로 위치할것인가? (null, 콘텐츠영역)</param>
        /// <returns></returns>
        public Button AddButton(string key, bool isEnableWhenStart, string text, string eventID, Transform targetTransform = null)
        {
            GameObject newButton = Instantiate(mButtonPrefab, Vector3.zero, Quaternion.identity, targetTransform == null ? mBottomContentsParent : targetTransform);

            //컴포넌트 획득
            Button currentButton = newButton.GetComponent<Button>();

            newButton.GetComponentInChildren<TextMeshProUGUI>(true).text = text;
            currentButton.onClick.AddListener(() => EventTrigger(eventID));

            //딕셔너리에 삽입
            AddInstantiatedObject(key, newButton);

            //활성화
            newButton.SetActive(isEnableWhenStart);
            return currentButton;
        }

        /// <summary>
        /// 요소들 간 간격을 추가
        /// </summary>
        /// <param name="key">레퍼런스를 위한 키</param>
        /// <param name="isEnableWhenStart">다이얼로그박스가 활성화될때 같이 활성화되는가?</param>
        /// <param name="height">높이</param>
        /// <param name="isCenter">콘텐츠 영역에 추가하는가? (false 하단 영역)</param>
        /// <returns></returns>
        public GameObject AddBorder(string key, bool isEnableWhenStart, int height, bool isCenter)
        {
            GameObject newBorder = Instantiate(mBorderPrefab, Vector3.zero, Quaternion.identity, isCenter ? mCenterContentsParent : mBottomContentsParent);

            Image borderImage = newBorder.GetComponent<Image>();
            borderImage.sprite = Sprite.Create(new Texture2D(height, height, TextureFormat.RGB24, false), new Rect(0, 0, height, height), new Vector2(0f, 0f), 100.0f);
            borderImage.type = Image.Type.Simple;
            newBorder.SetActive(isEnableWhenStart);

            //딕셔너리에 삽입
            AddInstantiatedObject(key, newBorder);

            return newBorder;
        }

        /// <summary>
        /// 이미지를 추가
        /// </summary>
        /// <param name="key">레퍼런스를 위한 키</param>
        /// <param name="isEnableWhenStart">다이얼로그박스가 활성화될때 같이 활성화되는가?</param>
        /// <param name="spriteImage">인스턴스 할 스프라이트 이미지</param>
        /// <param name="imageHeight">이미지의 높이</param>
        /// <param name="alignOption">이미지의 위치 정렬 옵션</param>
        /// <returns></returns>
        public Image AddImage(string key, bool isEnableWhenStart, Sprite spriteImage, int imageHeight, Align alignOption)
        {
            GameObject newBorder = AddBorder(null, isEnableWhenStart, imageHeight, true); //이미지를 사용하기위해 보더 생성

            //이미지 영역 생성
            GameObject newImage = Instantiate(mBorderPrefab, Vector3.zero, Quaternion.identity, mCenterContentsParent);
            newImage.transform.SetParent(newBorder.transform);

            //컴포넌트 획득
            Image currentImage = newImage.GetComponent<Image>();
            RectTransform currentTransform = newImage.GetComponent<RectTransform>();

            //가져올 이미지의 사이즈를 기반으로 동적으로 크기 보정
            float sizeCorrectionDelta = spriteImage.rect.size.y / imageHeight;
            currentTransform.sizeDelta = spriteImage.rect.size / sizeCorrectionDelta;
            currentImage.sprite = spriteImage;

            //이미지 위치 조정
            switch (alignOption)
            {
                case Align.LEFT:
                    {
                        currentTransform.pivot = new Vector2(0, 0.5f);
                        currentTransform.localPosition = Vector3.zero;
                        break;
                    }
                case Align.RIGHT:
                    {
                        currentTransform.pivot = new Vector2(1, 0.5f);
                        currentTransform.localPosition = new Vector2(mDialogBoxRootSize.x - BORDER_GAP, 0f);
                        break;
                    }
                case Align.CENTER:
                    {
                        currentTransform.pivot = new Vector2(0.5f, 0.5f);
                        currentTransform.localPosition = new Vector2((mDialogBoxRootSize.x - BORDER_GAP) * 0.5f, 0f);
                        break;
                    }
                case Align.EXPAND:
                    {
                        currentTransform.pivot = new Vector2(0.5f, 0.5f);
                        currentTransform.localPosition = new Vector2((mDialogBoxRootSize.x - BORDER_GAP) * 0.5f, 0f);
                        currentTransform.sizeDelta = new Vector2Int(mDialogBoxRootSize.x - BORDER_GAP, imageHeight);
                        break;
                    }
            }

            //딕셔너리에 삽입
            AddInstantiatedObject(key, newImage);

            //이미지 활성화
            currentImage.color = Color.white;
            newImage.SetActive(true);
            return currentImage;
        }

        /// <summary>
        /// 입력필드 추가
        /// </summary>
        /// <param name="key">레퍼런스를 위한 키</param>
        /// <param name="isEnableWhenStart">다이얼로그박스가 활성화될때 같이 활성화되는가?</param>
        /// <param name="widthPercentile">콘텐츠 영역에서 가로 길이의 비율 (0 ~ 1f)</param>
        /// <param name="height">높이</param>
        /// <param name="targetTransform">어떤 트랜스폼의 자식으로 위치할것인가? (null, 콘텐츠영역)</param>
        /// <param name="contentType">입력필드의 콘텐츠 타입</param>
        /// <param name="alignOption">입력필드 위치 정렬 옵션</param>
        /// <returns></returns>
        public TMP_InputField AddInputField(string key, bool isEnableWhenStart, float widthPercentile, int height, Transform targetTransform = null, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, Align alignOption = Align.LEFT)
        {
            GameObject newInputField = Instantiate(mInputFieldPrefab, Vector3.zero, Quaternion.identity, targetTransform == null ? mCenterContentsParent : targetTransform);

            //컴포넌트 획득
            RectTransform currentTransform = newInputField.GetComponent<RectTransform>();
            TMP_InputField currentInputField = newInputField.GetComponent<TMP_InputField>();

            //사이즈 조절
            widthPercentile = Mathf.Clamp(widthPercentile, 0f, 1f);
            currentTransform.sizeDelta = new Vector2Int((int)((mDialogBoxRootSize.x - BORDER_GAP) * widthPercentile), height);

            //입력필드 위치 조정
            switch (alignOption)
            {
                case Align.LEFT:
                    {
                        currentTransform.pivot = new Vector2(0, 0.5f);
                        currentTransform.localPosition = Vector3.zero;
                        break;
                    }
                case Align.RIGHT:
                    {
                        GameObject newBorder = AddBorder(null, true, height, true);
                        currentTransform.SetParent(newBorder.transform);

                        currentTransform.pivot = new Vector2(1, 0.5f);
                        currentTransform.localPosition = new Vector2(mDialogBoxRootSize.x - BORDER_GAP, 0f);
                        break;
                    }
                case Align.CENTER:
                    {
                        GameObject newBorder = AddBorder(null, true, height, true);
                        currentTransform.SetParent(newBorder.transform);

                        currentTransform.pivot = new Vector2(0.5f, 0.5f);
                        currentTransform.localPosition = new Vector2((mDialogBoxRootSize.x - BORDER_GAP) * 0.5f, 0f);
                        break;
                    }
                case Align.EXPAND:
                    {
                        currentTransform.pivot = new Vector2(0.5f, 0.5f);
                        currentTransform.localPosition = new Vector2((mDialogBoxRootSize.x - BORDER_GAP) * 0.5f, 0f);
                        currentTransform.sizeDelta = new Vector2Int(mDialogBoxRootSize.x - BORDER_GAP, height);
                        break;
                    }
            }

            //입력필드 옵션 설정
            currentInputField.contentType = contentType;

            //딕셔너리에 삽입
            AddInstantiatedObject(key, newInputField);

            //입력필드 활성화
            newInputField.SetActive(isEnableWhenStart);
            return currentInputField;
        }

        /// <summary>
        /// 콘텐츠 영역에 가로레이아웃을 추가
        /// </summary>
        /// <param name="key">레퍼런스를 위한 키</param>
        /// <param name="isEnableWhenStart">다이얼로그박스가 활성화될때 같이 활성화되는가?</param>
        /// <param name="height">높이</param>
        /// <param name="spacing">레이아웃 내부에서 요소들 간 간격</param>
        /// <param name="isAutoAlign">자동으로 크기를 정렬하는가?</param>
        /// <returns></returns>
        public Transform AddHorizontalLayout(string key, bool isEnableWhenStart, int height, int spacing, bool isAutoAlign)
        {
            GameObject newHorizontalLayoutBorder = AddBorder(null, isEnableWhenStart, height, true);

            //컴포넌트 획득
            HorizontalLayoutGroup currentLayout = newHorizontalLayoutBorder.AddComponent<HorizontalLayoutGroup>();

            //레이아웃 설정
            currentLayout.childControlWidth = isAutoAlign; //내부 요소들을 자동으로 정렬하는가?
            currentLayout.spacing = spacing; //간격 설정
            newHorizontalLayoutBorder.GetComponent<RectTransform>().sizeDelta = new Vector2Int(mDialogBoxRootSize.x - BORDER_GAP, height); //크기 설정

            //딕셔너리에 삽입
            AddInstantiatedObject(key, newHorizontalLayoutBorder);

            return newHorizontalLayoutBorder.transform; //트랜스폼을 리턴
        }

        /// <summary>
        /// 프리팹으로 미리 지정해둔 UI요소를 다이얼로그박스에 동적으로 추가
        /// </summary>
        /// <param name="key">레퍼런스를 위한 키</param>
        /// <param name="isEnableWhenStart">다이얼로그박스가 활성화될때 같이 활성화되는가?</param>
        /// <param name="prefab">인스턴스 할 프리팹</param>
        /// <param name="isDeliverEvent">프리팹에서 호출하는 이벤트를 받을것인가?</param>
        /// <returns></returns>
        public DialogBoxPrefabDeliver AddExistsPrefab(string key, bool isEnableWhenStart, DialogBoxPrefabDeliver prefab, bool isDeliverEvent)
        {
            //이벤트 리시버 설정
            if (isDeliverEvent)
            {
                prefab.EventReciever = this;
            }

            //컴포넌트 획득
            RectTransform currentRectTransform = prefab.GetComponent<RectTransform>();

            //크기 조정수치 구하기
            float scaleCorrection = mDialogBoxRootSize.x / currentRectTransform.sizeDelta.x;

            //프리팹의 크기를 기반으로 보더 생성
            GameObject newBorder = AddBorder(null, isEnableWhenStart, (int)(currentRectTransform.sizeDelta.y * scaleCorrection), true);

            //프리팹 위치 및 크기 조절 
            RectTransform newPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity, newBorder.transform).GetComponent<RectTransform>();
            newPrefab.localScale = Vector3.one * scaleCorrection;
            newPrefab.localPosition = new Vector3((mDialogBoxRootSize.x * 0.5f) - BORDER_GAP * 0.5f, 0f);

            //프리팹 활성화
            prefab.gameObject.SetActive(true);

            //활성화 여부에 따른 부모오브젝트인 보더 활성화
            newBorder.SetActive(isEnableWhenStart);

            //딕셔너리에 삽입
            AddInstantiatedObject(key, newPrefab.gameObject);

            return prefab;
        }
        #endregion

        #region 유틸리티

        /// <summary>
        /// 박스를 활성화/비활성화
        /// </summary>
        /// <param name="isEnable">활성화 할것인가?</param>
        public void ToggleDialogBox(bool isEnable)
        {
            gameObject.SetActive(isEnable);
        }

        /// <summary>
        /// 박스를 파괴
        /// </summary>
        public void DestroyBox()
        {
            foreach (KeyValuePair<string, DialogBoxController> caller in mDestroyCallEvents)
            {
                caller.Value.EventTrigger(caller.Key);
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 다이얼로그박스에서 레퍼런스할 UI요소를 획득
        /// </summary>
        /// <param name="key">레퍼런스의 키</param>
        /// <typeparam name="T">어떤 타입을 획득할것인가?</typeparam>
        /// <returns></returns>
        public T GetInstantiatedObject<T>(string key)
        {
            return mInstantiatedObjects[key].GetComponent<T>();
        }

        /// <summary>
        /// 입력필드에서 발생하는 이벤트를 추가
        /// </summary>
        /// <param name="inputField">이벤트를 호출할 입력필드</param>
        /// <param name="eventID">호출할 이벤트 ID</param>
        /// <param name="eventType">이벤트 타입</param>
        public void AddInputFieldEvent(TMP_InputField inputField, string eventID, InputFieldEvent eventType)
        {
            string argument = null;

            switch (eventType)
            {
                case InputFieldEvent.OnDeselct:
                    inputField.onDeselect.AddListener((argument) => EventTrigger(eventID));
                    break;
                case InputFieldEvent.OnEndEdit:
                    inputField.onEndEdit.AddListener((argument) => EventTrigger(eventID));
                    break;
                case InputFieldEvent.OnSelect:
                    inputField.onSelect.AddListener((argument) => EventTrigger(eventID));
                    break;
                case InputFieldEvent.OnValueChanged:
                    inputField.onValueChanged.AddListener((argument) => EventTrigger(eventID));
                    break;
            }
        }

        /// <summary>
        /// 다이얼로그박스에서 다른 다이얼로그박스를 레퍼런스 할 경우 딕셔너리에 추가
        /// </summary>
        /// <param name="dialogBoxKey">추가할 다이얼로그박스의 키</param>
        /// <param name="dialogBox">추가할 다이얼로그박스</param>
        public void AddReferenceDialogBox(string dialogBoxKey, DialogBoxController dialogBox)
        {
            mReferenceDialogBoxes.Add(dialogBoxKey, dialogBox);
        }

        /// <summary>
        /// 다이얼로그박스에서 다른 다이얼로그박스를 획득
        /// </summary>
        /// <param name="dialogBoxKey">지정한 다이얼로그박스의 키</param>
        /// <returns></returns>
        public DialogBoxController GetReferenceDialogBox(string dialogBoxKey)
        {
            return mReferenceDialogBoxes[dialogBoxKey];
        }

        /// <summary>
        /// 파괴시 호출할 이벤트 리스너를 등록
        /// </summary>
        /// <param name="eventID">파괴시 호출할 이벤트</param>
        /// <param name="eventReciever">누가 이벤트를 호출할것인가?</param>
        public void AddDestroyListener(string eventID, DialogBoxController eventReciever)
        {
            mDestroyCallEvents.Add(eventID, eventReciever);
        }

        /// <summary>
        /// 상단 영역의 높이를 조절
        /// </summary>
        /// <param name="height">설정할 높이</param>
        public void SetTopBoxHeight(int height)
        {
            //크기 설정
            mTopContentsParent.sizeDelta = new Vector2(0, height);

            //Height가 0보다 크면(존재하는경우) 활성화
            mTopContentsParent.gameObject.SetActive(height > 0);

            //글자 크기는 높이의 75%
            mTitleLabel.fontSizeMax = height * 0.75f;

            //박스 크기 조절
            RefreshBoxSize();
        }

        /// <summary>
        /// 상단 영역의 텍스트를 설정
        /// </summary>
        /// <param name="text">텍스트</param>
        /// <param name="alignOption">정렬 옵션</param>
        public void SetTitleBox(string text, TextAlignmentOptions alignOption = TextAlignmentOptions.Center)
        {
            TextMeshProUGUI titleLabel = mTitleLabel.GetComponent<TextMeshProUGUI>();

            titleLabel.text = text;
            titleLabel.alignment = alignOption;
        }

        /// <summary>
        /// 하단 영역의 높이를 조절
        /// </summary>
        /// <param name="height">설정할 높이</param>
        public void SetBottomBoxHeight(int height)
        {
            mBottomContentsParent.sizeDelta = new Vector2(0, height);

            mBottomContentsParent.gameObject.SetActive(height > 0);
            RefreshBoxSize();
        }
        #endregion

        #region 내부 클래스
        /// <summary>
        /// 동적으로 생성한 인스턴스를 딕셔너리에 삽입
        /// </summary>
        private void AddInstantiatedObject(string key, GameObject obj)
        {
            if (key == null) { return; }
            mInstantiatedObjects.Add(key, obj);
        }

        /// <summary>
        /// 콘텐츠 박스 사이즈를 조절
        /// </summary>
        private void RefreshBoxSize()
        {
            mContentsScrollViewRoot.sizeDelta = new Vector2(0, mDialogBoxRootSize.y - mTopContentsParent.sizeDelta.y - mBottomContentsParent.sizeDelta.y);
            mContentsScrollViewRoot.anchorMax = new Vector2(1.0f, 0.5f);

            mContentsScrollViewRoot.localPosition = new Vector3(-mDialogBoxRootSize.x * 0.5f, (mBottomContentsParent.sizeDelta.y - mTopContentsParent.sizeDelta.y) * 0.5f, 0f);
        }
        #endregion

        #region UI 이벤트 트리거

        /// <summary>
        /// 동적으로 생성한 UI요소들에서 이벤트를 받음
        /// </summary>
        /// <param name="eventID"></param>
        public void EventTrigger(string eventID)
        {
            switch (eventID) //호출한 이벤트가 예약되어있는 이벤트인가?
            {
                case RESERVED_EVENT_CLOSE: //단순히 닫는 다이얼로그박스인경우
                    {
                        DestroyBox();

                        break;
                    }
            }

            mEventAction.Invoke(this, eventID); //이벤트 호출
        }
        #endregion
    }
}