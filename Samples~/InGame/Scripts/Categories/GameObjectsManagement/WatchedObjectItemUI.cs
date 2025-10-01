using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rusleo.Graphics
{
    /// <summary>
    /// Простой UI-элемент: текст названия, кнопка и картинка состояния.
    /// Ты сам верстаешь, просто привяжи поля в инспекторе.
    /// </summary>
    public class WatchedObjectItemUI : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Image stateImage;

        [Header("Visuals")]
        [Tooltip("Спрайт/цвет для ВКЛ")]
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Color onColor = Color.green;

        [Tooltip("Спрайт/цвет для ВЫКЛ")]
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Color offColor = Color.red;

        [Tooltip("Цвет, если ссылка потеряна/объект уничтожен.")]
        [SerializeField] private Color missingColor = new Color(1f, 0.6f, 0.2f);

        public event Action<WatchedObjectItemUI, bool> OnUserToggled;

        private GameObject _target;
        private string _displayName;

        public void Bind(string displayName, GameObject target)
        {
            _displayName = displayName;
            _target = target;

            if (nameText) nameText.text = string.IsNullOrWhiteSpace(displayName) ? "(no name)" : displayName;

            if (toggleButton)
            {
                toggleButton.onClick.RemoveAllListeners();
                toggleButton.onClick.AddListener(OnToggleClicked);
            }

            RefreshFromTarget();
        }

        private void OnToggleClicked()
        {
            if (_target == null)
            {
                MarkMissingTarget();
                return;
            }

            bool newState = !_target.activeSelf; // пользователь хочет переключить
            OnUserToggled?.Invoke(this, newState);
        }

        public void RefreshFromTarget()
        {
            if (_target == null)
            {
                MarkMissingTarget();
                return;
            }

            SetState(_target.activeSelf, silent:true);
        }

        public void SetState(bool active, bool silent = false)
        {
            if (stateImage)
            {
                stateImage.sprite = active ? onSprite : offSprite;
                stateImage.color  = active ? onColor  : offColor;
            }

            // Доп. визуал — можно слегка приглушать текст когда выключено
            if (nameText) nameText.alpha = active ? 1f : 0.6f;

            if (!silent && _target) _target.SetActive(active);
        }

        public void MarkMissingTarget()
        {
            if (stateImage) stateImage.color = missingColor;
            if (nameText) nameText.text = $"{_displayName}  <missing>";
        }
    }
}
