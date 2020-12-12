using System;
using System.Collections.Generic;
using System.Text;

namespace Aetherium.CoreModules
{
    /// <summary>
    /// Basic holder class to allow easy initialization of core modules for Aetherium
    /// </summary>
    public abstract class CoreModule
    {
        public abstract void Init();
    }
}
