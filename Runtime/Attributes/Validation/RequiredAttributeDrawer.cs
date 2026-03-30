#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Attributes.Validation
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public sealed class RequiredAttributeDrawer : PropertyDrawer
    {
        private const float HelpBoxSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var requiredAttribute = (RequiredAttribute)attribute;
            var isMissing = IsMissing(property);

            var fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);
            var fieldRect = new Rect(position.x, position.y, position.width, fieldHeight);

            if (isMissing)
            {
                var tintRect = new Rect(
                    fieldRect.x,
                    fieldRect.y,
                    fieldRect.width,
                    EditorGUIUtility.singleLineHeight);

                EditorGUI.DrawRect(tintRect, new Color(1f, 0f, 0f, 0.12f));
            }

            EditorGUI.BeginProperty(fieldRect, label, property);
            EditorGUI.PropertyField(fieldRect, property, label, true);
            EditorGUI.EndProperty();

            if (!isMissing)
            {
                return;
            }

            var helpHeight = GetHelpBoxHeight(requiredAttribute.Message, position.width);
            var helpRect = new Rect(
                position.x,
                fieldRect.yMax + HelpBoxSpacing,
                position.width,
                helpHeight);

            EditorGUI.HelpBox(helpRect, requiredAttribute.Message, MessageType.Error);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property, label, true);

            if (!IsMissing(property))
            {
                return height;
            }

            var requiredAttribute = (RequiredAttribute)attribute;
            var helpHeight = GetHelpBoxHeight(requiredAttribute.Message, EditorGUIUtility.currentViewWidth - 40f);

            return height + HelpBoxSpacing + helpHeight;
        }

        private static float GetHelpBoxHeight(string message, float width)
        {
            var safeWidth = Mathf.Max(120f, width);
            var content = EditorGUIUtility.TrTempContent(message);
            return EditorStyles.helpBox.CalcHeight(content, safeWidth);
        }

        private static bool IsMissing(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                SerializedPropertyType.ManagedReference => property.managedReferenceValue == null,
                _ => false
            };
        }
    }
}
#endif