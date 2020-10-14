using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TILER2;
using UnityEngine;

namespace Aetherium.Equipment
{
    public class JarOfReshaping : Equipment<JarOfReshaping>
    {
        public override string displayName => "Jar of Reshaping";

        protected override string NewLangName(string langID = null) => displayName;

        protected override string NewLangDesc(string langID = null) => "";

        protected override string NewLangLore(string langID = null) => "";

        protected override string NewLangPickup(string langID = null) => "";

        public JarOfReshaping()
        {

        }

        protected override void LoadBehavior()
        {
        }

        protected override void UnloadBehavior()
        {
        }

        protected override bool OnEquipUseInner(EquipmentSlot slot)
        {
            var body = slot.characterBody;
            if (body)
            {
                
            }
        }

        public class JarBulletTracker : MonoBehaviour
        {
            public int BulletsGrabbed;

            public void FixedUpdate()
            {
                if(BulletsGrabbed <= 0)
                {
                    UnityEngine.Object.Destroy(this);
                }
            }
        }
    }
}
