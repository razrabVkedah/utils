using Rusleo.Graphics;
using UnityEngine;
using UnityEngine.Serialization;

public class QualityDevMenu : MonoBehaviour
{
    [Header("UI")] 
    [SerializeField] private Transform buttonsRoot; // Контейнер для кнопок
    [SerializeField] private QualityButtonItem qualityButtonPrefab; // Префаб одной кнопки

    [Header("Options")]
    [Tooltip("0: Low, 1: Medium, ...")]
    [SerializeField] private bool addIndexPrefix = false; // 
    
    [Header("Height")] [SerializeField] private float emptyHeight = 100;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private SettingsCategoryTab settingsCategoryTab;

    private void Awake()
    {
        if (buttonsRoot == null || qualityButtonPrefab == null)
        {
            Debug.LogError("[QualityDevMenu] Не заданы buttonsRoot или qualityButtonPrefab.");
            return;
        }

        BuildButtons();
    }

    public void BuildButtons()
    {
        // очистка
        for (var i = buttonsRoot.childCount - 1; i >= 0; i--)
            Destroy(buttonsRoot.GetChild(i).gameObject);

        var names = QualitySettings.names;
        var active = QualitySettings.GetQualityLevel();

        for (var i = 0; i < names.Length; i++)
        {
            var title = addIndexPrefix ? $"{i}: {names[i]}" : names[i];
            var item = Instantiate(qualityButtonPrefab, buttonsRoot);
            item.Init(title, i, OnClickQuality, isActive: i == active);
        }
        
        var h = names.Length * qualityButtonPrefab.GetHeight() + (names.Length - 1) * spacing + emptyHeight;
        settingsCategoryTab.SetExpandedHeight(h);
    }

    private void OnClickQuality(int index)
    {
        // Применяем «дорогие» изменения (пересоздание RT, теней и т.п.)
        QualitySettings.SetQualityLevel(index, true);
        // Обновим визуальное состояние кнопок
        BuildButtons();
    }
}