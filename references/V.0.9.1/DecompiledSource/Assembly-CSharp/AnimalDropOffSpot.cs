using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalDropOffSpot : MonoBehaviour
{
	public enum depositType
	{
		AnimalTrap,
		SellByWeight,
		GemCrusher
	}

	public depositType dropOffType;

	public NPCSchedual.Locations foundInLocation;

	private List<SellByWeight> objectsInThePickUpPoint = new List<SellByWeight>();

	public bool lineUpOnly;

	public Transform ejectPos;

	private void OnTriggerEnter(Collider other)
	{
		if (lineUpOnly)
		{
			return;
		}
		if (dropOffType == depositType.AnimalTrap)
		{
			TrappedAnimal componentInParent = other.GetComponentInParent<TrappedAnimal>();
			if ((bool)componentInParent && !componentInParent.GetComponent<PickUpAndCarry>().delivered)
			{
				componentInParent.GetComponent<PickUpAndCarry>().Networkdelivered = true;
				NetworkMapSharer.Instance.placeAnimalInCollectionPoint(componentInParent.netId);
			}
		}
		if (!NetworkMapSharer.Instance.isServer)
		{
			return;
		}
		if (dropOffType == depositType.SellByWeight && other.CompareTag("CarryObject"))
		{
			SellByWeight componentInParent2 = other.GetComponentInParent<SellByWeight>();
			if ((bool)componentInParent2 && componentInParent2.GetComponentInParent<PickUpAndCarry>().GetLastCarriedBy() != 0)
			{
				NetworkMapSharer.Instance.RpcSellByWeight(componentInParent2.GetComponentInParent<PickUpAndCarry>().GetLastCarriedBy(), componentInParent2.netId, (int)foundInLocation);
			}
		}
		if (dropOffType == depositType.GemCrusher && other.CompareTag("CarryObject"))
		{
			CrushableCarry componentInParent3 = other.GetComponentInParent<CrushableCarry>();
			if ((bool)componentInParent3 && !componentInParent3.inserted)
			{
				componentInParent3.inserted = true;
				StartCoroutine(DelayGemCrush(componentInParent3));
			}
		}
	}

	private IEnumerator DelayGemCrush(CrushableCarry toCrush)
	{
		BoxCollider myCol = GetComponent<BoxCollider>();
		myCol.enabled = false;
		FarmAnimalHouseFloor myTileId = GetComponentInParent<FarmAnimalHouseFloor>();
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(1, myTileId.xPos, myTileId.yPos);
		NetworkMapSharer.Instance.RpcPlayBigStoneGrinderEffects(base.transform.position);
		int dropId = toCrush.dropsOnCrush.getItemId();
		int perfectId = -1;
		if ((bool)toCrush.perfectGem)
		{
			perfectId = toCrush.perfectGem.getItemId();
		}
		int dropAmount = toCrush.dropAmount;
		yield return new WaitForSeconds(1f);
		NetworkMapSharer.Instance.RpcPlayDestroyCarrySound(toCrush.transform.position, toCrush.GetComponent<PickUpAndCarry>().prefabId);
		toCrush.CrushNow();
		StartCoroutine(DropWithDelay(dropId, dropAmount, perfectId));
		yield return new WaitForSeconds(1.5f);
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(0, myTileId.xPos, myTileId.yPos);
		myCol.enabled = true;
	}

	private IEnumerator DropWithDelay(int dropId, int dropAmount, int perfectId)
	{
		for (int i = 0; i < dropAmount; i++)
		{
			yield return new WaitForSeconds(0.25f);
			NetworkMapSharer.Instance.spawnAServerDrop(dropId, 1, ejectPos.position);
		}
		if (perfectId != -1 && Random.Range(0, 100) < 20)
		{
			yield return new WaitForSeconds(0.25f);
			NetworkMapSharer.Instance.spawnAServerDrop(perfectId, 1, ejectPos.position);
		}
	}
}
