// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    public class TitleCGGalleryButton : ScriptableButton
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

            var galleryUI = uiManager.GetUI<ICGGalleryUI>();
            if (galleryUI is null || galleryUI.CGCount == 0)
                gameObject.SetActive(false);
        }

        protected override void OnButtonClick () => uiManager.GetUI<ICGGalleryUI>()?.Show();
    }
}
