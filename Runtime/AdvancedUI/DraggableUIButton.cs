using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Rusleo.Utils.Runtime.AdvancedUI
{
    [RequireComponent(typeof(RectTransform))]
    public class DraggableUIButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Refs")]
        [SerializeField, Tooltip("Canvas where the button lives (if empty — will be found automatically)")]
        private Canvas canvas;

        [SerializeField, Tooltip("Bounds within which dragging is allowed (usually Canvas or parent)")]
        private RectTransform bounds;

        [Header("Behavior")]
        [SerializeField, Tooltip("Screen pixel threshold to distinguish between click and drag")]
        private float dragThreshold = 10f;

        [SerializeField, Tooltip("Enable boundary constraints during dragging")]
        private bool constrainToBounds = true;

        [Header("Events")]
        [SerializeField, Tooltip("Invoked if it was a click (without dragging)")]
        private UnityEvent onClicked;


        private RectTransform _rt;
        private Vector2 _pressPosScreen;
        private Vector2 _dragOffsetAnchored;
        private bool _dragging;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            if (bounds == null && canvas != null) bounds = canvas.transform as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressPosScreen = eventData.position;
            _dragging = false;

            // Точка указателя в системе координат родителя
            var parentRt = _rt.parent as RectTransform;
            if (parentRt == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, eventData.position, eventData.pressEventCamera, out var pointerLocalInParent);

            // разница между текущей позицией и точкой хвата (в anchor space)
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

            // Конвертируем экран → локальные координаты родителя
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRt, eventData.position, eventData.pressEventCamera, out var pointerLocalInParent);

            // Новая позиция = точка указателя + смещение, сохранённое при нажатии
            var targetAnchored = pointerLocalInParent + _dragOffsetAnchored;

            _rt.anchoredPosition = targetAnchored;

            if (constrainToBounds && bounds != null)
            {
                Utils.ClampToBounds(_rt, bounds);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // ничего особенного; клик обработаем в OnPointerClick
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // пусто
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Если сдвиг меньше порога — считаем это кликом
            var moved = (eventData.position - _pressPosScreen).magnitude;
            if (!_dragging || moved < dragThreshold)
            {
                onClicked?.Invoke();
            }
        }
    }
}