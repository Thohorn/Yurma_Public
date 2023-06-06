using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _headerField;
    [SerializeField] private TextMeshProUGUI _contentField;
    [SerializeField] private LayoutElement _layoutElement;

    [SerializeField] private int _characterWrapLimit;
    [SerializeField] private RectTransform _rectTransform;

    private void OnEnable()
    {
        ToolTipSystem.onSetToolTip += SetText;
    }

    private void OnDisable()
    {
        ToolTipSystem.onSetToolTip -= SetText;
    }

    private void Awake()
    {
        _rectTransform.GetComponent<RectTransform>();
    }

    private void SetText(string header, string content)
    {
        if (_contentField.gameObject.activeInHierarchy)
        {
            if(string.IsNullOrEmpty(header))
            {
                _headerField.gameObject.SetActive(false);
            }
            else
            {
                _headerField.gameObject.SetActive(true);
                _headerField.text = header;
            }

            _contentField.text = content;

            int headerLength = _headerField.text.Length;
            int contentLength = _contentField.text.Length;

            _layoutElement.enabled = (headerLength > _characterWrapLimit || contentLength > _characterWrapLimit) ? true : false;
        }

    }

    private void Update()
    {
        // Show the tool tip on the screen; bottom right of the mouse if possible, else it is on the left of the mouse.
        Vector2 position = Input.mousePosition;

        float pivoteX = position.x / Screen.width;
        float pivoteY = position.y / Screen.height;
        Debug.Log("pivotX: " + pivoteX);
        Debug.Log("pivotY: " + pivoteY);
        Vector2 newPosition = new Vector2(position.x, position.y);
        if(position.x > ((Screen.width / 4) * 3.5 ))
        {
            newPosition = new Vector2(position.x - (_rectTransform.rect.width * 3), position.y - (_rectTransform.rect.height * 2));
        }
        else
        {
            newPosition = new Vector2(position.x + (_rectTransform.rect.width / 2), position.y - (_rectTransform.rect.height * 2));
        }
        transform.position = newPosition;
    }
}
