// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    public static class TransitionUtils
    {
        public static readonly string DefaultTransition = TransitionType.Crossfade.ToString();

        private const string keywordPrefix = "NANINOVEL_TRANSITION_";
        private const string crossfade = "Crossfade";

        private static readonly Dictionary<string, Vector4> nameToDefaultParamsMap = new Dictionary<string, Vector4>(StringComparer.OrdinalIgnoreCase) {
            [TransitionType.Crossfade.ToString()] = Vector4.zero,
            [TransitionType.BandedSwirl.ToString()] = new Vector4(5, 10),
            [TransitionType.Blinds.ToString()] = new Vector4(6, 0),
            [TransitionType.CircleReveal.ToString()] = new Vector4(.25f, 0),
            [TransitionType.CircleStretch.ToString()] = Vector4.zero,
            [TransitionType.CloudReveal.ToString()] = Vector4.zero,
            [TransitionType.Crumble.ToString()] = Vector4.zero,
            [TransitionType.Dissolve.ToString()] = new Vector4(99999, 0),
            [TransitionType.DropFade.ToString()] = Vector4.zero,
            [TransitionType.LineReveal.ToString()] = new Vector4(.025f, .5f, .5f),
            [TransitionType.Pixelate.ToString()] = Vector4.zero,
            [TransitionType.RadialBlur.ToString()] = Vector4.zero,
            [TransitionType.RadialWiggle.ToString()] = Vector4.zero,
            [TransitionType.RandomCircleReveal.ToString()] = Vector4.zero,
            [TransitionType.Ripple.ToString()] = new Vector4(20f, 10f, .05f),
            [TransitionType.RotateCrumble.ToString()] = Vector4.zero,
            [TransitionType.Saturate.ToString()] = Vector4.zero,
            [TransitionType.Shrink.ToString()] = new Vector4(200, 0),
            [TransitionType.SlideIn.ToString()] = new Vector4(1, 0),
            [TransitionType.SwirlGrid.ToString()] = new Vector4(15, 10),
            [TransitionType.Swirl.ToString()] = new Vector4(15, 0),
            [TransitionType.Water.ToString()] = Vector4.zero,
            [TransitionType.Waterfall.ToString()] = Vector4.zero,
            [TransitionType.Wave.ToString()] = new Vector4(.1f, 14, 20),
            [TransitionType.Custom.ToString()] = new Vector4(0, 0)
        };

        /// <summary>
        /// Converts provided transition name to corresponding shader keyword.
        /// Transition effect names are case-insensitive.
        /// </summary>
        public static string ToShaderKeyword (string transition)
        {
            return string.Concat(keywordPrefix, transition.ToUpperInvariant());
        }

        /// <summary>
        /// Attempts to find default transition parameters for transition effect with the provided name;
        /// returns <see cref="Vector4.zero"/> when not found. Transition effect names are case-insensitive.
        /// </summary>
        public static Vector4 GetDefaultParams (string transition)
        {
            return nameToDefaultParamsMap.TryGetValue(transition, out var result) ? result : Vector4.zero;
        }

        /// <summary>
        /// Attempts to find which transition effect is currently enabled in the provided material by checking enabled keywords;
        /// returns <see cref="TransitionType.Crossfade"/> when no transition keyword is enabled or found.
        /// </summary>
        public static string GetEnabled (Material material)
        {
            for (int i = 0; i < material.shaderKeywords.Length; i++)
                if (material.shaderKeywords[i].StartsWith(keywordPrefix) && material.IsKeywordEnabled(material.shaderKeywords[i]))
                    return material.shaderKeywords[i].GetAfter(keywordPrefix);
            return crossfade; // Crossfade is executed by default when no keywords enabled.
        }

        /// <summary>
        /// Enables a shader keyword corresponding to transition effect with the provided name in the provided material.
        /// Transition effect names are case-insensitive.
        /// </summary>
        public static void EnableKeyword (Material material, string transition)
        {
            for (int i = 0; i < material.shaderKeywords.Length; i++)
                if (material.shaderKeywords[i].StartsWith(keywordPrefix) && material.IsKeywordEnabled(material.shaderKeywords[i]))
                    material.DisableKeyword(material.shaderKeywords[i]);

            // Crossfade is executed when no transition keywords enabled.
            if (transition.EqualsFastIgnoreCase(crossfade)) return;

            var keyword = ToShaderKeyword(transition);
            material.EnableKeyword(keyword);
        }
    }
}
