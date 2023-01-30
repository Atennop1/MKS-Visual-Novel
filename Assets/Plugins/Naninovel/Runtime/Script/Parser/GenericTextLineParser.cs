// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Naninovel.Commands;
using Naninovel.Parsing;

namespace Naninovel
{
    public class GenericTextLineParser : ScriptLineParser<GenericTextScriptLine, GenericTextLine, Parsing.GenericTextLineParser>
    {
        protected virtual CommandParser CommandParser { get; } = new CommandParser();
        protected virtual GenericTextLine Model { get; private set; }
        protected virtual IList<Command> InlinedCommands { get; } = new List<Command>();
        protected virtual string AuthorId => Model.AuthorIdentifier.Text;
        protected virtual string AuthorAppearance => Model.AuthorAppearance.Text;

        private readonly List<(int, int)> ranges = new List<(int, int)>();

        protected override GenericTextScriptLine Parse (GenericTextLine lineModel)
        {
            ResetState(lineModel);
            AddAppearanceChange();
            AddContent();
            AddLastWaitInput();
            return new GenericTextScriptLine(InlinedCommands, LineIndex, LineHash);
        }

        protected virtual void ResetState (GenericTextLine model)
        {
            Model = model;
            InlinedCommands.Clear();
        }

        protected virtual void AddAppearanceChange ()
        {
            if (string.IsNullOrEmpty(AuthorId)) return;
            if (string.IsNullOrEmpty(AuthorAppearance)) return;
            AddCommand($"char {AuthorId}.{AuthorAppearance} wait:false");
        }

        protected virtual void AddContent ()
        {
            foreach (var content in Model.Content)
                if (content is Parsing.Command command) AddCommand(command);
                else AddGenericText(content as GenericText);
        }

        protected virtual void AddCommand (Parsing.Command commandModel)
        {
            var command = CommandParser.Parse(commandModel, ScriptName,
                LineIndex, InlinedCommands.Count, Errors);
            AddCommand(command);
        }

        protected virtual void AddCommand (string bodyText)
        {
            var command = CommandParser.Parse(bodyText, ScriptName,
                LineIndex, InlinedCommands.Count, Errors);
            AddCommand(command);
        }

        protected virtual void AddCommand (Command command)
        {
            if (command is WaitForInput && InlinedCommands.LastOrDefault() is PrintText print)
                print.WaitForInput = true;
            else InlinedCommands.Add(command);
        }

        protected virtual void AddGenericText (GenericText genericText)
        {
            var text = GetEncodedValue();
            var author = string.IsNullOrEmpty(AuthorId) ? "" : $"author:{AuthorId}";
            var printedBefore = InlinedCommands.Any(c => c is PrintText);
            var reset = printedBefore ? "reset:false" : "";
            var br = printedBefore ? "br:0" : "";
            AddCommand($"print {text} {author} {reset} {br} wait:true waitInput:false");

            string GetEncodedValue ()
            {
                ranges.Clear();
                genericText.Expressions.GetRelativeRanges(genericText, ranges);
                return ValueCoder.Encode(genericText.Text, false, ranges);
            }
        }

        protected virtual void AddLastWaitInput ()
        {
            if (InlinedCommands.Any(c => c is SkipInput)) return;
            if (InlinedCommands.LastOrDefault() is WaitForInput) return;
            if (InlinedCommands.LastOrDefault() is PrintText print)
                print.WaitForInput = true;
            else AddCommand("i");
        }
    }
}
