// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public class ControlPanelAutoPlayButton : ScriptableLabeledButton
    {
        [SerializeField] private Color activeColorMultiplier = Color.red;

        private IScriptPlayer player;

        protected override void Awake ()
        {
            base.Awake();

            player = Engine.GetService<IScriptPlayer>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();
            player.OnAutoPlay += HandleAutoModeChange;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            player.OnAutoPlay -= HandleAutoModeChange;
        }

        protected override void OnButtonClick ()
        {
            player.SetAutoPlayEnabled(!player.AutoPlayActive);
        }

        private void HandleAutoModeChange (bool enabled)
        {
            UIComponent.LabelColorMultiplier = enabled ? activeColorMultiplier : Color.white;
        }
    } 
}
