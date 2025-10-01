using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rusleo.Graphics
{
    public class RuntimeQuickToggleManager : MonoBehaviour
    {
        public enum StartState
        {
            Inherit,
            ForceOn,
            ForceOff
        }

        [Header("UI Wiring")] [Tooltip("Куда спавнить элементы.")] [SerializeField]
        private Transform contentRoot;

        [Tooltip("Префаб элемента UI (см. WatchedObjectItemUI).")] [SerializeField]
        private WatchedObjectItemUI itemPrefab;

        [Header("Options")] [Tooltip("Скрыть сам Canvas/панель из управления (на всякий случай).")] [SerializeField]
        private GameObject toolRoot;

        [Tooltip("Автоматически пересобирать список при загрузке сцены.")] [SerializeField]
        private bool rebuildOnSceneLoaded = true;

        private readonly List<WatchedObjectItemUI> _spawned = new();

        private SceneWatchEntries[] _allEntries;
        private List<WatchEntry> _entries = new();

        private void Awake()
        {
            if (rebuildOnSceneLoaded)
                SceneManager.sceneLoaded += OnSceneLoaded;

            ApplyStartStates();
            Rebuild();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Rebuild();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void ApplyStartStates()
        {
            foreach (var e in _entries)
            {
                if (e?.target == null) continue;
                switch (e.startState)
                {
                    case StartState.ForceOn: e.target.SetActive(true); break;
                    case StartState.ForceOff: e.target.SetActive(false); break;
                }
            }
        }

        [ContextMenu("Rebuild Now")]
        public void Rebuild()
        {
            if (!contentRoot || !itemPrefab)
            {
                Debug.LogWarning("[RuntimeQuickToggleManager] Не назначены contentRoot или itemPrefab.");
                return;
            }

            // clear
            foreach (var it in _spawned.Where(it => it))
                Destroy(it.gameObject);
            _spawned.Clear();

            _allEntries = FindObjectsOfType<SceneWatchEntries>(false);
            if (_allEntries == null || _allEntries.Length == 0) return;
            _entries.Clear();
            foreach (var it in _allEntries)
            {
                if (it.entries == null || it.entries.Count == 0) continue;

                foreach (var itEntry in it.entries)
                {
                    if (itEntry.target == null) continue;
                    if (_entries.Any(e => e.target == itEntry.target)) continue;
                    _entries.Add(itEntry);
                }
            }

            ApplyStartStates();

            foreach (var e in _entries)
            {
                // фильтруем сам инструмент
                if (ShouldSkip(e?.target)) continue;

                var item = Instantiate(itemPrefab, contentRoot);
                _spawned.Add(item);

                var displayName = string.IsNullOrWhiteSpace(e.label) && e.target
                    ? e.target.name
                    : e.label;

                item.Bind(displayName, e?.target);

                // подписка на клик пользователя
                item.OnUserToggled += (ui, newState) =>
                {
                    if (e == null || e.target == null)
                    {
                        ui.MarkMissingTarget(); // подсветим, что ссылка потеряна
                        return;
                    }

                    e.target.SetActive(newState);
                    ui.SetState(newState);
                };
            }
        }

        private bool ShouldSkip(GameObject go)
        {
            if (!go || !toolRoot) return false;
            if (go == toolRoot) return true;

            if (go.scene == toolRoot.scene)
            {
                var t = go.transform;
                while (t)
                {
                    if (t == toolRoot.transform) return true;
                    t = t.parent;
                }
            }

            return false;
        }

        // На случай, если объект был включён/выключен где-то ещё — можно дернуть это публично.
        public void RefreshVisuals()
        {
            foreach (var it in _spawned) it?.RefreshFromTarget();
        }
    }
}