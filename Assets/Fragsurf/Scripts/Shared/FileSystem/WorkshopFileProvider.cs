using System.IO;
using System.Collections.Generic;
using Steamworks.Ugc;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;
using Steamworks;

namespace Fragsurf.Shared
{
    public class WorkshopFileProvider : IFileProvider
    {
        public Dictionary<string, FSFileInfo> Files { get; } = new Dictionary<string, FSFileInfo>();

        public void Build()
        {
            BuildAsync();
        }

        public async Task BuildAsync() 
        {
            if (!SteamClient.IsValid)
            {
                return;
            }

            var query = Query.All.WhereUserSubscribed();
            var result = await query.GetPageAsync(1);
            if (result.HasValue)
            {
                Files.Clear();
                foreach (var item in result.Value.Entries)
                {
                    if (!Directory.Exists(item.Directory))
                    {
                        continue;
                    }
                    var files = Directory.GetFiles(item.Directory, "*.fsm").Union(Directory.GetFiles(item.Directory, "*.rfsm"));
                    if (files != null && files.Count() > 0)
                    {
                        var fileInfo = new FSFileInfo(files.ElementAt(0), Path.GetFileName(files.ElementAt(0)));
                        if(Files.ContainsKey(fileInfo.RelativePath))
                        {
                            continue;
                        }
                        fileInfo.WorkshopId = item.Id;
                        Files.Add(fileInfo.RelativePath, fileInfo);
                    }
                }
                result.Value.Dispose();
            }
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

            if (result != null && result.Hash == null)
            {
                CreateMD5(result);
            }

            return result;
        }

        private void CreateMD5(FSFileInfo file)
        {
            using (var md5 = MD5.Create())
            {
                using (var fs = File.OpenRead(file.FullPath))
                {
                    file.Hash = md5.ComputeHash(fs);
                    file.Length = fs.Length;
                }
            }
        }
    }
}
