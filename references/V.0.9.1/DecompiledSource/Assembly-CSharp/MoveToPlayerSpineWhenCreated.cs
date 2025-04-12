using System.Collections;
using UnityEngine;

public class MoveToPlayerSpineWhenCreated : MonoBehaviour
{
	private GameObject originalParent;

	private void OnEnable()
	{
		StartCoroutine(moveToPlayerSpine());
	}

	private IEnumerator moveToPlayerSpine()
	{
		yield return null;
		if ((bool)GetComponentInParent<CharMovement>() && base.transform.parent.parent.parent.parent.parent.name == "Spine_002")
		{
			Vector3 localPosition = new Vector3(1.02f, 0f, 0f);
			Quaternion localRotation = base.transform.localRotation;
			base.transform.SetParent(base.transform.parent.parent.parent.parent.parent);
			base.transform.localPosition = localPosition;
			base.transform.localRotation = localRotation;
		}
	}
}
