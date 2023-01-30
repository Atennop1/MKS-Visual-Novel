// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    /// <summary>
    /// Allows working with visual representation of multiple <see cref="Script"/> assets and their relations.
    /// </summary>
    public class ScriptGraphView : GraphView
    {
        public const string ProgressBarTitle = "Generating Script Graph";

        public static StyleSheet StyleSheet { get; }
        public static StyleSheet CustomStyleSheet { get; private set; }

        private readonly ScriptsConfiguration config;
        private readonly ScriptGraphState state;
        private readonly List<Script> scripts;
        private readonly List<ScriptGraphNode> scriptNodes = new List<ScriptGraphNode>();

        static ScriptGraphView ()
        {
            var styleSheetPath = PathUtils.AbsoluteToAssetPath(PathUtils.Combine(PackagePath.EditorResourcesPath, "ScriptGraph.uss"));
            StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
        }

        public ScriptGraphView (ScriptsConfiguration config, ScriptGraphState state, List<Script> scripts)
        {
            this.config = config;
            this.scripts = scripts;
            this.state = state;

            CustomStyleSheet = config.GraphCustomStyleSheet;
            styleSheets.Add(StyleSheet);
            if (CustomStyleSheet != null)
                styleSheets.Add(CustomStyleSheet);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            var minimap = new MiniMap();
            minimap.anchored = true;
            minimap.SetPosition(new Rect(10, 30, 200, 140));
            minimap.visible = false;
            Add(minimap);

            var toolbar = new Toolbar();
            Add(toolbar);
            var rebuildButton = new Button(HandleRebuildButtonClicked);
            rebuildButton.text = "Rebuild Graph";
            toolbar.Add(rebuildButton);
            var alignButton = new Button(AutoAlign);
            alignButton.text = "Auto Align";
            toolbar.Add(alignButton);
            var minimapToggle = new Toggle();
            minimapToggle.label = "Show Minimap";
            minimapToggle.RegisterValueChangedCallback(evt => minimap.visible = evt.newValue);
            toolbar.Add(minimapToggle);
            var saveButton = new Button(SerializeState);
            saveButton.text = "Save";
            toolbar.Add(saveButton);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            RebuildGraph();
        }

        public override List<Port> GetCompatiblePorts (Port startPort, NodeAdapter nodeAdapter) => ports.ToList();

        public void SerializeState ()
        {
            state.NodesState.Clear();
            foreach (var node in scriptNodes)
                if (ObjectUtils.IsValid(node.Script))
                    state.NodesState.Add(new ScriptGraphNodeState(node.Script.Name, node.GetPosition()));
            EditorUtility.SetDirty(state);
            AssetDatabase.SaveAssets();
        }

        private void HandleRebuildButtonClicked ()
        {
            SerializeState();
            RebuildGraph();
        }

        private void RebuildGraph ()
        {
            scriptNodes.ForEach(RemoveElement);
            scriptNodes.Clear();
            edges.ForEach(RemoveElement);

            // Generate one node per script asset.
            for (int i = 0; i < scripts.Count; i++)
            {
                var script = scripts[i];

                // Discard deleted script assets.
                if (!ObjectUtils.IsValid(script)) continue;

                var progress = i / (float)scripts.Count;
                EditorUtility.DisplayProgressBar(ProgressBarTitle, $"Building `{script.Name}` node...", progress);

                var node = new ScriptGraphNode(config, script, scripts);
                scriptNodes.Add(node);
                AddElement(node);

                if (state.NodesState.Exists(s => s.ScriptName == script.Name))
                {
                    var nodeState = state.NodesState.First(s => s.ScriptName == script.Name);
                    node.SetPosition(nodeState.Position);
                }
            }

            // Generate connections between nodes.
            for (int i = 0; i < scriptNodes.Count; i++)
            {
                var node = scriptNodes[i];
                var progress = i / (float)scripts.Count;
                EditorUtility.DisplayProgressBar(ProgressBarTitle, "Connecting nodes...", progress);

                foreach (var data in node.OutputPorts)
                {
                    var gotoNode = scriptNodes.FirstOrDefault(n => n.Script.Name == data.ScriptName);
                    if (gotoNode is null) continue;

                    gotoNode.InputPorts.TryGetValue(data.Label, out var gotoPort);
                    if (gotoPort is null) continue;

                    var edge = data.Port.ConnectTo(gotoPort);
                    edge.capabilities = Capabilities.Ascendable | Capabilities.Selectable;
                    edge.edgeControl.interceptWidth = 0;  
                    AddElement(edge);
                }
            }

            // Refresh nodes representation.
            foreach (var node in scriptNodes)
            {
                node.RefreshPorts();
                node.RefreshExpandedState();
            }

            EditorUtility.ClearProgressBar();

            if (state.NodesState.Count == 0) // Auto align on first run.
                EditorApplication.delayCall += AutoAlign;
        }

        private void AutoAlign ()
        {
            if (scriptNodes.Count == 0) return;

            if (float.IsNaN(scriptNodes.First().resolvedStyle.width)) // Layout not ready.
            {
                EditorApplication.delayCall += AutoAlign;
                return;
            }

            var orphans = scriptNodes.Where(n =>
                !n.InputPorts.Any(p => p.Value.connections.Any(c => c.output.node != c.input.node)) &&
                !n.OutputPorts.Any(p => p.Port.connections.Any(c => c.output.node != c.input.node))).ToList();
            var nodes = scriptNodes.Except(orphans).TopologicalOrder(n => n.GetConnectedNodes(), false);

            var prevPos = Vector2.zero;
            foreach (var orphan in orphans)
            {
                var rect = orphan.GetPosition();
                var pos = new Vector2(0, prevPos.y + config.GraphAutoAlignPadding.y * 2);
                orphan.SetPosition(new Rect(pos, rect.size));
                prevPos = new Vector2(0, pos.y + orphan.resolvedStyle.height);
            }

            var maxOrphanWidth = orphans.Count > 0 ? orphans.Max(n => n.resolvedStyle.width) + config.GraphAutoAlignPadding.x : 0;
            prevPos = new Vector2(maxOrphanWidth, 0);
            foreach (var node in nodes)
            {
                var rect = node.GetPosition();
                var pos = new Vector2(prevPos.x + config.GraphAutoAlignPadding.x * 2, 0);
                node.SetPosition(new Rect(pos, rect.size));
                prevPos = new Vector2(pos.x + node.resolvedStyle.width, 0);
            }
        }
    }
}
