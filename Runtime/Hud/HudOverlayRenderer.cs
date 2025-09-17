using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud
{
    [DefaultExecutionOrder(int.MaxValue)]
    public sealed class HudOverlayRenderer : MonoBehaviour
    {
        private GUIStyle _textStyle;
        private GUIStyle _boxStyle;

        private void EnsureStyles(HudTheme t)
        {
            _textStyle ??= new GUIStyle(GUI.skin.label);
            _textStyle.font = t.font;
            _textStyle.wordWrap = t.wordWrap;
            _textStyle.richText = false;
            _textStyle.normal.textColor = t.textColor;

            _boxStyle ??= new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = t.GetSolidTex(Color.white); // белая 1x1
            _boxStyle.border = new RectOffset(1, 1, 1, 1);
            _boxStyle.margin = new RectOffset(0, 0, 0, 0);
            _boxStyle.padding = new RectOffset(t.padding, t.padding, t.padding, t.padding);
        }

        private static int FindFittingFontSize(GUIStyle style, GUIContent content, float textWidth, float textHeight,
            int minSize, int maxSize)
        {
            // быстрый проход сверху вниз; без аллокаций
            for (var fs = maxSize; fs >= minSize; fs--)
            {
                style.fontSize = fs;
                var needed = style.CalcHeight(content, textWidth);
                if (needed <= textHeight) return fs;
            }

            return minSize;
        }

        private void OnGUI()
        {
            var theme = HudService.Instance.Theme;
            if (!theme || !theme.visible) return;

            EnsureStyles(theme);

            var text = HudService.Instance.GetText();
            if (string.IsNullOrEmpty(text)) return;

            GUI.depth = int.MinValue;

            // --- Размер окна в процентах ---
            var width = Mathf.Clamp(Screen.width * (theme.widthPercent / 100f), 64f, Screen.width);
            var height = Mathf.Clamp(Screen.height * (theme.heightPercent / 100f), 32f, Screen.height);

            // Позиция по якорю
            var x = theme.anchor switch
            {
                HudTheme.AnchorCorner.TopLeft => theme.margin.x,
                HudTheme.AnchorCorner.TopRight => Screen.width - width - theme.margin.x,
                HudTheme.AnchorCorner.BottomLeft => theme.margin.x,
                HudTheme.AnchorCorner.BottomRight => Screen.width - width - theme.margin.x,
                _ => theme.margin.x
            };
            var y = theme.anchor switch
            {
                HudTheme.AnchorCorner.TopLeft => theme.margin.y,
                HudTheme.AnchorCorner.TopRight => theme.margin.y,
                HudTheme.AnchorCorner.BottomLeft => Screen.height - height - theme.margin.y,
                HudTheme.AnchorCorner.BottomRight => Screen.height - height - theme.margin.y,
                _ => theme.margin.y
            };

            var rect = new Rect(x, y, width, height);

            // Бордер
            if (theme.drawBorder)
            {
                var prev = GUI.color;
                GUI.color = theme.borderColor;
                GUI.DrawTexture(new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.height + 2), theme.white1X1);
                GUI.color = prev;
            }

            // Фон
            {
                var prev = GUI.color;
                GUI.color = theme.backgroundColor;
                GUI.Box(rect, GUIContent.none, _boxStyle);
                GUI.color = prev;
            }

            // --- Текст с авто-размером ---
            var textRect = new Rect(
                rect.x + theme.padding,
                rect.y + theme.padding,
                rect.width - 2 * theme.padding,
                rect.height - 2 * theme.padding
            );

            var content = new GUIContent(text);

            // подбираем размер шрифта, чтобы весь текст поместился в textRect
            var fs = FindFittingFontSize(_textStyle, content, textRect.width, textRect.height,
                theme.minFontSize, theme.maxFontSize);


            _textStyle.fontSize = fs;
            GUI.Label(textRect, content, _textStyle);
        }
    }
}