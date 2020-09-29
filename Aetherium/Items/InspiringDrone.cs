using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using System;
using KomradeSpectre.Aetherium;

namespace Aetherium.Items
{
    class InspiringDrone : Item<InspiringDrone>
    {
        public override string displayName => "Inspiring Drone";

        public override ItemTier itemTier => RoR2.ItemTier.Tier3;

        public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[] { ItemTag.Utility });
        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangPickup(string langID = null) => "When a drone is purchased, it is granted a portion of your stats.";

        protected override string NewLangDesc(string langid = null) => "When a drone is purchased, it gains [X]% of your damage (+X% per stack), [X]% of your HP (+X% per stack), and [X]% of your armor (+X% per stack).";

        protected override string NewLangLore(string langID = null) => "[Engineer's Notes]: Let me preface this by saying that none of us have built a drone with the model number '1N-5P1R3'." +
            "We have no concrete evidence of how this model came to be, but thinking back on it, maybe it can explain the recent events onboard the station. I'll jot down some of the weird things that happened the last few days.\n" +
            "\n- One of our computer screens went missing from the Tech Lab and it was almost surgically removed from its unit.\n" +
            "\n- Some metal parts were missing nearby our assembly line, and the welding fuel on the line was lower than it was left during its last operation. Additionally, the welding arms on the line have no reports of activity, as if they were wiped.\n" +
            "\n- The major's favorite hat went missing, and the one on this model looks really similar to it.\n" +
            "\n- All of our old style computer mice had their trackball component removed from them.\n" +
            "\n- I lost a pair of walkie talkies. I tuned that pair specifically to be able to communicate with the drones at a much higher efficiency than normal.\n" +
            "\n- We lost a few of our self-charging ion batteries.\n" +
            "\n- Finally, one of the boys in Engineering apparently made a fool of himself for 30 minutes straight, as they were trying to activate their vernier thrusters that were missing from their exosuit.\n" +
            "\nThe model's construction is sound, the drone seems really docile, and drones nearby 1N-5P1R3 seem to have boosted efficacy as if they are motivated by it to do just as well as their human operators." +
            "Looking at it, I have to admit I'm kind of inspired to create some things myself. I'll just keep these musings to myself instead of reporting this.";


        public static GameObject ItemBodyModelPrefab;

        public InspiringDrone()
        {
            modelPathName = "@Aetherium:Assets/Models/Prefabs/InspiringDrone.prefab";
            iconPathName = "@Aetherium:Assets/Textures/Icons/InspiringDroneIcon.png";
        }

        protected override void LoadBehavior()
        {

        }

        protected override void UnloadBehavior()
        {
        }
    }
}
