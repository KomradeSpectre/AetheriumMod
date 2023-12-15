using UnityEngine;
using System;

public class ChildLocatorCustom : MonoBehaviour
{
	[Serializable]
	public struct NameTransformPair
	{
		public string name;
		public Transform transform;
	}

	[SerializeField]
	public NameTransformPair[] transformPairs;
}
