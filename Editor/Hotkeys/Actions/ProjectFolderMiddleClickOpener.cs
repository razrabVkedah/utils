using System;
using System.Reflection;
using Rusleo.Utils.Runtime.Logging;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    [InitializeOnLoad]
    public static class ProjectFolderMiddleClickOpener
    {
        private const string NewWindowTitle = "Project (Locked)";
        private const double DebounceSeconds = 0.05;

        private static double _lastInvokeTime;

        static ProjectFolderMiddleClickOpener()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;
        }

        private static void OnProjectItemGUI(string guid, Rect rect)
        {
            var e = Event.current;
            if (e == null) return;

            if (e.type != EventType.MouseDown || e.button != 2) return; // только средняя кнопка
            if (!rect.Contains(e.mousePosition)) return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path)) return;

            // Окно Project, над которым находится мышь сейчас (в нём был клик)
            var sourceWindow = EditorWindow.mouseOverWindow;

            var now = EditorApplication.timeSinceStartup;
            if (now - _lastInvokeTime < DebounceSeconds) return;
            _lastInvokeTime = now;

            e.Use();

            // всё делаем после выхода из текущего OnGUI
            EditorApplication.delayCall += () => OpenNewAndLockBoth(path, sourceWindow);
        }

        private static void OpenNewAndLockBoth(string folderPath, EditorWindow sourceWindow)
        {
            var pbType = Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
            if (pbType == null)
            {
                Log.Warn("[Rusleo.Utils] ProjectBrowser type not found.");
                return;
            }

            // Подстрахуемся: лочим исходное окно, если это ProjectBrowser и оно ещё живо
            if (sourceWindow != null && sourceWindow.GetType() == pbType)
            {
                TryLockProjectWindow(sourceWindow, pbType);
            }

            var folderObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
            if (folderObj == null)
            {
                Log.Warn($"[Rusleo.Utils] Folder not found: {folderPath}");
                return;
            }

            // Создаём ИМЕННО НОВОЕ окно Project
            var newWnd = ScriptableObject.CreateInstance(pbType) as EditorWindow;
            if (newWnd == null)
            {
                Log.Warn("[Rusleo.Utils] Failed to create ProjectBrowser window.");
                return;
            }

            newWnd.titleContent = new GUIContent(NewWindowTitle);
            newWnd.Show();
            newWnd.Focus();

            // Навигация + lock нового окна — на следующем тике
            EditorApplication.delayCall += () =>
            {
                NavigateToFolder(newWnd, pbType, folderObj.GetInstanceID());
                TryLockProjectWindow(newWnd, pbType);
            };
        }

        private static void NavigateToFolder(EditorWindow wnd, Type pbType, int folderInstanceId)
        {
            if (wnd == null || pbType == null) return;

            try
            {
                var setFolderSelection = pbType.GetMethod(
                    "SetFolderSelection",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null,
                    new[] { typeof(int[]), typeof(bool) },
                    null
                );

                if (setFolderSelection != null)
                {
                    setFolderSelection.Invoke(wnd, new object[] { new[] { folderInstanceId }, true });
                }
                else
                {
                    var showFolderContents = pbType.GetMethod(
                        "ShowFolderContents",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                    );
                    showFolderContents?.Invoke(wnd, new object[] { folderInstanceId });
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"[Rusleo.Utils] Navigate to folder failed: {ex.Message}");
            }
        }

        private static void TryLockProjectWindow(EditorWindow wnd, Type pbType)
        {
            if (wnd == null || pbType == null) return;

            try
            {
                // 1) Пробуем свойство isLocked на самом ProjectBrowser
                var isLockedProp = pbType.GetProperty("isLocked",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (isLockedProp != null)
                {
                    var cur = false;
                    if (isLockedProp.CanRead)
                    {
                        var val = isLockedProp.GetValue(wnd);
                        bool b = false;
                        if (val is bool bb && bb)
                        {
                            return; // уже залочено
                        }

                        cur = b;
                    }

                    if (isLockedProp.CanWrite && !cur)
                    {
                        isLockedProp.SetValue(wnd, true);
                        return;
                    }
                }

                // 2) Через m_LockTracker
                var lockField = pbType.GetField("m_LockTracker", BindingFlags.Instance | BindingFlags.NonPublic);
                if (lockField != null)
                {
                    var lockTracker = lockField.GetValue(wnd);
                    if (lockTracker != null)
                    {
                        var setLocked = lockTracker.GetType().GetMethod("SetLocked",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (setLocked != null)
                        {
                            setLocked.Invoke(lockTracker, new object[] { true });
                            return;
                        }

                        var lockedProp = lockTracker.GetType().GetProperty("isLocked",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (lockedProp != null && lockedProp.CanWrite)
                        {
                            lockedProp.SetValue(lockTracker, true);
                            return;
                        }

                        var lockedField = lockTracker.GetType().GetField("m_IsLocked",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (lockedField != null)
                        {
                            lockedField.SetValue(lockTracker, true);
                            return;
                        }
                    }
                }

                Log.Warn("[Rusleo.Utils] Failed to lock Project window via reflection.");
            }
            catch (Exception ex)
            {
                Log.Warn($"[Rusleo.Utils] Lock failed: {ex.Message}");
            }
        }
    }
}