using UnityEngine;

public class OpenThunderboxDoor : MonoBehaviour
{
	public Animator anim;

	private Transform inCollider;

	private void OnEnable()
	{
		inCollider = null;
		anim.Rebind();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (inCollider == null && ((bool)other.GetComponentInParent<CharMovement>() || (bool)other.GetComponentInParent<NPCAI>()))
		{
			inCollider = other.transform.root;
			anim.SetBool("Open", value: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (inCollider != null && ((bool)other.GetComponentInParent<CharMovement>() || (bool)other.GetComponentInParent<NPCAI>()) && other.transform.root == inCollider)
		{
			inCollider = null;
			anim.SetBool("Open", value: false);
		}
	}
}
