using System.Collections;
using UnityEngine;

public class ChestPlaceable : MonoBehaviour
{
	public ASound openSound;

	public ASound closeSound;

	public Animator chestAnim;

	private TileObject myTileObject;

	public bool isStash;

	public bool isFishPond;

	public bool isBugTerrarium;

	public bool isAutoSorter;

	public bool isMannequin;

	public bool isToolRack;

	public bool isDisplayStand;

	private Coroutine chestCloseRoutine;

	public static WaitForSeconds chestWait = new WaitForSeconds(1f);

	private void Awake()
	{
		myTileObject = GetComponent<TileObject>();
		base.gameObject.AddComponent<InteractableObject>().isChest = this;
	}

	public bool checkIfEmpty(int xPos, int yPos, HouseDetails inside)
	{
		return ContainerManager.manage.checkIfEmpty(xPos, yPos, inside);
	}

	public int myXPos()
	{
		return myTileObject.xPos;
	}

	public int myYPos()
	{
		return myTileObject.yPos;
	}

	public void openChest()
	{
		chestAnim.enabled = true;
		chestAnim.SetBool("Open", value: true);
	}

	public void closeChest()
	{
		if (chestCloseRoutine != null)
		{
			StopCoroutine(chestCloseRoutine);
		}
		if (!isAutoSorter)
		{
			chestCloseRoutine = StartCoroutine(disableAnimatorOnChestClose());
		}
		chestAnim.SetBool("Open", value: false);
	}

	private IEnumerator disableAnimatorOnChestClose()
	{
		chestAnim.enabled = true;
		yield return chestWait;
		chestAnim.enabled = false;
	}
}
