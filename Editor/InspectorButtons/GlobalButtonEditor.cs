using System.Linq;
using System.Reflection;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using Rusleo.Utils.Runtime.Attributes;

namespace Rusleo.Utils.Editor.InspectorButtons
{
    internal static class GlobalButtonEditor
    {
        internal static void DrawButtons(UnityEngine.Object[] targets, System.Type type)
        {
            if (type.GetCustomAttribute<DisableGlobalButtonEditorAttribute>(true) != null)
                return;

            var methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                            BindingFlags.NonPublic)
                .Select(m => (Method: m, Attr: m.GetCustomAttribute<InspectorButtonAttribute>(true)))
                .Where(x => x.Attr != null && x.Method.GetParameters().Length == 0) // без параметров
                .OrderBy(x => x.Attr.Order)
                .ThenBy(x => x.Method.Name)
                .ToArray();

            if (methods.Length == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            var isPlaying = Application.isPlaying;

            foreach (var (method, attr) in methods)
            {
                bool enabled = attr.Mode switch
                {
                    ButtonMode.Always => true,
                    ButtonMode.EditorOnly => !isPlaying,
                    ButtonMode.PlayModeOnly => isPlaying,
                    _ => true
                };

                using (new EditorGUI.DisabledScope(!enabled))
                {
                    var label = string.IsNullOrEmpty(attr.Label) ? method.Name : attr.Label;
                    if (GUILayout.Button(label))
                    {
                        if (!string.IsNullOrEmpty(attr.Confirm))
                        {
                            if (!EditorUtility.DisplayDialog("Подтверждение", attr.Confirm, "Да", "Отмена"))
                                continue;
                        }

                        // Статические методы — один вызов
                        if (method.IsStatic)
                        {
                            InvokeAndHandle(null, method);
                        }
                        else
                        {
                            // Для мульти-выделения — вызвать для каждого таргета
                            foreach (var t in targets)
                            {
                                InvokeAndHandle(t, method);
                            }
                        }
                    }
                }
            }
        }

        private static void InvokeAndHandle(object target, MethodInfo method)
        {
            try
            {
                var result = method.Invoke(target, null);

                // Поддержка IEnumerator в Play (корутина)
                if (result is IEnumerator enumerator && Application.isPlaying && target is MonoBehaviour mb)
                {
                    mb.StartCoroutine(enumerator);
                }
                // Поддержка Task ( fire-and-forget в редакторе )
                else if (result is Task task)
                {
                    // Без await в редакторе; при желании можно подписаться на ContinueWith и логировать.
                    // task.ContinueWith(t => Debug.Log($"Task finished: {method.Name}"));
                }

                // Обновить инспектор
                if (target is UnityEngine.Object obj)
                    EditorUtility.SetDirty(obj);
            }
            catch (TargetInvocationException e)
            {
                Debug.LogError(
                    $"[InspectorButton] {method.DeclaringType?.Name}.{method.Name} threw: {e.InnerException}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InspectorButton] Invoke error: {e}");
            }
        }
    }
}