// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Text;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Adds a [choice](/guide/choices.md) option to a choice handler with the specified ID (or default one).
    /// </summary>
    /// <remarks>
    /// When `goto`, `gosub` and `do` parameters are not specified, will continue script execution from the next script line.
    /// </remarks>
    [CommandAlias("choice")]
    public class AddChoice : Command, Command.ILocalizable, Command.IPreloadable
    {
        /// <summary>
        /// Text to show for the choice.
        /// When the text contain spaces, wrap it in double quotes (`"`). 
        /// In case you wish to include the double quotes in the text itself, escape them.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), LocalizableParameter]
        public StringParameter ChoiceSummary;
        /// <summary>
        /// Path (relative to a `Resources` folder) to a [button prefab](/guide/choices.md#choice-button) representing the choice. 
        /// The prefab should have a `ChoiceHandlerButton` component attached to the root object.
        /// Will use a default button when not provided.
        /// </summary>
        [ParameterAlias("button")]
        public StringParameter ButtonPath;
        /// <summary>
        /// Local position of the choice button inside the choice handler (if supported by the handler implementation).
        /// </summary>
        [ParameterAlias("pos")]
        public DecimalListParameter ButtonPosition;
        /// <summary>
        /// ID of the choice handler to add choice for. Will use a default handler if not provided.
        /// </summary>
        [ParameterAlias("handler"), ActorContext(ChoiceHandlersConfiguration.DefaultPathPrefix)]
        public StringParameter HandlerId;
        /// <summary>
        /// Path to go when the choice is selected by user;
        /// see [@goto] command for the path format.
        /// </summary>
        [ParameterAlias("goto"), ResourceContext(ScriptsConfiguration.DefaultPathPrefix, 0)]
        public NamedStringParameter GotoPath;
        /// <summary>
        /// Path to a subroutine to go when the choice is selected by user;
        /// see [@gosub] command for the path format. When `goto` is assigned this parameter will be ignored.
        /// </summary>
        [ParameterAlias("gosub"), ResourceContext(ScriptsConfiguration.DefaultPathPrefix, 0)]
        public NamedStringParameter GosubPath;
        /// <summary>
        /// Set expression to execute when the choice is selected by user; 
        /// see [@set] command for syntax reference.
        /// </summary>
        [ParameterAlias("set"), ExpressionContext]
        public StringParameter SetExpression;
        /// <summary>
        /// Script commands to execute when the choice is selected by user.
        /// Escape commas inside list values to prevent them being treated as delimiters.
        /// The commands will be invoked in order after `set`, `goto` and `gosub` are handled (if assigned).
        /// </summary>
        [ParameterAlias("do")]
        public StringListParameter OnSelected;
        /// <summary>
        /// Whether to automatically continue playing script from the next line, 
        /// when neither `goto` nor `gosub` parameters are specified. 
        /// Has no effect in case the script is already playing when the choice is processed.
        /// </summary>
        [ParameterAlias("play"), ParameterDefaultValue("true")]
        public BooleanParameter AutoPlay = true;
        /// <summary>
        /// Whether to also show choice handler the choice is added for;
        /// enabled by default.
        /// </summary>
        [ParameterAlias("show"), ParameterDefaultValue("true")]
        public BooleanParameter ShowHandler = true;
        /// <summary>
        /// Duration (in seconds) of the fade-in (reveal) animation.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration;

        protected IChoiceHandlerManager HandlerManager => Engine.GetService<IChoiceHandlerManager>();

        public async UniTask PreloadResourcesAsync ()
        {
            if (Assigned(HandlerId) && !HandlerId.DynamicValue)
            {
                var handlerId = Assigned(HandlerId) ? HandlerId.Value : HandlerManager.Configuration.DefaultHandlerId;
                var handler = await HandlerManager.GetOrAddActorAsync(handlerId);
                await handler.HoldResourcesAsync(null, this);
            }

            if (Assigned(ButtonPath) && !ButtonPath.DynamicValue)
                await HandlerManager.ChoiceButtonLoader.LoadAndHoldAsync(ButtonPath, this);
        }

        public void ReleasePreloadedResources ()
        {
            if (Assigned(HandlerId) && !HandlerId.DynamicValue)
            {
                var handlerId = Assigned(HandlerId) ? HandlerId.Value : HandlerManager.Configuration.DefaultHandlerId;
                if (HandlerManager.ActorExists(handlerId)) HandlerManager.GetActor(handlerId).ReleaseResources(null, this);
            }

            if (Assigned(ButtonPath) && !ButtonPath.DynamicValue)
                HandlerManager.ChoiceButtonLoader.Release(ButtonPath, this);
        }

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var handlerId = Assigned(HandlerId) ? HandlerId.Value : HandlerManager.Configuration.DefaultHandlerId;
            var choiceHandler = await HandlerManager.GetOrAddActorAsync(handlerId);
            asyncToken.ThrowIfCanceled();

            if (!choiceHandler.Visible && ShowHandler)
            {
                var duration = Assigned(Duration) ? Duration.Value : HandlerManager.Configuration.DefaultDuration;
                choiceHandler.ChangeVisibilityAsync(true, duration, asyncToken: asyncToken).Forget();
            }

            var builder = new StringBuilder();

            if (Assigned(SetExpression))
                builder.AppendLine($"{Parsing.Identifiers.CommandLine}{nameof(SetCustomVariable)} {SetExpression}");

            if (Assigned(GotoPath))
                builder.AppendLine($"{Parsing.Identifiers.CommandLine}{nameof(Goto)} {GotoPath.Name ?? string.Empty}{(GotoPath.NamedValue.HasValue ? $".{GotoPath.NamedValue.Value}" : string.Empty)}");
            else if (Assigned(GosubPath))
                builder.AppendLine($"{Parsing.Identifiers.CommandLine}{nameof(Gosub)} {GosubPath.Name ?? string.Empty}{(GosubPath.NamedValue.HasValue ? $".{GosubPath.NamedValue.Value}" : string.Empty)}");

            if (Assigned(OnSelected))
                foreach (var line in OnSelected)
                    builder.Append(line?.Value?.Trim() ?? string.Empty).Append('\n');

            var onSelectScript = builder.ToString().TrimFull();
            var buttonPos = Assigned(ButtonPosition) ? (Vector2?)ArrayUtils.ToVector2(ButtonPosition) : null;
            var autoPlay = AutoPlay && !Assigned(GotoPath) && !Assigned(GosubPath);

            var choice = new ChoiceState(ChoiceSummary, ButtonPath, buttonPos, onSelectScript, autoPlay);
            choiceHandler.AddChoice(choice);
        }
    }
}
