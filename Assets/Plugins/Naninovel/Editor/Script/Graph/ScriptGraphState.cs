// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of <see cref="ScriptGraphView"/>.
    /// </summary>
    [Serializable]
    public class ScriptGraphState : ScriptableObject
    {
        public List<ScriptGraphNodeState> NodesState = new List<ScriptGraphNodeState>();

        /// <summary>
        /// Loads an existing asset from package data folder or creates a new default instance.
        /// </summary>
        public static ScriptGraphState LoadOrDefault ()
        {
            var fullPath = PathUtils.Combine(PackagePath.GeneratedDataPath, $"{nameof(ScriptGraphState)}.asset");
            var assetPath = PathUtils.AbsoluteToAssetPath(fullPath);
            var obj = AssetDatabase.LoadAssetAtPath<ScriptGraphState>(assetPath);
            if (!obj)
            {
                if (File.Exists(fullPath)) throw new UnityException("Unity failed to load an existing asset. Try restarting the editor.");
                obj = CreateInstance<ScriptGraphState>();
                AssetDatabase.CreateAsset(obj, assetPath);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            return obj;
        }
    }
}
