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
            Bind = config.Bind(ConfigExtension.SanitizeConfigName(categoryName), ConfigExtension.SanitizeConfigName(configOptionName), defaultValue, fullDescription);
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
        private static readonly char[] InvalidConfigChars = { '=', '\n', '\t', '\\', '"', '\'', '[', ']' };

        /// <summary>
        /// BepInEx throws on these characters in section and key names (e.g. the apostrophe in "Apollo's Brilliance"), so strip them out.
        /// </summary>
        public static string SanitizeConfigName(string name)
        {
            if(string.IsNullOrEmpty(name) || name.IndexOfAny(InvalidConfigChars) < 0) { return name; }

            var builder = new StringBuilder(name.Length);
            foreach(char character in name)
            {
                if(Array.IndexOf(InvalidConfigChars, character) < 0) { builder.Append(character); }
            }
            return builder.ToString();
        }

        public static ConfigOption<T> ActiveBind<T>(this ConfigFile configWrapper, string categoryName, string configOptionName, T defaultValue, string fullDescription)
        {
            return new ConfigOption<T>(configWrapper, categoryName, configOptionName, defaultValue, fullDescription);
        }
    }
}
