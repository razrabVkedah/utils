# 📖 Rusleo.Utils — Inspector Buttons

### Что это?

Атрибуты, которые позволяют **добавлять кнопки прямо в инспектор** для любых методов MonoBehaviour и ScriptableObject.
Больше не нужно писать кастомный `Editor` для каждой мелочи.

---

## 🚀 Быстрый старт

```csharp
using UnityEngine;
using Rusleo.Utils.Runtime.Attributes;

public class Example : MonoBehaviour
{
    [InspectorButton("Сбросить позицию", confirm: "Точно сбросить?")]
    private void ResetPosition()
    {
        transform.position = Vector3.zero;
        Debug.Log("Position reset");
    }
}
```

В инспекторе появится кнопка **"Сбросить позицию"**.
При нажатии метод вызовется, объект отметится как изменённый (Undo работает), для ScriptableObject изменения сохранятся автоматически.

---

## ⚡ Атрибуты

### 🔹 `InspectorButton`

Главный атрибут. Вешается на метод без параметров.

```csharp
[InspectorButton("Кнопка", 
                 mode: ButtonMode.EditorOnly, 
                 order: 0, 
                 confirm: "Are you sure?", 
                 size: ButtonSize.Large, 
                 icon: "d_PlayButton", 
                 tooltip: "Запустить что-то")]
private void DoSomething() { ... }
```

Параметры:

* **label** — подпись на кнопке (по умолчанию имя метода).
* **mode** — когда активна:

    * `Always` — всегда
    * `EditorOnly` — только в режиме редактирования
    * `PlayModeOnly` — только в Play Mode
* **order** — порядок сортировки кнопок.
* **confirm** — текст диалога подтверждения перед запуском.
* **size** — `Small`, `Normal`, `Large`.
* **icon** — имя встроенной editor-иконки Unity (например `d_PlayButton`, `console.infoicon`).
* **tooltip** — всплывающая подсказка.

---

### 🔹 `EnableIf`

Условно **включает/выключает** кнопку.

```csharp
public bool canRun = true;

[InspectorButton("Run")]
[EnableIf(nameof(canRun))]
private void RunAction() { ... }
```

Проверяет поле, свойство или метод без параметров, возвращающий `bool`.

---

### 🔹 `VisibleIf`

Условно **показывает/скрывает** кнопку.

```csharp
private bool IsReady() => Application.isPlaying;

[InspectorButton("Only in PlayMode")]
[VisibleIf(nameof(IsReady))]
private void PlayOnlyAction() { ... }
```

---

### 🔹 Автосохранение и Undo

* Все вызовы методов регистрируются в **Undo/Redo**.
* `MonoBehaviour` → сцена помечается «грязной».
* `ScriptableObject` → автоматически сохраняется в `AssetDatabase`.

---

## 🎨 Примеры

### Мини-панель действий

```csharp
[InspectorButton("Log", icon:"console.infoicon")]
private void PrintLog() => Debug.Log("Hi!");

[InspectorButton("Clear", icon:"TreeEditor.Trash")]
private void ClearLog() => Debug.ClearDeveloperConsole();
```

### Сценарий с подтверждением

```csharp
[InspectorButton("Удалить всё", confirm:"Точно удалить ВСЁ?")]
private void DestroyAll() { ... }
```

### Кнопка только в Play Mode

```csharp
[InspectorButton("Jump", ButtonMode.PlayModeOnly)]
private void Jump() { ... }
```

---

## 💡 Советы

* Если хочешь **группировать кнопки**, используй `order` (отсортируются).
* Для красивых иконок смотри список встроенных (`PlayButton`, `d_PlayButton On`, `console.erroricon`, и т.п.).
* Если метод возвращает `IEnumerator`, в Play Mode он будет запущен как **корутина**.
* Если метод возвращает `Task`, он будет запущен в фоне (fire-and-forget).

---

## ✅ Итог

С помощью `InspectorButton`, `EnableIf` и `VisibleIf` можно быстро делать **маленькие тулзы прямо в инспекторе**: кнопки-шорткаты, отладочные действия, админ-панели для ScriptableObject.
