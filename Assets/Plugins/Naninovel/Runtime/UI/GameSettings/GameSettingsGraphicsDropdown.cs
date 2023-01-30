// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;
using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsGraphicsDropdown : ScriptableDropdown
    {
        [ManagedText("DefaultUI")]
        protected static string GraphicOption1 = "Very Low";
        [ManagedText("DefaultUI")]
        protected static string GraphicOption2 = "Low";
        [ManagedText("DefaultUI")]
        protected static string GraphicOption3 = "Medium";
        [ManagedText("DefaultUI")]
        protected static string GraphicOption4 = "High";
        [ManagedText("DefaultUI")]
        protected static string GraphicOption5 = "Very High";
        [ManagedText("DefaultUI")]
        protected static string GraphicOption6 = "Ultra";
    
        private ICameraManager cameraManager;

        protected override void Awake ()
        {
            base.Awake();

            cameraManager = Engine.GetService<ICameraManager>();
        }
        
        protected override void OnEnable ()
        {
            base.OnEnable();
            
            InitializeOptions();

            if (Engine.TryGetService<ILocalizationManager>(out var localeManager))
                localeManager.OnLocaleChanged += HandleLocaleChanged;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            
            if (Engine.TryGetService<ILocalizationManager>(out var localeManager))
                localeManager.OnLocaleChanged -= HandleLocaleChanged;
        }

        protected override void OnValueChanged (int value)
        {
            cameraManager.QualityLevel = value;
        }

        private void InitializeOptions ()
        {
            var availableOptions = QualitySettings.names.ToList();
            
            if (availableOptions.IsIndexValid(0))
                availableOptions[0] = GraphicOption1;
            if (availableOptions.IsIndexValid(1))
                availableOptions[1] = GraphicOption2;
            if (availableOptions.IsIndexValid(2))
                availableOptions[2] = GraphicOption3;
            if (availableOptions.IsIndexValid(3))
                availableOptions[3] = GraphicOption4;
            if (availableOptions.IsIndexValid(4))
                availableOptions[4] = GraphicOption5;
            if (availableOptions.IsIndexValid(5))
                availableOptions[5] = GraphicOption6;
            
            UIComponent.ClearOptions();
            UIComponent.AddOptions(availableOptions);
            UIComponent.value = cameraManager.QualityLevel;
            UIComponent.RefreshShownValue();
        }
        
        private void HandleLocaleChanged (string locale) => InitializeOptions();
    }
}
