// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    [Serializable]
    public class EmptyScriptLine : ScriptLine
    {
        public EmptyScriptLine (int lineIndex)
            : base(lineIndex, string.Empty) { }
    }
}
