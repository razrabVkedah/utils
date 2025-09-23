Хорошо 👍
Сделаем полноценную **документацию для Rusleo.Utils.Logging** в стиле README.md: описание архитектуры, установки, настройки, и примеры кода с разными сценариями (от простого использования до продвинутых приёмов).

---

# 📘 Rusleo.Utils.Logging

`Rusleo.Utils.Logging` — это лёгкая и расширяемая система логирования для Unity.
Она позволяет логировать события в **консоль Unity**, **файл**, а также легко подключать другие приёмники (сеть, БД, Telegram-бот и т. п.).
Система поддерживает **контекстные логгеры** с метаданными, фильтры, форматтеры и **Editor-окно для просмотра логов**.

---

## ✨ Возможности

* ✅ Автоматическая инициализация (пользователю не нужен bootstrap-код).
* ✅ **Project Settings** для настройки логирования (уровень, приёмники, формат).
* ✅ **Unity Console Sink** (отправка в стандартную консоль).
* ✅ **File Sink** (с ротацией логов по размеру).
* ✅ **Log Viewer Window** (живой просмотр в Editor с фильтрацией).
* ✅ Статический API по аналогии с `Debug.Log`.
* ✅ Контекстные адаптеры `Logger` с owner, metadata и tags.
* ✅ Поддержка Unity-контекста (`MonoBehaviour`, `GameObject`).
* ✅ Возможность расширять sinks/formatters/filters.

---

## 📂 Структура

```
Rusleo.Utils.Logging
 ├── LogLevel.cs          // уровни логов
 ├── LogEvent.cs          // объект события
 ├── LogDispatcher.cs     // диспетчер
 ├── Logger.cs            // контекстный логгер
 ├── Log.cs               // статический API
 ├── ILogSink.cs          // приёмник (консоль, файл, сеть...)
 ├── ILogFormatter.cs     // форматтер
 ├── ILogFilter.cs        // фильтр
 ├── Sinks/
 ├── Formatters/
 ├── Filters/
 ├── LogConfig.cs         // ScriptableObject с настройками
 ├── LoggingBootstrap.cs  // авто-инициализация
 └── Editor/
     ├── LoggingSettingsProvider.cs  // Project Settings
     └── LogViewerWindow.cs          // Editor окно
```

---

## ⚙️ Настройка

1. В меню Unity открой:
   **Edit → Project Settings → Rusleo Utils → Logging**

2. Настрой:

    * Минимальный уровень логов (`Trace`, `Debug`, `Info`, …).
    * Приёмники (Console, File).
    * Файл, директорию и размер ротации.
    * Форматтер (по умолчанию текстовый).

3. Автоматически создастся ресурс:
   `Assets/Resources/RusleoLoggingSettings.asset`

4. Для просмотра логов в Editor:
   **Rusleo → Log Viewer**

---

## 🖥️ API

### 1) Статический фасад `Log`

Простое использование (аналог `Debug.Log`):

```csharp
using Rusleo.Utils.Logging;

public class Example : MonoBehaviour
{
    void Start()
    {
        Log.Info("Game started!");
        Log.Warn("Low FPS detected");
        Log.Error("Failed to load resource");
    }
}
```

### 2) Контекстный логгер `Logger`

Удобно, если нужен **owner** и метаданные:

```csharp
using Rusleo.Utils.Logging;

public class EnemyAI : MonoBehaviour
{
    private Logger _log;

    void Awake()
    {
        _log = this.GetLogger()
            .WithMeta("scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            .WithTags("enemy", "AI");
    }

    void OnEnable() => _log.Info("Spawned");
    void OnDisable() => _log.Warn("Despawned");
    void OnDeath(Exception ex) => _log.Error("Killed by player", ex);
}
```

Лог автоматически будет содержать:

```
[Info] {EnemyAI} Spawned | meta: scene=Level01 | tags: [enemy,AI]
```

### 3) Логирование с Unity-контекстом

Чтобы лог был кликабелен в Console:

```csharp
Log.Warn("Missing Rigidbody!", gameObject);
```

### 4) Метаданные и теги

```csharp
var logger = new Logger("InventorySystem")
    .WithMeta("playerId", 42)
    .WithTags("inventory", "gameplay");

logger.Info("Item added");
logger.Error("Inventory overflow");
```

### 5) Системное событие с CorrelationId

Для цепочки событий (например, загрузка уровня):

```csharp
var log = new Logger("LevelLoader") { CorrelationId = Guid.NewGuid().ToString() };

log.Info("Start loading assets");
log.Info("Scene activated");
log.Info("Loading complete");
```

Все логи будут иметь одинаковый `corr=…`.

---

## 🛠️ Расширение

### Новый приёмник (sink)

Например, отправка в HTTP API:

```csharp
public class HttpSink : ILogSink
{
    private readonly string _url;

    public HttpSink(string url) => _url = url;

    public void Emit(in LogEvent e)
    {
        var json = JsonUtility.ToJson(new { level = e.Level.ToString(), msg = e.Message });
        // здесь можно сделать UnityWebRequest.Post(_url, json)
    }

    public void Flush() { }
    public void Dispose() { }
}
```

Добавить sink можно динамически:

```csharp
LogDispatcher.Instance.AddSink(new HttpSink("https://example.com/logs"));
```

---

## 📊 Editor Log Viewer

В меню **Rusleo → Log Viewer** открывается окно:

* Фильтры по уровню, owner, тегам, тексту.
* Живой поток логов.
* Кнопки: Pause, Clear, Copy.
* Цветовая подсветка по уровню.

![](пример_скриншота_сюда.png)

---

## 🚀 Сценарии использования

### 🔹 Сценарий 1: Простая игра

Игрок делает инди-игру. Хочет только Console-логи.

* В Project Settings → включаем **Console Sink**, выключаем File Sink.
* Используем `Log.Info()` в коде.
* Всё автоматически работает, ничего писать вручную не нужно.

### 🔹 Сценарий 2: Мобильный билд + лог-файл

Нужно, чтобы логи писались на устройство:

* В Settings включаем **File Sink**.
* В Player билд логи сохраняются в `persistentDataPath/Logs/game.log`.
* При баг-репортах можно просить у игрока этот файл.

### 🔹 Сценарий 3: Продвинутая система с owner/tags

Большой проект с подсистемами (AI, Inventory, Network):

* Для каждой подсистемы создаём `Logger("Subsystem")`.
* В Viewer можно фильтровать только `owner=Network`.
* Можно включить в метаданные `playerId`, `sessionId`.

### 🔹 Сценарий 4: Подключение удалённого приёмника

Хочешь отправлять **Fatal** ошибки в Telegram/Slack:

* Реализуешь свой sink (например, через Webhook API).
* В `Emit` фильтруешь `if (e.Level == LogLevel.Fatal) …`.
* Подключаешь sink в `LogDispatcher`.

---

## ✅ Итог

* Пользователь **не пишет bootstrap** — всё работает “из коробки”.
* Настройки и просмотр логов — через **Project Settings** и **Log Viewer Window**.
* Есть как **простой API** (`Log.Info`) так и **контекстный** (`Logger`).
* Архитектура расширяема: можно добавлять свои sinks, форматтеры и фильтры.

---