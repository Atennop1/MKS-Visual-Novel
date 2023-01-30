// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.Parsing;

namespace Naninovel
{
    public class CommentLineParser : ScriptLineParser<CommentScriptLine, CommentLine, Parsing.CommentLineParser>
    {
        protected override CommentScriptLine Parse (CommentLine lineModel)
        {
            return new CommentScriptLine(lineModel.CommentText, LineIndex, LineHash);
        }
    }
}
