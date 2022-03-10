using RoR2;
using R2API;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.Networking;

namespace Aetherium.Artifacts
{
    public abstract class ArtifactBase<T> : ArtifactBase where T : ArtifactBase<T>
    {
        public static T instance { get; private set; }

        public ArtifactBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ArtifactBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ArtifactBase
    {
        public abstract string ArtifactName { get; }

        public abstract string ArtifactLangTokenName { get; }

        public abstract string ArtifactDescription { get; }

        public abstract Sprite ArtifactEnabledIcon { get; }

        public abstract Sprite ArtifactDisabledIcon { get; }

        public ArtifactDef ArtifactDef;

        public bool ArtifactEnabled => RunArtifactManager.instance.IsArtifactEnabled(ArtifactDef);

        public abstract void Init(ConfigFile config);

        protected void CreateLang()
        {
            LanguageAPI.Add("ARTIFACT_" + ArtifactLangTokenName + "_NAME", ArtifactName);
            LanguageAPI.Add("ARTIFACT_" + ArtifactLangTokenName + "_DESCRIPTION", ArtifactDescription);
        }

        protected void CreateArtifact()
        {
            ArtifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            ArtifactDef.cachedName = "ARTIFACT_" + ArtifactLangTokenName;
            ArtifactDef.nameToken = "ARTIFACT_" + ArtifactLangTokenName + "_NAME";
            ArtifactDef.descriptionToken = "ARTIFACT_" + ArtifactLangTokenName + "_DESCRIPTION";
            ArtifactDef.smallIconSelectedSprite = ArtifactEnabledIcon;
            ArtifactDef.smallIconDeselectedSprite = ArtifactDisabledIcon;

            ContentAddition.AddArtifactDef(ArtifactDef);
        }

        public abstract void Hooks();
    }
}
