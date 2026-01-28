using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rusleo.Utils.Runtime.AdvancedUI
{
    /// <summary>
    /// SimpleEventButton: лёгкая кнопка-ретранслятор UI событий в UnityEvent.
    /// Поддержка: PointerEnter/Exit, PointerDown/Up, Click (опционально отмена при drag),
    /// DragBegin/Drag/DragEnd.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SimpleEventButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Basics")] [SerializeField] private bool interactable = true;

        [Tooltip("Клик засчитывается только если PointerUp произошёл внутри RectTransform.")] [SerializeField]
        private bool requirePointerUpInside = true;

        [Tooltip("Если true — клик отменяется, если был drag (по порогу).")] [SerializeField]
        private bool cancelClickOnDrag = true;


        [Tooltip("Порог начала drag в пикселях. Если <= 0 — берётся EventSystem.current.pixelDragThreshold.")]
        [SerializeField]
        private float dragThresholdOverride;

        [Header("Unity Events")] public UnityEvent onPointerEnterUI;
        public UnityEvent onPointerExitUI;
        public UnityEvent onPointerDownUI;
        public UnityEvent onPointerUpUI;
        public UnityEvent onClickUI;
        public UnityEvent onClickCanceledUI;
        public UnityEvent onBeginDragUI;
        public UnityEvent onDragUI;
        public UnityEvent onEndDragUI;

        private RectTransform _rect;
        private Graphic _graphic;

        private bool _isPressed;
        private bool _isDragging;
        private int _pressedPointerId = -999;
        private Vector2 _pressScreenPos;

        public bool Interactable
        {
            get => interactable && IsRaycastAllowedByCanvasGroups();
            set
            {
                interactable = value;
                UpdateGraphicState();
            }
        }

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _graphic = GetComponent<Graphic>();
            UpdateGraphicState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Interactable) return;
            SafeInvoke(onPointerEnterUI);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Interactable) return;
            SafeInvoke(onPointerExitUI);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (_isPressed) return;

            _isPressed = true;
            _isDragging = false;
            _pressedPointerId = eventData.pointerId;
            _pressScreenPos = eventData.position;

            SafeInvoke(onPointerDownUI);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (!_isPressed || eventData.pointerId != _pressedPointerId) return;

            _isPressed = false;

            SafeInvoke(onPointerUpUI);

            var cancelBecauseDrag = cancelClickOnDrag && _isDragging;
            var cancelBecauseOutside = requirePointerUpInside && !IsPointerInside(eventData);

            if (cancelBecauseDrag || cancelBecauseOutside)
            {
                SafeInvoke(onClickCanceledUI);
                ResetPressState();
                return;
            }

            // Клик засчитываем на Up, а IPointerClickHandler оставляем как совместимость.
            SafeInvoke(onClickUI);

            ResetPressState();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // no-op: кликаем в OnPointerUp, чтобы учитывать requirePointerUpInside/cancelClickOnDrag
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (!_isPressed || eventData.pointerId != _pressedPointerId) return;

            if (!_isDragging && MovedBeyondThreshold(eventData))
            {
                _isDragging = true;
                SafeInvoke(onBeginDragUI);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (!_isDragging || eventData.pointerId != _pressedPointerId) return;

            SafeInvoke(onDragUI);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (!_isDragging || eventData.pointerId != _pressedPointerId) return;

            SafeInvoke(onEndDragUI);
        }

        private bool IsPointerInside(PointerEventData eventData)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(
                _rect, eventData.position, eventData.pressEventCamera);
        }

        private bool MovedBeyondThreshold(PointerEventData eventData)
        {
            var threshold = dragThresholdOverride > 0f
                ? dragThresholdOverride
                : (EventSystem.current ? EventSystem.current.pixelDragThreshold : 5f);

            return (eventData.position - _pressScreenPos).sqrMagnitude >= threshold * threshold;
        }

        private void ResetPressState()
        {
            _isPressed = false;
            _isDragging = false;
            _pressedPointerId = -999;
        }

        private void SafeInvoke(UnityEvent evt)
        {
            try
            {
                evt?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private bool IsRaycastAllowedByCanvasGroups()
        {
            var cg = GetComponentsInParent<CanvasGroup>(false);
            return cg.All(c => c.interactable && c.blocksRaycasts);
        }

        private void UpdateGraphicState()
        {
            if (_graphic == null) return;
            _graphic.raycastTarget = interactable && IsRaycastAllowedByCanvasGroups();
        }
    }
}