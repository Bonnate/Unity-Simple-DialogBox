using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogBox
{
    [System.Serializable]
    public struct UiElement
    {
        public string key;
        public GameObject go;
    }

    public class DialogBoxPrefabDeliver : MonoBehaviour
    {
        [Header("전달할 엘리먼트들")]
        [SerializeField] private UiElement[] mElements; //런타임 도중 사용할 엘리먼트

        private Dictionary<string, GameObject> mElementsDictionary = new Dictionary<string, GameObject>(); //런타임 도중 찾을 요소를 저장한 딕셔너리

        /// <summary>
        /// 전달자가 이벤트를 전달할 리시버, null일경우 이벤트를 전달하지 않음
        /// </summary>
        [HideInInspector] public DialogBoxController EventReciever = null; //Deliver 프리팹에서 호출되는 이벤트를 호출할 다이얼로그박스 컨트롤러

        private void Awake()
        {
            //인스펙터에서 설정한 요소들을 집어넣는다.
            foreach (UiElement element in mElements) { mElementsDictionary.Add(element.key, element.go); }
        }

        public T GetElement<T>(string key) { return mElementsDictionary[key].GetComponent<T>(); }

        public void Event_Deliver(string eventID)
        {
            if (EventReciever != null) { EventReciever.EventTrigger(eventID); }
        }
    }
}
