using UnityEngine;
using Steamworks;
using System.IO;
using System;
using UnityEditor;
using Steamworks.Ugc;
using Fragsurf;
using System.Threading.Tasks;

namespace ModTool.Exporting.Editor
{
    public class UploadToWorkshop : ExportStep, IProgress<float>
    {
        public override string message => "Uploading to Workshop";

        private float _progress;

        public void Report(float value)
        {
            if(_progress >= value)
            {
                return;
            }
            _progress = value;
            EditorUtility.DisplayProgressBar("Uploading", "Your mod is uploading...", _progress);
        }

        internal override async void Execute(ExportData data)
        {
            if (!ExportSettings.uploadToWorkshop)
            {
                return;
            }

            var modDirectory = Path.Combine(ExportSettings.outputDirectory, ExportSettings.name);
            var di = new DirectoryInfo(modDirectory);
            if (!di.Exists)
            {
                Debug.LogError("Failed to upload to the Steam Workshop, directory doesn't exist: " + modDirectory);
                return;
            }

            var modSizeBytes = DirSize(new DirectoryInfo(modDirectory));
            var sizeString = BytesToString(modSizeBytes);

            if(!EditorUtility.DisplayDialog("Really Upload?", "You're about to upload this to the Steam Workshop.  The file size is: " + sizeString, "Yes", "Cancel!"))
            {
                Debug.LogError("Upload cancelled!");
                return;
            }

            var editor = ExportSettings.WorkshopId == 0
                ? Steamworks.Ugc.Editor.NewCommunityFile
                : new Steamworks.Ugc.Editor(ExportSettings.WorkshopId);

            editor = editor.WithTitle(ExportSettings.name)
                .WithDescription(ExportSettings.description)
                .WithContent(modDirectory);

            PublishResult result = default;

            try
            {
                result = await Upload(editor);
            }
            catch(Exception e)
            {
                Debug.Log("Failed to upload: " + e.Message);
            }

            EditorUtility.ClearProgressBar();

            if(!result.Success)
            {
                Debug.LogError("Uploading to Steam Workshop failed!");
                return;
            }

            ExportSettings.WorkshopId = result.FileId;

            if (result.NeedsWorkshopAgreement)
            {
                if(EditorUtility.DisplayDialog("Workshop Agreement", "Before your item is visible you need to agree to Steam's Workshop Agreement", "Visit Workshop"))
                {
                    Application.OpenURL(ExportSettings.WorkshopUrl);
                }
            }

            EditorUtility.DisplayDialog("Upload Complete", $"{ExportSettings.name} v{ExportSettings.version} has been uploaded to the Steam Workshop!", "Close");
        }

        private async Task<PublishResult> Upload(Steamworks.Ugc.Editor editor)
        {
            if (!SteamClient.IsValid)
            {
                SteamClient.Init(Structure.AppId);
                await Task.Delay(1000);
            }

            var result = await editor.SubmitAsync(this);

            SteamClient.RunCallbacks();
            await Task.Delay(250);
            SteamClient.RunCallbacks();
            await Task.Delay(250);

            return result;
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

    }
}

