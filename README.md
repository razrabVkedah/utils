# Rusleo Utils

Набор runtime и editor утилит для Unity 2022.3+.

## Установка

Через Unity Package Manager → *Add package from git URL...*:

```
https://github.com/razrabVkedah/Rusleo.Utils.git
```

Или вручную в `manifest.json`:

```json
{
  "dependencies": {
    "com.rusleo.utils": "https://github.com/razrabVkedah/Rusleo.Utils.git"
  }
}
```

---

## Runtime

### Logging System

Замена `Debug.Log` с поддержкой форматтеров, фильтров, sink-ов и контекста. Инициализируется автоматически.

```csharp
var log = new Logger("MySystem");
log.Info("Игрок подключился");
log.WithMeta("userId", id).Warn("Нет сохранения");
```

Встроенные sink-и: консоль Unity, файл с ротацией (JSON или текст).
Живой просмотр логов — через **Tools → Rusleo → Log Viewer**.

---

### HUD Overlay

Оверлей с метриками поверх игры. Не требует Canvas.

```csharp
HudService.Instance.Register(new FpsMetric());
HudService.Instance.Register(new MemoryMetric());
HudService.Instance.Register(new CpuUsageMetric());
```

Доступные метрики: FPS, время кадра, память, GC, CPU, render stats.
Размер и позиция — в процентах от экрана, текст масштабируется автоматически.

---

### Math — Splines & Interpolations

Сплайны с arc-length параметризацией (равномерный проход по длине):

```csharp
var spline = new Spline3D(SplineType.CatmullRom, points);
Vector3 pos = spline.Evaluate(0.5f); // t = 0..1 по длине дуги
```

Типы: `CatmullRom`, `CatmullRomCentripetal`, `Hermite`, `Lerp`.
Интерполяции: `Linear`, `CubicHermite`, `Smoothstep`, `Smootherstep`, `Cosine`.

---

### Advanced UI Components

**`AdvancedButton`** — кнопка с расширенными жестами:
- Long Press, Double Click, Hold & Repeat
- Drag с событиями Begin/Delta/End
- Условная активность и видимость (`EnableIf`, `VisibleIf`)

**`SimpleEventButton`** — лёгкий вариант для базовых сценариев (Click, PointerEnter/Exit, Drag).

**`DraggableRectTransform`** — перетаскивание UI-элемента с ограничением границ.

---

### Inspector Attributes

```csharp
[InspectorButton("Сгенерировать")]
private void Generate() { ... }

[Required]
[SerializeField] private Transform _target;

[InspectorButton("Опасно!", confirmMessage: "Уверен?")]
[EnableIf(nameof(_isReady))]
private void DangerousAction() { ... }
```

`[InspectorButton]` поддерживает: иконки, размер кнопки, диалог подтверждения, Undo/Redo, `async`/`IEnumerator` методы.
`[Required]` подсвечивает поле красным и показывает предупреждение если не назначено.

---

## Editor

### Hotkeys System

Единая система горячих клавиш с окном управления.

Встроенные хоткеи (все перепривязываемые):
- Создать материал / папку
- Открыть Explorer / Console
- Выделение сцен и объектов

Все хоткеи доступны через **Tools → Rusleo → Shortcut Viewer** даже без привязки к клавишам.

---

### Gradient Studio

Редактор градиентов с сохранением пресетов и JSON-импортом/экспортом.
**Tools → Rusleo → Gradient Studio**

---

### Editor Icon Browser

Поиск и просмотр всех встроенных иконок Unity (5000+). Копирование имени или готового сниппета для кода.
**Tools → Rusleo → Icon Browser**

---

### Log Viewer

Живая лента логов с фильтрацией по уровню, тегам и тексту. Лимит — 2000 записей.
**Tools → Rusleo → Log Viewer**

---

## Структура пакета

```
Rusleo.Utils
 ┣ Runtime
 ┃ ┣ Logging        # Log, Logger, сinks, форматтеры, фильтры
 ┃ ┣ Hud            # HudService, метрики, рендерер
 ┃ ┣ Math           # Spline1/2/3D, интерполяции
 ┃ ┣ AdvancedUI     # AdvancedButton, SimpleEventButton, Draggable
 ┃ ┣ Attributes     # InspectorButton, Required, EnableIf, VisibleIf
 ┃ ┗ Extensions     # MonoBehaviour extensions
 ┣ Editor
 ┃ ┣ Hotkeys        # ShortcutManager интеграция, окно управления
 ┃ ┣ InspectorButtons # GlobalButtonEditor (drawer для [InspectorButton])
 ┃ ┣ GradientStudio # Редактор градиентов
 ┃ ┗ Windows        # IconBrowser, LogViewer, HierarchyTooltip
 ┗ Samples~
   ┗ InGame         # Демо: HUD, Logging, UI, FPS Limiter
```

---

## Требования

- Unity 2022.3+
- `com.unity.mathematics` 1.2.6+

---

✦ Автор: [Rusleo](https://github.com/razrabVkedah)
