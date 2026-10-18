using RoR2;
using UnityEngine;

namespace Aetherium.Utils.Components
{
    /// <summary>
    /// Floats an elite's display model above a body for as long as it has the elite buff.
    /// Works for every body without per-body display rules. Purely visual, so it runs on every client.
    /// </summary>
    public class EliteFloatingDisplay : MonoBehaviour
    {
        public CharacterBody Body;
        public BuffDef EliteBuffDef;
        public GameObject DisplayInstance;

        public float HeightOffset = 1f;
        public float SpinSpeed = 45f;
        public float BobAmplitude = 0.15f;
        public float BobFrequency = 1.5f;

        private float _stopwatch;

        /// <summary>
        /// Adds a floating display for the given elite buff to the body, unless it already has one.
        /// </summary>
        /// <param name="sizeMultiplier">Display size relative to the body's radius. 1 means the display is as wide as the body.</param>
        public static EliteFloatingDisplay Attach(CharacterBody body, BuffDef eliteBuffDef, GameObject displayPrefab, float sizeMultiplier = 0.35f, float heightOffset = 1f)
        {
            if(!body || !displayPrefab) { return null; }

            foreach(var existing in body.GetComponents<EliteFloatingDisplay>())
            {
                if(existing.EliteBuffDef == eliteBuffDef) { return existing; }
            }

            var floatingDisplay = body.gameObject.AddComponent<EliteFloatingDisplay>();
            floatingDisplay.Body = body;
            floatingDisplay.EliteBuffDef = eliteBuffDef;
            floatingDisplay.HeightOffset = heightOffset;

            var size = Mathf.Clamp(body.radius, 0.5f, 6f) * sizeMultiplier;
            floatingDisplay.DisplayInstance = Instantiate(displayPrefab);
            floatingDisplay.DisplayInstance.transform.localScale = displayPrefab.transform.localScale * size;
            floatingDisplay.UpdatePosition();

            return floatingDisplay;
        }

        public void LateUpdate()
        {
            if(!Body || !Body.HasBuff(EliteBuffDef) || (Body.healthComponent && !Body.healthComponent.alive))
            {
                Destroy(this);
                return;
            }

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if(!DisplayInstance || !Body) { return; }

            _stopwatch += Time.deltaTime;

            var top = Body.corePosition + Vector3.up * (Body.radius + HeightOffset);
            DisplayInstance.transform.position = top + Vector3.up * (Mathf.Sin(_stopwatch * BobFrequency) * BobAmplitude);
            DisplayInstance.transform.rotation = Quaternion.Euler(0f, _stopwatch * SpinSpeed, 0f);
        }

        public void OnDestroy()
        {
            if(DisplayInstance)
            {
                Destroy(DisplayInstance);
            }
        }
    }
}
