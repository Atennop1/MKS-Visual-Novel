// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    public class TitleTipsButton : ScriptableButton
    {
        private IUIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            uiManager = Engine.GetService<IUIManager>();
        }

        protected override void Start ()
        {
            base.Start();

            var tipsUI = uiManager.GetUI<ITipsUI>();
            if (tipsUI is null || tipsUI.TipsCount == 0)
                gameObject.SetActive(false);
        }

        protected override void OnButtonClick () => uiManager.GetUI<ITipsUI>()?.Show();
    }
}
