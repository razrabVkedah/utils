using UnityEngine;

public class FpsLimiter : MonoBehaviour
{
    private enum FpsMode { Unlimited, Preset, Custom }

    [Header("Основные настройки")]
    [SerializeField] private bool manageVSync = true;        // Управлять ли vSync из скрипта
    [SerializeField] private int vsyncCount = 0;             // 0 = выкл, 1 = ждать 1 кадр, и т.д.
    [SerializeField] private int presetFps = 60;             // Текущий пресет для режима Preset
    [SerializeField] private int customFps = 60;             // Значение для режима Custom
    [SerializeField] private FpsMode mode = FpsMode.Preset;  // Текущий режим

    // Приватные поля
    private const string _ppMode = "fps_limit_mode";
    private const string _ppPreset = "fps_limit_preset";
    private const string _ppCustom = "fps_limit_custom";
    private const string _ppVsync = "fps_limit_vsync";
    private const string _ppVsyncCount = "fps_limit_vsync_count";

    void Awake()
    {
        Load();
        Apply();
    }

    // --- ПУБЛИЧНОЕ API ДЛЯ UI-КНОПОК ---
    public void Set15() => SetPreset(15);
    public void Set30() => SetPreset(30);
    public void Set45() => SetPreset(45);
    public void Set60() => SetPreset(60);
    public void SetUnlimited()
    {
        mode = FpsMode.Unlimited;
        Save();
        Apply();
    }

    // Привяжи к слайдеру (OnValueChanged), передавай float value -> int FPS
    public void SetCustomFromSlider(float value)
    {
        var fps = Mathf.Clamp(Mathf.RoundToInt(value), 10, 480);
        SetCustom(fps);
    }

    // --- БОЛЕЕ ОБЩИЕ МЕТОДЫ ---
    public void SetPreset(int fps)
    {
        presetFps = Mathf.Clamp(fps, 10, 480);
        mode = FpsMode.Preset;
        Save();
        Apply();
    }

    public void SetCustom(int fps)
    {
        customFps = Mathf.Clamp(fps, 10, 480);
        mode = FpsMode.Custom;
        Save();
        Apply();
    }

    public void ToggleVSync(bool en, int countIfEnabled = 1)
    {
        manageVSync = true;
        vsyncCount = en ? Mathf.Clamp(countIfEnabled, 1, 4) : 0;
        Save();
        Apply();
    }

    // --- ТЕКУЩЕЕ СОСТОЯНИЕ (можно показать в HUD) ---
    public int CurrentTargetFps
    {
        get
        {
            return mode switch
            {
                FpsMode.Unlimited => -1,
                FpsMode.Preset    => presetFps,
                FpsMode.Custom    => customFps,
                _                 => -1
            };
        }
    }

    public bool IsUnlimited => mode == FpsMode.Unlimited;

    // --- ВНУТРЕННЕЕ ПРИМЕНЕНИЕ НАСТРОЕК ---
    private void Apply()
    {
        if (manageVSync)
            QualitySettings.vSyncCount = vsyncCount;

        // Если vSync включён (count > 0), Unity будет ограничивать FPS синхронизацией —
        // targetFrameRate в этом случае игнорируется по дизайну движка.
        if (QualitySettings.vSyncCount > 0)
        {
            Application.targetFrameRate = -1; // отдаём контроль vSync
            return;
        }

        var target = CurrentTargetFps;
        Application.targetFrameRate = target; // -1 = без лимита
    }

    // --- ПERSISTENCE ---
    private void Save()
    {
        PlayerPrefs.SetInt(_ppMode, (int)mode);
        PlayerPrefs.SetInt(_ppPreset, presetFps);
        PlayerPrefs.SetInt(_ppCustom, customFps);
        PlayerPrefs.SetInt(_ppVsync, manageVSync ? 1 : 0);
        PlayerPrefs.SetInt(_ppVsyncCount, vsyncCount);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(_ppMode)) return;

        mode = (FpsMode)PlayerPrefs.GetInt(_ppMode, (int)FpsMode.Preset);
        presetFps = PlayerPrefs.GetInt(_ppPreset, 60);
        customFps = PlayerPrefs.GetInt(_ppCustom, 60);
        manageVSync = PlayerPrefs.GetInt(_ppVsync, 1) == 1;
        vsyncCount = PlayerPrefs.GetInt(_ppVsyncCount, 0);
    }

    // --- Удобные ContextMenu для быстрого теста в инспекторе ---
    [ContextMenu("Preset 15")]
    private void _Ctx15() => Set15();

    [ContextMenu("Preset 30")]
    private void _Ctx30() => Set30();
    
    [ContextMenu("Preset 45")]
    private void _Ctx45() => Set45();

    [ContextMenu("Preset 60")]
    private void _Ctx60() => Set60();

    [ContextMenu("Unlimited")]
    private void _CtxUnlimited() => SetUnlimited();

    [ContextMenu("Custom = 90")]
    private void _CtxCustom90() => SetCustom(90);

    [ContextMenu("Toggle vSync (1)")]
    private void _CtxVsyncOn() => ToggleVSync(true, 1);

    [ContextMenu("Toggle vSync OFF")]
    private void _CtxVsyncOff() => ToggleVSync(false);
}
