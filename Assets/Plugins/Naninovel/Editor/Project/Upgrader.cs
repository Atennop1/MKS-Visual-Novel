// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

// using System;
// using System.IO;
// using System.Linq;
// using System.Text;
// using UnityEditor;
// using UnityEngine;
//
// namespace Naninovel
// {
//     public static class Upgrader
//     {
//         [MenuItem("Naninovel/Upgrade/v1.12 to v1.13")]
//         private static void Upgrade112To113 ()
//         {
//             if (!EditorUtility.DisplayDialog("Perform upgrade?",
//                 "Are you sure you want to perform v1.12-v1.13 upgrade? Configuration assets will be modified. Make sure to perform a backup before confirming.",
//                 "Upgrade", "Cancel")) return;
//             
//             // Handle LayeredActorBehaviour replaced with LayeredBackgroundBehaviour and LayeredCharacterBehaviour.  
//             try
//             {
//                 const string layeredBehaviourComponentGuid = "0ed8eaa5eef74e849a7f97276a748279";
//                 const string layeredCharacterComponentGuid = "3645880df9c1965479dfe05f712a1711";
//                 const string layeredBackgroundComponentGuid = "5fd416f37425423409b956ac79ed74bc";
//                 var editorResources = EditorResources.LoadOrDefault();
//                 var records = editorResources.GetAllRecords().ToArray();
//                 for (int i = 0; i < records.Length; i++)
//                 {
//                     var resourcePath = records[i].Key;
//                     var resourceGuid = records[i].Value;
//                     var assetPath = AssetDatabase.GUIDToAssetPath(resourceGuid);
//                     if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath)) continue;
//                     if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(GameObject)) continue;
//                     EditorUtility.DisplayProgressBar("Upgrading project to Naninovel v1.13", $"Processing `{assetPath}`", i / (float)records.Length);
//                     var assetText = File.ReadAllText(assetPath);
//                     if (!assetText.Contains(layeredBehaviourComponentGuid)) continue;
//                     var isCharacter = resourcePath.Contains(CharactersConfiguration.DefaultPathPrefix);
//                     var isBackground = resourcePath.Contains(BackgroundsConfiguration.DefaultPathPrefix);
//                     if (!isCharacter && !isBackground) continue;
//                     assetText = assetText.Replace(layeredBehaviourComponentGuid, isCharacter ? layeredCharacterComponentGuid : layeredBackgroundComponentGuid);
//                     File.WriteAllText(assetPath, assetText);
//                     Debug.Log($"Upgrader: Replaced `LayeredActorBehaviour` component on `{assetPath}`.");
//                 }
//             }
//             finally { EditorUtility.ClearProgressBar(); }
//
//             AssetDatabase.Refresh();
//             AssetDatabase.SaveAssets();
//         }
//     }
// }
