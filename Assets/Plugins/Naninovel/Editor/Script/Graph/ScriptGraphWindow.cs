// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Naninovel
{
    /// <summary>
    /// Hosts <see cref="ScriptGraphView"/>.
    /// </summary>
    public class ScriptGraphWindow : EditorWindow
    {
        private ScriptGraphView graphView;

        [MenuItem("Naninovel/Script Graph", priority = 2)]
        public static void OpenWindow ()
        {
            GetWindow<ScriptGraphWindow>("Script Graph", true);
        }

        private void OnEnable ()
        {
            var config = ProjectConfigurationProvider.LoadOrDefault<ScriptsConfiguration>();
            var state = ScriptGraphState.LoadOrDefault();
            var resources = EditorResources.LoadOrDefault();
            var records = resources.GetAllRecords(config.Loader.PathPrefix).ToArray();
            var scripts = new List<Script>();
            for (int i = 0; i < records.Length; i++)
            {
                var record = records[i];
                var progress = i / (float)records.Length;
                EditorUtility.DisplayProgressBar(ScriptGraphView.ProgressBarTitle, $"Loading `{record.Key}` script...", progress);

                var guid = record.Value;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                var script = AssetDatabase.LoadAssetAtPath<Script>(path);
                if (!ObjectUtils.IsValid(script)) continue;
                scripts.Add(script);
            }

            graphView = new ScriptGraphView(config, state, scripts);
            graphView.name = "Script Graph";
            rootVisualElement.Add(graphView);
            graphView.StretchToParentSize();
        }

        private void OnDisable ()
        {
            graphView?.SerializeState();
            rootVisualElement.Remove(graphView);
        }
    }
}
