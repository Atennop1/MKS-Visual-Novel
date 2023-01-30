// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Naninovel.Parsing;
using Naninovel.Searcher;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    public class ScriptView : VisualElement
    {
        public static StyleSheet StyleSheet { get; }
        public static StyleSheet DarkStyleSheet { get; }
        public static StyleSheet CustomStyleSheet { get; private set; }
        public static bool ScriptModified { get; set; }

        public readonly List<ScriptLineView> Lines = new List<ScriptLineView>();
        public IntRange ViewRange { get; private set; }

        private const int showLoadAt = 100;
        private const string playedLineClass = "PlayedScriptLine";
        private const string waitInputLineClass = "WaitInputScriptLine";

        private readonly ScriptsConfiguration config;
        private readonly EditorResources editorResources;
        private readonly List<SearcherItem> searchItems;
        private readonly Action saveAssetAction;
        private readonly VisualElement linesContainer;
        private readonly Label infoLabel;
        private readonly PaginationView paginationView;
        private readonly Lexer lexer = new Lexer();
        private readonly List<Token> tokens = new List<Token>();

        private ScrollView scrollView;
        private Script scriptAsset;
        private int page = 1;
        private int lastGeneratedTextHash = default;
        private string copiedLineText;

        static ScriptView ()
        {
            var styleSheetPath = PathUtils.AbsoluteToAssetPath(PathUtils.Combine(PackagePath.EditorResourcesPath, "ScriptEditor.uss"));
            StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            var darkStyleSheetPath = PathUtils.AbsoluteToAssetPath(PathUtils.Combine(PackagePath.EditorResourcesPath, "ScriptEditorDark.uss"));
            DarkStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(darkStyleSheetPath);
        }

        public ScriptView (ScriptsConfiguration config, Action drawHackGuiAction, Action saveAssetAction)
        {
            ScriptModified = false;

            this.config = config;
            this.saveAssetAction = saveAssetAction;
            editorResources = EditorResources.LoadOrDefault();
            ViewRange = new IntRange(0, config.EditorPageLength - 1);

            styleSheets.Add(StyleSheet);
            if (EditorGUIUtility.isProSkin)
                styleSheets.Add(DarkStyleSheet);
            CustomStyleSheet = config.EditorCustomStyleSheet;
            if (CustomStyleSheet)
                styleSheets.Add(CustomStyleSheet);

            var commentItem = new SearcherItem("Comment");
            var labelItem = new SearcherItem("Label");
            var genericTextItem = new SearcherItem("Generic Text", config.InsertLineKey != KeyCode.None
                ? $"{(config.InsertLineModifier != EventModifiers.None ? $"{config.InsertLineModifier}+" : string.Empty)}{config.InsertLineKey}"
                : null);
            var commandsItem = new SearcherItem("Commands");
            foreach (var commandId in Command.CommandTypes.Keys.OrderBy(k => k))
                commandsItem.AddChild(new SearcherItem(char.ToLowerInvariant(commandId[0]) + commandId.Substring(1)));
            searchItems = new List<SearcherItem> { commandsItem, genericTextItem, labelItem, commentItem };

            Add(CreateIMGUI(drawHackGuiAction));
            Add(CreateIMGUI(() => HandleKeyDownEvent(null)));

            linesContainer = new VisualElement();
            Add(linesContainer);

            paginationView = new PaginationView(SelectNextPage, SelectPreviousPage);
            paginationView.style.display = DisplayStyle.None;
            Add(paginationView);

            infoLabel = new Label("Loading, please wait...");
            infoLabel.name = "InfoLabel";
            ColorUtility.TryParseHtmlString(EditorGUIUtility.isProSkin ? "#cccccc" : "#555555", out var color);
            infoLabel.style.color = color;
            Add(infoLabel);

            RegisterCallback<KeyDownEvent>(HandleKeyDownEvent, TrickleDown.TrickleDown);
            RegisterCallback<MouseDownEvent>(HandleMouseDownEvent, TrickleDown.TrickleDown);

            new ContextualMenuManipulator(ContextMenu).target = this;
        }

        public void GenerateForScript (string scriptText, Script scriptAsset, bool forceRebuild = false)
        {
            this.scriptAsset = scriptAsset;
            ScriptModified = false;

            // Prevent re-generating the editor after saving the script (applying the changes done in the editor).
            if (!forceRebuild && lastGeneratedTextHash == scriptText.GetHashCode())
            {
                // Highlight played line if we're here after a hot-reload.
                if (Engine.Initialized && Engine.Behaviour is RuntimeBehaviour)
                    HighlightPlayedCommand(Engine.GetService<IScriptPlayer>()?.PlayedCommand);
                return;
            }

            // Otherwise the script will generate twice when entering playmode.
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying) return;

            // Otherwise nullref could happen when recompiling with a script asset selected.
            EditorApplication.delayCall += GenerateDelayed;

            void GenerateDelayed ()
            {
                var editorLocked = !config.HotReloadScripts && EditorApplication.isPlayingOrWillChangePlaymode;
                linesContainer.SetEnabled(!editorLocked);
                infoLabel.style.display = editorLocked ? DisplayStyle.None : DisplayStyle.Flex;

                LineTextField.ResetPerScriptStaticData();
                Lines.Clear();
                linesContainer.Clear();
                var textLines = Parsing.ScriptParser.SplitText(scriptText);
                for (int i = 0; i < textLines.Length; i++)
                {
                    if (textLines.Length > showLoadAt && (i % showLoadAt) == 0) // Update bar for each n processed items.
                    {
                        if (EditorUtility.DisplayCancelableProgressBar("Generating Visual Editor", "Processing naninovel script...", i / (float)textLines.Length))
                        {
                            infoLabel.style.display = DisplayStyle.None;
                            linesContainer.Clear();
                            EditorUtility.ClearProgressBar();
                            Add(new IMGUIContainer(() => EditorGUILayout.HelpBox("Visual editor generation has been canceled.", MessageType.Error)));
                            return;
                        }
                    }
                    var textLine = textLines[i];
                    if (string.IsNullOrEmpty(textLine))
                    {
                        Lines.Add(null); // Skip empty lines.
                        continue;
                    }
                    var lineView = CreateLineView(i, textLine);
                    Lines.Add(lineView);
                    if (ViewRange.Contains(i))
                        linesContainer.Add(lineView);
                }

                EditorUtility.ClearProgressBar();

                if (Lines.Count > config.EditorPageLength)
                {
                    paginationView.style.display = DisplayStyle.Flex;
                    UpdatePaginationLabel();
                }
                else paginationView.style.display = DisplayStyle.None;

                Engine.OnInitializationFinished -= HandleEngineInitialized;
                if (Engine.Initialized) HandleEngineInitialized();
                else Engine.OnInitializationFinished += HandleEngineInitialized;

                if (textLines.Length > showLoadAt)
                    EditorUtility.DisplayProgressBar("Generating Visual Editor", "Building layout...", .5f);
                EditorApplication.delayCall += EditorUtility.ClearProgressBar;

                var hotKeyInfo = config.InsertLineKey == KeyCode.None ? string.Empty : $" or {config.InsertLineKey}";
                var modifierInfo = (config.InsertLineKey == KeyCode.None || config.InsertLineModifier == EventModifiers.None) ? string.Empty : $"{config.InsertLineModifier}+";
                if (!string.IsNullOrEmpty(modifierInfo)) hotKeyInfo = hotKeyInfo.Insert(4, modifierInfo);
                infoLabel.text = $"Right-click{hotKeyInfo} to insert a new line";
                infoLabel.tooltip = "Hotkeys can be changed in the script configuration menu (Naninovel -> Configuration -> Script).";
            }
        }

        public string GenerateText ()
        {
            var builder = new StringBuilder();
            foreach (var line in Lines)
            {
                if (line is null)
                {
                    builder.AppendLine();
                    continue;
                }
                var lineText = line.GenerateLineText().Replace("\n", string.Empty).Replace("\r", string.Empty);
                builder.AppendLine(lineText);
            }
            var result = builder.ToString().TrimEnd();
            result += Environment.NewLine;
            lastGeneratedTextHash = result.GetHashCode();
            return result;
        }

        public ScriptLineView CreateLineView (int lineIndex, string lineText)
        {
            tokens.Clear();
            var lineType = lexer.TokenizeLine(lineText, tokens);
            var lineView = default(ScriptLineView);
            switch (lineType)
            {
                case LineType.Comment:
                    lineView = new CommentLineView(lineIndex, lineText, linesContainer);
                    break;
                case LineType.Label:
                    lineView = new LabelLineView(lineIndex, lineText, linesContainer);
                    break;
                case LineType.Command:
                    lineView = CommandLineView.CreateOrError(lineIndex, lineText, tokens, linesContainer, config.HideUnusedParameters);
                    break;
                default:
                    lineView = new GenericTextLineView(lineIndex, lineText, linesContainer);
                    break;
            }
            return lineView;
        }

        public void FocusLine (ScriptLineView lineView, bool focusFirstField = false)
        {
            ScriptLineView.SetFocused(lineView);

            if (focusFirstField)
                EditorApplication.update += FocusFieldDelayed;

            void FocusFieldDelayed () // Otherwise editor steals the focus.
            {
                lineView?.Q<TextField>()?.Q<VisualElement>(TextInputBaseField<string>.textInputUssName)?.Focus();
                EditorApplication.update -= FocusFieldDelayed;
            }
        }

        public void ScrollToLine (ScriptLineView lineView, bool onlyIfOutOfView = true)
        {
            if (scrollView is null) scrollView = GetFirstAncestorOfType<ScrollView>();
            if (scrollView is null) return;

            while (!ViewRange.Contains(lineView.LineIndex))
            {
                if (lineView.LineIndex < ViewRange.StartIndex)
                    SelectPreviousPage();
                else SelectNextPage();
            }

            if (onlyIfOutOfView && scrollView.worldBound.Contains(lineView.worldBound.min)) return;
            var scroller = scrollView.verticalScroller;
            scroller.value = Mathf.Lerp(scroller.lowValue, scroller.highValue, linesContainer.IndexOf(lineView) / (float)linesContainer.childCount);
        }

        public void InsertLine (ScriptLineView lineView, int index, int? viewIndex = default)
        {
            if (ViewRange.Contains(index))
            {
                var insertViewIndex = viewIndex ?? index - ViewRange.StartIndex;
                linesContainer.Insert(insertViewIndex, lineView);
                ViewRange = new IntRange(ViewRange.StartIndex, ViewRange.EndIndex + 1);
                HandleLineReordered(lineView);
                UpdatePaginationLabel();
            }
            else
            {
                Lines.Insert(index, lineView);
                SyncLineIndexes();
                ScriptModified = true;
            }
        }

        public void RemoveLine (ScriptLineView scriptLineView)
        {
            Lines.Remove(scriptLineView);

            if (linesContainer.Contains(scriptLineView))
            {
                linesContainer.Remove(scriptLineView);
                ViewRange = new IntRange(ViewRange.StartIndex, ViewRange.EndIndex - 1);
                UpdatePaginationLabel();
            }

            ScriptModified = true;
        }

        public void HandleLineReordered (ScriptLineView lineView)
        {
            var viewIndex = linesContainer.IndexOf(lineView);
            var insertIndex = ViewToGlobalIndex(viewIndex);
            Lines.Remove(lineView);
            Lines.Insert(insertIndex, lineView);
            SyncLineIndexes();

            ScriptModified = true;
        }

        public int ViewToGlobalIndex (int viewIndex)
        {
            var curViewIndex = 0;
            var globalIndex = ViewRange.StartIndex;
            for (; globalIndex < Mathf.Min(ViewRange.EndIndex, Lines.Count); globalIndex++)
            {
                if (Lines[globalIndex] is null) continue; // Skip empty lines.
                if (curViewIndex >= viewIndex) break;
                curViewIndex++;
            }
            return globalIndex;
        }

        private static IMGUIContainer CreateIMGUI (Action onGUI)
        {
            var container = new IMGUIContainer(onGUI);
            container.style.height = 0;
            return container;
        }

        private void SyncLineIndexes ()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                if (line != null)
                    line.LineIndex = i;
            }
        }

        private ScriptLineView FindLineNearPosition (Vector2 worldPos)
        {
            var localPos = linesContainer.WorldToLocal(worldPos);
            return linesContainer.Children().OrderBy(v => Vector2.Distance(localPos, v.layout.center)).FirstOrDefault() as ScriptLineView;
        }

        private void ContextMenu (ContextualMenuPopulateEvent evt)
        {
            var worldPos = evt.mousePosition;
            var localPos = linesContainer.WorldToLocal(worldPos);
            var nearLine = FindLineNearPosition(worldPos);
            var nearLineViewIndex = linesContainer.IndexOf(nearLine);
            var insertViewIndex = nearLine is null ? 0 : nearLine.layout.center.y > localPos.y ? nearLineViewIndex : nearLineViewIndex + 1;
            var insertIndex = ViewToGlobalIndex(insertViewIndex);
            var hoveringLine = nearLine != null && nearLine.ContainsPoint(new Vector2(nearLine.transform.position.x, nearLine.WorldToLocal(evt.mousePosition).y));

            if (config.HotReloadScripts || !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                evt.menu.AppendAction("Insert...", _ => ShowSearcher(worldPos, insertIndex, insertViewIndex));
                evt.menu.AppendAction("Copy", _ => copiedLineText = nearLine?.GenerateLineText(), hoveringLine ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendAction("Paste", _ => InsertLine(CreateLineView(insertIndex, copiedLineText), insertIndex, insertViewIndex), !string.IsNullOrEmpty(copiedLineText) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendAction("Remove", _ => {
                    RemoveLine(nearLine);
                    focusable = true;
                    Focus();
                    focusable = false;
                }, hoveringLine ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode && hoveringLine && (nearLine is CommandLineView || nearLine is GenericTextLineView))
            {
                var player = Engine.GetService<IScriptPlayer>();
                var stateManager = Engine.GetService<IStateManager>();
                if (stateManager != null && player != null && player.PlayedScript != null && player.PlayedScript.Name == scriptAsset.name)
                {
                    var rewindIndex = ViewToGlobalIndex(nearLineViewIndex);
                    var status = (rewindIndex > player.PlaybackSpot.LineIndex ||
                                  stateManager.CanRollbackTo(s => s.PlaybackSpot.ScriptName == player.PlayedScript.Name && s.PlaybackSpot.LineIndex == rewindIndex))
                        ? DropdownMenuAction.Status.Normal
                        : DropdownMenuAction.Status.Disabled;
                    evt.menu.AppendAction("Rewind", _ => player.RewindAsync(rewindIndex).Forget(), status);
                }
            }
            if (hoveringLine)
            {
                if (nearLine is CommandLineView cmdLine && (cmdLine.CommandId.EqualsFastIgnoreCase("goto") || cmdLine.CommandId.EqualsFastIgnoreCase("gosub")))
                {
                    var path = cmdLine.Q<LineTextField>().value;
                    evt.menu.AppendAction($"Open `{path}`", _ => HandleGoto(path));
                }
                else if (nearLine.Q("Content")?.Children().FirstOrDefault(c => c is LineTextField field && field.label.EqualsFastIgnoreCase("goto")) is LineTextField gotoField)
                    evt.menu.AppendAction($"Open `{gotoField.value}`", _ => HandleGoto(gotoField.value));

                evt.menu.AppendAction("Help", _ => OpenHelpFor(nearLine));
            }

            void HandleGoto (string path)
            {
                var scriptPath = path.Contains(".") ? path.GetBefore(".") : path;
                var scriptLabel = path.GetAfter(".");

                if (!string.IsNullOrEmpty(scriptPath))
                {
                    var resourcePath = string.IsNullOrEmpty(config.Loader.PathPrefix) ? scriptPath : $"{config.Loader.PathPrefix}/{scriptPath}";
                    var guid = editorResources.GetGuidByPath(resourcePath);
                    if (string.IsNullOrEmpty(guid))
                    {
                        Debug.LogWarning($"Failed to open `{scriptPath}`: script is not found in project resources. Make sure to add it via the script resources menu.");
                        return;
                    }
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        Debug.LogWarning($"Failed to open `{scriptPath}`: GUID is not valid. Make sure the record points to a valid asset in the script resources menu.");
                        return;
                    }
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Script>(assetPath);
                }
                else if (!string.IsNullOrEmpty(scriptLabel))
                {
                    var line = linesContainer.Children().FirstOrDefault(l => l is LabelLineView labelLine && labelLine.ValueField.value.EqualsFast(scriptLabel)) as LabelLineView;
                    if (line != null)
                    {
                        FocusLine(line);
                        ScrollToLine(line);
                    }
                }
            }
        }

        private void SelectNextPage ()
        {
            if (ViewRange.EndIndex >= (Lines.Count - 1)) return;
            page++;

            EditorUtility.DisplayProgressBar("Generating Visual Editor", "Building layout...", .5f);
            EditorApplication.delayCall += EditorUtility.ClearProgressBar;

            ViewRange = new IntRange((page - 1) * config.EditorPageLength, page * config.EditorPageLength - 1);

            linesContainer.Clear();
            for (int i = ViewRange.StartIndex; i <= Mathf.Min(ViewRange.EndIndex, Lines.Count - 1); i++)
            {
                var line = Lines[i];
                if (line is null) continue;
                linesContainer.Add(line);
            }

            UpdatePaginationLabel();
        }

        private void SelectPreviousPage ()
        {
            if (page == 1) return;
            page--;

            EditorUtility.DisplayProgressBar("Generating Visual Editor", "Building layout...", .5f);
            EditorApplication.delayCall += EditorUtility.ClearProgressBar;

            ViewRange = new IntRange((page - 1) * config.EditorPageLength, page * config.EditorPageLength - 1);

            linesContainer.Clear();
            for (int i = ViewRange.StartIndex; i <= Mathf.Min(ViewRange.EndIndex, Lines.Count - 1); i++)
            {
                var line = Lines[i];
                if (line is null) continue;
                linesContainer.Add(line);
            }

            UpdatePaginationLabel();
        }

        private void UpdatePaginationLabel ()
        {
            paginationView?.SetLabel($" {ViewRange.StartIndex + 1}-{Mathf.Min(Lines.Count, ViewRange.EndIndex + 1)} / {Lines.Count} ");
        }

        private void OpenHelpFor (ScriptLineView line)
        {
            var url = @"https://naninovel.com/";
            switch (line)
            {
                case CommentLineView _:
                    url += "guide/naninovel-scripts.html#comment-lines";
                    break;
                case LabelLineView _:
                    url += "guide/naninovel-scripts.html#label-lines";
                    break;
                case GenericTextLineView _:
                    url += "guide/naninovel-scripts.html#generic-text-lines";
                    break;
                case CommandLineView commandLine:
                    url += $"api/#{commandLine.CommandId.ToLowerInvariant()}";
                    break;
                case ErrorLineView errorLine:
                    url += $"api/#{errorLine.CommandId}";
                    break;
                default:
                    url += "guide/naninovel-scripts.html";
                    break;
            }
            Application.OpenURL(url);
        }

        private void ShowSearcher (Vector2 position, int insertIndex, int insertViewIndex)
        {
            SearcherWindow.Show(EditorWindow.focusedWindow, searchItems, "Insert Line", item => {
                if (item is null) return true; // Prevent nullref when focus is lost before item is selected.
                var lineText = default(string);
                var lineView = default(ScriptLineView);
                switch (item.Name)
                {
                    case "Commands": return false; // Do nothing.
                    case "Comment":
                        lineText = Identifiers.CommentLine;
                        break;
                    case "Label":
                        lineText = Identifiers.LabelLine;
                        break;
                    case "Generic Text":
                        lineView = new GenericTextLineView(insertIndex, string.Empty, linesContainer);
                        break;
                    default: // Create command line.
                        lineView = CommandLineView.CreateDefault(insertIndex, item.Name, linesContainer, config.HideUnusedParameters);
                        break;
                }
                if (lineView is null) lineView = CreateLineView(insertIndex, lineText);
                InsertLine(lineView, insertIndex, insertViewIndex);
                FocusLine(lineView, true);
                return true;
            }, position);
        }

        private void HandleKeyDownEvent (KeyDownEvent evt)
        {
            if (evt != null)
            {
                if (evt.keyCode == config.InsertLineKey && (evt.modifiers & config.InsertLineModifier) != 0)
                {
                    DoShowSearcher();
                    evt.StopImmediatePropagation();
                }
                else if (evt.keyCode == config.SaveScriptKey && (evt.modifiers & config.SaveScriptModifier) != 0)
                {
                    saveAssetAction?.Invoke();
                    evt.StopImmediatePropagation();
                }
            }
            else if (Event.current != null && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == config.InsertLineKey && Event.current.modifiers == config.InsertLineModifier)
                {
                    DoShowSearcher();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == config.SaveScriptKey && Event.current.modifiers == config.SaveScriptModifier)
                {
                    saveAssetAction?.Invoke();
                    Event.current.Use();
                }
            }

            void DoShowSearcher ()
            {
                var insertViewIndex = ScriptLineView.FocusedLine != null ? linesContainer.IndexOf(ScriptLineView.FocusedLine) + 1 : linesContainer.childCount;
                var insertIndex = ViewToGlobalIndex(insertViewIndex);
                this.ShowSearcher(Event.current.mousePosition, insertIndex, insertViewIndex);
            }
        }

        private void HandleMouseDownEvent (MouseDownEvent evt)
        {
            if (!Engine.Initialized || !EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (evt.button != config.RewindMouseButton || (config.RewindModifier != EventModifiers.None && (evt.modifiers & config.RewindModifier) == 0)) return;

            evt.StopPropagation();
            evt.PreventDefault();

            var nearLine = FindLineNearPosition(evt.mousePosition);
            var nearLineViewIndex = linesContainer.IndexOf(nearLine);
            var hoveringLine = nearLine != null && nearLine.ContainsPoint(new Vector2(nearLine.transform.position.x, nearLine.WorldToLocal(evt.mousePosition).y));
            if (!hoveringLine) return;

            var player = Engine.GetService<IScriptPlayer>();
            var stateManager = Engine.GetService<IStateManager>();
            if (stateManager is null || player is null || !player.PlayedScript || player.PlayedScript.Name != scriptAsset.name) return;

            var rewindIndex = ViewToGlobalIndex(nearLineViewIndex);
            if (rewindIndex == player.PlaybackSpot.LineIndex) return;
            if (rewindIndex < player.PlaybackSpot.LineIndex)
            {
                var lastIndex = player.Playlist.GetLastCommand()?.PlaybackSpot.LineIndex ?? -1;
                bool CanRollback () => stateManager.CanRollbackTo(s => s.PlaybackSpot.ScriptName == player.PlaybackSpot.ScriptName && s.PlaybackSpot.LineIndex == rewindIndex);
                while (!CanRollback() && rewindIndex <= lastIndex)
                    rewindIndex++; // When clicking non-rollbackable lines, perform rollback to the next rollbackable one.
                if (!CanRollback()) return;
            }
            player.RewindAsync(rewindIndex).Forget();
        }

        private void HandleEngineInitialized ()
        {
            if (!(Engine.Behaviour is RuntimeBehaviour)) return;

            var player = Engine.GetService<IScriptPlayer>();
            player.OnCommandExecutionStart += HighlightPlayedCommand;
            player.OnWaitingForInput += HandleWaitForInput;
            var stateManager = Engine.GetService<IStateManager>();
            stateManager.OnRollbackFinished += () => HighlightPlayedCommand(player.PlayedCommand);
            if (player.PlayedCommand != null)
                HighlightPlayedCommand(player.PlayedCommand);
        }

        private void HighlightPlayedCommand (Command command)
        {
            if (!(Selection.activeObject is Script)) return;

            var player = Engine.GetService<IScriptPlayer>();
            if (player is null || command is null) return;

            var scriptName = command.PlaybackSpot.ScriptName;
            if (!scriptAsset || scriptName != scriptAsset.name)
            {
                if (!config.SelectPlayedScript) return;
                var resourcePath = string.IsNullOrEmpty(config.Loader.PathPrefix) ? scriptName : $"{config.Loader.PathPrefix}/{scriptName}";
                var guid = editorResources.GetGuidByPath(resourcePath);
                if (guid is null) return;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
            }

            if (!Lines.IsIndexValid(command.PlaybackSpot.LineIndex)) return;

            var prevPlayedLine = linesContainer.Query<ScriptLineView>(className: playedLineClass);
            prevPlayedLine.ForEach(v => v?.RemoveFromClassList(playedLineClass));
            var prevWaitInputLine = linesContainer.Query<ScriptLineView>(className: waitInputLineClass);
            prevWaitInputLine.ForEach(v => v?.RemoveFromClassList(waitInputLineClass));

            var playedLine = Lines[command.PlaybackSpot.LineIndex];
            if (playedLine is null) return; // Could happen if we delete a line in visual script editor and don't save.
            playedLine.AddToClassList(player.WaitingForInput ? waitInputLineClass : playedLineClass);

            ScrollToLine(playedLine);
        }

        private void HandleWaitForInput (bool enabled)
        {
            if (!enabled) return;

            var player = Engine.GetService<IScriptPlayer>();

            if (player?.PlayedCommand is null || !ObjectUtils.IsValid(scriptAsset) || player.PlaybackSpot.ScriptName != scriptAsset.name || !Lines.IsIndexValid(player.PlaybackSpot.LineIndex) || !ViewRange.Contains(player.PlaybackSpot.LineIndex))
                return;

            var playedLine = Lines[player.PlaybackSpot.LineIndex];
            if (playedLine is null) return; // Could happen if we delete a line in visual script editor and don't save.

            playedLine.AddToClassList(waitInputLineClass);
        }
    }
}
