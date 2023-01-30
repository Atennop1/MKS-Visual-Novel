// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    [ExpressionFunctions]
    public static class ExpressionFunctions
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        public sealed class DocAttribute : Attribute
        {
            public string Description { get; }
            public string Example { get; }

            public DocAttribute (string description, string example = null)
            {
                Description = description;
                Example = example;
            }
        }

        [Doc("Return a random float number between min [inclusive] and max [inclusive].", "Random(0.1, 0.85)")]
        public static float Random (double min, double max) => UnityEngine.Random.Range((float)min, (float)max);

        [Doc("Return a random integer number between min [inclusive] and max [inclusive].", "Random(0, 100)")]
        public static int Random (int min, int max) => UnityEngine.Random.Range(min, max + 1);

        [Doc("Return a string chosen from one of the provided strings.", "Random(\"Foo\", \"Bar\", \"Foobar\")")]
        public static string Random (params string[] args) => args.Random();

        [Doc("Return a float number in 0.0 to 1.0 range, representing how many unique commands were ever executed compared to the total number of commands in all the available naninovel scripts. 1.0 means the player had `read through` or `seen` all the available game content. Make sure to enable `Count Total Commands` in the script configuration menu before using this function.", "CalculateProgress()")]
        public static float CalculateProgress ()
        {
            var scriptManager = Engine.GetService<IScriptManager>();
            var player = Engine.GetService<IScriptPlayer>();
            if (scriptManager.TotalCommandsCount == 0)
            {
                Debug.LogWarning("`CalculateProgress` script expression function were used, while to total number of script commands is zero. You've most likely disabled `UpdateActionCountOnInit` in the script player configuration menu or didn't add any naninovel scripts to the project resources.");
                return 0;
            }

            return player.PlayedCommandsCount / (float)scriptManager.TotalCommandsCount;
        }

        [Doc("Checks whether an unlockable item with the provided ID is currently unlocked.", "IsUnlocked(\"Tips/MyTip\")")]
        public static bool IsUnlocked (string unlockableId) => Engine.GetService<IUnlockableManager>()?.ItemUnlocked(unlockableId) ?? false;

        [Doc("Checks whether currently played command has ever been played before.", "HasPlayed()")]
        public static bool HasPlayed ()
        {
            var player = Engine.GetService<IScriptPlayer>();
            if (player?.PlayedScript is null) return false;

            var playedScriptName = player.PlayedScript.Name;
            var playedIndex = player.PlayedIndex;
            return player.HasPlayed(playedScriptName, playedIndex);
        }

        [Doc("Returns display name of a character actor with the provided ID.", "GetName(\"Kohaku\")")]
        public static string GetName (string id) => Engine.GetService<ICharacterManager>().GetDisplayName(id);
    }
}
