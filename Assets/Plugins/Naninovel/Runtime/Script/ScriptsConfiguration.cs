// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    [EditInProjectSettings]
    public class ScriptsConfiguration : Configuration
    {
        public enum GraphOrientationType
        {
            Vertical,
            Horizontal
        }

        public const string DefaultPathPrefix = "Scripts";

        [Tooltip("Configuration of the resource loader used with naninovel script resources.")]
        public ResourceLoaderConfiguration Loader = new ResourceLoaderConfiguration { PathPrefix = DefaultPathPrefix };
        [Tooltip(nameof(IScriptParser) + " implementation to use for creating script assets from text. Don't forget to re-import script assets after modifying this property.")]
        public string ScriptParser = typeof(ScriptParser).AssemblyQualifiedName;
        [Tooltip("Name of the script to play right after the engine initialization.")]
        [ResourcePopup(DefaultPathPrefix)]
        public string InitializationScript = default;
        [Tooltip("Name of the script to play when showing the Title UI. Can be used to setup the title screen scene (background, music, etc).")]
        [ResourcePopup(DefaultPathPrefix)]
        public string TitleScript = default;
        [Tooltip("Name of the script to play when starting a new game. Will use first available when not provided.")]
        [ResourcePopup(DefaultPathPrefix, DefaultPathPrefix, null)]
        public string StartGameScript = default;
        [Tooltip("Whether to automatically add created naninovel scripts to the resources.")]
        public bool AutoAddScripts = true;
        [Tooltip("Whether to reload modified (both via visual and external editors) scripts and apply changes during playmode without restarting the playback.")]
        public bool HotReloadScripts = true;
        [Tooltip("Whether to run a file system watcher over `.nani` files in the project. Required to register script changes when edited with an external application.")]
        public bool WatchScripts = true;
        [Tooltip("When `Watch Scripts` is enabled, select a specific directory to watch instead of the whole project to reduce CPU usage.")]
        public string WatchedDirectory = string.Empty;
        [Tooltip("Whether to calculate number of commands existing in all the available naninovel scripts on service initialization. If you don't use `TotalCommandsCount` property of a script manager and `CalculateProgress` function in naninovel script expressions, disable to reduce engine initialization time.")]
        public bool CountTotalCommands = false;

        [Header("Visual Editor")]
        [Tooltip("Whether to show visual script editor when a script is selected.")]
        public bool EnableVisualEditor = true;
        [Tooltip("Whether to hide un-assigned parameters of the command lines when the line is not hovered or focused.")]
        public bool HideUnusedParameters = true;
        [Tooltip("Whether to automatically select currently played script when visual editor is open.")]
        public bool SelectPlayedScript = true;
        [Tooltip("Hot key used to show `Insert Line` window when the visual editor is in focus. Set to `None` to disable.")]
        public KeyCode InsertLineKey = KeyCode.Space;
        [Tooltip("Modifier for the `Insert Line Key`. Set to `None` to disable.")]
        public EventModifiers InsertLineModifier = EventModifiers.Control;
        [Tooltip("Hot key used to save (serialize) the edited script when the visual editor is in focus. Set to `None` to disable.")]
        public KeyCode SaveScriptKey = KeyCode.S;
        [Tooltip("Modifier for the `Save Script Key`. Set to `None` to disable.")]
        public EventModifiers SaveScriptModifier = EventModifiers.Control;
        [Tooltip("When clicked a line in visual editor, which mouse button should activate rewind: `0` is left, `1` right, `2` middle; set to `-1` to disable.")]
        public int RewindMouseButton = 0;
        [Tooltip("Modifier for `Rewind Mouse Button`. Set to `None` to disable.")]
        public EventModifiers RewindModifier = EventModifiers.Shift;
        [Tooltip("How many script lines should be rendered per visual editor page.")]
        public int EditorPageLength = 300;
        [Tooltip("Allows modifying default style of the visual editor.")]
        public StyleSheet EditorCustomStyleSheet = null;

        [Header("Script Graph")]
        [Tooltip("Whether to build the graph vertically or horizontally.")]
        public GraphOrientationType GraphOrientation = GraphOrientationType.Horizontal;
        [Tooltip("Padding to add for each node when performing auto align.")]
        public Vector2 GraphAutoAlignPadding = new Vector2(10, 0);
        [Tooltip("Whether to show fist comment lines of the script inside the graph node.")]
        public bool ShowSynopsis = true;
        [Tooltip("Allows modifying default style of the script graph.")]
        public StyleSheet GraphCustomStyleSheet = null;

        [Header("Community Modding")]
        [Tooltip("Whether to allow adding external naninovel scripts to the build.")]
        public bool EnableCommunityModding = false;
        [Tooltip("Configuration of the resource loader used with external naninovel script resources.")]
        public ResourceLoaderConfiguration ExternalLoader = new ResourceLoaderConfiguration {
            ProviderTypes = new List<string> { ResourceProviderConfiguration.LocalTypeName },
            PathPrefix = DefaultPathPrefix
        };

        [Header("Script Navigator")]
        [Tooltip("Whether to initialize script navigator to browse available naninovel scripts.")]
        public bool EnableNavigator = true;
        [Tooltip("Whether to show naninovel script navigator when script manager is initialized.")]
        public bool ShowNavigatorOnInit = false;
        [Tooltip("UI sort order of the script navigator.")]
        public int NavigatorSortOrder = 900;
    }
}
