using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] string _headerText;
    [SerializeField] string _contentText;
    public delegate void OnTriggerToolTip(string header, string content);
    public static event OnTriggerToolTip onTriggerToolTip;
    public void OnPointerEnter(PointerEventData eventData)
    {
        onTriggerToolTip?.Invoke(_headerText, _contentText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onTriggerToolTip?.Invoke(_headerText, _contentText);
    }

}
 