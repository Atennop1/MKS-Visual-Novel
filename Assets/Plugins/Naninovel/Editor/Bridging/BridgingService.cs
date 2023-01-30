// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Naninovel.Bridging;
using Naninovel.UI;
using Naninovel.WebSocketSharp.Server;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public static class BridgingService
    {
        private static readonly Server server;
        private static readonly string metadataFilePath;

        static BridgingService ()
        {
            metadataFilePath = Path.Combine(PackagePath.GeneratedDataPath, "Metadata.xml");
            server = new Server($"{Application.productName} (Unity)", new BridgingListener());
            server.OnClientException += (e, _) => Debug.LogError($"Bridging: {e.Message}");
            server.OnClientConnected += c => c.Send(new UpdateMetadata { Metadata = LoadCachedMetadata() });
            server.Subscribe<GotoRequest>(HandleGotoRequest);
        }

        [InitializeOnLoadMethod]
        public static void RestartServer ()
        {
            StopServer();
            var config = ProjectConfigurationProvider.LoadOrDefault<EngineConfiguration>();
            if (!config.EnableBridging) return;
            if (config.AutoGenerateMetadata) GenerateMetadataOnEditorStart();
            StartServer(FindServerPort(config.ServerPort));
        }

        [MenuItem("Naninovel/Update Metadata %#u", priority = 3)]
        public static void UpdateMetadata ()
        {
            var meta = MetadataGenerator.GenerateProjectMetadata();
            var xml = MetadataGenerator.SerializeMetadata(meta);
            File.WriteAllText(metadataFilePath, xml);
            BroadcastMetadata(meta);
        }

        private static ProjectMetadata LoadCachedMetadata ()
        {
            if (!File.Exists(metadataFilePath)) return new ProjectMetadata();
            var xml = File.ReadAllText(metadataFilePath);
            return MetadataGenerator.DeserializeMetadata(xml);
        }

        private static void StartServer (int port)
        {
            server.Start(port);
            Engine.OnInitializationFinished += AttachServiceListeners;
            Engine.OnDestroyed += BroadcastPlaybackStopped;
            server.WaitForExit.AsUniTask().Forget();
        }

        private static void StopServer ()
        {
            server.StopAsync().AsUniTask().Forget();
            Engine.OnInitializationFinished -= AttachServiceListeners;
            Engine.OnDestroyed -= BroadcastPlaybackStopped;
        }

        private static void AttachServiceListeners ()
        {
            if (!(Engine.Behaviour is RuntimeBehaviour)) return;
            Engine.GetService<IScriptPlayer>().OnCommandExecutionStart += BroadcastPlayedCommand;
        }

        private static void HandleGotoRequest (GotoRequest request)
        {
            var script = request.PlaybackSpot.ScriptName;
            var line = request.PlaybackSpot.LineIndex;
            if (!Application.isPlaying) EditorApplication.EnterPlaymode();
            if (Engine.Initialized) OnEngineInit();
            else Engine.OnInitializationFinished += OnEngineInit;

            void OnEngineInit ()
            {
                Engine.OnInitializationFinished -= OnEngineInit;
                var player = Engine.GetService<IScriptPlayer>();
                if (player.PlayedScript != null && player.PlayedScript.Name == script)
                    player.RewindAsync(line).Forget();
                else
                    Engine.GetService<IStateManager>().ResetStateAsync()
                        .ContinueWith(() => player.PreloadAndPlayAsync(script))
                        .ContinueWith(() => Engine.GetService<IUIManager>().GetUI<ITitleUI>()?.Hide())
                        .ContinueWith(() => player.RewindAsync(line)).Forget();
            }
        }

        private static void BroadcastMetadata (ProjectMetadata meta)
        {
            var message = new UpdateMetadata { Metadata = meta };
            server.Broadcast(message);
        }

        private static void BroadcastPlayedCommand (Command command)
        {
            var status = new PlaybackStatus {
                Playing = true,
                PlayedSpot = GetPlaybackSpot(command)
            };
            server.Broadcast(new UpdatePlaybackStatus { PlaybackStatus = status });
        }

        private static Bridging.PlaybackSpot GetPlaybackSpot (Command command)
        {
            return new Bridging.PlaybackSpot {
                ScriptName = command.PlaybackSpot.ScriptName,
                LineIndex = command.PlaybackSpot.LineIndex,
                InlineIndex = command.PlaybackSpot.InlineIndex
            };
        }

        private static void BroadcastPlaybackStopped ()
        {
            var status = new PlaybackStatus { Playing = false };
            server.Broadcast(new UpdatePlaybackStatus { PlaybackStatus = status });
        }

        private static void GenerateMetadataOnEditorStart ()
        {
            const string key = "NaninovelMetaGeneratedAfterStart";
            if (SessionState.GetBool(key, false)) return;
            SessionState.SetBool(key, true);
            UpdateMetadata();
        }

        private static int FindServerPort (int preferredPort)
        {
            if (IsPortAvailableHack(preferredPort)) return preferredPort;
            var port = PortFinder.GetAvailablePort();
            Debug.LogWarning($"Default bridging port ({preferredPort}) is occupied. {port} will be used instead. " +
                             $"You can change the port or disable bridging in the Naninovel engine configuration.");
            return port;
        }

        private static bool IsPortAvailableHack (int port)
        {
            // IPGlobalProperties.GetActiveTcpListeners()
            // is not implemented in Unity (Mono?) for Mac,
            // so using this hack instead of PortFinder.IsPortAvailable()

            try
            {
                var server = new WebSocketServer(port);
                server.Start();
                server.Stop();
                return true;
            }
            catch (SocketException) { return false; }
            catch (InvalidOperationException) { return false; }
        }
    }
}
