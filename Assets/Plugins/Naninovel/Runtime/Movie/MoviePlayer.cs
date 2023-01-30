// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Naninovel
{
    /// <inheritdoc cref="IMoviePlayer"/>
    [InitializeAtRuntime]
    public class MoviePlayer : IMoviePlayer
    {
        public event Action OnMoviePlay;
        public event Action OnMovieStop;

        public virtual MoviesConfiguration Configuration { get; }
        public virtual bool Playing { get; private set; }

        protected virtual VideoPlayer Player { get; private set; }
        protected virtual bool UrlStreaming => Application.platform == RuntimePlatform.WebGLPlayer && !Application.isEditor;

        private readonly IInputManager inputManager;
        private readonly IResourceProviderManager providerManager;
        private readonly ILocalizationManager localeManager;
        private LocalizableResourceLoader<VideoClip> videoLoader;
        private string playedMovieName;
        private IInputSampler cancelInput;
        private string streamExtension;

        public MoviePlayer (MoviesConfiguration config, IResourceProviderManager providerManager, ILocalizationManager localeManager, IInputManager inputManager)
        {
            this.Configuration = config;
            this.providerManager = providerManager;
            this.localeManager = localeManager;
            this.inputManager = inputManager;
        }

        public virtual UniTask InitializeServiceAsync ()
        {
            videoLoader = Configuration.Loader.CreateLocalizableFor<VideoClip>(providerManager, localeManager);
            streamExtension = Engine.GetConfiguration<ResourceProviderConfiguration>().VideoStreamExtension;
            cancelInput = inputManager.GetCancel();

            Player = CreatePlayer();
            SetupAudioSource(Player);

            if (Configuration.SkipOnInput && cancelInput != null)
                cancelInput.OnStart += Stop;

            return UniTask.CompletedTask;
        }

        public virtual void ResetService ()
        {
            if (Playing) Stop();
            videoLoader?.ReleaseAll(this);
        }

        public virtual void DestroyService ()
        {
            if (Playing) Stop();
            if (Player) ObjectUtils.DestroyOrImmediate(Player.gameObject);
            if (cancelInput != null) cancelInput.OnStart -= Stop;
            videoLoader?.ReleaseAll(this);
        }

        public virtual async UniTask<Texture> PlayAsync (string movieName, AsyncToken asyncToken = default)
        {
            if (Playing) Stop();
            playedMovieName = movieName;
            SetIsPlaying(true);
            if (UrlStreaming) Player.url = BuildStreamUrl(movieName);
            else Player.clip = await LoadMovieClipAsync(movieName, asyncToken);
            await PreparePlayerAsync(asyncToken);
            Player.Play();
            return Player.texture;
        }

        public virtual void Stop ()
        {
            if (!Playing) return;

            if (Player) Player.Stop();
            videoLoader?.Release(playedMovieName, this);
            playedMovieName = null;
            SetIsPlaying(false);
        }

        public virtual async UniTask HoldResourcesAsync (string movieName, object holder)
        {
            if (UrlStreaming) return;
            await videoLoader.LoadAndHoldAsync(movieName, holder);
        }

        public virtual void ReleaseResources (string movieName, object holder)
        {
            if (UrlStreaming) return;
            videoLoader?.Release(movieName, holder);
        }

        protected virtual VideoPlayer CreatePlayer ()
        {
            var player = Engine.CreateObject<VideoPlayer>(nameof(MoviePlayer));
            player.playOnAwake = false;
            player.skipOnDrop = Configuration.SkipFrames;
            player.source = UrlStreaming ? VideoSource.Url : VideoSource.VideoClip;
            player.renderMode = VideoRenderMode.APIOnly;
            player.isLooping = false;
            player.loopPointReached += _ => Stop();
            return player;
        }

        protected virtual void SetupAudioSource (VideoPlayer player)
        {
            var audioManager = Engine.GetService<IAudioManager>();
            var audioSource = player.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.bypassReverbZones = true;
            audioSource.bypassEffects = true;
            audioSource.outputAudioMixerGroup = audioManager.AudioMixer.FindMatchingGroups("Master")[0];
            player.audioOutputMode = VideoAudioOutputMode.AudioSource;
            player.SetTargetAudioSource(0, audioSource);
        }

        protected virtual string BuildStreamUrl (string movieName)
        {
            var clipPath = $"{Configuration.Loader.PathPrefix}/{movieName}{streamExtension}";
            return PathUtils.Combine(Application.streamingAssetsPath, clipPath);
        }

        protected virtual async Task<VideoClip> LoadMovieClipAsync (string movieName, AsyncToken asyncToken)
        {
            var videoResource = await videoLoader.LoadAndHoldAsync(movieName, this);
            asyncToken.ThrowIfCanceled();
            if (!videoResource.Valid) throw new Exception($"Failed to load `{movieName}` movie.");
            return videoResource.Object;
        }

        protected virtual async UniTask PreparePlayerAsync (AsyncToken asyncToken)
        {
            Player.Prepare();
            while (!Player.isPrepared)
                await AsyncUtils.WaitEndOfFrameAsync(asyncToken);
        }

        private void SetIsPlaying (bool playing)
        {
            Playing = playing;
            if (playing) OnMoviePlay?.Invoke();
            else OnMovieStop?.Invoke();
        }
    }
}
