// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Marks the beginning of a conditional execution block. 
    /// Should always be closed with an [@endif] command.
    /// For usage examples see [conditional execution](/guide/naninovel-scripts.md#conditional-execution) guide.
    /// </summary>
    [CommandAlias("if")]
    public class BeginIf : Command
    {
        /// <summary>
        /// A [script expression](/guide/script-expressions.md), which should return a boolean value. 
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter, ExpressionContext]
        public StringParameter Expression;

        public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            // In case the condition is met, do nothing and continue playing the script.
            if (ExpressionEvaluator.Evaluate<bool>(Expression, LogEvalError))
                return UniTask.CompletedTask;

            // Otherwise, play command after next @else, @elseif or @endif command of the same conditional block.
            HandleConditionalBlock(false);

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// When invoked while inside a conditional block, will navigate the playback to the appropriate command.
        /// </summary>
        /// <param name="conditionMet">Whether condition of the current block branch is met.</param>
        public static void HandleConditionalBlock (bool conditionMet)
        {
            var player = Engine.GetService<IScriptPlayer>();
            var depth = 0; // Depth of the conditional block (changes upon getting in our out of the nested if blocks).
            for (var i = player.PlayedIndex + 1; i < player.Playlist.Count; i++)
            {
                var command = player.Playlist[i];

                if (command is BeginIf) { depth++; continue; }
                if (depth != 0 && command is EndIf) { depth--; continue; }
                if (depth != 0) continue;

                if (command is EndIf || (!conditionMet && command is Else) || (!conditionMet && command is ElseIf elseIf && elseIf.EvaluateExpression()))
                {
                    if (!player.Playlist.IsIndexValid(i + 1))
                    {
                        command.LogErrorWithPosition($"A conditional (`{command.GetType().Name}`) control doesn't have any following commands; script playback will be stopped.");
                        player.Stop();
                    }
                    else player.Play(player.Playlist, i + 1);
                    return;
                }
            }

            player.PlayedCommand.LogErrorWithPosition("Conditional (`@if`) block is malformed. Make sure it has a closing `@endif` command.");
        }

        private void LogEvalError (string desc = null) => LogErrorWithPosition($"Failed to evaluate conditional (`@if`) expression `{Expression}`. {desc ?? string.Empty}");
    }
}
