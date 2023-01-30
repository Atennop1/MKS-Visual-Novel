// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Representation of a text file used to author naninovel scenario flow.
    /// </summary>
    [Serializable]
    public class Script : ScriptableObject
    {
        /// <summary>
        /// Name of the script asset.
        /// </summary>
        public string Name => name;
        /// <summary>
        /// The list of lines this script asset contains.
        /// </summary>
        public IReadOnlyList<ScriptLine> Lines => lines;

        private static IScriptParser cachedParser;

        [SerializeReference] private List<ScriptLine> lines = default;

        /// <summary>
        /// Creates a new instance from the provided script lines.
        /// </summary>
        public static Script FromLines (string scriptName, IEnumerable<ScriptLine> scriptLines)
        {
            var asset = CreateInstance<Script>();
            asset.name = scriptName;
            asset.lines = scriptLines.ToList();
            return asset;
        }

        /// <summary>
        /// Creates a new instance by parsing the provided script text.
        /// </summary>
        /// <param name="scriptName">Name of the script asset.</param>
        /// <param name="scriptText">The script text to parse.</param>
        /// <param name="errors">When an error occurs while parsing, will add it to the collection.</param>
        public static Script FromScriptText (string scriptName, string scriptText, ICollection<ScriptParseError> errors)
        {
            return GetCachedParser().ParseText(scriptName, scriptText, errors);

            IScriptParser GetCachedParser ()
            {
                var typeName = Configuration.GetOrDefault<ScriptsConfiguration>().ScriptParser;
                if (cachedParser != null && cachedParser.GetType().AssemblyQualifiedName == typeName) return cachedParser;
                cachedParser = Activator.CreateInstance(Type.GetType(typeName)) as IScriptParser;
                return cachedParser;
            }
        }

        /// <inheritdoc cref="FromScriptText(string,string,ICollection{ScriptParseError})"/>
        public static Script FromScriptText (string scriptName, string scriptText)
        {
            var logger = ScriptParseErrorLogger.GetFor(scriptName);
            var script = FromScriptText(scriptName, scriptText, logger);
            ScriptParseErrorLogger.Return(logger);
            return script;
        }

        /// <summary>
        /// Logs a message to the console with the additional script line position info.
        /// </summary>
        public static void LogWithPosition (string scriptName, int lineNumber, int inlineIndex, string message, LogType logType = LogType.Log)
        {
            var line = $"line #{lineNumber}{(inlineIndex <= 0 ? string.Empty : $".{inlineIndex}")}";
            var fullMessage = string.IsNullOrEmpty(scriptName) ? $"Transient Naninovel script: {message}" : $"Naninovel script `{scriptName}` at {line}: {message}";
            Debug.LogFormat(logType, default, default, fullMessage);
        }

        /// <summary>
        /// Logs a message to the console with the additional script line position info.
        /// </summary>
        public static void LogWithPosition (PlaybackSpot playbackSpot, string message, LogType logType = LogType.Log)
        {
            LogWithPosition(playbackSpot.ScriptName, playbackSpot.LineNumber, playbackSpot.InlineIndex, message, logType);
        }

        /// <summary>
        /// Collects all the contained commands (preserving the order).
        /// </summary>
        /// <param name="localizationScript">When provided, will attempt to replace localizable commands based on <see cref="ScriptLine.LineHash"/>.</param>
        /// <remarks>
        /// Localization script is expected in the following format:
        /// # Localized (source) line hash (as label line)
        /// ; Text to localize from the source script (as comment line, optional)
        /// The localized text to use as replacement (as generic or command lines)
        /// </remarks>
        public List<Command> ExtractCommands (Script localizationScript = null)
        {
            var commands = new List<Command>();
            var usedLocalizedLineIndexes = new HashSet<int>();
            var rollbackEnabled = Engine.Initialized ? Engine.GetConfiguration<StateConfiguration>().EnableStateRollback : ProjectConfigurationProvider.LoadOrDefault<StateConfiguration>().EnableStateRollback;

            for (int i = 0; i < lines.Count; i++)
            {
                var sourceLine = lines[i];
                if (sourceLine is CommentScriptLine || sourceLine is LabelScriptLine || sourceLine is EmptyScriptLine) continue;

                var li = localizationScript != null
                    ? (localizationScript.FindLine<LabelScriptLine>(l =>
                        // Exclude used localized lines to prevent overriding lines with equal content hashes.
                        !usedLocalizedLineIndexes.Contains(l.LineIndex) && l.LabelText == sourceLine.LineHash)?.LineIndex ?? -1)
                    : -1;
                if (localizationScript != null && li > -1) // Localized lines available.
                {
                    usedLocalizedLineIndexes.Add(li);
                    var replacedAnything = false;
                    var inlineIndex = 0;
                    while (localizationScript.lines.IsIndexValid(li + 1))
                    {
                        li++;
                        var localizedLine = localizationScript.lines[li];
                        if (localizedLine is CommentScriptLine || localizedLine is EmptyScriptLine) continue;
                        if (localizedLine is LabelScriptLine) break;
                        if (!rollbackEnabled && replacedAnything)
                        {
                            Debug.LogWarning($"Multiple localized lines mapped to a single source line detected in localization script `{localizationScript.Name}` at line #{localizedLine.LineNumber}. " +
                                             "That is not supported when state rollback is disabled. The extra lines won't be included to the localized version of the script.");
                            break;
                        }
                        if (!rollbackEnabled && localizedLine is GenericTextScriptLine localizedGenericLine
                                             && (!(sourceLine is GenericTextScriptLine sourceGenericLine) || sourceGenericLine.InlinedCommands.Count != localizedGenericLine.InlinedCommands.Count))
                            Debug.LogWarning($"Inlined commands count in the localized content not equals to the source in `{localizationScript.Name}` script at line #{localizedLine.LineNumber}. " +
                                             "That could break the playback when changing the locale while state rollback is disabled. Either enable state rollback or fix the localized content.");
                        OverrideCommandIndexInLine(localizedLine, i, ref inlineIndex);
                        AddCommandsFromLine(localizedLine);
                        replacedAnything = true;
                    }

                    if (replacedAnything) continue;
                    // Else: no localized lines found; fallback to the source line.
                }

                AddCommandsFromLine(sourceLine);
            }

            return commands;

            void AddCommandsFromLine (ScriptLine line)
            {
                if (line is CommandScriptLine commandLine)
                    commands.Add(commandLine.Command);
                else if (line is GenericTextScriptLine genericLine)
                    commands.AddRange(genericLine.InlinedCommands);
            }

            void OverrideCommandIndexInLine (ScriptLine line, int lineIndex, ref int inlineIndex)
            {
                if (line is CommandScriptLine commandLine)
                {
                    commandLine.Command.PlaybackSpot = new PlaybackSpot(commandLine.Command.PlaybackSpot.ScriptName, lineIndex, inlineIndex);
                    inlineIndex++;
                }
                else if (line is GenericTextScriptLine genericLine)
                    for (int i = 0; i < genericLine.InlinedCommands.Count; i++)
                    {
                        var command = genericLine.InlinedCommands[i];
                        command.PlaybackSpot = new PlaybackSpot(command.PlaybackSpot.ScriptName, lineIndex, inlineIndex);
                        inlineIndex++;
                    }
            }
        }

        /// <summary>
        /// Returns first script line of <typeparamref name="TLine"/> filtered by <paramref name="predicate"/> or null.
        /// </summary>
        public TLine FindLine<TLine> (Predicate<TLine> predicate) where TLine : ScriptLine
        {
            return lines.FirstOrDefault(l => l is TLine tline && predicate(tline)) as TLine;
        }

        /// <summary>
        /// Returns all the script lines of <typeparamref name="TLine"/> filtered by <paramref name="predicate"/>.
        /// </summary>
        public List<TLine> FindLines<TLine> (Predicate<TLine> predicate) where TLine : ScriptLine
        {
            return lines.Where(l => l is TLine tline && predicate(tline)).Cast<TLine>().ToList();
        }

        /// <summary>
        /// Checks whether a <see cref="LabelScriptLine"/> with the provided value exists in this script.
        /// </summary>
        public bool LabelExists (string label)
        {
            return lines.Exists(l => l is LabelScriptLine labelLine && labelLine.LabelText.EqualsFast(label));
        }

        /// <summary>
        /// Attempts to retrieve index of a <see cref="LabelScriptLine"/> with the provided <see cref="LabelScriptLine.LabelText"/>.
        /// Returns -1 in case the label is not found.
        /// </summary>
        public int GetLineIndexForLabel (string label)
        {
            foreach (var line in lines)
                if (line is LabelScriptLine labelLine && labelLine.LabelText.EqualsFast(label))
                    return labelLine.LineIndex;
            return -1;
        }

        /// <summary>
        /// Returns first <see cref="LabelScriptLine.LabelText"/> located above line with the provided index.
        /// Returns null when not found.
        /// </summary>
        public string GetLabelForLine (int lineIndex)
        {
            if (!lines.IsIndexValid(lineIndex)) return null;
            for (var i = lineIndex; i >= 0; i--)
                if (lines[i] is LabelScriptLine labelLine)
                    return labelLine.LabelText;
            return null;
        }

        /// <summary>
        /// Returns first <see cref="CommentScriptLine.CommentText"/> located above line with the provided index.
        /// Returns null when not found.
        /// </summary>
        public string GetCommentForLine (int lineIndex)
        {
            if (!lines.IsIndexValid(lineIndex)) return null;
            for (var i = lineIndex; i >= 0; i--)
                if (lines[i] is CommentScriptLine commentLine)
                    return commentLine.CommentText;
            return null;
        }
    }
}
