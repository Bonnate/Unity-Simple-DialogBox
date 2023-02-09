using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

namespace DialogBox
{
    public enum Align
    {
        LEFT,
        RIGHT,
        CENTER,
        EXPAND,
    }

    public enum InputFieldEvent
    {
        OnValueChanged,
        OnEndEdit,
        OnSelect,
        OnDeselct,
    }

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


        private bool mIsNotifyMode = true; //특정한 이벤트 없이 확인버튼을 누르면 사라지는 창인가?
        private Vector2Int mDialogBoxRootSize; //활성화 된 다이얼로그박스의 사이즈
        private Action<DialogBoxController, string> mEventAction; //이 다이얼로그박스가 호출할 이벤트
        private Dictionary<string, DialogBoxController> mDestroyCallEvents = new Dictionary<string, DialogBoxController>(); //다이얼로그박스가 파괴될때 호출할 이벤트
        private Dictionary<string, DialogBoxController> mReferenceDialogBoxes = new Dictionary<string, DialogBoxController>(); //참조할 다이얼로그 박스

        #region 인스턴스된 오브젝트
        private Dictionary<string, GameObject> mInstantiatedObjects = new Dictionary<string, GameObject>(); //인스턴스된 입력필드들
        #endregion

        public void InitDialogBox(int boxWidth, int boxHeight, Action<DialogBoxController, string> eventAction = null)
        {
            //크기 설정
            RectTransform rootRectTransform = mDialogBoxRoot.GetComponent<RectTransform>();
            rootRectTransform.sizeDelta = mDialogBoxRootSize = new Vector2Int(boxWidth, boxHeight);
            RefreshBoxSize();

            //이벤트 버튼 설정 (있는경우)
            if (eventAction != null)
            {
                Debug.Log("Not null");
                mIsNotifyMode = false; //단순 알림 모드가 아님
                mEventAction += eventAction; //이벤트 등록
            }
        }

        /// <summary>
        /// 텍스트를 추가합니다. 콘텐츠 영역에 추가됩니다.
        /// </summary>
        /// <param name="text"></param>
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
        /// 버튼을 추가합니다. 하단 영역에 추가됩니다.
        /// </summary>
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
        /// 콘텐츠 영역 및 하단영역에 보더를 추가합니다.
        /// </summary>
        /// <param name="size">추가할 보더의 사이즈</param>
        /// <param name="isCenter">콘텐츠 영역에 추가할것인가?</param>
        public GameObject AddBorder(string key, bool isEnableWhenStart, int size, bool isCenter)
        {
            GameObject newBorder = Instantiate(mBorderPrefab, Vector3.zero, Quaternion.identity, isCenter ? mCenterContentsParent : mBottomContentsParent);

            Image borderImage = newBorder.GetComponent<Image>();
            borderImage.sprite = Sprite.Create(new Texture2D(size, size, TextureFormat.RGB24, false), new Rect(0, 0, size, size), new Vector2(0f, 0f), 100.0f);
            borderImage.type = Image.Type.Simple;
            newBorder.SetActive(isEnableWhenStart);

            //딕셔너리에 삽입
            AddInstantiatedObject(key, newBorder);

            return newBorder;
        }

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


        #region 유틸리티
        public void ToggleDialogBox(bool isEnable)
        {
            gameObject.SetActive(isEnable);
        }

        public void DestroyBox()
        {
            foreach (KeyValuePair<string, DialogBoxController> caller in mDestroyCallEvents)
            {
                caller.Value.EventTrigger(caller.Key);
            }

            Destroy(gameObject);
        }

        public T GetInstantiatedObject<T>(string key)
        {
            return mInstantiatedObjects[key].GetComponent<T>();
        }

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

        public void AddReferenceDialogBox(string dialogBoxKey, DialogBoxController dialogBox)
        {
            mReferenceDialogBoxes.Add(dialogBoxKey, dialogBox);
        }

        public DialogBoxController GetReferenceDialogBox(string dialogBoxKey)
        {
            return mReferenceDialogBoxes[dialogBoxKey];
        }        

        public void AddDestroyListener(string eventID, DialogBoxController eventReciever)
        {
            mDestroyCallEvents.Add(eventID, eventReciever);
        }
        
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

        public void SetTitleBox(string text, TextAlignmentOptions alignOption = TextAlignmentOptions.Center)
        {
            TextMeshProUGUI titleLabel =  mTitleLabel.GetComponent<TextMeshProUGUI>(); 

            titleLabel.text = text;
            titleLabel.alignment = alignOption;
        }

        public void SetBottomBoxHeight(int height)
        {
            mBottomContentsParent.sizeDelta = new Vector2(0, height);

            mBottomContentsParent.gameObject.SetActive(height > 0);
            RefreshBoxSize();
        }
        #endregion

        #region 내부 클래스
        private void AddInstantiatedObject(string key, GameObject obj)
        {
            if (key == null) { return; }
            mInstantiatedObjects.Add(key, obj);
        }

        private void RefreshBoxSize()
        {
            mContentsScrollViewRoot.sizeDelta = new Vector2(0, mDialogBoxRootSize.y - mTopContentsParent.sizeDelta.y - mBottomContentsParent.sizeDelta.y);
            mContentsScrollViewRoot.anchorMax = new Vector2(1.0f, 0.5f);

            mContentsScrollViewRoot.localPosition = new Vector3(-mDialogBoxRootSize.x * 0.5f, (mBottomContentsParent.sizeDelta.y - mTopContentsParent.sizeDelta.y) * 0.5f, 0f);
        }
        #endregion

        #region UI 이벤트
        public void EventTrigger(string eventID)
        {
            switch(eventID)
            {
                case RESERVED_EVENT_CLOSE:
                {
                    DestroyBox();

                    break;
                }
            }

            if (mIsNotifyMode) 
            { 
                ToggleDialogBox(false);
                return;
            }

            mEventAction.Invoke(this, eventID);
        }
        #endregion
    }
}