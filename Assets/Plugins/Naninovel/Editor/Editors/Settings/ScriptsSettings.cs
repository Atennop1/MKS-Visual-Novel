// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Naninovel
{
    public class ScriptsSettings : ResourcefulSettings<ScriptsConfiguration>
    {
        protected override string HelpUri => "guide/naninovel-scripts.html";

        protected override Type ResourcesTypeConstraint => typeof(Script);
        protected override string ResourcesCategoryId => Configuration.Loader.PathPrefix;
        protected override bool AllowRename => false;
        protected override string ResourcesSelectionTooltip => "Use `@goto %name%` in naninovel scripts to load and start playing selected naninovel script.";

        private static readonly string[] implementations, labels;

        static ScriptsSettings ()
        {
            InitializeImplementationOptions<IScriptParser>(ref implementations, ref labels);
        }

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(ScriptsConfiguration.ScriptParser)] = p => DrawImplementations(implementations, labels, p);
            drawers[nameof(ScriptsConfiguration.WatchScripts)] = p => OnChanged(ScriptFileWatcher.Initialize, p);
            drawers[nameof(ScriptsConfiguration.WatchedDirectory)] = p => DrawWhen(Configuration.WatchScripts,
                () => OnChanged(ScriptFileWatcher.Initialize, () => EditorUtils.FolderField(p)));
            drawers[nameof(ScriptsConfiguration.ExternalLoader)] = p => DrawWhen(Configuration.EnableCommunityModding, p);
            drawers[nameof(ScriptsConfiguration.ShowNavigatorOnInit)] = p => DrawWhen(Configuration.EnableNavigator, p);
            drawers[nameof(ScriptsConfiguration.NavigatorSortOrder)] = p => DrawWhen(Configuration.EnableNavigator, p);
            drawers[nameof(ScriptsConfiguration.HideUnusedParameters)] = p => DrawWhen(Configuration.EnableVisualEditor, p);
            drawers[nameof(ScriptsConfiguration.InsertLineKey)] = p => DrawWhen(Configuration.EnableVisualEditor, p);
            drawers[nameof(ScriptsConfiguration.InsertLineModifier)] = p => DrawWhen(Configuration.EnableVisualEditor, p);
            drawers[nameof(ScriptsConfiguration.SaveScriptKey)] = p => DrawWhen(Configuration.EnableVisualEditor, p);
            drawers[nameof(ScriptsConfiguration.SaveScriptModifier)] = p => DrawWhen(Configuration.EnableVisualEditor, p);
            drawers[nameof(ScriptsConfiguration.EditorPageLength)] = p => DrawWhen(Configuration.EnableVisualEditor, p);
            drawers[nameof(ScriptsConfiguration.EditorCustomStyleSheet)] = p => DrawWhen(Configuration.EnableVisualEditor, p);
            return drawers;
        }

        [MenuItem("Naninovel/Resources/Scripts")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
