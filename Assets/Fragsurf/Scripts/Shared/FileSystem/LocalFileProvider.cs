using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fragsurf.Shared
{
    public class LocalFileProvider : IFileProvider
    {

        private class DirToScan
        {
            public string Path;
            public string SearchPattern;
        }

        public Dictionary<string, FSFileInfo> Files { get; } = new Dictionary<string, FSFileInfo>();
        private List<DirToScan> _directoriesToScan = new List<DirToScan>();

        public void Build()
        {
            Files.Clear();

            foreach (var dir in _directoriesToScan)
            {
                if (Directory.Exists(dir.Path))
                {
                    ScanDirectory(dir);
                }
            }
        }

        public async Task BuildAsync()
        {
            Build();
            await Task.Delay(0);
        }

        public void AddFileDirectory(string dir, string searchPattern = "*")
        {
            _directoriesToScan.Add(new DirToScan() { Path = dir, SearchPattern = searchPattern });
        }

        public bool ContainsFile(string path)
        {
            path = path.ToLower();
            if (Files.ContainsKey(path))
                return true;
            foreach (var kvp in Files)
            {
                if (kvp.Key.EndsWith(path))
                    return true;
            }
            return false;
        }

        public FSFileInfo GetFile(string path)
        {
            path = path.ToLower();

            FSFileInfo result = null;

            if (Files.ContainsKey(path))
            {
                result = Files[path];
            }
            else
            {
                foreach (var kvp in Files)
                {
                    if (kvp.Key.EndsWith(path))
                    {
                        result = kvp.Value;
                    }
                }
            }

            return result;
        }

        private void ScanDirectory(DirToScan dir)
        {
            // get all files except those dirty .meta files when in editor
            var allFiles = Directory.GetFiles(dir.Path, dir.SearchPattern, SearchOption.AllDirectories)
                .ToList()
                .FindAll(x => !x.EndsWith(".meta", StringComparison.InvariantCultureIgnoreCase));

            dir.Path = dir.Path.Replace('/', '\\').ToLower();

            foreach (var fullPath in allFiles)
            {
                var normalizedPath = fullPath.Replace('/', '\\').ToLower();
                var relativePath = normalizedPath.Replace(dir.Path, "");
                var fileInfo = new FSFileInfo(fullPath, relativePath);
                if(Files.ContainsKey(fileInfo.RelativePath))
                {
                    continue;
                }
                Files.Add(fileInfo.RelativePath, fileInfo);
            }
        }

    }
}

