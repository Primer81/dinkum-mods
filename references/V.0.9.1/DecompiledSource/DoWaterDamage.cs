using UnityEngine;

public class DoWaterDamage : MonoBehaviour
{
	public LayerMask damagelayer;

	public float radius = 8f;

	private void Start()
	{
		if (NetworkMapSharer.Instance.isServer)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, radius, damagelayer);
			for (int i = 0; i < array.Length; i++)
			{
				CheckCollider(array[i]);
			}
		}
	}

	private void CheckCollider(Collider other)
	{
		Damageable componentInParent = other.transform.GetComponentInParent<Damageable>();
		if ((bool)componentInParent)
		{
			if (componentInParent.onFire)
			{
				componentInParent.NetworkonFire = false;
				componentInParent.RpcPutOutFireInWater();
			}
			if (componentInParent.fireImmune)
			{
				componentInParent.attackAndDoDamage(15, base.transform);
			}
		}
	}
}
