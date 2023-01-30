// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Stores data about played script commands.
    /// </summary>
    [System.Serializable]
    public class PlayedScriptRegister
    {
        [System.Serializable]
        private class PlayedScript
        {
            public string ScriptName;
            public List<IntRange> PlayedIndexes;

            public PlayedScript (string scriptName)
            {
                ScriptName = scriptName;
                PlayedIndexes = new List<IntRange>();
            }

            public void AddIndex (int index)
            {
                for (int i = 0; i < PlayedIndexes.Count; i++)
                {
                    var range = PlayedIndexes[i];
                    if (range.Contains(index)) return;
                    if (range.StartIndex == (index + 1)) { PlayedIndexes[i] = new IntRange(index, range.EndIndex); return; }
                    if (range.EndIndex == (index - 1)) { PlayedIndexes[i] = new IntRange(range.StartIndex, index); return; }
                }
                PlayedIndexes.Add(new IntRange(index, index));
            }

            public bool ContainsIndex (int index)
            {
                for (int i = 0; i < PlayedIndexes.Count; i++)
                    if (PlayedIndexes[i].Contains(index)) return true;
                return false;
            }
        }

        [SerializeField] private List<PlayedScript> playedScripts = new List<PlayedScript>();

        public void RegisterPlayedIndex (string scriptName, int playlistIndex)
        {
            if (IsIndexPlayed(scriptName, playlistIndex)) return;
            var data = GetOrCreateDataForScript(scriptName);
            data.AddIndex(playlistIndex);
        }

        public bool IsIndexPlayed (string scriptName, int playlistIndex)
        {
            var data = GetOrCreateDataForScript(scriptName);
            return data.ContainsIndex(playlistIndex);
        }
        
        public bool IsScriptPlayed (string scriptName)
        {
            foreach (var script in playedScripts)
                if (script.ScriptName == scriptName)
                    return true;
            return false;
        }

        public int CountPlayed ()
        {
            var counter = 0;
            foreach (var script in playedScripts)
                foreach (var range in script.PlayedIndexes)
                    counter += range.EndIndex - range.StartIndex + 1;
            return counter;
        }

        private PlayedScript GetOrCreateDataForScript (string scriptName)
        {
            for (int i = 0; i < playedScripts.Count; i++)
                if (playedScripts[i].ScriptName == scriptName) return playedScripts[i];
            var result = new PlayedScript(scriptName);
            playedScripts.Add(result);
            return result;
        }
    }
}
