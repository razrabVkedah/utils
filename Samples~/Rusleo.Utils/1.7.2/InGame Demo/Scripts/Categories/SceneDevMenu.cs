using System.IO;
using Rusleo.Graphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SceneDevMenu : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private Transform buttonsRoot; // Куда спавнить кнопки сцен

    [SerializeField] private SceneButtonItem sceneButtonPrefab; // Префаб кнопки сцены (см. компонент ниже)

    [Header("Options")] [SerializeField] private bool includeCurrentScene = false; // Показывать текущую сцену в списке
    [SerializeField] private bool addIndexPrefix = false; // Добавлять индекс к названию (например, "0: Boot")
    [Header("Height")] [SerializeField] private float emptyHeight = 100;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private SettingsCategoryTab settingsCategoryTab;

    // приватные поля
    private bool _isPaused;

    private void Awake()
    {
        if (buttonsRoot == null || sceneButtonPrefab == null)
        {
            Debug.LogError("[SceneDevMenu] Не заданы buttonsRoot или sceneButtonPrefab.");
            return;
        }

        BuildSceneButtons();
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
        // очистить старые
        for (var i = buttonsRoot.childCount - 1; i >= 0; i--)
            Destroy(buttonsRoot.GetChild(i).gameObject);

        var activeIdx = SceneManager.GetActiveScene().buildIndex;
        var total = SceneManager.sceneCountInBuildSettings;

        for (var i = 0; i < total; i++)
        {
            if (!includeCurrentScene && i == activeIdx) continue;

            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            var title = addIndexPrefix ? $"{i}: {sceneName}" : sceneName;

            var item = Instantiate(sceneButtonPrefab, buttonsRoot);
            item.Init(title, i, OnSceneButtonClicked);
        }

        var c = includeCurrentScene ? total : total - 1;
        var h = c * sceneButtonPrefab.GetHeight() + (c - 1) * spacing + emptyHeight;
        settingsCategoryTab.SetExpandedHeight(h);
    }

    private void OnSceneButtonClicked(int buildIndex)
    {
        GoToSceneByIndex(buildIndex);
    }
}