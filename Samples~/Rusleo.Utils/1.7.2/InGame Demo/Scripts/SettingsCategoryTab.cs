using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Rusleo.Graphics
{
    public class SettingsCategoryTab : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform categoryRect;
        [SerializeField] private bool isExpanded;
        [SerializeField] private float expandedHeight;
        [SerializeField] private float collapsedHeight;
        [SerializeField] private Transform content;
        public UnityEvent onExpanded;
        public UnityEvent onCollapsed;
        
        public void SetExpandedHeight(float height)
        {
            expandedHeight = height;
            Apply();
        }

        private void Awake()
        {
            Apply();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            isExpanded = !isExpanded;
            Apply();
        }

        private void Apply()
        {
            if (isExpanded) onExpanded?.Invoke();
            else onCollapsed?.Invoke();

            if (content != null) content.gameObject.SetActive(isExpanded);

            if (categoryRect == null) return;

            categoryRect.sizeDelta = new Vector2(categoryRect.sizeDelta.x, isExpanded ? expandedHeight : collapsedHeight);
        }
    }
}