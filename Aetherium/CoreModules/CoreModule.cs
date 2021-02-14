namespace Aetherium.CoreModules
{
    /// <summary>
    /// Basic holder class to allow easy initialization of core modules for Aetherium
    /// </summary>
    public abstract class CoreModule
    {
        public abstract string Name { get; }
        public abstract void Init();
    }
}