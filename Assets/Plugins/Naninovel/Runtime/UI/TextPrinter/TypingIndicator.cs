// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class TypingIndicator : MonoBehaviour
    {
        [SerializeField] private float printDotDelay = .5f;
        [SerializeField] private string typeSymbol = ". ";
        [SerializeField] private int symbolCount = 3;
        [SerializeField] private Text text = default;

        private float lastPrintDotTime = 0f;

        private void Awake ()
        {
            this.AssertRequiredObjects(text);
            text.text = string.Empty;
        }

        private void Update ()
        {
            if (Time.time < lastPrintDotTime + printDotDelay) return;

            lastPrintDotTime = Time.time;
            text.text = text.text.Length >= typeSymbol.Length * symbolCount ? string.Empty : text.text + typeSymbol;
        }
    }
}
