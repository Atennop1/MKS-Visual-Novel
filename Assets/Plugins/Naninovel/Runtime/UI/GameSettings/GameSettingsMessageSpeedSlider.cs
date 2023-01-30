// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsMessageSpeedSlider : ScriptableSlider
    {
        protected virtual bool PreviewPrinterAvailable => ObjectUtils.IsValid(previewPrinter) && previewPrinter.isActiveAndEnabled;
        protected GameSettingsPreviewPrinter PreviewPrinter => previewPrinter;

        [SerializeField] private GameSettingsPreviewPrinter previewPrinter = default;

        private ITextPrinterManager printerManager;

        protected override void Awake ()
        {
            base.Awake();

            printerManager = Engine.GetService<ITextPrinterManager>();
            UIComponent.value = printerManager.BaseRevealSpeed;
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            if (PreviewPrinterAvailable)
                previewPrinter.Show();
        }

        protected override void OnDisable ()
        {
            if (PreviewPrinterAvailable)
                previewPrinter.Hide();

            base.OnDisable();
        }

        protected override void OnValueChanged (float value)
        {
            printerManager.BaseRevealSpeed = value;

            if (PreviewPrinterAvailable)
                previewPrinter.StartPrinting();
        }
    }
}
