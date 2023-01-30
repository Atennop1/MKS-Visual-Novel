// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Shows an input field UI where user can enter an arbitrary text.
    /// Upon submit the entered text will be assigned to the specified custom variable.
    /// </summary>
    /// <remarks>
    /// Check out this [video guide](https://youtu.be/F9meuMzvGJw) on usage example.
    /// <br/><br/>
    /// To assign a display name for a character using this command consider [binding the name to a custom variable](/guide/characters.html#display-names).
    /// </remarks>
    [CommandAlias("input")]
    public class InputCustomVariable : Command, Command.ILocalizable
    {
        /// <summary>
        /// Name of a custom variable to which the entered text will be assigned.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public StringParameter VariableName;
        /// <summary>
        /// An optional summary text to show along with input field.
        /// When the text contain spaces, wrap it in double quotes (`"`). 
        /// In case you wish to include the double quotes in the text itself, escape them.
        /// </summary>
        [LocalizableParameter]
        public StringParameter Summary;
        /// <summary>
        /// A predefined value to set for the input field.
        /// </summary>
        [ParameterAlias("value"), LocalizableParameter]
        public StringParameter PredefinedValue;
        /// <summary>
        /// Whether to automatically resume script playback when user submits the input form.
        /// </summary>
        [ParameterAlias("play"), ParameterDefaultValue("true")]
        public BooleanParameter PlayOnSubmit = true;

        public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var inputUI = Engine.GetService<IUIManager>().GetUI<UI.IVariableInputUI>();
            inputUI?.Show(VariableName, Summary, PredefinedValue, PlayOnSubmit);

            return UniTask.CompletedTask;
        }
    }
}
