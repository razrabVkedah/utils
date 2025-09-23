using UnityEngine;

namespace Rusleo.Utils.Runtime.AdvancedUI
{
    public static class Utils
    {
        /// <summary>
        /// Ограничивает перемещение внутри прямоугольника bounds.
        /// Вынесено отдельным методом — можешь подменить своей реализацией при необходимости.
        /// </summary>
        /// <param name="moving">Перетаскиваемый RectTransform</param>
        /// <param name="boundsRect">RectTransform границ (обычно Canvas или родитель)</param>
        public static void ClampToBounds(RectTransform moving, RectTransform boundsRect)
        {
            // Предпочтительно, чтобы boundsRect был родителем moving — тогда всё в одной системе координат.
            var parentRt = moving.parent as RectTransform;
            if (parentRt == null) return;

            // Если bounds не является родителем — считаем, что их pivot и anchors настроены «по-умолчанию».
            // В большинстве UI кейсов bounds = parent, это самый стабильный вариант.
            if (boundsRect != parentRt)
            {
                // На случай, если всё же другой контейнер — просто выходим, чтобы не вносить неожиданные сдвиги.
                // Хочешь — допиши сюда конвертацию в координаты bounds.
                return;
            }

            var pos = moving.anchoredPosition;
            var halfSize = moving.rect.size * 0.5f;
            var boundsHalf = boundsRect.rect.size * 0.5f;

            // Учитываем pivot — чтоб элемент был полностью внутри
            var left = -boundsHalf.x + halfSize.x - moving.rect.width * (moving.pivot.x - 0.5f);
            var right = boundsHalf.x - halfSize.x + moving.rect.width * (moving.pivot.x - 0.5f);
            var bottom = -boundsHalf.y + halfSize.y - moving.rect.height * (moving.pivot.y - 0.5f);
            var top = boundsHalf.y - halfSize.y + moving.rect.height * (moving.pivot.y - 0.5f);

            pos.x = Mathf.Clamp(pos.x, left, right);
            pos.y = Mathf.Clamp(pos.y, bottom, top);

            moving.anchoredPosition = pos;
        }
    }
}