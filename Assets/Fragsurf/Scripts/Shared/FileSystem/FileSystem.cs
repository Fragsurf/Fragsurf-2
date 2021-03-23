using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Fragsurf.Utility;

namespace Fragsurf.Shared
{
    public static class FileSystem 
    {
        public static List<FSFileInfo> DownloadList { get; } = new List<FSFileInfo>();
        public static List<IFileProvider> FileProviders { get; } = new List<IFileProvider>();
        public static List<IFileAcquirer> Acquirers { get; } = new List<IFileAcquirer>();

        private static bool _initialized;

        public static void Init()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            FileProviders.Add(new WorkshopFileProvider());
            AddLocalProvider(Structure.RuntimePath);
            Build();
        }

        public static void ClearDownloadList()
        {
            DownloadList.Clear();
        }

        public static void Build()
        {
            foreach (var provider in FileProviders)
            {
                provider.Build();
            }
        }

        public static async Task BuildAsync()
        {
            Build();

            foreach(var provider in FileProviders)
            {
                await provider.BuildAsync();
            }
        }

        public static LocalFileProvider AddLocalProvider(params string[] directories)
        {
            var provider = new LocalFileProvider();
            foreach(var dir in directories)
            {
                provider.AddFileDirectory(dir);
            }
            FileProviders.Add(provider);
            return provider;
        }

        public static bool AcquireFile(string fileName)
        {
            foreach(var acquirer in Acquirers)
            {
                if (acquirer.Acquire(fileName) != null)
                    return true;
            }
            return false;
        }

        public static IEnumerable<FSFileInfo> GetFilesWithExtension(params string[] extensions)
        {
            var result = new List<FSFileInfo>();

            foreach (var provider in FileProviders)
            {
                foreach(var kvp in provider.Files)
                {
                    if(hasExt(kvp.Value.Extension))
                    {
                        result.Add(kvp.Value);
                    }
                }
            }

            return result;

            bool hasExt(string ext)
            {
                for(int i = 0; i < extensions.Length; i++)
                {
                    if(ext.Contains(extensions[i], StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static IEnumerable<FSFileInfo> GetFiles(string prefix = null)
        {
            var result = new List<FSFileInfo>();

            foreach(var provider in FileProviders)
            {
                foreach(var kvp in provider.Files)
                {
                    if(prefix == null || kvp.Value.Name.StartsWith(prefix))
                    {
                        result.Add(kvp.Value);
                    }
                }
            }

            return result;
        }

        private static char[] _slashes = new char[] { '/', '\\' };
        public static FSFileInfo GetFileInfo(string fileName, bool addToDownloadList = false)
        {
            FSFileInfo result = null;

            var isFullPath = fileName.IndexOfAny(_slashes) != -1;
            var isWorkshopId = ulong.TryParse(fileName, out ulong workshopId);

            foreach (var provider in FileProviders)
            {
                foreach(var file in provider.Files)
                {
                    if((isFullPath && PathsAreEqual(file.Value.FullPath, fileName))
                        || (!isFullPath && string.Equals(fileName, file.Value.NameWithExtension, StringComparison.InvariantCultureIgnoreCase))
                        || (isWorkshopId && file.Value.WorkshopId == workshopId))
                    {
                        result = file.Value;
                        break;
                    }
                }
            }

            if (result != null && addToDownloadList)
            {
                DownloadList.Add(result);
            }

            return result;
        }

        public static bool PathsAreEqual(string a, string b)
        {
            var path1 = System.IO.Path.GetFullPath(a);
            var path2 = System.IO.Path.GetFullPath(b);
            return string.Equals(path1, path2);
        }

        public static FSFileInfo GetOrAcquire(string fileName, bool addToDownloadList = false)
        {
            if(string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var result = GetFileInfo(fileName, addToDownloadList);
            if(result != null && result.Hash == null)
            {
                using (var md5 = MD5.Create())
                {
                    using (var fs = File.OpenRead(result.FullPath))
                    {
                        result.Hash = md5.ComputeHash(fs);
                        result.Length = fs.Length;
                    }
                }
            }

            return result;
        }

        public static FileCheck CheckFile(string localPath, byte[] hash)
        {
            var fileInfo = GetFileInfo(localPath);
            if(fileInfo == null)
                return FileCheck.Missing;
            if (hash.SequenceEqual(fileInfo.Hash))
                return FileCheck.Same;
            return FileCheck.Different;
        }

        public static FileCheck CheckFile(FSFileInfo file)
        {
            return CheckFile(file.RelativePath, file.Hash);
        }

        public static void EmptyTempFolder()
        {
            var dir = new DirectoryInfo(Structure.TempPath);
            if (dir.Exists)
            {
                foreach (FileInfo file in dir.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in dir.GetDirectories())
                {
                    subDirectory.Delete(true);
                }
            }
        }

    }
}

