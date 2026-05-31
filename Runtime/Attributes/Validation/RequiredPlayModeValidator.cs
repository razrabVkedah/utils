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
        private static readonly Dictionary<Type, CachedFieldData[]> _fieldCache = new();

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

            foreach (var cached in GetCachedFields(value.GetType()))
            {
                var fieldValue = cached.Field.GetValue(value);
                var fieldPath = $"{path}.{cached.Field.Name}";

                if (cached.Required != null && IsMissing(cached, fieldValue))
                {
                    var message = $"[Required] {fieldPath} is null.";
                    if (!string.IsNullOrWhiteSpace(cached.Required.Message))
                    {
                        message += $" {cached.Required.Message}";
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

                if (cached.ShouldRecurse)
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

        private static CachedFieldData[] GetCachedFields(Type type)
        {
            if (_fieldCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var result = new List<CachedFieldData>();
            var current = type;

            while (current != null && current != typeof(object))
            {
                foreach (var field in current.GetFields(flags))
                {
                    if (field.IsStatic || field.IsNotSerialized)
                    {
                        continue;
                    }

                    var required = field.GetCustomAttribute<RequiredAttribute>();
                    var hasSerializeField = field.GetCustomAttribute<SerializeField>() != null;
                    var hasSerializeReference = field.GetCustomAttribute<SerializeReference>() != null;

                    if (required == null && !hasSerializeField && !hasSerializeReference && !field.IsPublic)
                    {
                        continue;
                    }

                    result.Add(new CachedFieldData(
                        field,
                        required,
                        hasSerializeReference,
                        ShouldRecurseInto(field.FieldType)));
                }

                current = current.BaseType;
            }

            cached = result.ToArray();
            _fieldCache[type] = cached;
            return cached;
        }

        private static bool IsMissing(CachedFieldData cached, object value)
        {
            if (typeof(Object).IsAssignableFrom(cached.Field.FieldType))
            {
                return value as Object == null;
            }

            if (cached.IsSerializeReference)
            {
                return value == null;
            }

            return false;
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

        private static bool ShouldTrackReference(object value)
        {
            return !value.GetType().IsValueType && value is not string;
        }

        private readonly struct CachedFieldData
        {
            public readonly FieldInfo Field;
            public readonly RequiredAttribute Required;
            public readonly bool IsSerializeReference;
            public readonly bool ShouldRecurse;

            public CachedFieldData(FieldInfo field, RequiredAttribute required, bool isSerializeReference, bool shouldRecurse)
            {
                Field = field;
                Required = required;
                IsSerializeReference = isSerializeReference;
                ShouldRecurse = shouldRecurse;
            }
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