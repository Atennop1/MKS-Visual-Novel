// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(Text))]
    public class EngineVersionText : MonoBehaviour
    {
        private void Start ()
        {
            var version = EngineVersion.LoadFromResources();
            GetComponent<Text>().text = $"Naninovel {version.Version}{Environment.NewLine}Build {version.Build}";
        }

    }
}
