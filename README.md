# Rusleo Utils

Набор утилит для Unity, собранных под общий пакет.
Содержит полезные инструменты для разработки: логгирование, горячие клавиши и др.

## 🚀 Установка

Добавьте в `manifest.json` вашего проекта:

```json
{
  "dependencies": {
    "com.rusleo.utils": "https://github.com/razrabVkedah/Rusleo.Utils.git#1.0.0"
  }
}
```

или через Unity Package Manager → *Add package from git URL...*

```
https://github.com/razrabVkedah/Rusleo.Utils.git
```

## 📦 Возможности

* **Logging System** — удобный логгер вместо `Debug.Log` с кастомными форматтерами.
* **Hotkeys System** — набор горячих клавиш (с fallback в меню Unity).
* **Shortcut Viewer** — окно в Editor для просмотра всех хоткеев.

## 🔑 Горячие клавиши

Пример (Windows):

* `Ctrl + Shift + M` — создать материал
* `Ctrl + Shift + E` — выделить текущую сцену

## 🛠 Расширение

Хотите добавить свой хоткей? Просто создайте класс в Editor:

```csharp
using Rusleo.Utils.Editor.Hotkeys.Core;

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

---

✦ Автор: [Rusleo](https://github.com/razrabVkedah)
