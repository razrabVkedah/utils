using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class DevPauseController : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private bool muteAudioOnPause = true;  // Глушить звук на паузе
    [SerializeField] private float resumeTimeScale = 1f;    // Во что возвращать timeScale при Play

    [Header("Events")]
    [SerializeField] private UnityEvent<bool> onPauseChanged; // true=Paused, false=Playing (для UI-переключателей)

    private bool _isPaused;
    private float _prePauseTimeScale = 1f;
    private Coroutine _stepRoutine;

    private void Awake()
    {
        // Инициализируем состояние согласно текущему timeScale
        _isPaused = Mathf.Approximately(Time.timeScale, 0f);
        if (!_isPaused)
            _prePauseTimeScale = Time.timeScale;

        ApplyPauseSideEffects(_isPaused);
        // Нормализуем resumeTimeScale
        resumeTimeScale = Mathf.Max(0.01f, resumeTimeScale);
    }

    private void OnDisable()
    {
        // На всякий случай выходим из паузы при отключении компонента
        if (_isPaused) Play();
    }

    // === ПУБЛИЧНОЕ API ДЛЯ UI ===

    public void TogglePause()
    {
        if (_isPaused) Play();
        else Pause();
    }

    public void Pause()
    {
        if (_isPaused) return;

        _prePauseTimeScale = Time.timeScale > 0f ? Time.timeScale : resumeTimeScale;
        Time.timeScale = 0f;
        _isPaused = true;

        ApplyPauseSideEffects(true);
        onPauseChanged?.Invoke(true);
    }

    public void Play()
    {
        if (!_isPaused) return;

        var ts = Mathf.Max(0.01f, _prePauseTimeScale);
        Time.timeScale = ts;
        _isPaused = false;

        ApplyPauseSideEffects(false);
        onPauseChanged?.Invoke(false);
    }

    /// <summary>
    /// Прокрутить ровно один кадр Update/Render при паузе (как Step в Unity).
    /// Если сейчас не пауза — сначала ставим на паузу.
    /// </summary>
    public void NextFrame()
    {
        if (_stepRoutine != null) return;
        if (!_isPaused) Pause();
        _stepRoutine = StartCoroutine(CoStepOneFrame());
    }

    /// <summary>
    /// Альтернатива: прокрутить кусочек реального времени при паузе (например, 0.1s).
    /// </summary>
    public void StepRealtimeSeconds(float seconds)
    {
        if (_stepRoutine != null) return;
        if (!_isPaused) Pause();
        _stepRoutine = StartCoroutine(CoStepRealtime(seconds));
    }

    // === ПРОПЕРТИ ДЛЯ HUD ===
    public bool IsPaused => _isPaused;

    // === ВНУТРЕННЕЕ ===
    private void ApplyPauseSideEffects(bool paused)
    {
        if (muteAudioOnPause)
            AudioListener.pause = paused;
        // Важно: анимации/твины Dev-UI должны быть на unscaled time
        // (Animator.UpdateMode = Unscaled Time, WaitForSecondsRealtime и т.п.)
    }

    private IEnumerator CoStepOneFrame()
    {
        // Временно выходим из паузы на один кадр
        Time.timeScale = 1f;
        yield return null; // один кадр Update/Render
        Time.timeScale = 0f;

        _stepRoutine = null;
        // Сохраняем состояние «на паузе»
        _isPaused = true;
        onPauseChanged?.Invoke(true);
    }

    private IEnumerator CoStepRealtime(float seconds)
    {
        var dur = Mathf.Max(0f, seconds);
        Time.timeScale = 1f;
        var t0 = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - t0 < dur)
            yield return null;
        Time.timeScale = 0f;

        _stepRoutine = null;
        _isPaused = true;
        onPauseChanged?.Invoke(true);
    }

    // === ContextMenu для быстрого теста в инспекторе ===
    [ContextMenu("Toggle Pause")]
    private void _CtxToggle() => TogglePause();

    [ContextMenu("Next Frame")]
    private void _CtxStep() => NextFrame();
}
