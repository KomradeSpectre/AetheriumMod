using EntityStates;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Aetherium.MyEntityStates
{
    public class AbyssalDash : EntityState
    {
		private Transform modelTransform;

		public static GameObject blinkPrefab;

		public static Material destealthMaterial;

		private float stopwatch;

		private Vector3 blinkDestination = Vector3.zero;

		private Vector3 blinkStart = Vector3.zero;

		public Vector3 lastBlinkPosition;

		public float duration = 0.2f;

		public float blinkDistance = 10f;

		public static string beginSoundString;

		public static string endSoundString;

		private Animator animator;

		private CharacterModel characterModel;

		private HurtBoxGroup hurtboxGroup;

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
			writer.Write(blinkStart);
			writer.Write(blinkDestination);
			writer.Write(stopwatch);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
			blinkDestination = reader.ReadVector3();
			blinkStart = reader.ReadVector3();
			stopwatch = reader.ReadSingle();
        }

        public override void OnEnter()
		{
			base.OnEnter();
			Util.PlaySound(EntityStates.ImpMonster.BlinkState.beginSoundString, gameObject);
			modelTransform = GetModelTransform();
			if (modelTransform)
			{
				animator = modelTransform.GetComponent<Animator>();
				characterModel = modelTransform.GetComponent<CharacterModel>();
				hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
			}
			if (characterModel)
			{
				characterModel.invisibilityCount++;
			}
			if (hurtboxGroup)
			{
				hurtboxGroup.hurtBoxesDeactivatorCounter++;
			}
			if (characterMotor)
			{
				characterMotor.enabled = false;
			}
			Vector3 vector = inputBank.moveVector * blinkDistance;
			blinkDestination = transform.position;
			blinkStart = transform.position;
			var isAerial = (characterBody && characterBody.isFlying) || (characterMotor && !characterMotor.isGrounded);
			NodeGraph nodes =  isAerial ? SceneInfo.instance.airNodes : SceneInfo.instance.groundNodes;
			NodeGraph.NodeIndex nodeIndex = nodes.FindClosestNode(transform.position + vector, characterBody.hullClassification);

			//Correct any node errors here if possible, else we'll just blink to the same spot we were in.
			if(nodeIndex == NodeGraph.NodeIndex.invalid)
            {
                if (isAerial)
                {
					nodes = SceneInfo.instance.groundNodes;
					nodeIndex = nodes.FindClosestNode(transform.position + vector, characterBody.hullClassification);
                }
                else
                {
					nodes = SceneInfo.instance.airNodes;
					nodeIndex = nodes.FindClosestNode(transform.position + vector, characterBody.hullClassification);
                }
            }

			nodes.GetNodePosition(nodeIndex, out blinkDestination);
			blinkDestination += transform.position - characterBody.footPosition;
			CreateBlinkEffect(Util.GetCorePosition(gameObject));
		}

		private void CreateBlinkEffect(Vector3 origin)
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(blinkDestination - blinkStart);
			effectData.origin = origin;
			EffectManager.SpawnEffect(EntityStates.ImpMonster.BlinkState.blinkPrefab, effectData, false);
		}

		private void SetPosition(Vector3 newPosition)
		{
			if (characterMotor)
			{
				characterMotor.Motor.SetPositionAndRotation(newPosition, characterMotor.transform.rotation);
			}
            else
            {
				if (characterBody.rigidbody) characterBody.rigidbody.interpolation = RigidbodyInterpolation.None;
				characterBody.transform.SetPositionAndRotation(newPosition, characterBody.transform.rotation);
			}
		}
		
		private static readonly FieldInfo Velo = typeof(CharacterMotor).GetField("velocity");
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			stopwatch += Time.fixedDeltaTime;
			if (characterMotor && characterDirection)
			{
				Velo.SetValue(characterMotor, Vector3.zero);
			}
			SetPosition(Vector3.Lerp(blinkStart, blinkDestination, stopwatch / duration));
			if (stopwatch >= duration && isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}

		public override void OnExit()
		{
			Util.PlaySound(EntityStates.ImpMonster.BlinkState.endSoundString, gameObject);
			CreateBlinkEffect(Util.GetCorePosition(gameObject));
			modelTransform = GetModelTransform();
			if (modelTransform && destealthMaterial)
			{
				TemporaryOverlay temporaryOverlay = animator.gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 1f;
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = destealthMaterial;
				temporaryOverlay.inspectorCharacterModel = animator.gameObject.GetComponent<CharacterModel>();
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.animateShaderAlpha = true;
			}
			if (characterModel)
			{
				characterModel.invisibilityCount--;
			}
			if (hurtboxGroup)
			{
				hurtboxGroup.hurtBoxesDeactivatorCounter--;
			}
			if (characterMotor)
			{
				characterMotor.enabled = true;
			}

			//PlayAnimation("Gesture, Additive", "BlinkEnd");
			base.OnExit();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
