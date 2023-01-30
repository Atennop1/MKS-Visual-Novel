// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Parsing;

namespace Naninovel
{
    public abstract class ScriptLineParser<TResult, TModel, TParser>
        where TResult : ScriptLine
        where TModel : class, new()
        where TParser : LineParser<TModel>, new()
    {
        protected virtual string ScriptName { get; private set; }
        protected virtual int LineIndex { get; private set; }
        protected virtual string LineText { get; private set; }
        protected virtual string LineHash { get; private set; }
        protected virtual TParser Parser { get; } = new TParser();
        protected virtual List<ParseError> Errors { get; } = new List<ParseError>();

        /// <summary>
        /// Produces a persistent hash code from the provided script line text (trimmed).
        /// </summary>
        public static string GetHash (string lineText)
        {
            return CryptoUtils.PersistentHexCode(lineText.TrimFull());
        }

        public virtual TResult Parse (string scriptName, int lineIndex, string lineText,
            IReadOnlyList<Token> tokens, ICollection<ScriptParseError> errors = null)
        {
            ScriptName = scriptName;
            LineIndex = lineIndex;
            LineText = lineText;
            LineHash = GetHash(lineText);
            Errors.Clear();
            var lineModel = Parser.Parse(lineText, tokens, Errors);
            var result = Parse(lineModel);
            PopulateErrors(errors);
            Parser.ReturnLine(lineModel);
            return result;
        }

        protected abstract TResult Parse (TModel lineModel);

        protected virtual void PopulateErrors (ICollection<ScriptParseError> parseErrors)
        {
            if (parseErrors is null || Errors.Count == 0) return;
            foreach (var error in Errors)
                parseErrors.Add(error.Message, LineIndex);
        }
    }
}
