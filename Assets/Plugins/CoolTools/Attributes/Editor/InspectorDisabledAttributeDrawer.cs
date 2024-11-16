﻿using CoolTools.Attributes;
using UnityEditor;
using UnityEngine;

namespace CoolTools.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(InspectorDisabledAttribute))]
    public class InspectorDisabledAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);

            EditorGUI.PropertyField(position, property, label);

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }
    }
}