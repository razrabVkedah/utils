# Rusleo.Utils — Hotkeys Module (Editor)

Док — чтобы быстро подключить, понять архитектуру и **легко расширять** хоткеи.

---

## TL;DR

* Хоткеи регистрируются через **Unity Shortcut Manager** (`[Shortcut]`) — настраиваются в **Edit → Shortcuts…**.
* В меню **Rusleo/Hotkeys** есть окно:

    * поиск и список всех действий,
    * просмотр текущих биндингов,
    * **редактирование/сброс** (в Editable-профиле, напр. *Rusleo*),
    * кнопка перехода в системный менеджер шорткатов.
* Fallback-меню **без акселераторов** (только подсказка в названии) — чтобы не ловить конфликтов.

---

## Требования

* Unity **2022.3 LTS** (поддержка `UnityEditor.ShortcutManagement`).
* Папка `Editor/` (весь модуль — только для редактора).

---

## Установка (структура)

```
Rusleo.Utils/
└─ Editor/
   └─ Hotkeys/
      ├─ Core/
      │  ├─ HotkeyIds.cs              // строковые ID действий (единая точка правды)
      │  ├─ HotkeysCatalog.cs         // список действий для окна (название+описание)
      │  ├─ HotkeysDefaults.cs        // дефолтные биндинги пакета (для Reset)
      │  ├─ ShortcutBindingUtils.cs   // форматирование и сборка биндингов
      │  └─ HotkeysMenuFallback.cs    // пункты меню без реальных акселераторов
      ├─ Actions/
      │  ├─ CreateMaterialHotkey.cs
      │  ├─ SelectCurrentSceneHotkey.cs
      │  ├─ ShowInExplorerSelectedHotkey.cs
      │  ├─ CreateFolderHotkey.cs
      │  ├─ FocusConsoleHotkey.cs
      │  └─ RevealPersistentDataHotkey.cs
      └─ Windows/
         └─ HotkeysWindow.cs          // окно Rusleo/Hotkeys
```

---

## Как это работает

### 1) Shortcut Manager — основа

Каждое действие — отдельный статический класс в `Actions/*`, у метода стоит атрибут:

```csharp
[Shortcut(HotkeyIds.CreateMaterial, KeyCode.M, ShortcutModifiers.Shift | ShortcutModifiers.Action)]
public static void InvokeShortcut() => Execute();
```

* `Action` = **Ctrl** (Windows) / **Cmd** (macOS).
* Биндинг можно менять в **Edit → Shortcuts…** или прямо из нашего окна.

### 2) Окно Rusleo/Hotkeys

Доступ: **Rusleo → Hotkeys**. Возможности:

* Поиск по названию/описанию.
* Просмотр текущего биндинга.
* **Edit / Apply** — задать новый биндинг (KeyCode + Ctrl/Cmd + Shift + Alt).
* **Reset** — сброс к *дефолту пакета* (из `HotkeysDefaults.cs`).
* **Clear** — очистить биндинг.
* **Reset All (Rusleo)** — массовый сброс всех действий к дефолтам пакета.
* **Open Unity Shortcuts…** — открывает системное окно (Edit → Shortcuts…).

> Если активный профиль read-only (например, Default), окно предложит переключиться/создать профиль **Rusleo** и уже в нём применит ребинды.

### 3) Fallback-меню

`HotkeysMenuFallback.cs` дублирует действия в меню для мыши (discoverability).
**Важно:** пункты **без** `"%#..."` — подсказка комбинации указана текстом, чтобы **не дублировать** хоткей и не ловить конфликт.

---

## Встроенные действия (по умолчанию)

* **Create Material** — `Ctrl+Shift+M`
  Создаёт `Material` в активной папке Project. Шейдер выбирается под RP (URP/HDRP/Standard).

* **Select Current Scene** — `Ctrl+Shift+E`
  Выделяет ассет активной сцены в Project и пингует.

* **Show In Explorer (Selected)** — `Ctrl+Shift+X`
  Открывает системный проводник и выделяет ассет.

* **Create Folder** — `Ctrl+Shift+N`
  Создаёт новую папку в активной папке Project (с уникальным именем).

* **Focus Console** — `Ctrl+Shift+C`
  Открывает и фокусирует окно Console.

* **Reveal persistentDataPath** — `Ctrl+Shift+Alt+P`
  Открывает проводник на `Application.persistentDataPath`.

---

## Частые вопросы / Траблшутинг

### Конфликт шорткатов при нажатии (диалог “Shortcut Conflict”)

Убедись, что в `HotkeysMenuFallback.cs` **нет** строк вида `"[MenuItem(... %#e)]"`.
Они создают **второй** хоткей через меню. Оставь в `[MenuItem]` только текстовую подсказку.

### “Cannot rebind shortcut on read-only profile”

Активен read-only профиль (обычно **Default**). Открой окно **Rusleo/Hotkeys** и нажми **Use Rusleo Profile** / **Reset All (Rusleo)** — окно создаст/включит редактируемый профиль и применит бинды.

### Где редактировать хоткеи «по-системному»?

**Edit → Shortcuts…** (это стандартное окно Unity Shortcut Manager).

---

## Расширение: как добавить новый хоткей

### 1) Добавь строковый ID

`HotkeyIds.cs`:

```csharp
public const string DuplicateSelected = Group + "/Duplicate Selected (Smart)";
```

### 2) Добавь описание в каталог

`HotkeysCatalog.cs`:

```csharp
new Entry {
    Id = HotkeyIds.DuplicateSelected,
    DisplayName = "Duplicate Selected (Smart)",
    Description = "Дублирует выделенный ассет/объект с умным именованием/расположением."
},
```

### 3) Задай дефолт

`HotkeysDefaults.cs`:

```csharp
{ HotkeyIds.DuplicateSelected, Make(KeyCode.D, action:true, shift:true, alt:false) }, // Ctrl/Cmd+Shift+D
```

### 4) Реализуй действие

`Actions/DuplicateSelectedHotkey.cs`:

```csharp
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    public static class DuplicateSelectedHotkey
    {
        [Shortcut(HotkeyIds.DuplicateSelected, KeyCode.D, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        public static void Invoke() => Execute();

        public static void Execute()
        {
            // Пример: если выделен ассет — дублировать ассет; если GameObject в сцене — дублировать GO.
            var obj = Selection.activeObject;
            if (obj == null)
            {
                Debug.LogWarning("[Rusleo.Utils] Nothing selected to duplicate.");
                return;
            }

            // ассеты
            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path))
            {
                var copyPath = AssetDatabase.GenerateUniqueAssetPath(path);
                if (AssetDatabase.CopyAsset(path, copyPath))
                {
                    AssetDatabase.SaveAssets();
                    var copy = AssetDatabase.LoadAssetAtPath<Object>(copyPath);
                    Selection.activeObject = copy;
                    EditorGUIUtility.PingObject(copy);
                    Debug.Log($"[Rusleo.Utils] Duplicated asset: {copyPath}");
                }
                return;
            }

            // объекты сцены
            if (Selection.activeGameObject != null)
            {
                var dup = Object.Instantiate(Selection.activeGameObject, Selection.activeGameObject.transform.parent);
                dup.name = GameObjectUtility.GetUniqueNameForSibling(dup.transform.parent, Selection.activeGameObject.name);
                Undo.RegisterCreatedObjectUndo(dup, "Duplicate Selected");
                Selection.activeGameObject = dup;
                EditorGUIUtility.PingObject(dup);
                return;
            }

            Debug.LogWarning("[Rusleo.Utils] Unsupported selection.");
        }
    }
}
```

Готово — новый хоткей автоматически появится в окне Rusleo/Hotkeys.

---

## Рекомендации по стилю и качеству

* **Один файл — одно действие.** Имя файла = имя класса.
* Всегда давай **говорящее имя** (DisplayName) и **краткое описание** (Description) в `HotkeysCatalog`.
* ID формируй через `HotkeyIds` (единая точка правды), чтобы не было строковых «магических констант».
* В `HotkeysDefaults` придерживайся логичных букв (`N` — New/Folder, `C` — Console, `E` — s**E**ene, `M` — Material, и т.д.).
* Действия для ассетов: **используй активную папку** (`ProjectWindowUtil.GetActiveFolderPath()` через рефлексию) и **пингуй** созданный объект.
* При доступе к RP подбирай шейдер **динамически** (URP/HDRP/Standard fallback).

---

## API-шпаргалка

* Регистрация хоткея:
  `[Shortcut(string id, KeyCode key, ShortcutModifiers mods)]`
* Модификаторы:
  `ShortcutModifiers.Action` (Ctrl/Cmd), `Shift`, `Alt`
* Текущий биндинг:
  `ShortcutManager.instance.GetShortcutBinding(id)`
* Ребинд (только в Editable-профиле):
  `ShortcutManager.instance.RebindShortcut(id, binding)`
* Системное окно шорткатов:
  `EditorApplication.ExecuteMenuItem("Edit/Shortcuts...")`
* Открыть файл/папку в проводнике:
  `EditorUtility.RevealInFinder(path)`
* Выбрать ассет:
  `Selection.activeObject = obj; EditorGUIUtility.PingObject(obj);`

---

## Минимальный пример (заготовка для быстрого старта)

```csharp
// Editor/RusleoHotkeysQuick.cs
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class RusleoHotkeysQuick
{
    [Shortcut("Rusleo/Quick/Create Empty", KeyCode.E, ShortcutModifiers.Action | ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
    public static void CreateEmptyGo()
    {
        var go = new GameObject("Empty");
        Undo.RegisterCreatedObjectUndo(go, "Create Empty");
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
    }

    [MenuItem("Rusleo/Quick/Create Empty (Ctrl+Shift+Alt+E)")]
    private static void MenuCreateEmpty() => CreateEmptyGo();
}
```

> В меню — только подсказка в скобках; реальный хоткей сидит на `[Shortcut]`.

---
