using System;
using UnityEngine;

namespace Rusleo.Graphics
{
    [Serializable]
    public class WatchEntry
    {
        [Tooltip("Человекочитаемое имя в UI (если пусто — возьмём имя GameObject).")]
        public string label;

        [Tooltip("Объект, которым будем управлять.")]
        public GameObject target;

        [Tooltip(
            "Принудительно выставить стартовое состояние при запуске менеджера? Оставь 'Inherit' чтобы не трогать.")]
        public RuntimeQuickToggleManager.StartState startState = RuntimeQuickToggleManager.StartState.Inherit;
    }
}