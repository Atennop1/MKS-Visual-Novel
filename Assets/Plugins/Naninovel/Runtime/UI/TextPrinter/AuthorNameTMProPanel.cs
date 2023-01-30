// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using TMPro;
using UnityEngine;

namespace Naninovel.UI
{
    public class AuthorNameTMProPanel : AuthorNamePanel
    {
        public override string Text { get => TextComp ? TextComp.text : null; set { if (TextComp) TextComp.text = value ?? string.Empty; } }
        public override Color TextColor { get => TextComp ? TextComp.color : default; set { if (TextComp) TextComp.color = value; } }

        private TextMeshProUGUI TextComp => textCompCache ? textCompCache : (textCompCache = GetComponentInChildren<TextMeshProUGUI>());
        private TextMeshProUGUI textCompCache;
    }
}
