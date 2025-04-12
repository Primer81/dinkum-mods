using UnityEngine;

public class KiteDoesDamage : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		PaperLantern componentInParent = other.GetComponentInParent<PaperLantern>();
		if ((bool)componentInParent && componentInParent.isServer)
		{
			componentInParent.GetComponent<Damageable>().attackAndDoDamage(2, base.transform, 0f);
		}
	}
}
