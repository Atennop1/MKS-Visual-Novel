// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Can be provided to the script parser instead of error collection to log the errors.
    /// </summary>
    public class ScriptParseErrorLogger : ICollection<ScriptParseError>
    {
        public int Count { get; }
        public bool IsReadOnly => false;

        private static readonly Stack<ScriptParseErrorLogger> pool = new Stack<ScriptParseErrorLogger>();

        private readonly IEnumerator<ScriptParseError> enumerator = new List<ScriptParseError>.Enumerator();
        private string scriptName;

        private ScriptParseErrorLogger () { }

        public static ScriptParseErrorLogger GetFor (string scriptName)
        {
            var logger = pool.Count > 0 ? pool.Pop() : new ScriptParseErrorLogger();
            logger.scriptName = scriptName;
            return logger;
        }

        public static void Return (ScriptParseErrorLogger logger)
        {
            pool.Push(logger);
        }

        public void Add (ScriptParseError item) => Debug.LogError(item.ToString(scriptName));
        public bool Remove (ScriptParseError item) => true;
        public void Clear () { }
        public bool Contains (ScriptParseError item) => false;
        public void CopyTo (ScriptParseError[] array, int arrayIndex) { }
        public IEnumerator<ScriptParseError> GetEnumerator () => enumerator;
        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
    }
}
