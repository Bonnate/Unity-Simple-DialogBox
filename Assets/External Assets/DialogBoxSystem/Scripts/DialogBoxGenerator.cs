using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using DialogBox;

public class DialogBoxGenerator : MonoBehaviour
{
    [Header("다이얼로그 박스 프리맵의 최상위 부모")]
    [SerializeField] private GameObject mDialogBoxPrefab;

    public DialogBoxController CreateEmptyDialogBox()
    {
        DialogBoxController controller = Instantiate(mDialogBoxPrefab, Vector3.zero, Quaternion.identity).GetComponent<DialogBoxController>();
        return controller;
    }

    public void CreateSimpleDialogBox(string title, string context, string buttonText, int width = 200, int height = 150, int titleHeight = 20, int buttonHeight = 20)
    {
        //다이얼로그박스 생성
        DialogBoxController controller = CreateEmptyDialogBox();

        //다이얼로그박스 크기 설정
        controller.InitDialogBox(width, height);

        //사이즈 조절
        controller.SetTopBoxHeight(titleHeight);
        controller.SetBottomBoxHeight(buttonHeight);

        //타이틀
        controller.SetTitleBox(title);

        //콘텍스트 텍스트 생성
        controller.AddText(null, true, context, 20, TextAlignmentOptions.Center);

        //나가기 버튼 생성
        controller.AddButton(null, true, buttonText, DialogBoxController.RESERVED_EVENT_CLOSE);
    }

    private void Start()
    {
        CreateSimpleDialogBox("공지", "알림사항", "확인");
    }
}
