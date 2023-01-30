// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="ScriptParseError"/>.
    /// </summary>
    public static class ScriptParseErrorExtensions
    {
        /// <summary>
        /// Logs all the errors in the collection relative to script with the provided name.
        /// </summary>
        public static void Log (this IEnumerable<ScriptParseError> errors, string scriptName)
        {
            foreach (var error in errors)
                Debug.LogError(error.ToString(scriptName));
        }

        /// <summary>
        /// Adds provided error to the collection.
        /// </summary>
        public static void Add (this ICollection<ScriptParseError> errors, string description, int lineIndex)
        {
            var error = new ScriptParseError(lineIndex, description);
            errors.Add(error);
        }
    }
}
