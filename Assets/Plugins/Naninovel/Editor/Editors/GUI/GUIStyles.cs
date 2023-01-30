// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public static class GUIStyles
    {
        public static readonly GUIStyle NavigationButton;
        public static readonly GUIStyle IconButton;
        public static readonly GUIStyle TagIcon;
        public static readonly GUIStyle ScriptLabelTag;
        public static readonly GUIStyle ScriptGotoTag;
        public static readonly GUIStyle RichLabelStyle;
        public static readonly GUIStyle ResourceTooltipStyle;

        static GUIStyles ()
        {
            NavigationButton = new GUIStyle("AC Button");
            NavigationButton.stretchWidth = true;
            NavigationButton.fixedWidth = 0;

            IconButton = GetStyle("IconButton");
            TagIcon = GetStyle("AssetLabel Icon");
            ScriptLabelTag = GetStyle("AssetLabel");

            var scriptGotoTexture = Engine.LoadInternalResource<Texture2D>("ScriptGotoIcon");
            ScriptGotoTag = new GUIStyle(ScriptLabelTag);
            ScriptGotoTag.normal.background = scriptGotoTexture;

            RichLabelStyle = new GUIStyle(GUI.skin.label);
            RichLabelStyle.richText = true;

            ResourceTooltipStyle = GetStyle("HelpBox");
            ResourceTooltipStyle.margin = new RectOffset();
            ResourceTooltipStyle.padding = new RectOffset(5, 5, 5, 5);
        }

        private static GUIStyle GetStyle (string styleName)
        {
            var style = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (style is null) throw new Exception($"Missing built-in editor GUI style `{styleName}`.");
            return style;
        }
    }
}
