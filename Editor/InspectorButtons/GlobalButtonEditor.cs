using System.Linq;
using System.Reflection;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using Rusleo.Utils.Runtime.Attributes;
using System.Collections.Generic;

namespace Rusleo.Utils.Editor.InspectorButtons
{
    internal static class GlobalButtonEditor
    {
        internal static void DrawButtons(Object[] targets, System.Type type)
        {
            if (type.GetCustomAttribute<DisableGlobalButtonEditorAttribute>(true) != null)
                return;

            // Берём ВСЕ методы, у которых есть хотя бы один InspectorButtonAttribute
            var methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => !m.IsAbstract && !m.IsGenericMethod)
                .Select(m => new
                {
                    Method = m,
                    BtnAttr = m.GetCustomAttributes(typeof(InspectorButtonAttribute), true)
                        .OfType<InspectorButtonAttribute>()
                        .FirstOrDefault(),
                    VisibleIf = m.GetCustomAttributes(typeof(VisibleIfAttribute), true)
                        .OfType<VisibleIfAttribute>()
                        .ToArray(),
                    EnableIf = m.GetCustomAttributes(typeof(EnableIfAttribute), true)
                        .OfType<EnableIfAttribute>()
                        .ToArray()
                })
                .Where(x => x.BtnAttr != null && x.Method.GetParameters().Length == 0)
                .OrderBy(x => x.BtnAttr.Order)
                .ThenBy(x => x.Method.Name)
                .ToArray();

            if (methods.Length == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            var isPlaying = Application.isPlaying;

            foreach (var entry in methods)
            {
                var method = entry.Method;
                var attr = entry.BtnAttr;

                // VisibleIf: если есть хотя бы один атрибут и хоть одно условие ложно — кнопку не рендерим
                if (entry.VisibleIf.Length > 0)
                {
                    bool allVisible = targets.Any()
                        ? targets.All(t => EvaluateBool(t, t.GetType(), entry.VisibleIf))
                        : EvaluateBool(null, type, entry.VisibleIf); // для статических
                    if (!allVisible) continue;
                }

                // Mode: базовая доступность по режиму
                bool enabledByMode = attr.Mode switch
                {
                    ButtonMode.Always => true,
                    ButtonMode.EditorOnly => !isPlaying,
                    ButtonMode.PlayModeOnly => isPlaying,
                    _ => true
                };

                // EnableIf: если есть — объединяем по AND (все условия должны быть true)
                bool enabledByConditions = entry.EnableIf.Length == 0 || (targets.Any()
                    ? targets.All(t => EvaluateBool(t, t.GetType(), entry.EnableIf))
                    : EvaluateBool(null, type, entry.EnableIf));

                bool enabled = enabledByMode && enabledByConditions;

                // GUIContent: label + icon + tooltip
                var label = string.IsNullOrEmpty(attr.Label) ? method.Name : attr.Label;
                var content = MakeContent(label, attr.Icon, attr.Tooltip);

                // Размер
                float height = attr.Size switch
                {
                    ButtonSize.Small => EditorGUIUtility.singleLineHeight + 2f,
                    ButtonSize.Large => EditorGUIUtility.singleLineHeight * 2.0f + 6f,
                    _ => EditorGUIUtility.singleLineHeight + 6f
                };
                var buttonOptions = new[] { GUILayout.Height(height) };

                using (new EditorGUI.DisabledScope(!enabled))
                {
                    if (GUILayout.Button(content, buttonOptions))
                    {
                        // Опциональный Confirm — показываем один раз для всех целей
                        if (!string.IsNullOrEmpty(attr.Confirm))
                        {
                            if (!EditorUtility.DisplayDialog("Are you sure?", attr.Confirm, "Yes", "No"))
                                continue;
                        }

                        // Выполняем
                        InvokeForTargets(targets, type, method);
                    }
                }
            }
        }

        // ---- helpers -------------------------------------------------------

        private static GUIContent MakeContent(string label, string iconName, string tooltip)
        {
            if (!string.IsNullOrEmpty(iconName))
            {
                var icon = EditorGUIUtility.IconContent(iconName);
                if (icon != null && icon.image != null)
                {
                    var gc = new GUIContent(label, icon.image, tooltip);
                    return gc;
                }
            }

            return new GUIContent(label, tooltip);
        }

        private static bool EvaluateBool(object target, System.Type type, IEnumerable<VisibleIfAttribute> conds)
            => conds.All(c => EvaluateMemberBool(target, type, c.MemberName));

        private static bool EvaluateBool(object target, System.Type type, IEnumerable<EnableIfAttribute> conds)
            => conds.All(c => EvaluateMemberBool(target, type, c.MemberName));

        private static bool EvaluateMemberBool(object target, System.Type type, string memberName)
        {
            // 1) bool property
            var prop = type.GetProperty(memberName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool))
                return (bool)prop.GetValue(prop.GetGetMethod(true).IsStatic ? null : target);

            // 2) bool field
            var field = type.GetField(memberName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
                return (bool)field.GetValue(field.IsStatic ? null : target);

            // 3) bool method without parameters
            var method = type.GetMethod(memberName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null && method.GetParameters().Length == 0 && method.ReturnType == typeof(bool))
                return (bool)method.Invoke(method.IsStatic ? null : target, null);

            // если не нашли — считаем условие истинным (или можно логнуть предупреждение)
            return true;
        }

        private static void InvokeForTargets(UnityEngine.Object[] targets, System.Type ownerType, MethodInfo method)
        {
            bool anySOChanged = false;

            // Статические методы — единоразовый вызов
            if (method.IsStatic)
            {
                InvokeAndHandle(null, method, ref anySOChanged);
            }
            else
            {
                foreach (var t in targets)
                {
                    if (t == null) continue;

                    // Undo/Redo
                    Undo.RecordObject(t, $"Invoke {method.Name}");

                    InvokeAndHandle(t, method, ref anySOChanged);

                    // Помечаем объект грязным
                    EditorUtility.SetDirty(t);

                    // Если это MB на сцене — помечаем сцену грязной
                    if (t is MonoBehaviour mb)
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mb.gameObject.scene);
                    }
                }
            }

            // Auto-save для ScriptableObject
            if (anySOChanged)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private static void InvokeAndHandle(object target, MethodInfo method, ref bool anySOChanged)
        {
            try
            {
                var result = method.Invoke(target, null);

                // IEnumerator в Play — запускаем как корутину
                if (result is IEnumerator enumerator && Application.isPlaying && target is MonoBehaviour mb)
                {
                    mb.StartCoroutine(enumerator);
                }
                // Task — fire-and-forget
                else if (result is Task task)
                {
                    // при желании можно добавить ContinueWith для логов/ошибок
                }

                // Если работали с ScriptableObject — пометим, что надо сохранить
                if (target is ScriptableObject) anySOChanged = true;
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