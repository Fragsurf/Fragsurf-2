
namespace Fragsurf
{
    public abstract class DevConsoleEntry
    {
        protected DevConsoleEntry(object owner, string name, string description)
        {
            Owner = owner;
            Name = name;
            Description = description;
        }

        public ConVarFlags Flags;
        public readonly object Owner;
        public readonly string Name;
        public readonly string Description;

        public void Tick()
        {
            _OnTick();
        }

        public void Execute(string[] args)
        {
            _OnExecute(args);
        }

        protected abstract void _OnExecute(string[] args);
        protected virtual void _OnTick() { }

    }
}
