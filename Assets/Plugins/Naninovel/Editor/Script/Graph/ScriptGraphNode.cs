// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.Commands;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    /// <summary>
    /// Represents a node of <see cref="ScriptGraphView"/>.
    /// </summary>
    public sealed class ScriptGraphNode : Node
    {
        public readonly Script Script;
        public readonly Dictionary<string, Port> InputPorts = new Dictionary<string, Port>();
        public readonly List<ScriptGraphOutputPort> OutputPorts = new List<ScriptGraphOutputPort>();

        private const string startLabel = "Start";

        private readonly ScriptsConfiguration config;
        private readonly List<Script> availableScripts;

        public ScriptGraphNode (ScriptsConfiguration config, Script script, List<Script> availableScripts)
        {
            this.config = config;
            this.availableScripts = availableScripts;
            title = script.Name;
            expanded = true;
            m_CollapseButton.Clear();
            m_CollapseButton.SetEnabled(false);
            capabilities = Capabilities.Ascendable | Capabilities.Selectable | Capabilities.Movable;
            Script = script;

            RegisterCallback<MouseDownEvent>(OnNodeMouseDown);
            
            // Add synopsis.
            if (config.ShowSynopsis)
            {
                var synopsis = CreateSynopsis(script);
                mainContainer.Insert(1, synopsis);
            }

            // Add input port.
            InputPorts[string.Empty] = AddPort(Direction.Input, startLabel);

            // Add out ports.
            foreach (var line in script.Lines)
            {
                if (line is LabelScriptLine labelLine)
                {
                    InputPorts[labelLine.LabelText] = AddPort(Direction.Input, $"{Parsing.Identifiers.LabelLine}{labelLine.LabelText}");
                    continue;
                }

                if (line is CommandScriptLine commandLine)
                {
                    if (commandLine.Command is Goto @goto)
                        AddOutPort(@goto, @goto.Path.Name ?? @goto.PlaybackSpot.ScriptName, @goto.Path.NamedValue, $"goto {@goto.Path}");
                    if (commandLine.Command is Gosub gosub)
                        AddOutPort(gosub, gosub.Path.Name ?? gosub.PlaybackSpot.ScriptName, gosub.Path.NamedValue, $"gosub {gosub.Path}");
                    if (commandLine.Command is AddChoice choice && Command.Assigned(choice.GotoPath))
                        AddOutPort(choice, choice.GotoPath.Name ?? choice.PlaybackSpot.ScriptName, choice.GotoPath.NamedValue, $"choice goto:{choice.GotoPath}");
                    continue;
                }
            }

            void AddOutPort (Command command, string gotoScript, string gotoLabel, string portLabel)
            {
                portLabel = $"{Parsing.Identifiers.CommandLine}{portLabel}";
                var port = AddPort(Direction.Output, portLabel, command.ConditionalExpression);
                var label = string.IsNullOrEmpty(gotoLabel) ? string.Empty : gotoLabel;
                var portData = new ScriptGraphOutputPort(gotoScript, label, port);
                OutputPorts.Add(portData);
            }
        }

        public override void BuildContextualMenu (ContextualMenuPopulateEvent evt) { }

        public HashSet<ScriptGraphNode> GetConnectedNodes ()
        {
            var inputNodes = InputPorts.SelectMany(p => p.Value.connections.Select(c => c.output.node as ScriptGraphNode)).Where(n => n != this);
            var outputNodes = OutputPorts.SelectMany(p => p.Port.connections.Select(c => c.input.node as ScriptGraphNode)).Where(n => n != this);
            var result = new HashSet<ScriptGraphNode>();
            result.UnionWith(inputNodes);
            result.UnionWith(outputNodes);
            return result;
        }

        private static VisualElement CreateSynopsis (Script script)
        {
            var synopsisText = string.Empty;
            foreach (var scriptLine in script.Lines)
            {
                if (scriptLine is CommentScriptLine commentLine)
                {
                    if (string.IsNullOrWhiteSpace(commentLine.CommentText)) continue;
                    if (!string.IsNullOrEmpty(synopsisText)) synopsisText += "\n";
                    synopsisText += commentLine.CommentText;
                }
                else break;
            }

            if (string.IsNullOrEmpty(synopsisText)) 
                return new VisualElement();
            
            var synopsisContainer = new VisualElement { name = "synopsis" };
            var divider = new VisualElement { name = "divider" };
            divider.AddToClassList("horizontal");
            synopsisContainer.Add(divider);
            var synopsisLabel = new Label(synopsisText);
            synopsisContainer.Add(synopsisLabel);
            return synopsisContainer;
        }

        private Port AddPort (Direction direction, string label, string tooltip = null)
        {
            var orientation = config.GraphOrientation == ScriptsConfiguration.GraphOrientationType.Vertical ? Orientation.Vertical : Orientation.Horizontal;
            var port = InstantiatePort(orientation, direction, direction == Direction.Input ? Port.Capacity.Multi : Port.Capacity.Single, typeof(Script));
            port.capabilities = Capabilities.Selectable;
            port.portName = label;
            port.Q<Label>("type").pickingMode = PickingMode.Position;
            if (!string.IsNullOrEmpty(tooltip))
            {
                port.tooltip = tooltip;
                port.Q<Label>("type").AddToClassList("if");
            }
            port.edgeConnector.activators.Clear(); // Prevents user from creating connections.
            port.RegisterCallback<MouseDownEvent>(OnPortMouseDown);
            inputContainer.Add(port);
            return port;
        }

        private void OnNodeMouseDown (MouseDownEvent evt)
        {
            if (evt.button == 0 && evt.clickCount >= 2 && ObjectUtils.IsValid(Script))
            {
                Selection.activeObject = Script;
                EditorGUIUtility.PingObject(Script);
            }
        }

        private void OnPortMouseDown (MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            evt.StopImmediatePropagation();

            var port = evt.currentTarget as Port;
            if (port is null) return;

            (parent.parent.parent.parent as ScriptGraphView)?.edges.ForEach(e => { e.selected = false; e.UpdateEdgeControl(); });
            foreach (var edge in port.connections)
            {
                edge.selected = true;
                edge.UpdateEdgeControl();
            }

            var node = port.node as ScriptGraphNode;
            if (node is null) return;

            var gotoScript = default(Script);
            var gotoLabel = default(string);
            if (port.direction == Direction.Input)
            {
                gotoScript = node.Script;
                gotoLabel = node.InputPorts.FirstOrDefault(kv => kv.Value == port).Key;
            }
            else
            {
                var scriptName = OutputPorts.FirstOrDefault(d => d.Port == port)?.ScriptName;
                var script = availableScripts.FirstOrDefault(s => s.Name == scriptName);
                gotoScript = script;
                gotoLabel = OutputPorts.FirstOrDefault(d => d.Port == port)?.Label;
            }

            if (gotoScript != null)
            {
                Selection.activeObject = gotoScript;
                EditorGUIUtility.PingObject(gotoScript);

                // Scroll to line.
                if (gotoScript.Lines.Count == 0) return;
                EditorApplication.delayCall += ScrollToLineDelayed;
                void ScrollToLineDelayed ()
                {
                    if (gotoScript == null) return;
                    var editors = Resources.FindObjectsOfTypeAll<ScriptImporterEditor>();
                    if (editors.Length == 0) return;
                    var editorView = editors[0].VisualEditor;
                    var lineView = string.IsNullOrEmpty(gotoLabel) ? editorView?.Lines.FirstOrDefault(l => l != null) :
                        editorView?.Lines.FirstOrDefault(l => l?.LineIndex == gotoScript.GetLineIndexForLabel(gotoLabel));
                    if (lineView is null) EditorApplication.delayCall += ScrollToLineDelayed;
                    else editorView.ScrollToLine(lineView);
                }
            }
        }
    }
}
