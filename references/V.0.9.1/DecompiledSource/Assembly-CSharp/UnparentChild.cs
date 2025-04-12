using System.Collections;
using UnityEngine;

public class UnparentChild : MonoBehaviour
{
	public GameObject child;

	public bool resetRotation;

	public bool followOldParent;

	private Vector3 oldLocalSpace;

	public bool delayKinematicRelease;

	public Transform moveToTransformBeforeRelease;

	public void OnEnable()
	{
		if (followOldParent)
		{
			oldLocalSpace = child.transform.localPosition;
		}
		if (resetRotation)
		{
			child.transform.rotation = Quaternion.identity;
		}
		if (followOldParent)
		{
			StartCoroutine(followOldParentRoutine());
		}
		if (delayKinematicRelease)
		{
			StartCoroutine(delayRigidbodyRelease());
		}
		else
		{
			child.transform.SetParent(null);
		}
	}

	private IEnumerator delayRigidbodyRelease()
	{
		child.transform.position = moveToTransformBeforeRelease.position;
		yield return null;
		child.transform.SetParent(null);
		child.transform.localScale = Vector3.one;
		child.transform.position = moveToTransformBeforeRelease.position;
		yield return null;
		child.transform.position = moveToTransformBeforeRelease.position;
		child.GetComponent<Rigidbody>().isKinematic = false;
	}

	private IEnumerator followOldParentRoutine()
	{
		while (true)
		{
			yield return null;
			child.transform.position = base.transform.position + oldLocalSpace;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(child);
	}
}
