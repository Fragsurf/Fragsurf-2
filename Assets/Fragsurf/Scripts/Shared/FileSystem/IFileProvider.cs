using System.Threading.Tasks;
using System.Collections.Generic;

namespace Fragsurf.Shared
{
    public interface IFileProvider 
    {
        Dictionary<string, FSFileInfo> Files { get; }
        void Build();
        Task BuildAsync();
        FSFileInfo GetFile(string path);
        bool ContainsFile(string path);
    }
}

