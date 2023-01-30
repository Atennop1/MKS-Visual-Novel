// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.Parsing;

namespace Naninovel
{
    public class LabelLineParser : ScriptLineParser<LabelScriptLine, LabelLine, Parsing.LabelLineParser>
    {
        protected override LabelScriptLine Parse (LabelLine lineModel)
        {
            return new LabelScriptLine(lineModel.LabelText, LineIndex, LineHash);
        }
    }
}
