using UnityEngine;

public class DealFireDamage : MonoBehaviour
{
	public int additionalDamage;

	private void OnTriggerEnter(Collider other)
	{
		Damageable componentInParent = other.GetComponentInParent<Damageable>();
		if ((bool)componentInParent)
		{
			if (NetworkMapSharer.Instance.isServer)
			{
				componentInParent.setOnFire();
			}
			else if ((bool)NetworkMapSharer.Instance.localChar)
			{
				NetworkMapSharer.Instance.localChar.CmdSetOnFire(componentInParent.netId);
			}
		}
	}
}
