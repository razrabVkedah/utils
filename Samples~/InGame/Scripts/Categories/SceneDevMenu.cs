using System.Collections;
using System.IO;
using Rusleo.Graphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneDevMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform buttonsRoot;                   // Контейнер для кнопок сцен
    [SerializeField] private SceneButtonItem sceneButtonPrefab;       // Префаб пункта-кнопки
    [SerializeField] private SettingsCategoryTab settingsCategoryTab; // Панель, у которой меняем высоту

    [Header("Options")]
    [SerializeField] private bool includeCurrentScene = false; // Показывать текущую сцену в списке
    [SerializeField] private bool addIndexPrefix = false;      // Префикс с индексом "0: Boot"

    [Header("Layout/Height")]
    [SerializeField] private float emptyHeight = 100f;
    [SerializeField] private float spacing = 10f;

    [Header("Rebind On Scene Change (Optional)")]
    [Tooltip("Если true — после смены сцены попытаемся найти UI заново, если ссылки разрушены")]
    [SerializeField] private bool autoRebindUIOnSceneChange = false;
    [Tooltip("Тег объекта-контейнера для buttonsRoot (если нужен автопоиск)")]
    [SerializeField] private string buttonsRootTag = "";
    [Tooltip("Тег объекта с SettingsCategoryTab (если нужен автопоиск)")]
    [SerializeField] private string settingsCategoryTabTag = "";

    // приватные поля
    private bool _isPaused;
    private bool _isRebuildScheduled;

    private void Awake()
    {
        // Объект у тебя DontDestroyOnLoad — оставляем как есть.
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        BuildSceneButtons();

        // Базовая валидация — просто предупредим, но не упадём (вдруг автоперепривязка включена)
        if (!buttonsRoot || !sceneButtonPrefab)
            Debug.LogWarning("[SceneDevMenu] Не заданы buttonsRoot или sceneButtonPrefab.");
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        _isRebuildScheduled = false;
    }

    private void OnDisable()
    {
        // Если объект временно выключили — отменим запланированную перестройку
        _isRebuildScheduled = false;
    }

    private void OnActiveSceneChanged(Scene previousActiveScene, Scene newActiveScene)
    {
        // В момент смены сцены ссылки на UI из старой сцены уже уничтожены.
        // Ничего не трогаем синхронно — только планируем перестройку.
        ScheduleRebuild();
    }

    private void ScheduleRebuild()
    {
        if (_isRebuildScheduled) return;
        if (!this) return;

        if (gameObject.activeInHierarchy)
        {
            _isRebuildScheduled = true;
            StartCoroutine(RebuildNextFrame());
        }
    }

    private IEnumerator RebuildNextFrame()
    {
        // Дождёмся конца кадра, чтобы Unity успела корректно разрушить старые объекты
        yield return new WaitForEndOfFrame();
        _isRebuildScheduled = false;

        if (!this || !isActiveAndEnabled) yield break;

        // Попробуем автоперепривязать ссылки, если это разрешено и что-то разрушено
        if (autoRebindUIOnSceneChange)
            TryRebindUiReferences();

        // Если после всего нужных ссылок нет — смысла строить нет
        if (!buttonsRoot || !sceneButtonPrefab)
            yield break;

        BuildSceneButtons();
    }

    private void TryRebindUiReferences()
    {
        // В Unity «разрушенная ссылка» даёт !obj == true, поэтому такие проверки валидны
        if (!buttonsRoot && !string.IsNullOrWhiteSpace(buttonsRootTag))
        {
            var go = GameObject.FindGameObjectWithTag(buttonsRootTag);
            buttonsRoot = go ? go.transform : null;
        }

        if (!settingsCategoryTab && !string.IsNullOrWhiteSpace(settingsCategoryTabTag))
        {
            var go = GameObject.FindGameObjectWithTag(settingsCategoryTabTag);
            if (go) settingsCategoryTab = go.GetComponent<SettingsCategoryTab>();
        }
    }

    public void ReloadScene()
    {
        var idx = SceneManager.GetActiveScene().buildIndex;
        if (idx < 0)
        {
            Debug.LogWarning("[SceneDevMenu] Текущая сцена не в Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        _isPaused = false;
        SceneManager.LoadScene(idx);
    }

    public void Pause()
    {
        if (_isPaused) return;
        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void Resume()
    {
        if (!_isPaused) return;
        Time.timeScale = 1f;
        _isPaused = false;
    }

    public void GoToSceneByIndex(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[SceneDevMenu] Неверный индекс сцены: {buildIndex}");
            return;
        }

        Time.timeScale = 1f;
        _isPaused = false;
        SceneManager.LoadScene(buildIndex);
    }

    private void BuildSceneButtons()
    {
        // Двойные защиты: сам объект/контейнер/префаб могли исчезнуть (или ещё не найдены)
        if (!this || !buttonsRoot || !sceneButtonPrefab)
            return;

        // 1) Очистка старых кнопок
        for (var i = buttonsRoot.childCount - 1; i >= 0; i--)
        {
            var child = buttonsRoot.GetChild(i);
            if (child) Destroy(child.gameObject); // Destroy (не Immediate) в PlayMode
        }

        // 2) Построение нового списка
        var activeIdx = SceneManager.GetActiveScene().buildIndex;
        var total = SceneManager.sceneCountInBuildSettings;

        var created = 0;
        for (var i = 0; i < total; i++)
        {
            if (!includeCurrentScene && i == activeIdx) continue;

            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            var title = addIndexPrefix ? $"{i}: {sceneName}" : sceneName;

            var item = Instantiate(sceneButtonPrefab, buttonsRoot);
            if (!item) continue;

            item.Init(title, i, OnSceneButtonClicked);
            created++;
        }

        // 3) Обновление высоты секции (если она жива)
        if (settingsCategoryTab)
        {
            var c = created;
            var h = c > 0
                ? c * sceneButtonPrefab.GetHeight() + (c - 1) * spacing + emptyHeight
                : emptyHeight;

            settingsCategoryTab.SetExpandedHeight(h);
        }
    }

    private void OnSceneButtonClicked(int buildIndex)
    {
        GoToSceneByIndex(buildIndex);
    }
}
