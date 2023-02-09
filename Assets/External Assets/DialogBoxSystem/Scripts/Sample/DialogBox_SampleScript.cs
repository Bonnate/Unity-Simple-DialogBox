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
        DialogBoxController controller = DialogBoxGenerator.Instance.CreateSimpleDialogBox("알림창", "알림사항", "확인", EVENT_DialogBox);

        controller.AddBorder(null, true, 10, true);
        controller.AddText(null, true, "확인 버튼을 눌러 \n창을 끌 수 있습니다.", 20, TextAlignmentOptions.CenterGeoAligned);

        controller.AddExistsPrefab("TogglePrefab", true, mTogglesSamplePrefab, true);
    }

    private void EVENT_DialogBox(DialogBoxController controller, string eventID)
    {
        switch (eventID)
        {
            case "Toggle1":
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle1").SetIsOnWithoutNotify(Random.value > 0.5f);
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle2").SetIsOnWithoutNotify(Random.value > 0.5f);
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle3").SetIsOnWithoutNotify(Random.value > 0.5f);
                break;

            case "Toggle2":
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle1").SetIsOnWithoutNotify(Random.value > 0.5f);
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle2").SetIsOnWithoutNotify(Random.value > 0.5f);
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle3").SetIsOnWithoutNotify(Random.value > 0.5f);
                break;

            case "Toggle3":
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle1").SetIsOnWithoutNotify(Random.value > 0.5f);
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle2").SetIsOnWithoutNotify(Random.value > 0.5f);
                controller.GetInstantiatedObject<DialogBoxPrefabDeliver>("TogglePrefab").GetElement<Toggle>("Toggle3").SetIsOnWithoutNotify(Random.value > 0.5f);
                break;
        }
    }
}
