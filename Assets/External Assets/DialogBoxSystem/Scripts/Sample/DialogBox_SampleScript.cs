using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DialogBox;
using UnityEngine.UI;
using TMPro;

public class DialogBox_SampleScript : MonoBehaviour
{
    [SerializeField] private DialogBoxPrefabDeliver mTogglesSamplePrefab; //토글 샘플 프리팹

    private void Start()
    {
        //간단한 다이얼로그박스 생성
        DialogBoxController controller = DialogBoxGenerator.Instance.CreateSimpleDialogBox("알림창", "알림사항", "확인", EVENT_DialogBox, "BTN_Confirm");

        //다이얼로그박스에 요소 추가
        controller.AddBorder(null, true, 10, true);
        controller.AddText(null, true, "확인 버튼을 눌러 \n창을 끌 수 있습니다.", 20, TextAlignmentOptions.CenterGeoAligned);
        controller.AddBorder(null, true, 10, true);
        controller.AddText(null, true, "단, 토글을 한개 이상 켜세요.", 20, TextAlignmentOptions.CenterGeoAligned);

        //프리팹을 추가
        controller.AddExistsPrefab("TogglePrefab", true, mTogglesSamplePrefab, true);

        //파괴시 이벤트 호출(자기자신)
        controller.AddDestroyListener();
    }

    /// <summary>
    /// 다이얼로그박스에서 호출되는 이벤트를 수행하는 대리자
    /// </summary>
    private void EVENT_DialogBox(DialogBoxController controller, string eventID)
    {
        switch (eventID)
        {
            case "BTN_Confirm": //확인 버튼을 누른경우
            {
                bool isToggle1On = controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle1").isOn;
                bool isToggle2On = controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle2").isOn;
                bool isToggle3On = controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle3").isOn;

                if(isToggle1On || isToggle2On || isToggle3On)
                {
                    controller.DestroyBox();
                }
                else
                {
                    DialogBoxGenerator.Instance.CreateSimpleDialogBox("", "토글을 최소 한개 이상 켜야합니다.", "확인", null, DialogBoxController.RESERVED_EVENT_CLOSE, 150, 100, 0, 30);
                }

                break;
            }

            case "Toggle1":
            case "Toggle2":
            case "Toggle3":
                DialogBoxGenerator.Instance.CreateSimpleDialogBox("", eventID + "눌림", "확인", null, DialogBoxController.RESERVED_EVENT_CLOSE, 150, 100, 0, 30);

                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle1").SetIsOnWithoutNotify(Random.value > 0.5f);
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle2").SetIsOnWithoutNotify(Random.value > 0.5f);
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle3").SetIsOnWithoutNotify(Random.value > 0.5f);
                break;

            case DialogBoxController.RESERVED_EVENT_ON_DESTROY:
            {
                DialogBoxGenerator.Instance.CreateSimpleDialogBox("", "박스파괴", "종료", null, DialogBoxController.RESERVED_EVENT_CLOSE, 150, 100, 0, 30);
                break;
            }
        }
    }
}
