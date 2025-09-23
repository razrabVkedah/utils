using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SceneButtonItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private RectTransform buttonRect;

    private int _buildIndex;
    private UnityAction<int> _onClick;
    
    public float GetHeight() => buttonRect.sizeDelta.y;

    public void Init(string title, int buildIndex, UnityAction<int> onClick)
    {
        _buildIndex = buildIndex;
        _onClick = onClick;

        if (titleText != null)
            titleText.text = title;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            // снимаем старые подписки на случай репулинга
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
        }
        else
        {
            Debug.LogWarning("[SceneButtonItem] Не найден Button.");
        }
    }

    private void HandleClick()
    {
        _onClick?.Invoke(_buildIndex);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}