# Rusleo Utils

Набор утилит и инструментов для Unity, собранных в единый пакет.
Содержит как **runtime-модули** для игр, так и **editor-утилиты** для ускорения разработки.

## 🚀 Установка

Добавьте в `manifest.json` вашего проекта:

```json
{
  "dependencies": {
    "com.rusleo.utils": "https://github.com/razrabVkedah/Rusleo.Utils.git#1.0.0"
  }
}
```

или через Unity Package Manager → *Add package from git URL...*:

```
https://github.com/razrabVkedah/Rusleo.Utils.git
```

---

## 📦 Возможности

### 🔹 Runtime

* **Logging System** — удобный логгер вместо `Debug.Log` с форматтерами и категориями.
* **Hotkeys Runtime Fallback** — единый слой для регистрации хоткеев, работает и без Editor.
* **HUD System** — готовое оверлей-HUD с метриками:

  * FPS, время кадра
  * Использование памяти
  * GC и CPU
  * Автоадаптация размеров под экран
* **StringBuilderTarget** — безопасный и быстрый таргет для работы со строками без лишних аллокаций.

### 🔹 Editor

* **Hotkeys System** — единый набор горячих клавиш (с fallback в меню Unity).
* **Shortcut Viewer** — окно для просмотра и поиска всех хоткеев.
* **Gradient Window** — редактор градиентов с JSON-импортом/экспортом.
* **PlayerPrefs Visualizer** — просмотр и редактирование `PlayerPrefs`.
* **Editor Icon Browser** — удобный просмотр встроенных иконок Unity.

---

## 🔑 Примеры

### Горячие клавиши

```csharp
using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor;

internal static class MyHotkeys
{
    [MenuItem("Rusleo/Hotkeys/Do Something %#d")]
    private static void DoSomething()
    {
        // Ваш код
    }
}
```

Хоткей появится в меню и будет работать сразу.
В окне **Shortcut Viewer** можно увидеть все доступные комбинации.

---

### HUD Overlay

```csharp
// Автоматически создаётся при старте сцены
// Настройка через HudTheme (шрифт, цвет, масштаб)
HudService.Instance.Register(new FpsMetric());
HudService.Instance.Register(new MemoryMetric());
```

HUD подстраивается под экран, поддерживает проценты ширины/высоты и авто-resize текста.

---

## 🗂 Структура пакета

```
Rusleo.Utils
 ┣ Runtime
 ┃ ┣ Hud
 ┃ ┣ Logging
 ┃ ┗ Core
 ┣ Editor
 ┃ ┣ Hotkeys
 ┃ ┣ Windows
 ┃ ┣ PlayerPrefs
 ┃ ┗ Gradient
 ┗ Tests
```

---

## 📌 Планы

* Расширение HUD (свои метрики, кастомные панели).
* Единый Settings-Asset для глобальной конфигурации.
* Дополнительные Editor-инструменты.

---

✦ Автор: [Rusleo](https://github.com/razrabVkedah)
