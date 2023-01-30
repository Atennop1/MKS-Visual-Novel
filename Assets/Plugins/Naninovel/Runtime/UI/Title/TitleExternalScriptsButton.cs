// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    public class TitleExternalScriptsButton : ScriptableButton
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

            if (!Engine.GetConfiguration<ScriptsConfiguration>().EnableCommunityModding)
                gameObject.SetActive(false);
        }

        protected override void OnButtonClick () => uiManager.GetUI<IExternalScriptsUI>()?.Show();
    }
}
