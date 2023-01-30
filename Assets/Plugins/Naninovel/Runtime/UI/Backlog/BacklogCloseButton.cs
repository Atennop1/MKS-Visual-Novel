// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    public class BacklogCloseButton : ScriptableLabeledButton
    {
        private BacklogPanel backlogPanel;

        protected override void Awake ()
        {
            base.Awake();

            backlogPanel = GetComponentInParent<BacklogPanel>();
        }

        protected override void OnButtonClick () => backlogPanel.Hide();
    }
}
