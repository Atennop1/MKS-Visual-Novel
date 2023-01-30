// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    public class SaveLoadMenuReturnButton : ScriptableButton
    {
        private SaveLoadMenu saveLoadMenu;

        protected override void Awake ()
        {
            base.Awake();

            saveLoadMenu = GetComponentInParent<SaveLoadMenu>();
        }

        protected override void OnButtonClick () => saveLoadMenu.Hide();
    }
}
