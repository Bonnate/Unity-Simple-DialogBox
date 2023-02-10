using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using DialogBox;

public class DialogBoxGenerator : Singleton<DialogBoxGenerator>
{
    [Header("다이얼로그 박스 프리맵의 최상위 부모")]
    [SerializeField] private GameObject mDialogBoxPrefab;

    /// <summary>
    /// 비어있는 다이얼로그박스를 생성
    /// </summary>
    /// <returns></returns>
    public DialogBoxController CreateEmptyDialogBox()
    {
        DialogBoxController controller = Instantiate(mDialogBoxPrefab, Vector3.zero, Quaternion.identity).GetComponent<DialogBoxController>();
        return controller;
    }

    /// <summary>
    /// 단순한 다이얼로그박스를 생성
    /// </summary>
    /// <param name="title">타이틀 (상단 제목)</param>
    /// <param name="context">콘텐츠 (콘텐츠 글 내용)</param>
    /// <param name="buttonText">버튼 (하단 버튼 내용)</param>
    /// <param name="eventAction">이벤트 대리자</param>
    /// <param name="buttonEventID">기본 생성되는 버튼이 호출할 이벤트 (null: 단순 종료)</param>
    /// <param name="width">가로길이</param>
    /// <param name="height">세로길이</param>
    /// <param name="titleHeight">상단 영역의 높이</param>
    /// <param name="buttonHeight">하단 영역의 높이</param>
    /// <returns></returns>
    public DialogBoxController CreateSimpleDialogBox(string title, string context, string buttonText, System.Action<DialogBoxController, string> eventAction = null, string buttonEventID = DialogBoxController.RESERVED_EVENT_CLOSE, int width = 200, int height = 150, int titleHeight = 30, int buttonHeight = 30)
    {
        //다이얼로그박스 생성
        DialogBoxController controller = CreateEmptyDialogBox();

        //이벤트 호출 환경에서 대리자가 없는경우?
        if(buttonEventID != DialogBoxController.RESERVED_EVENT_CLOSE && eventAction == null)
        {
            Debug.LogWarningFormat("{0}has no reciever!", gameObject.name);
        }

        //다이얼로그박스 크기 설정
        controller.InitDialogBox(width, height, eventAction);

        //사이즈 조절
        controller.SetTopBoxHeight(titleHeight);
        controller.SetBottomBoxHeight(buttonHeight);

        //타이틀
        controller.SetTitleBox(title);

        //콘텍스트 텍스트 생성
        controller.AddText(null, true, context, 20, TextAlignmentOptions.Center);

        //나가기 버튼 생성
        controller.AddButton(null, true, buttonText, buttonEventID);

        //생성한 컨트롤러 리턴
        return controller;
    }
}
