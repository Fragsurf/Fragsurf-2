
namespace Fragsurf.Shared.UserPlugins
{
    public interface IUserPluginDescriptor
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }
        string Version { get; }
        string[] Dependencies { get; }
        string Directory { get; }
        string DirectoryName { get; }
        string EntryFile { get; }
        PluginSpace Space { get; }
        string EntryFilePath(string extension);
    }
}
