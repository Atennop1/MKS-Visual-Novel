// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Stores resource path to editor asset GUID map that is required by <see cref="EditorResourceProvider"/> when running under the Unity editor.
    /// </summary>
    /// <remarks>
    /// When user assign specific project assets for the resources via the editor menus (eg, sprites for character appearance or audio clips for BGM), the assigned asset references are stored in this asset.
    /// Before entering play mode in the editor, all the stored references are added to a <see cref="EditorResourceProvider"/> instance, which is included to the provider lists when the app is running under the editor.
    /// When building the player, the referenced assets are copied to a temp `Resources` folder; this allows the assets to be packaged with the build and makes them available for <see cref="ProjectResourceProvider"/>.
    /// </remarks>
    [Serializable]
    public class EditorResources : ScriptableObject
    {
        #pragma warning disable CS0649
        [Serializable]
        private class ResourceCategory
        {
            public string Id;
            public List<EditorResource> Resources;
        }
        #pragma warning restore CS0649

        [SerializeField] private List<ResourceCategory> resourceCategories = new List<ResourceCategory>();

        /// <summary>
        /// Loads an existing asset from package data folder or creates a new default instance.
        /// </summary>
        public static EditorResources LoadOrDefault ()
        {
            var fullPath = PathUtils.Combine(PackagePath.GeneratedDataPath, $"{nameof(EditorResources)}.asset");
            var assetPath = PathUtils.AbsoluteToAssetPath(fullPath);
            var obj = AssetDatabase.LoadAssetAtPath<EditorResources>(assetPath);
            if (!obj)
            {
                if (File.Exists(fullPath)) throw new UnityException("Unity failed to load an existing asset. Try restarting the editor.");
                obj = CreateInstance<EditorResources>();
                obj.AddBuiltinAssets();
                AssetDatabase.CreateAsset(obj, assetPath);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            return obj;
        }

        /// <summary>
        /// Finds a resource record by the corresponding asset GUID or null if not found.
        /// </summary>
        /// <param name="guid">GUID of the asset to look for.</param>
        public EditorResource? GetRecordByGuid (string guid)
        {
            foreach (var resourceCategory in resourceCategories)
            foreach (var resource in resourceCategory.Resources)
                if (resource.Guid.EqualsFast(guid))
                    return resource;
            return null;
        }

        /// <summary>
        /// Retrieves all the existing resources records in [path] -> [guid] map format.
        /// </summary>
        /// <param name="categoryId">When specified, will only fetch resources under the category.</param>
        /// <param name="skipEmpty">When enabled, will skip records where either path or guid is not defined.</param>
        public Dictionary<string, string> GetAllRecords (string categoryId = null, bool skipEmpty = true)
        {
            var records = new Dictionary<string, string>();

            foreach (var resourceCategory in resourceCategories)
                if (categoryId is null || resourceCategory.Id == categoryId)
                    foreach (var resource in resourceCategory.Resources)
                        if (!skipEmpty || (!string.IsNullOrEmpty(resource.Path) && !string.IsNullOrEmpty(resource.Guid)))
                            records[resource.Path] = resource.Guid;

            return records;
        }

        /// <summary>
        /// Attempts to find an added resource GUID based on its path (prefix + name).
        /// </summary>
        public string GetGuidByPath (string path)
        {
            var result = GetAllRecords().FirstOrDefault(kv => kv.Key == path);
            if (result.Key != path) return null;
            return result.Value;
        }

        /// <summary>
        /// Attempts to find an added resource path (prefix + name) based on its GUID.
        /// </summary>
        public string GetPathByGuid (string guid)
        {
            var result = GetAllRecords().FirstOrDefault(kv => kv.Value == guid);
            if (result.Value != guid) return null;
            return result.Key;
        }

        /// <summary>
        /// Checks whether a record with the provided parameters is added.
        /// </summary>
        public bool Exists (string name, string pathPrefix, string categoryId = default)
        {
            foreach (var category in resourceCategories)
                if ((categoryId is null || category.Id == categoryId)
                    && category.Resources.Any(r => r.PathPrefix == pathPrefix && r.Name == name))
                    return true;
            return false;
        }

        /// <summary>
        /// Adds a new record; don't forget to save the asset after the modification.
        /// </summary>
        public void AddRecord (string categoryId, string pathPrefix, string name, string guid)
        {
            var resource = new EditorResource(name, pathPrefix, guid);
            var category = resourceCategories.Find(c => c.Id == categoryId);
            if (category is null)
            {
                category = new ResourceCategory { Id = categoryId, Resources = new List<EditorResource>() };
                resourceCategories.Add(category);
            }
            category.Resources.Add(resource);
        }

        /// <summary>
        /// Removes a category with the provided ID and all the underlying records; don't forget to save the asset after the modification.
        /// </summary>
        public void RemoveCategory (string categoryId)
        {
            for (int i = 0; i < resourceCategories.Count; i++)
                if (resourceCategories[i].Id == categoryId)
                    resourceCategories.RemoveAt(i);
        }

        /// <summary>
        /// Removes all the records with provided GUID; don't forget to save the asset after the modification.
        /// </summary>
        public int RemoveAllRecordsWithGuid (string guid, string categoryId = null)
        {
            var removedCount = 0;
            foreach (var resourceCategory in resourceCategories)
                if (categoryId is null || resourceCategory.Id == categoryId)
                    removedCount += resourceCategory.Resources.RemoveAll(c => c.Guid == guid);
            return removedCount;
        }

        /// <summary>
        /// Removes all the records with provided path (prefix + name); don't forget to save the asset after the modification.
        /// </summary>
        public int RemoveAllRecordsWithPath (string pathPrefix, string name, string categoryId = null)
        {
            var removedCount = 0;
            foreach (var resourceCategory in resourceCategories)
                if (categoryId is null || resourceCategory.Id == categoryId)
                    removedCount += resourceCategory.Resources.RemoveAll(c => c.PathPrefix == pathPrefix && c.Name == name);
            return removedCount;
        }

        /// <inheritdoc cref="DrawPathPopup(Rect,SerializedProperty,string,string,string)"/>
        public void DrawPathPopup (SerializedProperty property, string categoryId = null, string pathPrefix = null, string emptyOption = null)
        {
            DrawPathPopup(EditorGUILayout.GetControlRect(), property, categoryId, pathPrefix, emptyOption);
        }

        /// <summary>
        /// Draws a dropdown selection list of strings fed by existing resource paths records.
        /// </summary>
        /// <param name="property">The property for which to assign value of the selected element.</param>
        /// <param name="categoryId">When specified, will only fetch resources under the category.</param>
        /// <param name="pathPrefix">When specified, will only fetch resources under the path prefix and trim the prefix from the values.</param>
        /// <param name="emptyOption">When specified, will include an additional option with the provided name and <see cref="string.Empty"/> value to the list.</param>
        public void DrawPathPopup (Rect rect, SerializedProperty property, string categoryId = null, string pathPrefix = null, string emptyOption = null)
        {
            const string allLiteral = "*";

            var menu = new GenericMenu();
            menu.allowDuplicateNames = true;
            if (emptyOption != null)
            {
                AddMenuItem(emptyOption);
                menu.AddSeparator("");
            }

            foreach (var category in resourceCategories)
                if (ShouldAddInCategory(category))
                    foreach (var resource in category.Resources)
                        AddResource(resource);

            if (!ShouldDrawMenu())
            {
                EditorGUI.PropertyField(rect, property, new GUIContent(property.displayName), true);
                return;
            }

            var label = EditorGUI.BeginProperty(Rect.zero, new GUIContent(property.displayName), property);
            rect = EditorGUI.PrefixLabel(rect, label);
            if (EditorGUI.DropdownButton(rect, GetSelectedLabel(), default))
                menu.DropDown(rect);
            EditorGUI.EndProperty();

            bool ShouldAddInCategory (ResourceCategory category)
            {
                return categoryId is null || category.Id == categoryId ||
                       categoryId.Contains(allLiteral) && category.Id.StartsWithFast(categoryId.GetBefore("*"));
            }

            void AddResource (EditorResource resource)
            {
                if (pathPrefix is null)
                {
                    AddMenuItem(resource.Path);
                    return;
                }

                var path = default(string);
                if (pathPrefix == allLiteral)
                    path = resource.Path.Contains("/") ? resource.Path.GetAfter("/") : resource.Path;
                else path = resource.Path.GetAfterFirst(pathPrefix + "/");
                if (!string.IsNullOrEmpty(path))
                    AddMenuItem(path);
            }

            void OnItemSelected (object item)
            {
                var path = (string)item;
                property.stringValue = path == emptyOption ? string.Empty : path;
                property.serializedObject.ApplyModifiedProperties();
            }

            bool ShouldDrawMenu () => menu.GetItemCount() > (emptyOption != null ? 2 : 0);
            void AddMenuItem (string path) => menu.AddItem(new GUIContent(path), IsSelected(path), OnItemSelected, path);
            bool IsSelected (string path) => path == GetSelected() || path == emptyOption && string.IsNullOrEmpty(GetSelected());
            string GetSelected () => property.stringValue;
            GUIContent GetSelectedLabel () => new GUIContent(string.IsNullOrEmpty(GetSelected()) ? emptyOption : GetSelected());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeEditorProvider ()
        {
            void InitializeProvider ()
            {
                ResourceProviderConfiguration.EditorProvider?.UnloadResources();
                var records = LoadOrDefault().GetAllRecords();
                var provider = new EditorResourceProvider();
                foreach (var record in records)
                    if (EditorUtils.AssetExistsByGuid(record.Value))
                        provider.AddResourceGuid(record.Key, record.Value);
                ResourceProviderConfiguration.EditorProvider = provider;
            }

            Engine.OnInitializationStarted -= InitializeProvider;
            Engine.OnInitializationStarted += InitializeProvider;
        }

        [ContextMenu("Add Built-In Assets")]
        private void AddBuiltinAssets ()
        {
            var config = default(ActorManagerConfiguration);

            config = ConfigurationSettings.LoadOrDefaultAndSave<TextPrintersConfiguration>();
            AddActorAsset(config, "Prefabs/TextPrinters/Dialogue.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/Fullscreen.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/Wide.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/Chat.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/Bubble.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/TMProDialogue.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/TMProFullscreen.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/TMProWide.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/TMProChat.prefab");
            AddActorAsset(config, "Prefabs/TextPrinters/TMProBubble.prefab");

            config = ConfigurationSettings.LoadOrDefaultAndSave<ChoiceHandlersConfiguration>();
            AddActorAsset(config, "Prefabs/ChoiceHandlers/ButtonList.prefab");
            AddActorAsset(config, "Prefabs/ChoiceHandlers/ButtonArea.prefab");

            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/Animate.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/DepthOfField.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/DigitalGlitch.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/Rain.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/ShakeBackground.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/ShakeCamera.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/ShakeCharacter.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/ShakePrinter.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/Snow.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/SunShafts.prefab");
            AddAsset(SpawnConfiguration.DefaultPathPrefix, "Prefabs/FX/Blur.prefab");

            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/ClickThroughPanel.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/BacklogUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/CGGalleryUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/ConfirmationUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/ContinueInputUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/ExternalScriptsUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/LoadingUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/MovieUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/RollbackUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/SaveLoadUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/SceneTransitionUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/SettingsUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/TipsUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/TitleUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/VariableInputUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/PauseUI.prefab");
            AddAsset(UIConfiguration.DefaultPathPrefix, "Prefabs/DefaultUI/ToastUI.prefab");

            void AddActorAsset (ActorManagerConfiguration managerConfig, string relativeAssetPath)
            {
                var actorId = Path.GetFileNameWithoutExtension(relativeAssetPath);
                var actorMeta = managerConfig.GetMetadataOrDefault(actorId);
                var category = $"{actorMeta.Loader.PathPrefix}/{actorMeta.Guid}";
                var pathPrefix = actorMeta.Loader.PathPrefix;
                var assetPath = $"{PathUtils.AbsoluteToAssetPath(PackagePath.PackageRootPath)}/{relativeAssetPath}";
                var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                AddRecord(category, pathPrefix, actorId, assetGuid);
            }

            void AddAsset (string categoryId, string relativeAssetPath)
            {
                var resourceName = Path.GetFileNameWithoutExtension(relativeAssetPath);
                var assetPath = $"{PathUtils.AbsoluteToAssetPath(PackagePath.PackageRootPath)}/{relativeAssetPath}";
                var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                AddRecord(categoryId, categoryId, resourceName, assetGuid);
            }
        }
    }
}
