// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine.UIElements;

namespace Naninovel
{
    public class GenericTextLineView : ScriptLineView
    {
        private readonly LineTextField valueField;

        public GenericTextLineView (int lineIndex, string lineText, VisualElement container)
            : base(lineIndex, container)
        {
            valueField = new LineTextField(value: lineText);
            valueField.multiline = true;
            Content.Add(valueField);
        }

        public override string GenerateLineText () => valueField.value;
    }
}
