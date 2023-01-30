// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine.UIElements;

namespace Naninovel
{
    public class CommentLineView : ScriptLineView
    {
        private readonly LineTextField valueField;

        public CommentLineView (int lineIndex, string lineText, VisualElement container)
            : base(lineIndex, container)
        {
            var value = lineText.GetAfterFirst(Parsing.Identifiers.CommentLine)?.TrimFull();
            valueField = new LineTextField(Parsing.Identifiers.CommentLine, value);
            valueField.multiline = true;
            Content.Add(valueField);
        }

        public override string GenerateLineText () => $"{Parsing.Identifiers.CommentLine} {valueField.value}";
    }
}
