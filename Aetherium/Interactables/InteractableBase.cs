using BepInEx.Configuration;
using R2API;
using System;

namespace Aetherium.Interactables
{
    public abstract class InteractableBase<T> : InteractableBase where T : InteractableBase<T>
    {
        public static T instance { get; private set; }

        public InteractableBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting PurchaseInteractableBase/Interactable was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class InteractableBase
    {
        public abstract string InteractableName { get; }

        public abstract string InteractableContext { get; }

        public abstract string InteractableLangToken { get; }

        public abstract string InteractableModelPath { get; }

        public abstract void Init(ConfigFile config);

        protected void CreateLang()
        {
            LanguageAPI.Add("INTERACTABLE_" + InteractableLangToken + "_NAME", InteractableName);
            LanguageAPI.Add("INTERACTABLE_" + InteractableLangToken + "_CONTEXT", InteractableContext);
        }
    }
}
