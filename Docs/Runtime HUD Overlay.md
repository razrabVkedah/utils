# 📊 Runtime HUD Overlay

**Назначение:**
Модуль лёгкого отладочного HUD, который отображается поверх сцены (без `Unity UI`), рендерится через `OnGUI` и автоматически обновляется. HUD показывает FPS, время кадра, использование памяти, CPU и любые другие метрики, которые ты зарегистрируешь.

---

## 🚀 Как работает

Модуль состоит из трёх ключевых компонентов:

1. **`HudService`**
   Центр управления. Хранит список провайдеров метрик, обновляет их и собирает текст в одну строку. Работает как синглтон.

2. **`HudOverlayRenderer`**
   MonoBehaviour, который в `OnGUI` рисует окошко HUD поверх всего. Берёт текст из `HudService`, учитывает настройки темы (позиция, размеры, цвета, шрифты).

3. **`HudTheme`**
   ScriptableObject с настройками оформления: угол экрана, отступы, фон, цвета текста, размеры в процентах от экрана, шрифт. Все изменения применяются на лету.

---

## 🛠 Настройка

1. В проекте уже есть `FpsHudBootstrap` (авто-инициализация). HUD поднимется автоматически в любой сцене.
2. Чтобы кастомизировать оформление:

    * Создай `HudTheme` через меню: **Assets → Create → Rusleo → Utils → HUD Theme**.
    * Укажи его в `FpsHudBootstrap` или подмени в рантайме через `HudService.Instance.Configure(theme, updatePeriod)`.
    * `FpsHudBootstrap` по дефолту берет `HudTheme` по `Assets/Resources/DefaultHudTheme.asset`. 
Если такого нет, то в runtime создастся новый. Также, вы можете самостоятельно создать `Assets/Resources/DefaultHudTheme.asset` 
и указать нужные вам настройки. Это оптимальный способ.

---

## 🎛 Поля `HudTheme`

* **visible** — включён ли HUD.
* **showHeader / headerText** — показывать ли заголовок и какой текст.
* **anchor** — угол экрана (TopLeft, TopRight, BottomLeft, BottomRight).
* **margin** — отступ от краёв.
* **padding** — внутренний отступ внутри окна.
* **font / fontSize** — шрифт и базовый размер текста.
* **textColor** — цвет текста.
* **backgroundColor** — цвет фона окна.
* **drawBorder / borderColor** — рамка и её цвет.
* **widthPercent / heightPercent** — ширина/высота HUD как доля экрана.

    * если `heightPercent = 0` → высота подстраивается под контент.
* **wordWrap** — перенос строк, чтобы текст не выходил за пределы окна.

---

## 🔌 Метрики

Метрика — класс, реализующий `IMetricsProvider`:

```csharp
public interface IMetricsProvider
{
    string Name { get; }
    bool Enabled { get; set; }
    void Update(float dt);          // вызывается каждый кадр
    void Emit(IStringBuilderTarget sb); // пишет строку в HUD
}
```

Примеры встроенных метрик:

* `FpsMetric` — сглаженный FPS.
* `FrameTimeMetric` — CPU/GPU время кадра (мс).
* `MemoryMetric` — Mono и Unity память.
* `GcMetric` — количество сборок мусора.
* `CpuUsageMetric` — загрузка CPU процесса.

---

## ➕ Как добавить свою метрику

1. Создай класс в `Runtime/Hud/Metrics/`:

```csharp
public sealed class BatteryMetric : IMetricsProvider
{
    public string Name => "Battery";
    public bool Enabled { get; set; } = true;
    private int _last;

    public void Update(float dt)
    {
        _last = (int)(SystemInfo.batteryLevel * 100f);
    }

    public void Emit(IStringBuilderTarget sb)
    {
        sb.Append("Battery: ");
        sb.Append(_last.ToString());
        sb.Append('%');
    }
}
```

2. Зарегистрируй метрику:

```csharp
HudService.Instance.Register(new BatteryMetric());
```

---

## 📐 Масштабирование текста

* HUD всегда занимает процент от экрана (`widthPercent`, `heightPercent`).
* Текст внутри автоматически переносится (`wordWrap = true`).
* При желании можно включить авто-подбор размера шрифта (shrink-to-fit), чтобы текст всегда помещался — это уже реализовано в `HudOverlayRenderer`.

---

## 📋 Пример использования

1. Включи сцену → HUD появится в левом верхнем углу.
2. В инспекторе темы задай:

    * Width Percent = 30
    * Height Percent = 20
    * Font Size = 14
    * Background Color = чёрный с альфой 0.5
3. Результат: прозрачное окошко с заголовком и метриками, которое всегда занимает 30% ширины и 20% высоты экрана.
