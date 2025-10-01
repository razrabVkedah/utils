using UnityEngine;
using System.Collections;

public class DevTimeController : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private bool stablePhysics = true;         // Компенсировать fixedDeltaTime при изменении timeScale
    [SerializeField] private bool muteAudioOnPause = true;      // Глушить звук при паузе
    [SerializeField] private float minNonZeroTimeScale = 0.01f; // Минимум для слайдера (0 делаем отдельной кнопкой Pause)
    [SerializeField] private float maxTimeScale = 3f;           // Верхняя граница слайдера
    [SerializeField] private float defaultBaseFixedDelta = 0.02f; // Дефолтная база FDT (50Гц), если вдруг Time.fixedDeltaTime модифицирован

    // Приватные поля
    private float _baseFixedDeltaTime; // База для компенсации (фиксируем при Awake)
    private bool _isPaused;
    private Coroutine _stepRoutine;

    // === ЖИЗНЕННЫЙ ЦИКЛ ===
    private void Awake()
    {
        // Фиксируем базу (что считаете "нормой" физики без замедления)
        _baseFixedDeltaTime = Time.fixedDeltaTime > 0f ? Time.fixedDeltaTime : defaultBaseFixedDelta;
        ApplyStablePhysics(Time.timeScale);
        UpdatePauseFlags();
    }

    private void OnDisable()
    {
        // На всякий случай — выход из паузы при отключении
        if (_isPaused) Resume();
    }

    // === ПУБЛИЧНОЕ API ДЛЯ UI ===

    // Пресеты TimeScale
    public void SetTimeScale_0()  => Pause();
    public void SetTimeScale_025() => SetTimeScale(0.25f);
    public void SetTimeScale_050() => SetTimeScale(0.5f);
    public void SetTimeScale_075() => SetTimeScale(0.75f);
    public void SetTimeScale_100() => SetTimeScale(1f);
    public void SetTimeScale_125() => SetTimeScale(1.25f);
    public void SetTimeScale_150() => SetTimeScale(1.5f);
    public void SetTimeScale_200() => SetTimeScale(2f);

    /// <summary>
    /// Установить Time.timeScale (0 — только через Pause()).
    /// Рекомендуется вешать на слайдер/инпут (диапазон 0..maxTimeScale).
    /// </summary>
    public void SetTimeScale(float value)
    {
        // Если пользователь тянет слайдер в ноль — не даём "почти ноль", либо явно жми Pause
        var clamped = Mathf.Clamp(value, minNonZeroTimeScale, maxTimeScale);
        Time.timeScale = clamped;
        _isPaused = false;

        ApplyStablePhysics(clamped);
        UpdatePauseFlags();
    }

    /// <summary>
    /// Пауза (безопасная): timeScale = 0, (опц.) mute аудио.
    /// </summary>
    public void Pause()
    {
        Time.timeScale = 0f;
        _isPaused = true;

        // На паузе физика всё равно не тикает, но базу не трогаем
        UpdatePauseFlags();
    }

    /// <summary>
    /// Возврат к 1x и нормальному FDT.
    /// </summary>
    public void Resume()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        ApplyStablePhysics(1f);
        UpdatePauseFlags();
    }

    /// <summary>
    /// Полный сброс к дефолтам: 1x и базовый fixedDeltaTime.
    /// </summary>
    public void ResetDefaults()
    {
        stablePhysics = true;
        Resume();
        Time.fixedDeltaTime = _baseFixedDeltaTime;
    }

    /// <summary>
    /// Включить/выключить компенсацию fixedDeltaTime относительно timeScale.
    /// </summary>
    public void SetStablePhysics(bool on)
    {
        stablePhysics = on;
        ApplyStablePhysics(Time.timeScale);
    }

    /// <summary>
    /// Прямое задание fixedDeltaTime (снимает «стабильную физику», чтобы уважить ручной ввод).
    /// </summary>
    public void SetFixedDeltaTime(float value)
    {
        var v = Mathf.Max(0.0001f, value); // защита от нулей
        Time.fixedDeltaTime = v;
        stablePhysics = false; // раз ручной ввод — не компенсируем автоматически
    }

    // Пресеты FDT (частота физики)
    public void SetFdt_50Hz()  => SetFixedDeltaTime(0.02f);
    public void SetFdt_60Hz()  => SetFixedDeltaTime(1f / 60f);   // ≈0.0166667
    public void SetFdt_100Hz() => SetFixedDeltaTime(0.01f);

    /// <summary>
    /// Шагнуть на один кадр рендера при паузе.
    /// </summary>
    public void StepOneFrame()
    {
        if (!_isPaused) return;
        if (_stepRoutine != null) StopCoroutine(_stepRoutine);
        _stepRoutine = StartCoroutine(CoStepOneFrame());
    }

    /// <summary>
    /// Шагнуть на заданное реальное время (в секундах) при паузе.
    /// </summary>
    public void StepRealtimeSeconds(float seconds)
    {
        if (!_isPaused) return;
        if (_stepRoutine != null) StopCoroutine(_stepRoutine);
        _stepRoutine = StartCoroutine(CoStepRealtime(seconds));
    }

    // === ПРОПЕРТИ ДЛЯ HUD ===
    public float CurrentTimeScale => Time.timeScale;
    public float CurrentFixedDeltaTime => Time.fixedDeltaTime;
    public bool IsPaused => _isPaused;
    public bool StablePhysics => stablePhysics;
    public float BaseFixedDeltaTime => _baseFixedDeltaTime;

    // === ВНУТРЕННЯЯ ЛОГИКА ===
    private void ApplyStablePhysics(float currentTimeScale)
    {
        if (!stablePhysics) return;

        if (currentTimeScale > 0f)
        {
            // Компенсируем так, чтобы «эффективная» частота физики оставалась прежней
            Time.fixedDeltaTime = _baseFixedDeltaTime / currentTimeScale;
        }
        else
        {
            // При паузе оставим текущее значение как есть (FixedUpdate всё равно не идёт)
        }
    }

    private void UpdatePauseFlags()
    {
        if (muteAudioOnPause)
            AudioListener.pause = _isPaused;

        // Важно: Dev-UI (анимации/таймеры) должны работать на UnscaledTime.
        // Это правило применяй в своих аниматорах/твиных/таймерах.
    }

    // === КОРУТИНЫ ШАГОВ ===
    private IEnumerator CoStepOneFrame()
    {
        // Временный выход из паузы на один Update/Render кадр
        Time.timeScale = 1f;
        yield return null; // один кадр
        Time.timeScale = 0f;
        _stepRoutine = null;
    }

    private IEnumerator CoStepRealtime(float seconds)
    {
        // Позволяет «прокрутить» кусочек симуляции при паузе (по реальному времени)
        var dur = Mathf.Max(0f, seconds);
        Time.timeScale = 1f;
        var t0 = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - t0 < dur)
            yield return null;
        Time.timeScale = 0f;
        _stepRoutine = null;
    }

    // === УДОБНЫЕ CONTEXT-MENU ДЛЯ ТЕСТОВ В ИНСПЕКТОРЕ ===
    [ContextMenu("Pause")]
    private void _CtxPause() => Pause();

    [ContextMenu("Resume (1x)")]
    private void _CtxResume() => Resume();

    [ContextMenu("TimeScale 0.5x")]
    private void _CtxTS050() => SetTimeScale_050();

    [ContextMenu("TimeScale 2x")]
    private void _CtxTS200() => SetTimeScale_200();

    [ContextMenu("FDT 50Hz (0.02)")]
    private void _CtxFdt50() => SetFdt_50Hz();

    [ContextMenu("FDT 60Hz (~0.0167)")]
    private void _CtxFdt60() => SetFdt_60Hz();

    [ContextMenu("Step One Frame")]
    private void _CtxStepFrame() => StepOneFrame();
}
