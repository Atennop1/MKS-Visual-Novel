// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class PingPongInputIndicator : ScriptableUIComponent<RawImage>, IInputIndicator
    {
        protected virtual bool TintPingPong => tintPingPong;
        protected virtual Color PingColor => pingColor;
        protected virtual Color PongColor => pongColor;
        protected virtual float PingPongTime => pingPongTime;
        protected virtual float RevealTime => revealTime;

        [Tooltip("Whether to tint the image in ping and pong colors when visible.")]
        [SerializeField] private bool tintPingPong = true;
        [SerializeField] private Color pingColor = ColorUtils.ClearWhite;
        [SerializeField] private Color pongColor = Color.white;
        [SerializeField] private float pingPongTime = 1.5f;
        [SerializeField] private float revealTime = 0.5f;

        private float showTime;

        public override void Show ()
        {
            showTime = Time.time;
            ChangeVisibilityAsync(true, revealTime).Forget();
        }

        public override void Hide () => Visible = false;

        protected override void Update ()
        {
            base.Update();

            if (Visible && tintPingPong)
                UIComponent.color = Color.Lerp(pingColor, pongColor, Mathf.PingPong(Time.time - showTime, pingPongTime));
        }
    } 
}
