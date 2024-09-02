using R2API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aetherium.Language
{
    /// <summary>
    /// Sourced from Henry mod template. https://github.com/ArcPh1r3/HenryTutorial
    /// </summary>
    internal static class Language
    {
        public static string TokensOutput = "";

        public static bool printingEnabled = true;

        public static void Init()
        {
            RoR2.Language.collectLanguageRootFolders += Language_collectLanguageRootFolders;
        }

        private static void Language_collectLanguageRootFolders(List<string> folders)
        {
            folders.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(AetheriumPlugin.Instance.Info.Location), "Language"));
        }

        public static void Add(string token, string text)
        {
            //if (!printingEnabled) return;

            //add a token formatted to language file
            TokensOutput += $"\n    \"{token}\" : \"{text.Replace(Environment.NewLine, "\\n").Replace("\n", "\\n")}\",";
        }

        public static void CreateLanguageFile(string fileName = "")
        {

            //wrap all tokens in a properly formatted language file
            string strings = $"{{\n    strings:\n    {{{TokensOutput}\n    }}\n}}";

            //spit out language dump in console for copy paste if you want
            AetheriumPlugin.ModLogger.LogMessage($"{fileName}: \n{strings}");

            //write a language file next to your mod. must have a folder called Language next to your mod dll.
            if (!string.IsNullOrEmpty(fileName))
            {
                string path = Path.Combine(Directory.GetParent(AetheriumPlugin.Instance.Info.Location).FullName, "Language", "en", fileName);
                File.WriteAllText(path, strings);
            }

            //empty the output each time this is printed, so you can print multiple language files
            TokensOutput = "";
        }
    }
}
