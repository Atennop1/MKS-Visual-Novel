// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Handles script hot reload feature.
    /// </summary>
    public static class HotReloadService
    {
        private static ScriptsConfiguration configuration;
        private static IScriptPlayer player;
        private static IScriptManager scriptManager;
        private static IStateManager stateManager;
        private static string[] playedLineHashes;

        /// <summary>
        /// Performs hot-reload of the currently played script.
        /// </summary>
        public static async UniTask ReloadPlayedScriptAsync ()
        {
            if (player?.Playlist is null || player.Playlist.Count == 0 || !player.PlayedScript)
            {
                Debug.LogError("Failed to perform hot reload: script player is not available or no script is currently played.");
                return;
            }

            var requireReload = string.IsNullOrEmpty(AssetDatabase.GetAssetPath(player.PlayedScript));
            if (requireReload) // In case the played script is stored outside of Unity project, force reload it.
            {
                var prevLineHashes = playedLineHashes; // Otherwise they're overridden when playing the loaded script below.
                var scriptName = player.PlayedScript.Name;
                scriptManager.UnloadScript(scriptName);
                var script = await scriptManager.LoadScriptAsync(scriptName);
                player.Play(script, player.PlaybackSpot.LineIndex, player.PlaybackSpot.InlineIndex);
                playedLineHashes = prevLineHashes;
            }

            var lastPlayedLineIndex = (player.PlayedCommand ?? player.Playlist.Last()).PlaybackSpot.LineIndex;

            // Find the first modified line in the updated script (before the played line).
            var rollbackIndex = -1;
            for (int i = 0; i < lastPlayedLineIndex; i++)
            {
                if (!player.PlayedScript.Lines.IsIndexValid(i)) // The updated script ends before the currently played line.
                {
                    rollbackIndex = player.Playlist.GetCommandBeforeLine(i - 1, 0)?.PlaybackSpot.LineIndex ?? 0;
                    break;
                }

                if (playedLineHashes?.IsIndexValid(i) ?? false)
                {
                    var oldLineHash = playedLineHashes[i];
                    var newLine = player.PlayedScript.Lines[i];
                    if (oldLineHash.EqualsFast(newLine.LineHash)) continue;
                }

                rollbackIndex = player.Playlist.GetCommandBeforeLine(i, 0)?.PlaybackSpot.LineIndex ?? 0;
                break;
            }

            if (rollbackIndex > -1) // Script has changed before the played line.
                // Rollback to the line before the first modified one.
                await stateManager.RollbackAsync(s => s.PlaybackSpot.LineIndex == rollbackIndex);

            // Update the playlist and play.
            var resumeLineIndex = rollbackIndex > -1 ? rollbackIndex : lastPlayedLineIndex;
            var playlist = new ScriptPlaylist(player.PlayedScript, scriptManager);
            var playlistIndex = player.Playlist.FindIndex(c => c.PlaybackSpot.LineIndex == resumeLineIndex);
            if (playlistIndex < 0)
                playlistIndex = 0;
            await playlist.PreloadResourcesAsync(playlistIndex, playlist.Count - 1);
            player.Play(playlist, playlistIndex);

            if (player.WaitingForInput)
                player.SetWaitingForInputEnabled(false);
        }

        [InitializeOnLoadMethod]
        private static void Initialize ()
        {
            ScriptFileWatcher.OnModified += HandleScriptModifiedAsync;
            Engine.OnInitializationFinished += HandleEngineInitialized;

            void HandleEngineInitialized ()
            {
                if (!(Engine.Behaviour is RuntimeBehaviour)) return;

                if (configuration is null)
                    configuration = ProjectConfigurationProvider.LoadOrDefault<ScriptsConfiguration>();

                scriptManager = Engine.GetService<IScriptManager>();
                player = Engine.GetService<IScriptPlayer>();
                stateManager = Engine.GetService<IStateManager>();
                player.OnPlay += HandleStartPlaying;
            }
        }

        private static void HandleStartPlaying (Script script)
        {
            playedLineHashes = script.Lines.Select(l => l.LineHash).ToArray();
        }

        private static async void HandleScriptModifiedAsync (string assetPath)
        {
            if (!Engine.Initialized || !(Engine.Behaviour is RuntimeBehaviour) || !configuration.HotReloadScripts ||
                !ObjectUtils.IsValid(player.PlayedScript) || player.Playlist?.Count == 0) return;

            var scriptAsset = AssetDatabase.LoadAssetAtPath<Script>(assetPath);
            if (!ObjectUtils.IsValid(scriptAsset)) return;

            if (player.PlayedScript.Name != scriptAsset.Name) return;

            await ReloadPlayedScriptAsync();
        }
    }
}
