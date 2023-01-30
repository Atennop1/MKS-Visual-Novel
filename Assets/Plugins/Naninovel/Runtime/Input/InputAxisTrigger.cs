// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class InputAxisTrigger
    {
        [Tooltip("Name of the axis.")]
        public string AxisName = string.Empty;
        [Tooltip("Whether trigger should happen when axis value is positive, negative or both.")]
        public InputAxisTriggerMode TriggerMode = InputAxisTriggerMode.Both;
        [Tooltip("When axis value is below or equal to this value, the trigger won't be activated."), Range(0, .999f)]
        public float TriggerTolerance = .001f;

        /// <summary>
        /// Returns the current axis value when it's above the trigger tolerance; zero otherwise.
        /// </summary>
        public float Sample ()
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            if (string.IsNullOrEmpty(AxisName)) return 0;

            var value = Input.GetAxis(AxisName);

            if (TriggerMode == InputAxisTriggerMode.Positive && value <= 0) return 0;
            if (TriggerMode == InputAxisTriggerMode.Negative && value >= 0) return 0;

            if (Mathf.Abs(value) < TriggerTolerance) return 0;

            return value;
            #else
            return 0;
            #endif
        }
    }
}
