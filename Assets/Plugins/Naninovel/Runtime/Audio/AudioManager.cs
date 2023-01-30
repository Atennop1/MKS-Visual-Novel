// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Naninovel
{
    /// <inheritdoc cref="IAudioManager"/>
    [InitializeAtRuntime]
    public class AudioManager : IStatefulService<SettingsStateMap>, IStatefulService<GameStateMap>, IAudioManager
    {
        [Serializable]
        public class Settings
        {
            public float MasterVolume;
            public float BgmVolume;
            public float SfxVolume;
            public float VoiceVolume;
            public string VoiceLocale;
            public List<NamedFloat> AuthorVolume;
        }

        [Serializable]
        public class GameState { public List<AudioClipState> BgmClips; public List<AudioClipState> SfxClips; }

        private class AuthorSource { public CharacterMetadata Metadata; public AudioSource Source; }

        public virtual AudioConfiguration Configuration { get; }
        public virtual AudioMixer AudioMixer { get; }
        public virtual float MasterVolume { get => GetMixerVolume(Configuration.MasterVolumeHandleName); set => SetMixerVolume(Configuration.MasterVolumeHandleName, value); }
        public virtual float BgmVolume { get => GetMixerVolume(Configuration.BgmVolumeHandleName); set { if (BgmGroupAvailable) SetMixerVolume(Configuration.BgmVolumeHandleName, value); } }
        public virtual float SfxVolume { get => GetMixerVolume(Configuration.SfxVolumeHandleName); set { if (SfxGroupAvailable) SetMixerVolume(Configuration.SfxVolumeHandleName, value); } }
        public virtual float VoiceVolume { get => GetMixerVolume(Configuration.VoiceVolumeHandleName); set { if (VoiceGroupAvailable) SetMixerVolume(Configuration.VoiceVolumeHandleName, value); } }
        public virtual string VoiceLocale { get => voiceLoader.OverrideLocale; set => voiceLoader.OverrideLocale = value; }
        public virtual IResourceLoader<AudioClip> AudioLoader => audioLoader;
        public virtual IResourceLoader<AudioClip> VoiceLoader => voiceLoader;

        protected virtual bool BgmGroupAvailable => bgmGroup;
        protected virtual bool SfxGroupAvailable => sfxGroup;
        protected virtual bool VoiceGroupAvailable => voiceGroup;

        private readonly IResourceProviderManager providerManager;
        private readonly ILocalizationManager localizationManager;
        private readonly ICharacterManager characterManager;
        private readonly Dictionary<string, AudioClipState> bgmMap = new Dictionary<string, AudioClipState>();
        private readonly Dictionary<string, AudioClipState> sfxMap = new Dictionary<string, AudioClipState>();
        private readonly Dictionary<string, float> authorVolume = new Dictionary<string, float>();
        private readonly Dictionary<string, AuthorSource> authorSources = new Dictionary<string, AuthorSource>();
        private AudioMixerGroup bgmGroup, sfxGroup, voiceGroup;
        private LocalizableResourceLoader<AudioClip> audioLoader, voiceLoader;
        private IAudioPlayer audioPlayer;
        private AudioClipState? voiceClip;

        public AudioManager (AudioConfiguration config, IResourceProviderManager providerManager, 
            ILocalizationManager localizationManager, ICharacterManager characterManager)
        {
            Configuration = config;
            this.providerManager = providerManager;
            this.localizationManager = localizationManager;
            this.characterManager = characterManager;

            AudioMixer = config.CustomAudioMixer ? config.CustomAudioMixer : Engine.LoadInternalResource<AudioMixer>("DefaultMixer");
        }

        public virtual UniTask InitializeServiceAsync ()
        {
            if (ObjectUtils.IsValid(AudioMixer))
            {
                bgmGroup = AudioMixer.FindMatchingGroups(Configuration.BgmGroupPath)?.FirstOrDefault();
                sfxGroup = AudioMixer.FindMatchingGroups(Configuration.SfxGroupPath)?.FirstOrDefault();
                voiceGroup = AudioMixer.FindMatchingGroups(Configuration.VoiceGroupPath)?.FirstOrDefault();
            }
            
            audioLoader = Configuration.AudioLoader.CreateLocalizableFor<AudioClip>(providerManager, localizationManager);
            voiceLoader = Configuration.VoiceLoader.CreateLocalizableFor<AudioClip>(providerManager, localizationManager);
            audioPlayer = (IAudioPlayer)Activator.CreateInstance(Type.GetType(Configuration.AudioPlayer));

            return UniTask.CompletedTask;
        }

        public virtual void ResetService ()
        {
            audioPlayer.StopAll();
            bgmMap.Clear();
            sfxMap.Clear();
            voiceClip = null;

            audioLoader?.ReleaseAll(this);
            voiceLoader?.ReleaseAll(this);
        }

        public virtual void DestroyService ()
        {
            if (audioPlayer is IDisposable disposable)
                disposable.Dispose();
            audioLoader?.ReleaseAll(this);
            voiceLoader?.ReleaseAll(this);
        }

        public virtual void SaveServiceState (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                MasterVolume = MasterVolume,
                BgmVolume = BgmVolume,
                SfxVolume = SfxVolume,
                VoiceVolume = VoiceVolume,
                VoiceLocale = VoiceLocale,
                AuthorVolume = authorVolume.Select(kv => new NamedFloat(kv.Key, kv.Value)).ToList()
            };
            stateMap.SetState(settings);
        }

        public virtual UniTask LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.GetState<Settings>();

            authorVolume.Clear();

            if (settings is null) // Apply default settings.
            {
                MasterVolume = Configuration.DefaultMasterVolume;
                BgmVolume = Configuration.DefaultBgmVolume;
                SfxVolume = Configuration.DefaultSfxVolume;
                VoiceVolume = Configuration.DefaultVoiceVolume;
                VoiceLocale = Configuration.VoiceLocales?.FirstOrDefault();
                return UniTask.CompletedTask;
            }

            MasterVolume = settings.MasterVolume;
            BgmVolume = settings.BgmVolume;
            SfxVolume = settings.SfxVolume;
            VoiceVolume = settings.VoiceVolume;
            VoiceLocale = Configuration.VoiceLocales?.Count > 0 ? settings.VoiceLocale ?? Configuration.VoiceLocales.First() : null;

            foreach (var item in settings.AuthorVolume)
                authorVolume[item.Name] = item.Value;

            return UniTask.CompletedTask;
        }

        public virtual void SaveServiceState (GameStateMap stateMap)
        {
            var state = new GameState { // Save only looped audio to prevent playing multiple clips at once when the game is (auto) saved in skip mode.
                BgmClips = bgmMap.Values.Where(s => IsBgmPlaying(s.Path) && s.Looped).ToList(),
                SfxClips = sfxMap.Values.Where(s => IsSfxPlaying(s.Path) && s.Looped).ToList()
            };
            stateMap.SetState(state);
        }

        public virtual async UniTask LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.GetState<GameState>() ?? new GameState();
            var tasks = new List<UniTask>();

            if (state.BgmClips != null && state.BgmClips.Count > 0)
            {
                foreach (var bgmPath in bgmMap.Keys.ToList())
                    if (!state.BgmClips.Exists(c => c.Path.EqualsFast(bgmPath)))
                        tasks.Add(StopBgmAsync(bgmPath));
                foreach (var clipState in state.BgmClips)
                    if (IsBgmPlaying(clipState.Path))
                        tasks.Add(ModifyBgmAsync(clipState.Path, clipState.Volume, clipState.Looped, 0));
                    else tasks.Add(PlayBgmAsync(clipState.Path, clipState.Volume, 0, clipState.Looped));
            }
            else tasks.Add(StopAllBgmAsync());

            if (state.SfxClips != null && state.SfxClips.Count > 0)
            {
                foreach (var sfxPath in sfxMap.Keys.ToList())
                    if (!state.SfxClips.Exists(c => c.Path.EqualsFast(sfxPath)))
                        tasks.Add(StopSfxAsync(sfxPath));
                foreach (var clipState in state.SfxClips)
                    if (IsSfxPlaying(clipState.Path))
                        tasks.Add(ModifySfxAsync(clipState.Path, clipState.Volume, clipState.Looped, 0));
                    else tasks.Add(PlaySfxAsync(clipState.Path, clipState.Volume, 0, clipState.Looped));
            }
            else tasks.Add(StopAllSfxAsync());

            await UniTask.WhenAll(tasks);
        }

        public virtual IReadOnlyCollection<string> GetPlayedBgmPaths () => bgmMap.Keys.Where(IsBgmPlaying).ToArray();

        public virtual IReadOnlyCollection<string> GetPlayedSfxPaths () => sfxMap.Keys.Where(IsSfxPlaying).ToArray();

        public virtual string GetPlayedVoicePath () => IsVoicePlaying(voiceClip?.Path) ? voiceClip?.Path : null;

        public virtual async UniTask<bool> AudioExistsAsync (string path) => await audioLoader.ExistsAsync(path);

        public virtual async UniTask<bool> VoiceExistsAsync (string path) => await voiceLoader.ExistsAsync(path);

        public virtual async UniTask ModifyBgmAsync (string path, float volume, bool loop, float time, AsyncToken asyncToken = default)
        {
            if (!bgmMap.ContainsKey(path)) return;

            bgmMap[path] = new AudioClipState(path, volume, loop);
            await ModifyAudioAsync(path, volume, loop, time, asyncToken);
        }

        public virtual async UniTask ModifySfxAsync (string path, float volume, bool loop, float time, AsyncToken asyncToken = default)
        {
            if (!sfxMap.ContainsKey(path)) return;

            sfxMap[path] = new AudioClipState(path, volume, loop);
            await ModifyAudioAsync(path, volume, loop, time, asyncToken);
        }

        public virtual void PlaySfxFast (string path, float volume = 1f, string group = default, bool restart = true, bool additive = true)
        {
            if (!audioLoader.IsLoaded(path))
                throw new Exception($"Failed to fast-play `{path}` SFX: the associated audio clip resource is not loaded.");
            var clip = audioLoader.GetLoadedOrNull(path);
            if (audioPlayer.IsPlaying(clip) && !restart && !additive) return;
            if (audioPlayer.IsPlaying(clip) && restart) audioPlayer.Stop(clip);
            audioPlayer.Play(clip, null, volume, false, FindAudioGroupOrDefault(group, sfxGroup), null, additive);
        }

        public virtual async UniTask PlayBgmAsync (string path, float volume = 1f, float fadeTime = 0f, bool loop = true, string introPath = null, string group = default, AsyncToken asyncToken = default)
        {
            var clipResource = await audioLoader.LoadAndHoldAsync(path, this);
            asyncToken.ThrowIfCanceled();
            if (!clipResource.Valid)
            {
                Debug.LogWarning($"Failed to play BGM `{path}`: resource not found.");
                return;
            }

            bgmMap[path] = new AudioClipState(path, volume, loop);

            var introClip = default(AudioClip);
            if (!string.IsNullOrEmpty(introPath))
            {
                var introClipResource = await audioLoader.LoadAndHoldAsync(introPath, this);
                asyncToken.ThrowIfCanceled();
                if (!introClipResource.Valid)
                    Debug.LogWarning($"Failed to load intro BGM `{path}`: resource not found.");
                else introClip = introClipResource.Object;
            }

            if (fadeTime <= 0) audioPlayer.Play(clipResource, null, volume, loop, FindAudioGroupOrDefault(group, bgmGroup), introClip);
            else await audioPlayer.PlayAsync(clipResource, fadeTime, null, volume, loop, FindAudioGroupOrDefault(group, bgmGroup), introClip, asyncToken: asyncToken);
        }

        public virtual async UniTask StopBgmAsync (string path, float fadeTime = 0f, AsyncToken asyncToken = default)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (bgmMap.ContainsKey(path))
                bgmMap.Remove(path);

            if (!audioLoader.IsLoaded(path)) return;
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (fadeTime <= 0) audioPlayer.Stop(clipResource);
            else await audioPlayer.StopAsync(clipResource, fadeTime, asyncToken);

            if (!IsBgmPlaying(path))
                audioLoader?.Release(path, this);
        }

        public virtual async UniTask StopAllBgmAsync (float fadeTime = 0f, AsyncToken asyncToken = default)
        {
            await UniTask.WhenAll(bgmMap.Keys.ToList().Select(p => StopBgmAsync(p, fadeTime, asyncToken)));
        }

        public virtual async UniTask PlaySfxAsync (string path, float volume = 1f, float fadeTime = 0f, bool loop = false, string group = default, AsyncToken asyncToken = default)
        {
            var clipResource = await audioLoader.LoadAndHoldAsync(path, this);
            asyncToken.ThrowIfCanceled();
            if (!clipResource.Valid)
            {
                Debug.LogWarning($"Failed to play SFX `{path}`: resource not found.");
                return;
            }

            sfxMap[path] = new AudioClipState(path, volume, loop);

            if (fadeTime <= 0) audioPlayer.Play(clipResource, null, volume, loop, FindAudioGroupOrDefault(group, sfxGroup));
            else await audioPlayer.PlayAsync(clipResource, fadeTime, null, volume, loop, FindAudioGroupOrDefault(group, sfxGroup), asyncToken: asyncToken);
        }

        public virtual async UniTask StopSfxAsync (string path, float fadeTime = 0f, AsyncToken asyncToken = default)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (sfxMap.ContainsKey(path))
                sfxMap.Remove(path);

            if (!audioLoader.IsLoaded(path)) return;
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (fadeTime <= 0) audioPlayer.Stop(clipResource);
            else await audioPlayer.StopAsync(clipResource, fadeTime, asyncToken);

            if (!IsSfxPlaying(path))
                audioLoader?.Release(path, this);
        }

        public virtual async UniTask StopAllSfxAsync (float fadeTime = 0f, AsyncToken asyncToken = default)
        {
            await UniTask.WhenAll(sfxMap.Keys.ToList().Select(p => StopSfxAsync(p, fadeTime, asyncToken)));
        }

        public virtual async UniTask PlayVoiceAsync (string path, float volume = 1f, string group = default, string authorId = default, AsyncToken asyncToken = default)
        {
            var clipResource = await voiceLoader.LoadAndHoldAsync(path, this);
            asyncToken.ThrowIfCanceled();
            if (!clipResource.Valid) return;

            if (Configuration.VoiceOverlapPolicy == VoiceOverlapPolicy.PreventOverlap)
                StopVoice();

            if (!string.IsNullOrEmpty(authorId))
            {
                var authorVolume = GetAuthorVolume(authorId);
                if (!Mathf.Approximately(authorVolume, -1))
                    volume *= authorVolume;
            }
            
            voiceClip = new AudioClipState(path, volume, false);

            var audioSource = !string.IsNullOrEmpty(authorId) ? GetOrInstantiateAuthorSource(authorId) : null;
            audioPlayer.Play(clipResource, audioSource, volume, false, FindAudioGroupOrDefault(group, voiceGroup));
        }

        public virtual async UniTask PlayVoiceSequenceAsync (IReadOnlyCollection<string> pathList, float volume = 1f, string group = default, AsyncToken asyncToken = default)
        {
            foreach (var path in pathList)
            {
                await PlayVoiceAsync(path, volume, group, asyncToken: asyncToken);
                await UniTask.WaitWhile(() => IsVoicePlaying(path) && asyncToken.EnsureNotCanceled());
            }
        }

        public virtual void StopVoice ()
        {
            if (!voiceClip.HasValue) return;

            var clipResource = voiceLoader.GetLoadedOrNull(voiceClip.Value.Path);
            audioPlayer.Stop(clipResource);
            voiceLoader.Release(voiceClip.Value.Path, this);
            voiceClip = null;
        }

        public virtual IAudioTrack GetAudioTrack (string path)
        {
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (clipResource is null || !clipResource.Valid) return null;
            return audioPlayer.GetTracks(clipResource.Object)?.FirstOrDefault();
        }

        public virtual IAudioTrack GetVoiceTrack (string path)
        {
            var clipResource = voiceLoader.GetLoadedOrNull(path);
            if (clipResource is null || !clipResource.Valid) return null;
            return audioPlayer.GetTracks(clipResource.Object)?.FirstOrDefault();
        }

        public virtual float GetAuthorVolume (string authorId)
        {
            if (string.IsNullOrEmpty(authorId)) return -1;
            else return authorVolume.TryGetValue(authorId, out var result) ? result : -1;
        }

        public virtual void SetAuthorVolume (string authorId, float volume)
        {
            if (string.IsNullOrEmpty(authorId)) return;
            authorVolume[authorId] = volume;
        }

        private bool IsAudioPlaying (string path)
        {
            if (!audioLoader.IsLoaded(path)) return false;
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (!clipResource.Valid) return false;
            return audioPlayer.GetTracks(clipResource)?.FirstOrDefault()?.Playing ?? false;
        }

        private async UniTask ModifyAudioAsync (string path, float volume, bool loop, float time, AsyncToken asyncToken = default)
        {
            if (!audioLoader.IsLoaded(path)) return;
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (!clipResource.Valid) return;
            var track = audioPlayer.GetTracks(clipResource)?.FirstOrDefault();
            if (track is null) return;
            track.Loop = loop;
            if (time <= 0) track.Volume = volume;
            else await track.FadeAsync(volume, time, asyncToken);
        }

        private float GetMixerVolume (string handleName)
        {
            float value;

            if (ObjectUtils.IsValid(AudioMixer))
            {
                AudioMixer.GetFloat(handleName, out value);
                value = MathUtils.DecibelToLinear(value);
            }
            else value = audioPlayer.Volume;

            return value;
        }

        private void SetMixerVolume (string handleName, float value)
        {
            if (ObjectUtils.IsValid(AudioMixer))
                AudioMixer.SetFloat(handleName, MathUtils.LinearToDecibel(value));
            else audioPlayer.Volume = value;
        }

        private AudioMixerGroup FindAudioGroupOrDefault (string path, AudioMixerGroup defaultGroup)
        {
            if (string.IsNullOrEmpty(path)) 
                return defaultGroup;
            var group = AudioMixer.FindMatchingGroups(path)?.FirstOrDefault();
            return ObjectUtils.IsValid(group) ? group : defaultGroup;
        }
        
        private bool IsBgmPlaying (string path)
        {
            if (string.IsNullOrEmpty(path) || !bgmMap.ContainsKey(path)) return false;
            return IsAudioPlaying(path);
        }

        private bool IsSfxPlaying (string path)
        {
            if (string.IsNullOrEmpty(path) || !sfxMap.ContainsKey(path)) return false;
            return IsAudioPlaying(path);
        }

        private bool IsVoicePlaying (string path)
        {
            if (!voiceClip.HasValue || voiceClip.Value.Path != path) return false;
            if (!voiceLoader.IsLoaded(path)) return false;
            var clipResource = voiceLoader.GetLoadedOrNull(path);
            if (!clipResource.Valid) return false;
            return audioPlayer.GetTracks(clipResource)?.FirstOrDefault()?.Playing ?? false;
        }

        private AudioSource GetOrInstantiateAuthorSource (string authorId)
        {
            if (authorSources.TryGetValue(authorId, out var authorSource))
            {
                if (!authorSource.Metadata.VoiceSource) return null;
                if (authorSource.Source) return authorSource.Source;
                return Instantiate();
            }
            else return Instantiate();
            
            AudioSource Instantiate ()
            {
                if (!characterManager.ActorExists(authorId)) return null;
                
                var metadata = characterManager.Configuration.GetMetadataOrDefault(authorId);
                var character = characterManager.GetActor(authorId) as MonoBehaviourActor<CharacterMetadata>;
                if (!metadata.VoiceSource || character is null)
                {
                    authorSources[authorId] = new AuthorSource { Metadata = metadata };
                    return null;
                }

                var source = UnityEngine.Object.Instantiate<AudioSource>(metadata.VoiceSource, character.GameObject.transform);
                authorSources[authorId] = new AuthorSource { Metadata = metadata, Source = source };
                return source;
            }
        }
    }
}
