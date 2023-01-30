// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

#if UNITY_GOOGLE_DRIVE_AVAILABLE

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityGoogleDrive;

namespace Naninovel
{
    public class ResourcesMigrationWindow : EditorWindow
    {
        protected string LocalPath { get => PlayerPrefs.GetString(localPathKey); set => PlayerPrefs.SetString(localPathKey, value); }
        protected string RemotePath { get => PlayerPrefs.GetString(remotePathKey); set => PlayerPrefs.SetString(remotePathKey, value); }

        private const string localPathKey = "Naninovel." + nameof(ResourcesMigrationWindow) + "." + nameof(LocalPath);
        private const string remotePathKey = "Naninovel." + nameof(ResourcesMigrationWindow) + "." + nameof(RemotePath);

        private bool isOperationCanceled;

        [MenuItem("Naninovel/Tools/Resources Migration")]
        private static void OpenWindow ()
        {
            var position = new Rect(100, 100, 500, 175);
            GetWindowWithRect<ResourcesMigrationWindow>(position, true, "Resources Migration", true);
        }

        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Naninovel Resources Migration", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The tool to transfer resources between project and Google Drive providers.", EditorStyles.miniLabel);

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                LocalPath = EditorGUILayout.TextField("Local Path", LocalPath);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                    LocalPath = EditorUtility.OpenFolderPanel("Local Path", "", "");
            }

            RemotePath = EditorGUILayout.TextField("Remote Path", RemotePath);

            using (new EditorGUI.DisabledGroupScope(string.IsNullOrWhiteSpace(LocalPath) || string.IsNullOrWhiteSpace(RemotePath)))
            {
                EditorGUILayout.HelpBox("Files with equal path will be overwritten in process.", MessageType.Warning);
                if (GUILayout.Button("Upload from local to remote path")) Upload();
                if (GUILayout.Button("Download from remote to local path")) Download();
            }
        }

        private async void Upload ()
        {
            if (!Directory.Exists(LocalPath)) return;

            EditorUtility.DisplayProgressBar("Migrating resources to Google Drive", "Gathering metadata...", 0);

            var files = new DirectoryInfo(LocalPath).GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                if (file.Extension == ".meta") continue;

                var relativeFilePath = file.FullName.Replace("\\", "/").Replace(LocalPath, string.Empty);
                if (relativeFilePath.StartsWithFast("/")) relativeFilePath = relativeFilePath.Remove(0, 1);
                var remoteFilePath = Path.Combine(RemotePath, relativeFilePath);
                var googleFile = new UnityGoogleDrive.Data.File { Content = File.ReadAllBytes(file.FullName) };
                string uploadMimeType = null;

                if (file.Extension == ".nani") // Transform nani scripts to google documents.
                {
                    googleFile.MimeType = Helpers.DocumentMimeType;
                    remoteFilePath = remoteFilePath.GetBeforeLast(".nani");
                    uploadMimeType = "text/plain";
                }

                if (EditorUtility.DisplayCancelableProgressBar("Migrating resources to Google Drive", $"Uploading '{relativeFilePath}' ({StringUtils.FormatFileSize(file.Length)})...", i / (float)files.Length))
                    break;

                await Helpers.CreateOrUpdateFileAtPathAsync(googleFile, remoteFilePath, uploadMimeType: uploadMimeType);
            }

            EditorUtility.ClearProgressBar();
        }

        private async void Download ()
        {
            isOperationCanceled = false;
            EditorUtility.DisplayProgressBar("Migrating resources from Google Drive", "Gathering metadata...", 0);

            await FindAndDownloadFiles(RemotePath + "/");

            EditorUtility.ClearProgressBar();
        }

        private async UniTask FindAndDownloadFiles (string directoryPath)
        {
            var localDirPath = directoryPath.Replace(RemotePath, LocalPath);
            if (!Directory.Exists(localDirPath)) Directory.CreateDirectory(localDirPath);

            var files = await Helpers.FindFilesByPathAsync(directoryPath, fields: new List<string> { "files(name, mimeType, md5Checksum, size)" });

            for (int i = 0; i < files.Count; i++)
            {
                if (isOperationCanceled) break;
                var file = files[i];

                if (file.MimeType == Helpers.FolderMimeType) await FindAndDownloadFiles(directoryPath + file.Name + "/");
                else
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Migrating resources from Google Drive", $"Downloading '{directoryPath.GetAfter(RemotePath) + file.Name}' ({StringUtils.FormatFileSize(file.Size ?? 0)})...", i / (float)files.Count))
                    { isOperationCanceled = true; break; }

                    var localFilePath = Path.Combine(localDirPath, file.Name);
                    if (File.Exists(localFilePath) && file.Md5Checksum == Helpers.CalculateMD5Checksum(File.ReadAllBytes(localFilePath))) continue;

                    byte[] content = null;
                    if (file.MimeType == Helpers.DocumentMimeType) // Transform google documents to nani scripts.
                    {
                        localFilePath += ".nani";
                        content = (await GoogleDriveFiles.Export(file.Id, "text/plain").Send())?.Content;
                    }
                    else content = (await GoogleDriveFiles.Download(file.Id).Send())?.Content;

                    if (content != null) CreateOrUpdateLocalFile(localFilePath, content);
                }
            }
        }

        private static void CreateOrUpdateLocalFile (string path, byte[] content)
        {
            if (File.Exists(path)) File.Delete(path);

            using (var fs = File.Create(path))
                fs.Write(content, 0, content.Length);
        }
    }
}

#endif
