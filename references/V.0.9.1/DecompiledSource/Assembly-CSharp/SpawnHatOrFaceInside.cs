using UnityEngine;

public class SpawnHatOrFaceInside : MonoBehaviour
{
	public Transform spawnHere;

	public void setUpForObject(int objectId)
	{
		if ((bool)Inventory.Instance.allItems[objectId].altDropPrefab)
		{
			Transform obj = Object.Instantiate(Inventory.Instance.allItems[objectId].altDropPrefab, base.transform).transform;
			obj.localRotation = Quaternion.Euler(0f, 0f, 0f);
			obj.localPosition = Vector3.zero;
		}
		else
		{
			Transform obj2 = Object.Instantiate(Inventory.Instance.allItems[objectId].equipable.hatPrefab, spawnHere).transform;
			obj2.localRotation = Quaternion.Euler(0f, 0f, 0f);
			obj2.localPosition = Vector3.zero;
		}
	}
}
