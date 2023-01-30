// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsFontSizeDropdown : ScriptableDropdown
    {
        [ManagedText("DefaultUI")]
        protected static string Small = "Small";
        [ManagedText("DefaultUI")]
        protected static string Default = "Default";
        [ManagedText("DefaultUI")]
        protected static string Large = "Large";
        [ManagedText("DefaultUI")]
        protected static string ExtraLarge = "Extra Large";

        [Tooltip("Index of the dropdown list associated with default font size.")]
        [SerializeField] private int defaultSizeIndex = 1;

        private IUIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            uiManager = Engine.GetService<IUIManager>();
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

        protected override void OnValueChanged (int index)
        {
            base.OnValueChanged(index);

            uiManager.FontSize = index == defaultSizeIndex ? -1 : index;
        }

        protected virtual void InitializeOptions ()
        {
            var options = new List<string> { Small, Default, Large, ExtraLarge };
            UIComponent.ClearOptions();
            UIComponent.AddOptions(options);

            var index = uiManager.FontSize == -1 ? defaultSizeIndex : uiManager.FontSize;
            if (!UIComponent.options.IsIndexValid(index))
                throw new Exception($"Failed to initialize font size dropdown: current index `{index}` is not available in `Font Sizes` list.");
            UIComponent.value = index;
            UIComponent.RefreshShownValue();
        }
        
        private void HandleLocaleChanged (string locale) => InitializeOptions();
    }
}
