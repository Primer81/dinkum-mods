using UnityEngine;

public class ImpostorWand : MonoBehaviour
{
	private CharInteract myCharInteract;

	private EquipItemToChar myEquip;

	public GameObject hideWhenInDisguise;

	public LayerMask damageLayer;

	public BoxCollider boxCheck;

	private void Start()
	{
		myCharInteract = GetComponentInParent<CharInteract>();
		myEquip = GetComponentInParent<EquipItemToChar>();
	}

	public void doDamageNow()
	{
		if (!myCharInteract || !myCharInteract.isLocalPlayer)
		{
			return;
		}
		int num = (int)myCharInteract.selectedTile.x;
		int num2 = (int)myCharInteract.selectedTile.y;
		int num3 = WorldManager.Instance.onTileMap[num, num2];
		if (CheckForAnimal())
		{
			return;
		}
		if (num3 >= 0 && num3 != 30)
		{
			if (!WorldManager.Instance.allObjects[num3].tileObjectBridge && (!WorldManager.Instance.allObjectSettings[num3].isMultiTileObject || (!WorldManager.Instance.allObjectSettings[num3].tileObjectLoadInside && WorldManager.Instance.allObjectSettings[num3].xSize <= 3 && WorldManager.Instance.allObjectSettings[num3].ySize <= 3)))
			{
				myEquip.CmdSetDisguise(WorldManager.Instance.onTileMap[num, num2]);
			}
		}
		else
		{
			myEquip.CmdSetDisguise(-1);
		}
	}

	public bool CheckForAnimal()
	{
		Collider[] array = Physics.OverlapBox(boxCheck.transform.position, boxCheck.size / 2f, boxCheck.transform.rotation, damageLayer);
		for (int i = 0; i < array.Length; i++)
		{
			AnimalAI componentInParent = array[i].transform.GetComponentInParent<AnimalAI>();
			if ((bool)componentInParent)
			{
				int num = componentInParent.animalId * 10 + componentInParent.getVariationNo();
				myEquip.CmdSetDisguise(-2000 - num);
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		if ((bool)myEquip && myEquip.disguiseId != -1)
		{
			hideWhenInDisguise.SetActive(value: false);
		}
		else
		{
			hideWhenInDisguise.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		if ((bool)myEquip && myEquip.isLocalPlayer)
		{
			myEquip.CmdSetDisguise(-1);
		}
	}
}
