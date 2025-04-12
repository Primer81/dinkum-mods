using UnityEngine;

public class ChangeCarryWhenTouched : MonoBehaviour
{
	public bool canTouch;

	public int carryIdToTouch;

	public int carryIdToSpawn;

	public PickUpAndCarry myCarry;

	private void OnTriggerEnter(Collider other)
	{
		if ((bool)myCarry && myCarry.isServer && canTouch)
		{
			PickUpAndCarry componentInParent = other.GetComponentInParent<PickUpAndCarry>();
			if ((bool)componentInParent && componentInParent.prefabId == carryIdToTouch)
			{
				canTouch = false;
				componentInParent.GetComponent<ChangeCarryWhenTouched>().canTouch = false;
				NetworkMapSharer.Instance.RpcPlayCarryDeathPart(carryIdToSpawn, base.transform.position);
				NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[carryIdToSpawn], base.transform.position);
				NetworkMapSharer.Instance.DestroyCarryable(myCarry);
				NetworkMapSharer.Instance.DestroyCarryable(componentInParent);
			}
		}
	}
}
