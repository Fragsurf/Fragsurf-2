using System.IO;

namespace Fragsurf.Shared
{
    public class FSFileInfo
    {
        public FSFileInfo() { }
        public FSFileInfo(string fullPath, string relativePath)
        {
            fullPath = fullPath.Replace('/', '\\').ToLower();
            relativePath = relativePath.Replace('/', '\\').ToLower();

            FullPath = fullPath;
            RelativePath = relativePath.Trim('\\', '/');
            Name = Path.GetFileNameWithoutExtension(fullPath);
            NameWithExtension = Path.GetFileName(fullPath);
            Extension = Path.GetExtension(fullPath);
        }

        /// <summary>
        /// File name without the extension
        /// </summary>
        public string Name;
        public string NameWithExtension;
        public string RelativePath;
        /// <summary>
        /// Includes the . separator
        /// </summary>
        public string Extension;
        public byte[] Hash;
        public long Length;
        public string FullPath;
        public ulong WorkshopId;
    }
}
