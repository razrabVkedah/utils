using UnityEngine;
using UnityEngine.Events;

public class InGameUI : MonoBehaviour
{
    public static InGameUI Instance;
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject openButton;
    [SerializeField] private bool isOpened;
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (dontDestroyOnLoad)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Apply();
    }

    public void Close()
    {
        isOpened = false;
        Apply();
    }

    public void Open()
    {
        isOpened = true;
        Apply();
    }

    public void ToggleOpened()
    {
        isOpened = !isOpened;
        Apply();
    }

    public bool IsOpened() => isOpened;

    private void Apply()
    {
        if (content) 
            content.SetActive(isOpened);
        
        if (openButton)
            openButton.SetActive(!isOpened);
        
        if (isOpened)
            onOpened?.Invoke();
        else
            onClosed?.Invoke();
    }
}