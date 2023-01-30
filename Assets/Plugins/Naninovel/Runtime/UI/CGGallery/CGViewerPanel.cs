// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class CGViewerPanel : ScriptableButton
    {
        protected virtual string ShaderName { get; } = "Naninovel/TransitionalUI";

        [Tooltip("The image where the assigned CGs will be shown.")]
        [SerializeField] private RawImage contentImage = default;
        [Tooltip("When multiple CGs assigned, controls crossfade duration, in seconds.")]
        [SerializeField] private float crossfadeDuration = .3f;

        private readonly Queue<Texture2D> textureQueue = new Queue<Texture2D>();
        private ImageCrossfader crossfader;

        public virtual void Show (IEnumerable<Texture2D> textures)
        {
            EnqueueTextures(textures);
            ShowNextTexture(0);
            base.Show();
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(contentImage);
            crossfader = new ImageCrossfader(contentImage);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();
            crossfader?.Dispose();
        }

        protected override void OnButtonClick ()
        {
            if (textureQueue.Count > 0)
                ShowNextTexture(crossfadeDuration);
            else Hide();
        }

        private void EnqueueTextures (IEnumerable<Texture2D> textures)
        {
            textureQueue.Clear();
            foreach (var texture in textures)
                if (texture != null)
                    textureQueue.Enqueue(texture);
        }

        private void ShowNextTexture (float duration)
        {
            var texture = textureQueue.Dequeue();
            crossfader.Crossfade(texture, duration);
        }
    }
}
