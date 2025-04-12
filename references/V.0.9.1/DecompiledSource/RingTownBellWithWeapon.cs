using UnityEngine;

public class RingTownBellWithWeapon : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		CharMovement componentInParent = other.transform.GetComponentInParent<CharMovement>();
		if ((bool)componentInParent && componentInParent.isLocalPlayer)
		{
			componentInParent.CmdRingTownBell();
		}
	}
}
