using System.Collections;
using UnityEngine;

public class AutoStackDropFly : MonoBehaviour
{
	private GameObject myDropPrefab;

	public ASound dropIntoBox;

	public void StartFlying(int dropId, Vector3 startingPos, Vector3 endPos, int endX, int endY, int houseX, int houseY)
	{
		CreateDropPrefab(dropId);
		StartCoroutine(FlyToPos(startingPos, endPos, endX, endY, houseX, houseY));
	}

	private IEnumerator FlyToPos(Vector3 startingPos, Vector3 endPos, int endX, int endY, int houseX, int houseY)
	{
		float travelTime2 = 0.75f;
		travelTime2 += Vector3.Distance(startingPos, endPos) / 25f;
		float timer = 0f;
		for (float num = 0f; num <= 0.25f; num += Time.deltaTime)
		{
			base.transform.position = Vector3.Lerp(startingPos, startingPos + Vector3.up * 2f, num * 4f);
		}
		endPos += Vector3.up;
		startingPos = base.transform.position;
		Vector3 peakPos = new Vector3((startingPos.x + endPos.x) / 2f, Mathf.Max(startingPos.y + 6f, endPos.y + 6f), (startingPos.z + endPos.z) / 2f);
		bool completedAnimation = false;
		for (; timer < travelTime2; timer += Time.deltaTime)
		{
			float num2 = timer / travelTime2;
			float num3 = Mathf.Pow(num2, 0.5f);
			Vector3 position = (1f - num3) * (1f - num3) * startingPos + 2f * (1f - num3) * num3 * peakPos + num3 * num3 * endPos;
			base.transform.position = position;
			if (num2 >= 0.75f)
			{
				base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.zero, Time.deltaTime * 10f);
			}
			if (!completedAnimation && num2 >= 0.9f)
			{
				completedAnimation = true;
				SoundManager.Instance.playASoundAtPoint(dropIntoBox, base.transform.position);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 3);
				EndChestWobbles(endX, endY, houseX, houseY);
			}
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	public void EndChestWobbles(int endX, int endY, int houseX, int houseY)
	{
		if (houseX == -1 && houseY == -1)
		{
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(endX, endY);
			if ((bool)tileObject)
			{
				tileObject.damage(damageWithSound: false, damageParticleOn: false);
			}
			return;
		}
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles)
		{
			TileObject tileObject2 = displayPlayerHouseTiles.tileObjectsInHouse[endX, endY];
			if ((bool)tileObject2)
			{
				tileObject2.damage(damageWithSound: false, damageParticleOn: false);
			}
		}
	}

	public void CreateDropPrefab(int myItemId)
	{
		if (((bool)Inventory.Instance.allItems[myItemId].equipable && Inventory.Instance.allItems[myItemId].equipable.cloths && Inventory.Instance.allItems[myItemId].equipable.hat) || ((bool)Inventory.Instance.allItems[myItemId].equipable && Inventory.Instance.allItems[myItemId].equipable.cloths && Inventory.Instance.allItems[myItemId].equipable.face))
		{
			myDropPrefab = Object.Instantiate(EquipWindow.equip.holdingHatOrFaceObject, base.transform);
			myDropPrefab.GetComponent<SpawnHatOrFaceInside>().setUpForObject(myItemId);
		}
		else if ((bool)Inventory.Instance.allItems[myItemId].altDropPrefab)
		{
			myDropPrefab = Object.Instantiate(Inventory.Instance.allItems[myItemId].altDropPrefab, base.transform);
		}
		else
		{
			myDropPrefab = Object.Instantiate(Inventory.Instance.allItems[myItemId].itemPrefab, base.transform);
		}
		myDropPrefab.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		myDropPrefab.transform.localPosition = Vector3.zero;
		myDropPrefab.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
		SetItemTexture componentInChildren = myDropPrefab.GetComponentInChildren<SetItemTexture>();
		if ((bool)componentInChildren)
		{
			componentInChildren.setTexture(Inventory.Instance.allItems[myItemId]);
			if ((bool)componentInChildren.changeSize)
			{
				componentInChildren.changeSizeOfTrans(Inventory.Instance.allItems[myItemId].transform.localScale);
			}
		}
		if ((bool)myDropPrefab.GetComponent<Animator>())
		{
			Object.Destroy(myDropPrefab.GetComponent<Animator>());
		}
	}
}
