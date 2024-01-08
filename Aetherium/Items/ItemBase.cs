using Aetherium.Achievements;
using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Achievements;
using HarmonyLib;
using On.RoR2.Items;

namespace Aetherium.Items
{
    // The directly below is entirely from TILER2 API (by ThinkInvis) specifically the Item module. Utilized to keep instance checking functionality as I migrate off TILER2.
    // TILER2 API can be found at the following places:
    // https://github.com/ThinkInvis/RoR2-TILER2
    // https://thunderstore.io/package/ThinkInvis/TILER2/

    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }

        public abstract ItemTier Tier { get; }
        public virtual ItemTag[] ItemTags { get; set; } = new ItemTag[] { };

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }

        public ItemDef ItemDef;

        public virtual string CorruptsItem { get; set; } = null;

        public virtual UnlockableDef ItemUnlockableDef { get; set; } = null;

        public virtual bool CanRemove { get; } = true;

        public virtual bool AIBlacklisted { get; set; } = false;

        public virtual bool PrinterBlacklisted { get; set; } = false;

        public virtual bool RequireUnlock { get; set; } = true;

        public abstract void Init(ConfigFile config);

        protected void CreateLang()
        {
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_" + ItemLangTokenName + "_LORE", ItemLore);
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();

        protected void CreateItem()
        {
            if (AIBlacklisted)
            {
                ItemTags = new List<ItemTag>(ItemTags) { ItemTag.AIBlacklist }.ToArray();
            }

            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = "ITEM_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_" + ItemLangTokenName + "_LORE";
            ItemDef.pickupModelPrefab = ItemModel;
            ItemDef.pickupIconSprite = ItemIcon;
            ItemDef.hidden = false;
            ItemDef.canRemove = CanRemove;
            ItemDef.deprecatedTier = Tier;
            ItemDef.requiredExpansion = AetheriumPlugin.AetheriumExpansionDef;

            if(ItemTags.Length > 0) { ItemDef.tags = ItemTags; }

            if (PrinterBlacklisted)
            {
                AetheriumPlugin.BlacklistedFromPrinter.Add(ItemDef);
            }

            if (ItemUnlockableDef)
            {
                ItemDef.unlockableDef = ItemUnlockableDef;
            }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));
        }

        public static void BlacklistFromPrinter(ILContext il)
        {
            var c = new ILCursor(il);

            int listIndex = -1;
            int thisIndex = -1;
            c.GotoNext(x => x.MatchSwitch(out _));
            var gotThisIndex = c.TryGotoNext(x => x.MatchLdarg(out thisIndex));
            var gotListIndex = c.TryGotoNext(x => x.MatchLdloc(out listIndex));
            c.GotoNext(MoveType.Before, x => x.MatchCall(out _));
            if(gotThisIndex && gotListIndex)
            {
                c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg, thisIndex);
                c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, listIndex);
                c.EmitDelegate<Action<ShopTerminalBehavior, List<PickupIndex>>>((shopTerminalBehavior, list) =>
                {
                    if (shopTerminalBehavior && shopTerminalBehavior.gameObject.name.Contains("Duplicator"))
                    {
                        list.RemoveAll(x => AetheriumPlugin.BlacklistedFromPrinter.Contains(ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(x).itemIndex)));
                    }
                });
            }
        }

        public abstract void Hooks();

        // The below is entirely from TILER2 API (by ThinkInvis) specifically the Item module. Utilized to keep easy count functionality as I migrate off TILER2.
        // TILER2 API can be found at the following places:
        // https://github.com/ThinkInvis/RoR2-TILER2
        // https://thunderstore.io/package/ThinkInvis/TILER2/

        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }
            
            return body.inventory.GetItemCount(ItemDef);
        }

        public int GetCount(CharacterMaster master)
        {
            if (!master || !master.inventory) { return 0; }

            return master.inventory.GetItemCount(ItemDef);
        }

        public int GetCountSpecific(CharacterBody body, ItemDef itemDef)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(itemDef);
        }

        internal static void RegisterVoidPairings(ContagiousItemManager.orig_Init orig)
        {
            var voidTiers = new ItemTier[]
{
                ItemTier.VoidBoss,
                ItemTier.VoidTier1,
                ItemTier.VoidTier2,
                ItemTier.VoidTier3
};

            foreach (KeyValuePair<ItemBase, bool> itemPair in AetheriumPlugin.ItemStatusDictionary)
            {
                if (itemPair.Value == true)
                {
                    var item = itemPair.Key;

                    if (item.ItemDef && voidTiers.Any(x => item.ItemDef.tier == x))
                    {

                        var itemToCorrupt = ItemCatalog.itemDefs.Where(x => x.nameToken == item.CorruptsItem).FirstOrDefault();
                        if (!itemToCorrupt)
                        {
                            AetheriumPlugin.ModLogger.LogError($"Tried to add {item.ItemName} in a Void item tier but no relationship was set for what it corrupts or could not be found. Aborting!");
                            continue;
                        }

                        var pair = new ItemDef.Pair[]
                        {
                            new ItemDef.Pair
                            {
                                itemDef1 = itemToCorrupt,
                                itemDef2 = item.ItemDef,
                            }
                        };

                        ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(pair);
                    }
                }
            }
            orig();
        }
    }
}
