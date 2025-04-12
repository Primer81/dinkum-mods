using System.Collections;
using UnityEngine;

public class TreeMovingGloves : MonoBehaviour
{
	private CharChestCarrier myChar;

	private EquipItemToChar myCharEquip;

	public Transform treePos;

	private TileObject treePrefab;

	public Animator anim;

	public GameObject TreeBag;

	public ASound pickUpTreeSound;

	private void OnEnable()
	{
		myChar = GetComponentInParent<CharChestCarrier>();
		myCharEquip = GetComponentInParent<EquipItemToChar>();
		if ((bool)myChar)
		{
			myChar.SetTreeCarryGloves(this);
			StartCoroutine(ControllGloves());
		}
	}

	private IEnumerator ControllGloves()
	{
		while (true)
		{
			yield return null;
			if (myChar.isLocalPlayer)
			{
				HandleControls();
			}
			if (!myCharEquip)
			{
				continue;
			}
			if (myChar.treeCarryId != -1)
			{
				anim.SetBool("Open", value: false);
				TreeBag.SetActive(value: true);
				continue;
			}
			TreeBag.SetActive(value: false);
			if (myCharEquip.usingItem)
			{
				anim.SetBool("Open", value: false);
			}
			else
			{
				anim.SetBool("Open", value: true);
			}
		}
	}

	private void HandleControls()
	{
		if (!InputMaster.input.Use() || !Inventory.Instance.CanMoveCharacter())
		{
			return;
		}
		if (!myChar.IsHoldingTree())
		{
			if (myChar.CheckIfTileIsTreeForButton())
			{
				myChar.PickUpTree();
				Inventory.Instance.quickBarLocked(isLocked: true);
			}
		}
		else if (myChar.CheckIfTileIsEmptyForTreeForButton())
		{
			myChar.PlaceTree();
			Inventory.Instance.quickBarLocked(isLocked: false);
		}
	}

	public void SpawnTreeInHands(int treeId, int treeStatus)
	{
		if (treePrefab != null && treePrefab.tileObjectId != treeId)
		{
			Object.Destroy(treePrefab.gameObject);
		}
		if (treeId != -1 && treePrefab == null)
		{
			treePrefab = Object.Instantiate(WorldManager.Instance.allObjects[treeId].GetComponent<TileObject>(), treePos);
			treePrefab.transform.localPosition = Vector3.zero;
			treePrefab.transform.localRotation = Quaternion.identity;
			treePrefab.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		if ((bool)treePrefab && (bool)treePrefab.tileObjectGrowthStages)
		{
			treePrefab.tileObjectGrowthStages.setStageForHands(treeStatus);
		}
	}
}
