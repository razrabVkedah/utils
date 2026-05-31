# TODO — Rusleo Utils

Задачи перед публикацией пакета. Порядок примерный.

---

## Приоритет 1 — Критично перед публикацией

### TimeTracking
- Решить: оставить в пакете или вынести в отдельный пакет
- Если оставить:
  - Добавить UI toggle в Project Settings (сейчас настройки есть, но спрятаны)
  - Добавить явный opt-in при первом запуске (пользователь должен знать, что идёт запись)
  - Задокументировать: что пишется, куда, как отключить
  - Добавить в README

### Документация незадокументированных фич
- `HierarchySceneTooltip` — тултип + Ctrl+C копирует путь сцены (добавить в README)
- `ProjectFolderMiddleClickOpener` — middle-click открывает новое locked-окно (добавить в README или вырезать — использует reflection в internal Unity API, хрупко)

---

## Приоритет 2 — Качество кода

### AdvancedUI: два похожих класса
- `AdvancedButton` и `SimpleEventButton` перекрываются по функциональности
- Задокументировать разницу: когда использовать какой
- Или объединить в один настраиваемый компонент

### ProjectFolderMiddleClickOpener
- Использует reflection в `ProjectBrowser` (internal Unity API)
- Может сломаться при обновлении Unity
- Рассмотреть: добавить version guard / try-catch с fallback, или вырезать

### Комментарии на русском
- `DraggableRectTransform` — один комментарий на русском, привести к единому стилю

---

## Приоритет 3 — UX пакета (Welcome Window + Settings)

### Welcome Window
- Окно при первом импорте пакета (или через меню)
- Показывает: что есть в пакете, ссылки на документацию, кнопка "Open Settings"
- Запоминает "уже показано" через EditorPrefs чтобы не лезть при каждом запуске
- Нужно проработать: какие фичи показывать, в каком порядке, нужны ли скриншоты

### Settings Window
- Единое окно (Project Settings или отдельное) для всех настроек пакета
- Кандидаты для настроек:
  - TimeTracking on/off + путь до файла
  - HUD: дефолтные метрики, позиция
  - Logging: дефолтный уровень, sink-и
  - Hotkeys: сейчас есть своё окно — возможно интегрировать
- Нужно решить: одно окно Project Settings (удобнее) или отдельный EditorWindow

---

## Приоритет 4 — Samples

### Расширить существующие Samples~/InGame
- Проверить покрытие: есть примеры для HUD, Logging, UI, FpsLimiter
- Добавить примеры для: Splines, AdvancedButton (LongPress, DoubleClick), Inspector Buttons

### Новая категория Samples~/Editor (если нужна)
- Примеры для: Hotkeys регистрация своих, кастомные Log-sinks, кастомные HUD-метрики

---

## Мелочи

- Проверить `RequiredPlayModeValidator` — непонятно что делает, задокументировать или удалить
- Убедиться что asmdef правильно настроены (зависимости между Runtime/Editor)
- Проверить `package.json`: version, keywords, description для UPM-листинга
- Добавить CHANGELOG.md
