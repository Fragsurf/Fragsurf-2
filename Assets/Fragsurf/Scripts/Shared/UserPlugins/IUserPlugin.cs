
namespace Fragsurf.Shared.UserPlugins
{
    public interface IUserPlugin
    {
        FSGameLoop Game { get; set; }
        string FileExtension { get; }
        ILogSystem LogSystem { get; set; }
        IUserPluginDescriptor Descriptor { get; set; }
        bool Initialize();
        bool Loaded { get; }
        void Load();
        void Unload();
        void Update();
        void RaiseEvent(string eventName, params object[] parameters);
        void Action(string path, params object[] parameters);
        object Func(string path, params object[] parameters);
        object GetValue(string path);
    }
}
