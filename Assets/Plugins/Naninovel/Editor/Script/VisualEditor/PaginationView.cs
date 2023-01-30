// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Naninovel
{
    public class PaginationView : VisualElement
    {
        private readonly Label label;

        public PaginationView (Action nextPageSelected, Action previousPageSelected)
        {
            styleSheets.Add(ScriptView.StyleSheet);
            if (EditorGUIUtility.isProSkin)
                styleSheets.Add(ScriptView.DarkStyleSheet);
            if (ScriptView.CustomStyleSheet)
                styleSheets.Add(ScriptView.CustomStyleSheet);

            var prevButton = new Button(previousPageSelected);
            prevButton.text = "<";
            Add(prevButton);

            label = new Label();
            Add(label);

            var nextButton = new Button(nextPageSelected);
            nextButton.text = ">";
            Add(nextButton);
        }

        public void SetLabel (string value)
        {
            label.text = value;
        }
    }

}