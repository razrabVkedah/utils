# Gradient Studio (Rusleo.Utils)

Окно: **Rusleo → Gradient Studio**

## Возможности
- Редактор градиента с большим превью.
- Таблица ключей: HEX, Alpha (0–255), Позиция (0–100).
- **JSON Export/Import** (формат как в примере).
- Копирование/вставка JSON одним кликом.
- Сохранение пресетов как ScriptableObject + быстрый список.
- Запекание превью в PNG (256×1, 1024×1).

## Формат JSON
```json
{
  "mode": 0,
  "colorKeys": [
    {"color": {"r": 0.039, "g": 0.039, "b": 0.074, "a": 1}, "time": 0.0},
    {"color": {"r": 0.949, "g": 0.459, "b": 0.808, "a": 1}, "time": 1.0}
  ],
  "alphaKeys": [
    {"alpha": 0.0, "time": 0.0},
    {"alpha": 1.0, "time": 1.0}
  ]
}
