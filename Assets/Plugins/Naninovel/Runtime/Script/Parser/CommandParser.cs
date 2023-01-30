// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Naninovel.Parsing;

namespace Naninovel
{
    public class CommandParser
    {
        protected virtual ICollection<ParseError> Errors { get; private set; }
        protected virtual Parsing.Command CommandModel => commandModel;
        protected virtual IList<Parameter> ParameterModels => CommandModel.Parameters;
        protected virtual Command Command => command;
        protected virtual Type CommandType => commandType;
        protected virtual string CommandId => commandModel.Identifier;

        private readonly HashSet<string> supportedParamIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Parsing.CommandParser bodyParser = new Parsing.CommandParser();
        private Parsing.Command commandModel;
        private Command command;
        private Type commandType;

        public virtual Command Parse (Parsing.Command model, string scriptName,
            int lineIndex, int inlineIndex, ICollection<ParseError> errors = null)
        {
            ResetState(errors, model);
            if (!TryGetCommandType(out commandType)) return null;
            if (!TryCreateCommand(out command)) return null;
            AssignPlaybackSpot(scriptName, lineIndex, inlineIndex);
            AssignParameters();
            return command;
        }

        public virtual Command Parse (string bodyText, string scriptName,
            int lineIndex, int inlineIndex, ICollection<ParseError> errors = null)
        {
            var model = bodyParser.GetCommand();
            bodyParser.ParseBody(bodyText, model);
            var command = Parse(model, scriptName, lineIndex, inlineIndex, errors);
            bodyParser.ReturnCommand(model);
            return command;
        }

        protected virtual void ResetState (ICollection<ParseError> errors, Parsing.Command model)
        {
            Errors = errors;
            supportedParamIds.Clear();
            commandModel = model;
            command = null;
            commandType = null;
        }

        protected virtual void AddError (string error)
        {
            Errors?.Add(new ParseError(error));
        }

        protected virtual bool TryGetCommandType (out Type commandType)
        {
            commandType = Command.ResolveCommandType(CommandId);
            if (commandType is null) AddError($"Command `{CommandId}` is not found.");
            return commandType != null;
        }

        protected virtual bool TryCreateCommand (out Command command)
        {
            command = Activator.CreateInstance(CommandType) as Command;
            if (command is null) AddError($"Failed to create instance of `{CommandType}` command.");
            return command != null;
        }

        protected virtual void AssignPlaybackSpot (string scriptName, int lineIndex, int inlineIndex)
        {
            Command.PlaybackSpot = new PlaybackSpot(scriptName, lineIndex, inlineIndex);
        }

        protected virtual void AssignParameters ()
        {
            var fields = CommandType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => typeof(ICommandParameter).IsAssignableFrom(f.FieldType)).ToArray();
            supportedParamIds.UnionWith(fields.Select(f => f.Name));
            foreach (var field in fields)
                AssignField(field);
            CheckUnsupportedParameters();
        }

        protected virtual void AssignField (FieldInfo field)
        {
            if (!TryFindModelFor(field, out var model)) return;
            if (!TryCreateParameter(field, out var parameter)) return;
            var value = ValueCoder.Decode(model.Value);
            if (model.Dynamic) AssignDynamicValue(parameter, value, model);
            else AssignValue(parameter, value);
            field.SetValue(Command, parameter);
        }

        protected virtual bool TryFindModelFor (FieldInfo field, out Parameter model)
        {
            var required = field.GetCustomAttribute<Command.RequiredParameterAttribute>() != null;
            var alias = field.GetCustomAttribute<Command.ParameterAliasAttribute>()?.Alias;
            if (alias != null) supportedParamIds.Add(alias);

            var id = alias != null && ParameterModels.Any(m => m.Identifier.Text.EqualsFastIgnoreCase(alias)) ? alias : field.Name;
            model = ParameterModels.FirstOrDefault(m => m.Identifier.Text.EqualsFastIgnoreCase(id));
            if (model is null && required) AddError($"Command `{CommandId}` is missing `{id}` parameter.");
            return model != null;
        }

        protected virtual void AssignValue (ICommandParameter parameter, string value)
        {
            parameter.SetValue(value, out var errors);
            if (!string.IsNullOrEmpty(errors)) AddError(errors);
        }

        protected virtual void AssignDynamicValue (ICommandParameter parameter, string value, Parameter model)
        {
            var expressions = model.Expressions.Select(e => e.Text);
            var dynamicValue = new DynamicValue(Command.PlaybackSpot, value, expressions);
            parameter.SetValue(dynamicValue);
        }

        protected virtual bool TryCreateParameter (FieldInfo field, out ICommandParameter parameter)
        {
            parameter = Activator.CreateInstance(field.FieldType) as ICommandParameter;
            if (parameter is null) AddError($"Failed to create instance of `{field.FieldType}` parameter for `{CommandId}` command.");
            return parameter != null;
        }

        protected virtual void CheckUnsupportedParameters ()
        {
            foreach (var model in ParameterModels)
                if (!supportedParamIds.Contains(model.Identifier))
                    AddError($"Command `{CommandId}` has an unsupported `{GetId(model)}` parameter.");
            string GetId (Parameter p) => p.Nameless ? "nameless" : p.Identifier;
        }
    }
}
