﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Callbacks;
#if UNITY_2017_1_OR_NEWER
using UnityEditor.Build;
#endif

namespace FMODUnity
{
    [InitializeOnLoad]
    public class EventManager : MonoBehaviour
    {
        const string CacheAssetName = "FMODStudioCache";
        const string CacheAssetFullName = "Assets/" + CacheAssetName + ".asset";
        static EventCache eventCache;

        const string StringBankExtension = "strings.bank";
        const string BankExtension = "bank";

        const int FilePollTimeSeconds = 5;
        
        // How many seconds to wait since last file activity to start the import
        const int CountdownTimerReset = 15 / FilePollTimeSeconds;

        static int countdownTimer;

#if UNITY_EDITOR
        [MenuItem("FMOD/Refresh Banks", priority = 1)]
        public static void RefreshBanks()
        {
            OnCacheChange();
        }
#endif

        static void ClearCache()
        {
            countdownTimer = CountdownTimerReset;
            eventCache.StringsBankWriteTime = DateTime.MinValue;
            eventCache.EditorBanks.Clear();
            eventCache.EditorEvents.Clear();
            eventCache.StringsBanks.Clear();
            eventCache.MasterBanks.Clear();
            OnCacheChange();
        }
        
        static public void UpdateCache()
        {
            // Deserialize the cache from the unity resources
            if (eventCache == null)
            {
                eventCache = AssetDatabase.LoadAssetAtPath(CacheAssetFullName, typeof(EventCache)) as EventCache;
                if (eventCache == null || eventCache.cacheVersion != EventCache.CurrentCacheVersion)
                {
                    UnityEngine.Debug.Log("FMOD Studio: Cannot find serialized event cache or cache in old format, creating new instance");
                    eventCache = ScriptableObject.CreateInstance<EventCache>();
                    eventCache.cacheVersion = EventCache.CurrentCacheVersion;

                    AssetDatabase.CreateAsset(eventCache, CacheAssetFullName);
                }
            }

            if (EditorUtils.GetBankDirectory() == null)
            {
                ClearCache();
                return;
            }

            string defaultBankFolder = null;
            
            if (!Settings.Instance.HasPlatforms)
            {
                defaultBankFolder = EditorUtils.GetBankDirectory();
            }
            else
            {
                FMODPlatform platform = RuntimeUtils.GetEditorFMODPlatform();
                if (platform == FMODPlatform.None)
                {
                    platform = FMODPlatform.PlayInEditor;
                }

                defaultBankFolder = Path.Combine(EditorUtils.GetBankDirectory(), Settings.Instance.GetBankPlatform(platform));
            }

            string[] bankPlatforms = EditorUtils.GetBankPlatforms();
            string[] bankFolders = new string[bankPlatforms.Length];
            for (int i = 0; i < bankPlatforms.Length; i++)
            {
                bankFolders[i] = Path.Combine(EditorUtils.GetBankDirectory(), bankPlatforms[i]);
            }

            List<String> stringBanks = new List<string>(0);
            try
            {
                var files = Directory.GetFiles(defaultBankFolder, "*." + StringBankExtension);
                stringBanks = new List<string>(files);
            }
            catch
            {
            }

            // Strip out OSX resource-fork files that appear on FAT32
            stringBanks.RemoveAll((x) => Path.GetFileName(x).StartsWith("._"));

            if (stringBanks.Count == 0)
            {
                bool wasValid = eventCache.StringsBankWriteTime != DateTime.MinValue;
                ClearCache();
                if (wasValid)
                {
                    UnityEngine.Debug.LogError(String.Format("FMOD Studio: Directory {0} doesn't contain any banks. Build from the tool or check the path in the settings", defaultBankFolder));
                }
                return;
            }

            // If we have multiple .strings.bank files find the most recent
            stringBanks.Sort((a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

            // Use the most recent string bank timestamp as a marker for the most recent build of any bank because it gets exported every time
            DateTime lastWriteTime = File.GetLastWriteTime(stringBanks[0]);

            if (lastWriteTime == eventCache.StringsBankWriteTime)
            {
                countdownTimer = CountdownTimerReset;
                return;
            }

            if (EditorUtils.IsFileOpenByStudio(stringBanks[0]))
            {
                countdownTimer = CountdownTimerReset;
                return;
            }

            // Most recent strings bank is newer than last cache update time, recache.

            // Get a list of all banks
            HashSet<string> bankFileNames = new HashSet<string>();
            List<string> reducedStringBanksList = new List<string>();
            HashSet<Guid> stringBankGuids = new HashSet<Guid>();

            foreach (string stringBankPath in stringBanks)
            {
                FMOD.Studio.Bank stringBank;
                EditorUtils.CheckResult(EditorUtils.System.loadBankFile(stringBankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out stringBank));

                if (!stringBank.isValid())
                {
                    countdownTimer = CountdownTimerReset;
                    return;
                }

                try
                {
                    Guid stringBankGuid;
                    EditorUtils.CheckResult(stringBank.getID(out stringBankGuid));

                    if (!stringBankGuids.Add(stringBankGuid))
                    {
                        // If we encounter multiple string banks with the same GUID then only use the first. This handles the scenario where
                        // a Studio project is cloned and extended for DLC with a new master bank name.
                        continue;
                    }

                    reducedStringBanksList.Add(stringBankPath);

                    // Iterate every string in the strings bank and look for any that identify banks
                    int stringCount;
                    stringBank.getStringCount(out stringCount);

                    for (int stringIndex = 0; stringIndex < stringCount; stringIndex++)
                    {
                        string currentString;
                        Guid currentGuid;
                        stringBank.getStringInfo(stringIndex, out currentGuid, out currentString);
                        const string BankPrefix = "bank:/";
                        int BankPrefixLength = BankPrefix.Length;
                        if (currentString.StartsWith(BankPrefix))
                        {
                            string bankFileName = currentString.Substring(BankPrefixLength) + "." + BankExtension;
                            if (!bankFileName.Contains(StringBankExtension)) // filter out the strings bank
                            {
                                bankFileNames.Add(bankFileName);
                            }
                        }
                    }
                }
                finally
                {
                    // Unload the strings bank
                    stringBank.unload();
                }
            }

            stringBanks = reducedStringBanksList;

            // Check if any of the files are still being written by studio
            foreach (string bankFileName in bankFileNames)
            {
                string bankPath = Path.Combine(defaultBankFolder, bankFileName);

                if (!File.Exists(bankPath))
                {
                    // TODO: this is meant to catch the case where we're in the middle of a build and a bank is being built 
                    // for the first time. But it also stops someone trying to import an incomplete set of banks without any error message.
                    countdownTimer = CountdownTimerReset;
                    return;
                }

                EditorBankRef bankRef = eventCache.EditorBanks.Find((x) => bankPath == x.Path);
                if (bankRef == null)
                {
                    if (EditorUtils.IsFileOpenByStudio(bankPath))
                    {
                        countdownTimer = CountdownTimerReset;
                        return;
                    }
                    continue;
                }

                if (bankRef.LastModified != File.GetLastWriteTime(bankPath))
                {
                    if (EditorUtils.IsFileOpenByStudio(bankPath))
                    {
                        countdownTimer = CountdownTimerReset;
                        return;
                    }
                }
            }

            // Count down the timer in case we catch studio in-between updating two files.
            if (countdownTimer-- > 0)
            {
                return;
            }

            eventCache.StringsBankWriteTime = lastWriteTime;

            // All files are finished being modified by studio so update the cache

            // Stop editor preview so no stale data being held
            EditorUtils.PreviewStop();

            // Reload the strings banks
            List<FMOD.Studio.Bank> loadedBanks = new List<FMOD.Studio.Bank>();

            try
            {
                AssetDatabase.StartAssetEditing();

                eventCache.EditorBanks.ForEach((x) => x.Exists = false);
                HashSet<string> masterBankFileNames = new HashSet<string>();

                foreach (string stringBankPath in stringBanks)
                {
                    FMOD.Studio.Bank stringBank;
                    EditorUtils.CheckResult(EditorUtils.System.loadBankFile(stringBankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out stringBank));

                    if (!stringBank.isValid())
                    {
                        ClearCache();
                        return;
                    }

                    loadedBanks.Add(stringBank);
                
                    FileInfo stringBankFileInfo = new FileInfo(stringBankPath);

                    string masterBankFileName = Path.GetFileName(stringBankPath).Replace(StringBankExtension, BankExtension);
                    masterBankFileNames.Add(masterBankFileName);

                    EditorBankRef stringsBankRef = eventCache.StringsBanks.Find(x => stringBankPath == x.Path);

                    if (stringsBankRef == null)
                    {
                        stringsBankRef = ScriptableObject.CreateInstance<EditorBankRef>();
                        stringsBankRef.FileSizes = new List<EditorBankRef.NameValuePair>();
                        eventCache.EditorBanks.Add(stringsBankRef);
                        AssetDatabase.AddObjectToAsset(stringsBankRef, eventCache);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stringsBankRef));
                        eventCache.StringsBanks.Add(stringsBankRef);
                    }

                    stringsBankRef.Path = stringBankPath;
                    stringsBankRef.LastModified = stringBankFileInfo.LastWriteTime;
                    stringsBankRef.Exists = true;
                    stringsBankRef.FileSizes.Clear();

                    if (Settings.Instance.HasPlatforms)
                    {
                        for (int i = 0; i < bankPlatforms.Length; i++)
                        {
                            stringsBankRef.FileSizes.Add(new EditorBankRef.NameValuePair(bankPlatforms[i], stringBankFileInfo.Length));
                        }
                    }
                    else
                    {
                        stringsBankRef.FileSizes.Add(new EditorBankRef.NameValuePair("", stringBankFileInfo.Length));
                    }
                }

                string[] folderContents = Directory.GetFiles(defaultBankFolder);

                foreach (string bankFileName in bankFileNames)
                {
                    // Get the true file path, can't trust the character case we got from the string bank
                    string bankPath = ArrayUtility.Find(folderContents, x => (string.Equals(bankFileName, Path.GetFileName(x), StringComparison.CurrentCultureIgnoreCase)));
                    
                    FileInfo bankFileInfo = new FileInfo(bankPath);
                    EditorBankRef bankRef = eventCache.EditorBanks.Find((x) => bankFileInfo.FullName == x.Path);

                    // New bank we've never seen before
                    if (bankRef == null)
                    {
                        bankRef = ScriptableObject.CreateInstance<EditorBankRef>();
                        AssetDatabase.AddObjectToAsset(bankRef, eventCache);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(bankRef));
                        bankRef.Path = bankFileInfo.FullName;
                        bankRef.LastModified = DateTime.MinValue;
                        bankRef.FileSizes = new List<EditorBankRef.NameValuePair>();
                        eventCache.EditorBanks.Add(bankRef);
                    }

                    bankRef.Exists = true;
                
                    // Timestamp check - if it doesn't match update events from that bank
                    if (bankRef.LastModified != bankFileInfo.LastWriteTime)
                    {
                        bankRef.LastModified = bankFileInfo.LastWriteTime;
                        UpdateCacheBank(bankRef);
                    }

                    // Update file sizes
                    bankRef.FileSizes.Clear();
                    if (Settings.Instance.HasPlatforms)
                    {
                        for (int i = 0; i < bankPlatforms.Length; i++)
                        {
                            string platformBankPath = Path.Combine(bankFolders[i], bankFileName);
                            var fileInfo = new FileInfo(platformBankPath);
                            if (fileInfo.Exists)
                            {
                                bankRef.FileSizes.Add(new EditorBankRef.NameValuePair(bankPlatforms[i], fileInfo.Length));
                            }
                        }
                    }
                    else
                    {
                        string platformBankPath = Path.Combine(EditorUtils.GetBankDirectory(), bankFileName);
                        var fileInfo = new FileInfo(platformBankPath);
                        if (fileInfo.Exists)
                        {
                            bankRef.FileSizes.Add(new EditorBankRef.NameValuePair("", fileInfo.Length));
                        }
                    }

                    if (masterBankFileNames.Contains(bankFileInfo.Name))
                    {
                        if (!eventCache.MasterBanks.Exists(x => bankFileInfo.FullName == x.Path))
                        {
                            eventCache.MasterBanks.Add(bankRef);
                        }
                    }
                }

                // Remove any stale entries from bank and event lists
                eventCache.EditorBanks.FindAll((x) => !x.Exists).ForEach(RemoveCacheBank);
                eventCache.EditorBanks.RemoveAll((x) => !x.Exists);
                eventCache.EditorEvents.RemoveAll((x) => x.Banks.Count == 0);

                OnCacheChange();
            }
            finally
            {
                // Unload the strings banks
                loadedBanks.ForEach(x => x.unload());
                AssetDatabase.StopAssetEditing();
            }
        }

        static void UpdateCacheBank(EditorBankRef bankRef)
        {
            // Clear out any cached events from this bank
            eventCache.EditorEvents.ForEach((x) => x.Banks.Remove(bankRef));

            FMOD.Studio.Bank bank;
            bankRef.LoadResult = EditorUtils.System.loadBankFile(bankRef.Path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);

            if (bankRef.LoadResult == FMOD.RESULT.ERR_EVENT_ALREADY_LOADED)
            {
                EditorUtils.System.getBank(bankRef.Name, out bank);
                bank.unload();
                bankRef.LoadResult = EditorUtils.System.loadBankFile(bankRef.Path, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out bank);
            }

            if (bankRef.LoadResult == FMOD.RESULT.OK)
            {
                // Iterate all events in the bank and cache them
                FMOD.Studio.EventDescription[] eventList;
                var result = bank.getEventList(out eventList);
                if (result == FMOD.RESULT.OK)
                {
                    foreach (var eventDesc in eventList)
                    {
                        string path;
                        result = eventDesc.getPath(out path);
                        EditorEventRef eventRef = eventCache.EditorEvents.Find((x) => x.Path == path);
                        if (eventRef == null)
                        {
                            eventRef = ScriptableObject.CreateInstance<EditorEventRef>();
                            AssetDatabase.AddObjectToAsset(eventRef, eventCache);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(eventRef));
                            eventRef.Banks = new List<EditorBankRef>();
                            eventCache.EditorEvents.Add(eventRef);
                        }

                        eventRef.Banks.Add(bankRef);
                        Guid guid;
                        eventDesc.getID(out guid);
                        eventRef.Guid = guid;
                        eventRef.Path = path;
                        eventDesc.is3D(out eventRef.Is3D);
                        eventDesc.isOneshot(out eventRef.IsOneShot);
                        eventDesc.isStream(out eventRef.IsStream);
                        eventDesc.getMaximumDistance(out eventRef.MaxDistance);
                        eventDesc.getMinimumDistance(out eventRef.MinDistance);
                        eventDesc.getLength(out eventRef.Length);
                        int paramCount = 0;
                        eventDesc.getParameterCount(out paramCount);
                        eventRef.Parameters = new List<EditorParamRef>(paramCount);
                        for (int paramIndex = 0; paramIndex < paramCount; paramIndex++)
                        {
                            FMOD.Studio.PARAMETER_DESCRIPTION param;
                            eventDesc.getParameterByIndex(paramIndex, out param);
                            if (param.type != FMOD.Studio.PARAMETER_TYPE.GAME_CONTROLLED)
                            {
                                continue;
                            }
                            EditorParamRef paramRef = ScriptableObject.CreateInstance<EditorParamRef>();
                            AssetDatabase.AddObjectToAsset(paramRef, eventCache);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(paramRef));
                            paramRef.Name = param.name;
                            paramRef.Min = param.minimum;
                            paramRef.Max = param.maximum;
                            paramRef.Default = param.defaultvalue;
                            eventRef.Parameters.Add(paramRef);
                        }
                    }
                }

                bank.unload();
            }
            else
            {
                Debug.LogError(String.Format("FMOD Studio: Unable to load {0}: {1}", bankRef.Name, FMOD.Error.String(bankRef.LoadResult)));
                eventCache.StringsBankWriteTime = DateTime.MinValue;
            }
        }

        static void RemoveCacheBank(EditorBankRef bankRef)
        {
            eventCache.EditorEvents.ForEach((x) => x.Banks.Remove(bankRef));
        }

        static EventManager()
        {
            countdownTimer = CountdownTimerReset;
            #if !UNITY_2017_1_OR_NEWER
            EditorUserBuildSettings.activeBuildTargetChanged += BuildTargetChanged;
            #endif
            EditorApplication.update += Update;
        }

        public static void CopyToStreamingAssets()
        {
            FMODPlatform platform = RuntimeUtils.GetEditorFMODPlatform();
            if (platform == FMODPlatform.None)
            {
                UnityEngine.Debug.LogWarning(String.Format("FMOD Studio: copy banks for platform {0} : Unsupported platform", EditorUserBuildSettings.activeBuildTarget.ToString()));
                return;
            }

            string bankTargetFolder =
                Settings.Instance.ImportType == ImportType.StreamingAssets
                ? Application.dataPath + "/StreamingAssets"
                : Application.dataPath + "/" + Settings.Instance.TargetAssetPath;
            Directory.CreateDirectory(bankTargetFolder);

            string bankTargetExension =
                Settings.Instance.ImportType == ImportType.StreamingAssets
                ? "bank"
                : "bytes";

            string bankSourceFolder = EditorUtils.GetBankDirectory() + "/" + Settings.Instance.GetBankPlatform(platform);

            if (Path.GetFullPath(bankTargetFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant() ==
                Path.GetFullPath(bankSourceFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant())
            {
                return;
            }

            bool madeChanges = false;

            try
            {
                // Clean out any stale .bank files
                string[] currentBankFiles = Directory.GetFiles(bankTargetFolder, "*." + bankTargetExension);
                foreach (var bankFileName in currentBankFiles)
                {
                    string bankName = Path.GetFileNameWithoutExtension(bankFileName);
                    if (!eventCache.EditorBanks.Exists((x) => bankName == x.Name))
                    {
                        File.Delete(bankFileName);
                        madeChanges = true;
                    }
                }

                // Copy over any files that don't match timestamp or size or don't exist
                foreach (var bankRef in eventCache.EditorBanks)
                {
                    string sourcePath = bankSourceFolder + "/" + bankRef.Name + ".bank";
                    string targetPath = bankTargetFolder + "/" + bankRef.Name + "." + bankTargetExension;

                    FileInfo sourceInfo = new FileInfo(sourcePath);
                    FileInfo targetInfo = new FileInfo(targetPath);

                    if (!targetInfo.Exists ||
                        sourceInfo.Length != targetInfo.Length ||
                        sourceInfo.LastWriteTime != targetInfo.LastWriteTime)
                    {
                        File.Copy(sourcePath, targetPath, true);
                        targetInfo = new FileInfo(targetPath);
                        targetInfo.IsReadOnly = false;
                        targetInfo.LastWriteTime = sourceInfo.LastWriteTime;
                        
                        madeChanges = true;
                    }
                }
            }
            catch(Exception exception)
            {
                UnityEngine.Debug.LogError(String.Format("FMOD Studio: copy banks for platform {0} : copying banks from {1} to {2}", platform.ToString(), bankSourceFolder, bankTargetFolder));
                UnityEngine.Debug.LogException(exception);
                return;
            }

            if (madeChanges)
            {
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log(String.Format("FMOD Studio: copy banks for platform {0} : copying banks from {1} to {2} succeeded", platform.ToString(), bankSourceFolder, bankTargetFolder));
            }
        }

        private static void BuildTargetChanged()
        {
            UpdateCache();

            // Copy over assets for the new platform
            CopyToStreamingAssets();
        }   

        static void OnCacheChange()
        {
            Settings.Instance.MasterBanks.Clear();

            foreach (EditorBankRef bankRef in eventCache.MasterBanks)
            {
                Settings.Instance.MasterBanks.Add(bankRef.Name);
            }

            Settings.Instance.Banks.Clear();

            foreach (var bankRef in eventCache.EditorBanks)
            {
                if (!eventCache.MasterBanks.Contains(bankRef) &&
                    !eventCache.StringsBanks.Contains(bankRef))
                {
                    Settings.Instance.Banks.Add(bankRef.Name);
                }
            }

            CopyToStreamingAssets();

            EventBrowser.RepaintEventBrowser();
        }

        static bool firstUpdate = true;
        static float lastCheckTime;
        static void Update()
        {
            if (firstUpdate)
            {
                UpdateCache();
                OnCacheChange();
                CopyToStreamingAssets();
                bool isValid;
                string validateMessage;
                EditorUtils.ValidateSource(out isValid, out validateMessage);
                if (!isValid)
                {
                    Debug.LogError("FMOD Studio: " + validateMessage);
                }
                firstUpdate = false;
                lastCheckTime = Time.realtimeSinceStartup;
            }

            if (lastCheckTime + FilePollTimeSeconds < Time.realtimeSinceStartup)
            {
                UpdateCache();
                lastCheckTime = Time.realtimeSinceStartup;
            }
        }

        public static List<EditorEventRef> Events
        {
            get
            {
                UpdateCache();
                return eventCache.EditorEvents;
            }
        }

        public static List<EditorBankRef> Banks
        {
            get
            {
                UpdateCache();
                return eventCache.EditorBanks;
            }
        }

        public static List<EditorBankRef> MasterBanks
        { 
            get
            {
                UpdateCache();
                return eventCache.MasterBanks;
            }
        }

        public static bool IsLoaded
        {
            get
            {
                return EditorUtils.GetBankDirectory() != null;
            }
        }

        public static bool IsValid
        {
            get
            {
                UpdateCache();
                return eventCache.StringsBankWriteTime != DateTime.MinValue;
            }
        }

        public static EditorEventRef EventFromPath(string path)
        {
            UpdateCache();
            return eventCache.EditorEvents.Find((x) => x.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase));
        }

        public static EditorEventRef EventFromGUID(Guid guid)
        {
            UpdateCache();
            return eventCache.EditorEvents.Find((x) => x.Guid == guid);
        }

        #if UNITY_2017_1_OR_NEWER
        public class ActiveBuildTargetListener : IActiveBuildTargetChanged
        {
            public int callbackOrder{ get { return 0; } }
            public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
            {
                BuildTargetChanged();
            }
        }
        #endif

    }
}
