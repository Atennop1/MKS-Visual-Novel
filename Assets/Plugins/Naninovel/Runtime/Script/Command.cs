// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a <see cref="Script"/> command.
    /// </summary>
    [Serializable]
    public abstract class Command
    {
        /// <summary>
        /// Implementing <see cref="Command"/> contains an asynchronous logic that should 
        /// always be awaited before executing next commands; <see cref="Wait"/> is ignored.
        /// </summary>
        public interface IForceWait { }

        /// <summary>
        /// Implementing <see cref="Command"/> will be included in localization scripts.
        /// </summary>
        public interface ILocalizable { }

        /// <summary>
        /// Implementing <see cref="Command"/> is able to preload resources it uses.
        /// </summary>
        public interface IPreloadable
        {
            /// <summary>
            /// Preloads the resources used by the command.
            /// </summary>
            UniTask PreloadResourcesAsync ();

            /// <summary>
            /// Releases the preloaded resources used by the command.
            /// </summary>
            void ReleasePreloadedResources ();
        }

        /// <summary>
        /// Assigns an alias name for <see cref="Command"/>.
        /// Aliases can be used instead of the command IDs (type names) to reference commands in naninovel script.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public sealed class CommandAliasAttribute : Attribute
        {
            public string Alias { get; }

            public CommandAliasAttribute (string alias)
            {
                Alias = alias;
            }
        }

        /// <summary>
        /// Registers the field as a required <see cref="ICommandParameter"/> logging error when it's not supplied in naninovel scripts.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public sealed class RequiredParameterAttribute : Attribute { }

        /// <summary>
        /// Assigns an alias name to a <see cref="ICommandParameter"/> field allowing it to be used instead of the field name in naninovel scripts.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public sealed class ParameterAliasAttribute : Attribute
        {
            public string Alias { get; }

            /// <param name="alias">Alias name of the parameter.</param>
            public ParameterAliasAttribute (string alias)
            {
                Alias = alias;
            }
        }

        /// <summary>
        /// Associated <see cref="ICommandParameter"/> field contains localizable data.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public sealed class LocalizableParameterAttribute : Attribute { }

        /// <summary>
        /// Associates a default value with the <see cref="ICommandParameter"/> field.
        /// Intended for external tools to access metadata; ignored at runtime.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        public sealed class ParameterDefaultValueAttribute : Attribute
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public string Value { get; }

            public ParameterDefaultValueAttribute (string value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// Namespace for all the built-in commands implementations.
        /// </summary>
        public const string DefaultNamespace = "Naninovel.Commands";
        /// <summary>
        /// Use this alias to specify a nameless command parameter.
        /// </summary>
        public const string NamelessParameterAlias = "";
        /// <summary>
        /// Contains all the available <see cref="Command"/> types in the application domain, 
        /// indexed by command alias (if available) or implementing type name. Keys are case-insensitive.
        /// </summary>
        public static LiteralMap<Type> CommandTypes => commandTypesCache ?? (commandTypesCache = GetCommandTypes());

        /// <summary>
        /// In case the command belongs to a <see cref="Script"/> asset, represents position inside the script.
        /// </summary>
        public PlaybackSpot PlaybackSpot { get => playbackSpot; set => playbackSpot = value; }
        /// <summary>
        /// Whether this command should be executed, as per <see cref="ConditionalExpression"/>.
        /// </summary>
        public bool ShouldExecute => string.IsNullOrEmpty(ConditionalExpression) || ExpressionEvaluator.Evaluate<bool>(ConditionalExpression);
        /// <summary>
        /// Whether this command should always be awaited before executing next commands, never mind the <see cref="Wait"/> parameter.
        /// </summary>
        public bool ForceWait => this is IForceWait;

        /// <summary>
        /// Whether the script player should wait for the async command execution before playing next command.
        /// </summary>
        public BooleanParameter Wait;
        /// <summary>
        /// A boolean [script expression](/guide/script-expressions.md), controlling whether this command should execute.
        /// </summary>
        [ParameterAlias("if"), ExpressionContext]
        public StringParameter ConditionalExpression;

        [SerializeField] private PlaybackSpot playbackSpot = PlaybackSpot.Invalid;

        private static LiteralMap<Type> commandTypesCache;

        /// <summary>
        /// Attempts to find a <see cref="Command"/> type based on the provided command alias or type name.
        /// </summary>
        public static Type ResolveCommandType (string commandId)
        {
            if (string.IsNullOrEmpty(commandId))
                return null;

            // First, try to resolve by key.
            CommandTypes.TryGetValue(commandId, out Type result);
            // If not found, look by type name (in case type name was requested for a command with a defined alias).
            return result ?? CommandTypes.Values.FirstOrDefault(commandType => commandType.Name.EqualsFastIgnoreCase(commandId));
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="asyncToken">Throw if canceled after each async operation (except when the operation handles the token itself).</param>
        public abstract UniTask ExecuteAsync (AsyncToken asyncToken = default);

        /// <summary>
        /// Logs a message to the console; will include script name and line number of the command.
        /// </summary>
        public void LogWithPosition (string message, LogType logType = LogType.Log)
        {
            Script.LogWithPosition(PlaybackSpot, message, logType);
        }

        /// <summary>
        /// Logs a warning to the console; will include script name and line number of the command.
        /// </summary>
        public void LogWarningWithPosition (string message) => LogWithPosition(message, LogType.Warning);

        /// <summary>
        /// Logs an error to the console; will include script name and line number of the command.
        /// </summary>
        public void LogErrorWithPosition (string message) => LogWithPosition(message, LogType.Error);

        /// <summary>
        /// Tests whether the provided parameter is not null and has a value assigned.
        /// </summary>
        public static bool Assigned (ICommandParameter parameter) => !(parameter is null) && parameter.HasValue;

        private static LiteralMap<Type> GetCommandTypes ()
        {
            var result = new LiteralMap<Type>();
            var commandTypes = Engine.Types
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Command)))
                // Put built-in commands first so they're overridden by custom commands with same aliases.
                .OrderByDescending(type => type.Namespace == DefaultNamespace);
            foreach (var commandType in commandTypes)
            {
                var commandKey = commandType.GetCustomAttributes(typeof(CommandAliasAttribute), false)
                    .FirstOrDefault() is CommandAliasAttribute tagAttribute && !string.IsNullOrEmpty(tagAttribute.Alias)
                    ? tagAttribute.Alias
                    : commandType.Name;
                result[commandKey] = commandType;
            }
            return result;
        }
    }
}
