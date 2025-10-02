using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rusleo.Utils.Runtime.AdvancedUI
{
    /// <summary>
    /// AdvancedButton: универсальная кнопка с детальными событиями ввода.
    /// Поддержка: PointerDown/Up, Click (с отменой при drag), LongPress, DoubleClick, HoldRepeat.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdvancedButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Basics")] [Tooltip("Если false — события ввода игнорируются.")] [SerializeField]
        private bool _interactable = true;

        [Tooltip("Клик засчитывается только если PointerUp произошёл внутри RectTransform.")] [SerializeField]
        private bool _requirePointerUpInside = true;

        [Tooltip("Порог начала drag в пикселях. Если 0 или меньше — берётся EventSystem.current.pixelDragThreshold.")]
        [SerializeField]
        private float _dragThresholdOverride = 0f;

        [Header("Long Press")] [SerializeField]
        private bool _enableLongPress = true;

        [Tooltip("Сколько удерживать, чтобы сработал LongPress (сек).")] [SerializeField]
        private float _longPressTime = 0.45f;

        [Header("Hold Repeat (авто-нажатие при удержании)")] [SerializeField]
        private bool _enableHoldRepeat = false;

        [Tooltip("Задержка перед первым повтором (сек).")] [SerializeField]
        private float _holdRepeatInitialDelay = 0.5f;

        [Tooltip("Интервал между повторами (сек).")] [SerializeField]
        private float _holdRepeatInterval = 0.1f;

        [Header("Double Click")] [SerializeField]
        private bool _enableDoubleClick = false;

        [Tooltip("Интервал между кликами для дабл-клика (сек).")] [SerializeField]
        private float _doubleClickInterval = 0.3f;

        [Header("Unity Events")] public UnityEvent OnPressed; // PointerDown
        public UnityEvent OnReleased; // PointerUp (всегда, даже если вышли за границы)
        public UnityEvent OnClicked; // Click (учитывает флаги и drag)
        public UnityEvent OnClickCanceled; // Отмена клика из-за drag/ухода указателя/не Inside
        public UnityEvent OnLongPress; // Сработает один раз при удержании
        public UnityEvent OnHoldRepeat; // Будет тикать при удержании (если включено)
        public UnityEvent OnDoubleClick; // Двойной клик
        public UnityEvent OnDragStart;
        public UnityEvent OnDragging;
        public UnityEvent OnDragEnd;
        public UnityEvent OnPointerEnterUI;
        public UnityEvent OnPointerExitUI;

        // --- runtime state ---
        RectTransform _rect;
        Canvas _rootCanvas;
        bool _isPointerInside;
        bool _isPressed;
        bool _isDragging;
        int _pressedPointerId = -999;
        Vector2 _pressScreenPos;
        float _pressTime;
        bool _longPressFired;

        float _nextRepeatTime;
        float _lastClickTime = -999f;

        // Для Interactable проверим CanvasGroup и optional Graphic
        Graphic _graphic;
        bool CanvasGroupBlocks => !IsParentCanvasGroupInteractable();

        public bool Interactable
        {
            get => _interactable && !CanvasGroupBlocks;
            set
            {
                _interactable = value;
                UpdateGraphicState();
            }
        }

        void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _rootCanvas = GetComponentInParent<Canvas>();
            _graphic = GetComponent<Graphic>();
            UpdateGraphicState();
        }

        void Update()
        {
            if (!Interactable) return;

            // Лонг-пресс
            if (_enableLongPress && _isPressed && !_isDragging && !_longPressFired)
            {
                if (Time.unscaledTime - _pressTime >= Mathf.Max(0.01f, _longPressTime))
                {
                    _longPressFired = true;
                    SafeInvoke(OnLongPress);
                }
            }

            // Холд-репит
            if (_enableHoldRepeat && _isPressed && !_isDragging)
            {
                if (Time.unscaledTime >= _nextRepeatTime)
                {
                    // Первый запуск планируется в OnPointerDown, потом тик здесь
                    _nextRepeatTime = Time.unscaledTime + Mathf.Max(0.01f, _holdRepeatInterval);
                    SafeInvoke(OnHoldRepeat);
                }
            }
        }

        // --- Pointer hover ---
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!Interactable) return;
            _isPointerInside = true;
            SafeInvoke(OnPointerEnterUI);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Interactable) return;
            _isPointerInside = false;
            SafeInvoke(OnPointerExitUI);
        }

        // --- Pointer press lifecycle ---
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (_isPressed) return; // игнорируем второй палец/мышь, чтобы не плодить состояния

            _isPressed = true;
            _isDragging = false;
            _longPressFired = false;
            _pressedPointerId = eventData.pointerId;
            _pressScreenPos = eventData.position;
            _pressTime = Time.unscaledTime;

            if (_enableHoldRepeat)
            {
                // планируем первый повтор
                _nextRepeatTime = Time.unscaledTime + Mathf.Max(0.01f, _holdRepeatInitialDelay);
            }

            SafeInvoke(OnPressed);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (!_isPressed || eventData.pointerId != _pressedPointerId) return;

            if (!_isDragging && MovedBeyondThreshold(eventData))
            {
                _isDragging = true;
                SafeInvoke(OnDragStart);
                // как только начался drag — потенциальный click считается «под вопросом»
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (_isDragging && eventData.pointerId == _pressedPointerId)
            {
                SafeInvoke(OnDragging);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (_isDragging && eventData.pointerId == _pressedPointerId)
            {
                SafeInvoke(OnDragEnd);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Interactable) return;
            if (!_isPressed || eventData.pointerId != _pressedPointerId) return;

            _isPressed = false;

            // Если был drag — это отмена клика
            if (_isDragging)
            {
                _isDragging = false;
                SafeInvoke(OnDragEnd);
                SafeInvoke(OnReleased);
                SafeInvoke(OnClickCanceled);
                ResetPressState();
                return;
            }

            // Если требуется отпускание внутри — проверяем
            if (_requirePointerUpInside && !IsPointerInside(eventData))
            {
                SafeInvoke(OnReleased);
                SafeInvoke(OnClickCanceled);
                ResetPressState();
                return;
            }

            SafeInvoke(OnReleased);
            // Клик засчитывается здесь (а не в OnPointerClick), чтобы соблюсти наши правила
            FireClickWithDoubleSupport();

            ResetPressState();
        }

        // Мы сами решаем, когда клик засчитывать, но оставим совместимость с IPointerClickHandler
        public void OnPointerClick(PointerEventData eventData)
        {
            // no-op: обработку клика делаем в OnPointerUp согласно правилам
        }

        // --- helpers ---
        void FireClickWithDoubleSupport()
        {
            if (_enableDoubleClick)
            {
                float now = Time.unscaledTime;
                if (now - _lastClickTime <= Mathf.Max(0.05f, _doubleClickInterval))
                {
                    _lastClickTime = -999f;
                    SafeInvoke(OnDoubleClick);
                    return; // двойной клик вместо одиночного
                }

                _lastClickTime = now;
            }

            SafeInvoke(OnClicked);
        }

        bool IsPointerInside(PointerEventData eventData)
        {
            // Если мы уже отслеживаем _isPointerInside (Enter/Exit) — можно использовать его,
            // но надёжнее пробить прямоугольник по текущим координатам.
            return RectTransformUtility.RectangleContainsScreenPoint(
                _rect, eventData.position, eventData.pressEventCamera);
        }

        bool MovedBeyondThreshold(PointerEventData eventData)
        {
            float threshold = _dragThresholdOverride > 0f
                ? _dragThresholdOverride
                : (EventSystem.current ? EventSystem.current.pixelDragThreshold : 5f);

            return (eventData.position - _pressScreenPos).sqrMagnitude >= threshold * threshold;
        }

        void ResetPressState()
        {
            _isPressed = false;
            _isDragging = false;
            _longPressFired = false;
            _pressedPointerId = -999;
        }

        void SafeInvoke(UnityEvent evt)
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

        bool IsParentCanvasGroupInteractable()
        {
            // Если в родителях есть CanvasGroup с interactable = false или blocksRaycasts = false — считаем неинтерактивным
            var list = ListPool<CanvasGroup>.Get();
            bool blocked = false;
            try
            {
                GetComponentsInParent(false, list);
                for (int i = 0; i < list.Count; i++)
                {
                    var cg = list[i];
                    if (!cg.interactable || !cg.blocksRaycasts)
                    {
                        blocked = true;
                        break;
                    }
                }
            }
            finally
            {
                ListPool<CanvasGroup>.Release(list);
            }

            return blocked;
        }

        void UpdateGraphicState()
        {
            if (_graphic == null) return;
            _graphic.raycastTarget = _interactable && !CanvasGroupBlocks;
            // Тут можно добавить визуальные состояния (цвет, альфа и т.п.)
        }

        // --- маленький внутренний пул листов, чтобы не аллоцировать ---
        static class ListPool<T>
        {
            static readonly System.Collections.Generic.Stack<System.Collections.Generic.List<T>> _pool = new();

            public static System.Collections.Generic.List<T> Get()
            {
                return _pool.Count > 0 ? _pool.Pop() : new System.Collections.Generic.List<T>(8);
            }

            public static void Release(System.Collections.Generic.List<T> list)
            {
                list.Clear();
                _pool.Push(list);
            }
        }
    }
}