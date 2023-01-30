// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to create <see cref="Script"/> asset from text string.
    /// </summary>
    public interface IScriptParser
    {
        /// <summary>
        /// Creates a new script instance by parsing the provided script text.
        /// </summary>
        /// <param name="scriptName">Name of the script asset.</param>
        /// <param name="scriptText">The script text to parse.</param>
        /// <param name="errors">When provided and error occurs while parsing, will add the error to the collection.</param>
        Script ParseText (string scriptName, string scriptText, ICollection<ScriptParseError> errors = null);
    }
}
