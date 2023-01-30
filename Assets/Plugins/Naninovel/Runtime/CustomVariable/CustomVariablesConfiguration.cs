// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    [EditInProjectSettings]
    public class CustomVariablesConfiguration : Configuration
    {
        /// <summary>
        /// Custom variable name prefix (case-insensitive) used to indicate a global variable.
        /// </summary>
        public const string GlobalPrefix = "G_";

        [Tooltip("The list of variables to initialize by default. Global variables (names starting with `G_` or `g_`) are initialized on first application start, and others on each state reset.")]
        public List<CustomVariable> PredefinedVariables = new List<CustomVariable>();

        /// <summary>
        /// Checks whether a custom variable with the provided name is global.
        /// </summary>
        public static bool IsGlobalVariable (string name) => name.StartsWith(GlobalPrefix, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Attempts to parse the provided value string into float (the string should contain a dot), integer and then boolean.
        /// When parsing fails will return the initial string.
        /// </summary>
        public static object ParseVariableValue (string value)
        {
            if (value.Contains(".") && ParseUtils.TryInvariantFloat(value, out var floatValue)) return floatValue;
            else if (ParseUtils.TryInvariantInt(value, out var intValue)) return intValue;
            else if (bool.TryParse(value, out var boolValue)) return boolValue;
            else return value;
        }
    }
}
