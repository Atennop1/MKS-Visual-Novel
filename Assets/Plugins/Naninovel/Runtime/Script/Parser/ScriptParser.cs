// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Parsing;

namespace Naninovel
{
    /// <inheritdoc cref="IScriptParser"/>
    public class ScriptParser : IScriptParser
    {
        protected virtual CommentLineParser CommentLineParser { get; } = new CommentLineParser();
        protected virtual LabelLineParser LabelLineParser { get; } = new LabelLineParser();
        protected virtual CommandLineParser CommandLineParser { get; } = new CommandLineParser();
        protected virtual GenericTextLineParser GenericTextLineParser { get; } = new GenericTextLineParser();

        private readonly Lexer lexer = new Lexer();
        private readonly List<Token> tokens = new List<Token>();
        private readonly List<ScriptLine> lines = new List<ScriptLine>();

        public virtual Script ParseText (string scriptName, string scriptText, ICollection<ScriptParseError> errors = null)
        {
            lines.Clear();
            var textLines = Parsing.ScriptParser.SplitText(scriptText);
            for (int i = 0; i < textLines.Length; i++)
                lines.Add(ParseLine(i, textLines[i]));
            var script = Script.FromLines(scriptName, lines);
            return script;

            ScriptLine ParseLine (int lineIndex, string lineText)
            {
                tokens.Clear();
                var lineType = lexer.TokenizeLine(lineText, tokens);
                switch (lineType)
                {
                    case LineType.Comment: return ParseCommentLine(scriptName, lineIndex, lineText, tokens, errors);
                    case LineType.Label: return ParseLabelLine(scriptName, lineIndex, lineText, tokens, errors);
                    case LineType.Command: return ParseCommandLine(scriptName, lineIndex, lineText, tokens, errors);
                    case LineType.GenericText: return ParseGenericTextLine(scriptName, lineIndex, lineText, tokens, errors);
                    default: return new EmptyScriptLine(lineIndex);
                }
            }
        }

        protected virtual CommentScriptLine ParseCommentLine (string scriptName, int lineIndex, string lineText,
            IReadOnlyList<Token> tokens, ICollection<ScriptParseError> errors = null)
        {
            return CommentLineParser.Parse(scriptName, lineIndex, lineText, tokens, errors);
        }

        protected virtual LabelScriptLine ParseLabelLine (string scriptName, int lineIndex, string lineText,
            IReadOnlyList<Token> tokens, ICollection<ScriptParseError> errors = null)
        {
            return LabelLineParser.Parse(scriptName, lineIndex, lineText, tokens, errors);
        }

        protected virtual CommandScriptLine ParseCommandLine (string scriptName, int lineIndex, string lineText,
            IReadOnlyList<Token> tokens, ICollection<ScriptParseError> errors = null)
        {
            return CommandLineParser.Parse(scriptName, lineIndex, lineText, tokens, errors);
        }

        protected virtual GenericTextScriptLine ParseGenericTextLine (string scriptName, int lineIndex, string lineText,
            IReadOnlyList<Token> tokens, ICollection<ScriptParseError> errors = null)
        {
            return GenericTextLineParser.Parse(scriptName, lineIndex, lineText, tokens, errors);
        }
    }
}
