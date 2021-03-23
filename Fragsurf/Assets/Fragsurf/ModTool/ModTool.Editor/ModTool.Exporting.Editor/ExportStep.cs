using System.IO;
using UnityEditor;
using ModTool.Shared.Editor;

namespace ModTool.Exporting.Editor
{
    public abstract class ExportStep
    {
        protected static readonly string assetsDirectory = "Assets";
        protected static readonly string assemblyDirectory = Path.Combine("Library", "ScriptAssemblies");
        protected static readonly string tempAssemblyDirectory = Path.Combine("Temp", "ScriptAssemblies");
        protected static string modToolDirectory => AssetUtility.GetModToolDirectory();
        protected static string dllPath => Path.Combine(modToolDirectory, "ModTool.dll");

        protected static readonly string[] scriptAssemblies =
        {
            "Assembly-CSharp.dll",
            "Assembly-Csharp-firstpass.dll",
            "Assembly-UnityScript.dll",
            "Assembly-UnityScript-firstpass.dll"
            //"Assembly-Boo.dll",
            //"Assembly-Boo-firstpass.dll"
        };
        
        public bool waitForAssemblyReload { get; private set; }

        public abstract string message { get; }

        internal abstract void Execute(ExportData data);

        protected void ForceAssemblyReload()
        {
            waitForAssemblyReload = true;
            AssetDatabase.ImportAsset(dllPath, ImportAssetOptions.ForceUpdate);
        }        
    }
}
