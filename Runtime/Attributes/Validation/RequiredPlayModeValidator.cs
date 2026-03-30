#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Rusleo.Utils.Runtime.Attributes.Validation
{
    [InitializeOnLoad]
    public static class RequiredPlayModeValidator
    {
        static RequiredPlayModeValidator()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            var errors = new List<ValidationError>();
            ValidateLoadedScenes(errors);

            if (errors.Count == 0)
            {
                return;
            }

            foreach (var error in errors)
            {
                Debug.LogError(error.Message, error.Context);
            }

            EditorUtility.DisplayDialog(
                "Required validation",
                $"Found {errors.Count} missing required reference(s). Check Console.",
                "OK");
        }

        private static void ValidateLoadedScenes(List<ValidationError> errors)
        {
            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                {
                    continue;
                }

                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    var components = root.GetComponentsInChildren<Component>(true);
                    foreach (var component in components)
                    {
                        if (component == null)
                        {
                            continue;
                        }

                        var visited = new HashSet<object>(ReferenceComparer.Instance);
                        ValidateObject(component, component, component.GetType().Name, errors, visited);
                    }
                }
            }
        }

        private static void ValidateObject(
            object value,
            Object context,
            string path,
            List<ValidationError> errors,
            HashSet<object> visited)
        {
            if (value == null)
            {
                return;
            }

            if (ShouldTrackReference(value) && !visited.Add(value))
            {
                return;
            }

            foreach (var field in GetAllFields(value.GetType()))
            {
                if (field.IsStatic || field.IsNotSerialized)
                {
                    continue;
                }

                if (!ShouldInspectField(field))
                {
                    continue;
                }

                var fieldValue = field.GetValue(value);
                var fieldPath = $"{path}.{field.Name}";
                var requiredAttribute = field.GetCustomAttribute<RequiredAttribute>();

                if (requiredAttribute != null && IsMissing(field, fieldValue))
                {
                    var message = $"[Required] {fieldPath} is null.";
                    if (!string.IsNullOrWhiteSpace(requiredAttribute.Message))
                    {
                        message += $" {requiredAttribute.Message}";
                    }

                    errors.Add(new ValidationError(message, context));
                }

                if (fieldValue == null)
                {
                    continue;
                }

                if (fieldValue is ScriptableObject scriptableObject)
                {
                    ValidateObject(scriptableObject, scriptableObject, fieldPath, errors, visited);
                    continue;
                }

                if (ShouldRecurseInto(field.FieldType))
                {
                    ValidateNestedValue(fieldValue, context, fieldPath, errors, visited);
                }
            }
        }

        private static void ValidateNestedValue(
            object value,
            Object context,
            string path,
            List<ValidationError> errors,
            HashSet<object> visited)
        {
            if (value is string)
            {
                return;
            }

            if (value is IEnumerable enumerable)
            {
                var index = 0;
                foreach (var element in enumerable)
                {
                    if (element != null && ShouldRecurseInto(element.GetType()))
                    {
                        ValidateObject(element, context, $"{path}[{index}]", errors, visited);
                    }

                    index++;
                }

                return;
            }

            ValidateObject(value, context, path, errors, visited);
        }

        private static bool IsMissing(FieldInfo field, object value)
        {
            if (typeof(Object).IsAssignableFrom(field.FieldType))
            {
                return value as Object == null;
            }

            if (field.GetCustomAttribute<SerializeReference>() != null)
            {
                return value == null;
            }

            return false;
        }

        private static bool ShouldInspectField(FieldInfo field)
        {
            if (field.GetCustomAttribute<RequiredAttribute>() != null)
            {
                return true;
            }

            if (field.GetCustomAttribute<SerializeField>() != null)
            {
                return true;
            }

            if (field.GetCustomAttribute<SerializeReference>() != null)
            {
                return true;
            }

            return field.IsPublic;
        }

        private static bool ShouldRecurseInto(Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            if (type.IsPrimitive || type.IsEnum)
            {
                return false;
            }

            if (typeof(Object).IsAssignableFrom(type))
            {
                return false;
            }

            return true;
        }

        private static IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            while (type != null && type != typeof(object))
            {
                var fields = type.GetFields(flags);
                foreach (var field in fields)
                {
                    yield return field;
                }

                type = type.BaseType;
            }
        }

        private static bool ShouldTrackReference(object value)
        {
            return !value.GetType().IsValueType && value is not string;
        }

        private readonly struct ValidationError
        {
            public ValidationError(string message, Object context)
            {
                Message = message;
                Context = context;
            }

            public string Message { get; }
            public Object Context { get; }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new();

            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}
#endif