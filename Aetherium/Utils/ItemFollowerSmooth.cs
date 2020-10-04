using System.Collections.ObjectModel;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.StatHooks;
using System;
using KomradeSpectre.Aetherium;

namespace Aetherium.Utils
{
	//Original From ROR2, Modified to smooth

	[RequireComponent(typeof(ItemDisplay))]
    public class ItemFollowerSmooth : MonoBehaviour
    {
		public GameObject followerPrefab;

		public GameObject targetObject;

		public BezierCurveLine followerCurve;

		public LineRenderer followerLineRenderer;

		public float distanceDampTime;

		public float distanceMaxSpeed;

		public ItemDisplay itemDisplay;

		public Vector3 velocityDistance;

		public Vector3 v0;

		public Vector3 v1;

		public float SmoothingNumber;

		[HideInInspector]
		public GameObject followerInstance;

		public void Start()
		{
			this.itemDisplay = base.GetComponent<ItemDisplay>();
			this.followerLineRenderer = base.GetComponent<LineRenderer>();
			this.Rebuild();
		}

		public void Rebuild()
		{
			if (this.itemDisplay.GetVisibilityLevel() == VisibilityLevel.Invisible)
			{
				if (this.followerInstance)
				{
					UnityEngine.Object.Destroy(this.followerInstance);
				}
				if (this.followerLineRenderer)
				{
					this.followerLineRenderer.enabled = false;
					return;
				}
			}
			else
			{
				if (!this.followerInstance)
				{
					this.followerInstance = UnityEngine.Object.Instantiate<GameObject>(this.followerPrefab, this.targetObject.transform.position, Quaternion.identity);
					this.followerInstance.transform.localScale = base.transform.localScale;
					if (this.followerCurve)
					{
						this.v0 = this.followerCurve.v0;
						this.v1 = this.followerCurve.v1;
					}
				}
				if (this.followerLineRenderer)
				{
					this.followerLineRenderer.enabled = true;
				}
			}
		}

		public void Update()
		{
			this.Rebuild();
			if (this.followerInstance)
			{
				Transform transform = this.followerInstance.transform;
				Transform transform2 = this.targetObject.transform;
				transform.position = Vector3.SmoothDamp(transform.position, transform2.position, ref this.velocityDistance, this.distanceDampTime);
				transform.rotation = Quaternion.Slerp(transform.rotation, transform2.rotation, SmoothingNumber);
				if (this.followerCurve)
				{
					this.followerCurve.v0 = base.transform.TransformVector(this.v0);
					this.followerCurve.v1 = transform.TransformVector(this.v1);
					this.followerCurve.p1 = transform.position;
				}
			}
		}

		public void OnDestroy()
		{
			if (this.followerInstance)
			{
				UnityEngine.Object.Destroy(this.followerInstance);
			}
		}


	}
}
