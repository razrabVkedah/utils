using UnityEngine;
using UnityEngine.EventSystems;

namespace Rusleo.Utils.Runtime.AdvancedUI
{
    [RequireComponent(typeof(RectTransform))]
    public class DraggableRectTransform : MonoBehaviour,
        IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Refs")]
        [SerializeField, Tooltip("Canvas, в котором живёт элемент (если пусто — найдётся автоматически)")]
        private Canvas canvas;

        [SerializeField, Tooltip("Границы, внутри которых разрешено перетаскивание (если пусто — родитель)")]
        private RectTransform bounds;

        [Header("Behavior")] [SerializeField, Tooltip("Если включено — элемент будет упираться в границы")]
        private bool containToBounds = true;

        private RectTransform _rt;
        private Vector2 _dragOffsetAnchored;
        private bool _dragging;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            if (bounds == null && _rt.parent is RectTransform parentRt) bounds = parentRt;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _dragging = false;

            var parentRt = _rt.parent as RectTransform;
            if (parentRt == null) return;

            // Точка указателя в локальных координатах родителя
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, eventData.position, eventData.pressEventCamera, out var pointerLocalInParent);

            // Разница между текущей позицией и точкой хвата (в anchor space)
            _dragOffsetAnchored = _rt.anchoredPosition - pointerLocalInParent;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var parentRt = _rt.parent as RectTransform;
            if (parentRt == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, eventData.position, eventData.pressEventCamera, out var pointerLocalInParent);

            var targetAnchored = pointerLocalInParent + _dragOffsetAnchored;
            _rt.anchoredPosition = targetAnchored;

            if (containToBounds && bounds != null)
            {
                // Ваш общий метод зажатия в границы
                Utils.ClampToBounds(_rt, bounds); // :contentReference[oaicite:2]{index=2}
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Ничего дополнительно не делаем
        }
    }
}