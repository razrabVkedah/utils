using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QualityButtonItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject activeMarker; // необязательный индикатор активного (иконка/рамка)

    private int _index;
    private UnityAction<int> _onClick;
    
    [SerializeField] private RectTransform buttonRect;
    
    public float GetHeight() => buttonRect.sizeDelta.y;

    public void Init(string title, int qualityIndex, UnityAction<int> onClick, bool isActive)
    {
        _index = qualityIndex;
        _onClick = onClick;

        if (titleText != null) titleText.text = title;
        if (button == null) button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }

        if (activeMarker != null)
            activeMarker.SetActive(isActive);
    }

    private void HandleClick()
    {
        _onClick?.Invoke(_index);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}