// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.Parsing;

namespace Naninovel
{
    public class CommandLineParser : ScriptLineParser<CommandScriptLine, CommandLine, Parsing.CommandLineParser>
    {
        protected virtual CommandParser CommandParser { get; } = new CommandParser();

        protected override CommandScriptLine Parse (CommandLine lineModel)
        {
            var command = CommandParser.Parse(lineModel.Command, ScriptName, LineIndex, 0, Errors);
            return new CommandScriptLine(command, LineIndex, LineHash);
        }
    }
}
