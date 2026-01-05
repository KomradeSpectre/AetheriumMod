using R2API;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Aetherium.Language
{
    internal static class Language
    {
        public static void Init()
        {
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
        }

        private static void Language_collectLanguageRootFolders(List<string> folders)
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(AetheriumPlugin.Instance.Info.Location), "Language");
            if(Directory.Exists(path))
            {
                folders.Add(path);
            }
        }

        public static void Add(string token, string text)
        {
            LanguageAPI.Add(token, text);
        }
    }
}