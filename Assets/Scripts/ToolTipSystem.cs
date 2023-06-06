using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipSystem : MonoBehaviour
{
    private static ToolTipSystem s_current;
    [SerializeField] ToolTip _toolTip;
    public delegate void OnSetToolTip(string header, string content);
    public static event OnSetToolTip onSetToolTip;

    private void OnEnable()
    {
        ToolTipTrigger.onTriggerToolTip += HandleToolTip;
    }

    private void OnDisable()
    {
        ToolTipTrigger.onTriggerToolTip -= HandleToolTip;
    }

    private void Awake()
    {
        s_current = this;
    }

    private void HandleToolTip(string header, string content)
    {
        if(_toolTip.gameObject.activeSelf)
        {
            Hide();
        }
        else
        {
            Show(header, content);
        }
    }

    private void Show(string header, string content)
    {
        s_current._toolTip.gameObject.SetActive(true);
        onSetToolTip?.Invoke(header, content);
    }

    private void Hide()
    {
        s_current._toolTip.gameObject.SetActive(false);
    }
}
