// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public static class GUIContents
    {
        public static readonly GUIContent HelpIcon;

        static GUIContents ()
        {
            var contentsType = typeof(EditorGUI).GetNestedType("GUIContents", BindingFlags.NonPublic);

            HelpIcon = contentsType.GetProperty("helpIcon", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as GUIContent;
        }
    }
}
