using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aetherium.Utils
{
    public static class ExtensionMethods
    {
        public static void FilterElites(this BullseyeSearch search)
        {
            if (search.candidatesEnumerable.Any())
            {
                search.candidatesEnumerable = search.candidatesEnumerable.Where(x => x.hurtBox && x.hurtBox.IsHurtboxAnElite());
            }
        }

        public static void FilterOutItemWielders(this BullseyeSearch search, ItemDef item)
        {
            if (search.candidatesEnumerable.Any())
            {
                search.candidatesEnumerable = search.candidatesEnumerable.Where(x => x.hurtBox && !x.hurtBox.DoesHurtboxHaveItem(item));
            }
        }

        public static bool DoesHurtboxHaveItem(this HurtBox hurtbox, ItemDef item)
        {
            if (!hurtbox.healthComponent || !hurtbox.healthComponent.body || !item)
            {
                AetheriumPlugin.ModLogger.LogError("Can't check if the hurtbox has the item, some information is missing!");
                return false;
            }

            var body = hurtbox.healthComponent.body;
            if(body.inventory.GetItemCount(item) > 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsHurtboxAnElite(this HurtBox hurtbox)
        {
            if (!hurtbox.healthComponent || !hurtbox.healthComponent.body)
            {
                AetheriumPlugin.ModLogger.LogError("Can't check if the hurtbox is an elite, some information is missing!");
                return false;
            }

            return hurtbox.healthComponent.body.isElite;
        }
    }
}
