// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    public class CGGalleryReturnButton : ScriptableButton
    {
        private ICGGalleryUI cgGalleryUI;

        protected override void Awake ()
        {
            base.Awake();

            cgGalleryUI = GetComponentInParent<ICGGalleryUI>();
        }

        protected override void OnButtonClick () => cgGalleryUI.Hide();
    }
}
