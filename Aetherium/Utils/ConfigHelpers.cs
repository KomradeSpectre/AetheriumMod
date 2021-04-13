using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Aetherium.Utils
{
    public class ConfigOption<T>
    {
        ConfigEntry<T> Bind;

        public ConfigOption(ConfigFile config, string categoryName, string configOptionName, T defaultValue, string fullDescription)
        {
            Bind = config.Bind(categoryName, configOptionName, defaultValue, fullDescription);
        }

        public static implicit operator T(ConfigOption<T> x)
        {
            return x.Bind.Value;
        }

        public override string ToString()
        {
            return Bind.Value.ToString();
        }
    }

    public static class ConfigExtension
    {
        public static ConfigOption<T> ActiveBind<T>(this ConfigFile configWrapper, string categoryName, string configOptionName, T defaultValue, string fullDescription)
        {
            return new ConfigOption<T>(configWrapper, categoryName, configOptionName, defaultValue, fullDescription);
        }
    }
}
