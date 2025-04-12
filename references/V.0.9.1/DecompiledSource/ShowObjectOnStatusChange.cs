using System.Collections;
using UnityEngine;

public class ShowObjectOnStatusChange : MonoBehaviour
{
	[Header("Default (show item from tile status)--------")]
	public GameObject[] objectsToShow;

	[Header("Chest stuff--------")]
	public ChestPlaceable isChest;

	[Header("Sign stuff--------")]
	public ItemSign isSign;

	[Header("Clothing Stuff--------")]
	public ClothingDisplay isClothing;

	[Header("Scale stuff--------")]
	public Transform toScale;

	public int fullSizeAtNumber = 1;

	public ASound playSoundOnSizeChange;

	[Header("Other Stuff-------")]
	public bool animatePopUpOnChange;

	private int lastShowing = -2;

	public bool canBePickedUpByHand;

	private int insideLastShowing = -2;

	[Header("Flowerbed Stuff --------")]
	public FlowerBedObjects hasFlowers;

	[Header("BoomBox Stuff --------")]
	public BoomBox isBoomBox;

	public void showGameObject(int status)
	{
		if ((bool)isClothing)
		{
			isClothing.updateStatus(status);
		}
		if ((bool)isSign)
		{
			isSign.updateStatus(status);
		}
		if ((bool)isBoomBox)
		{
			isBoomBox.SetSongFromStatus(status);
		}
	}

	public void showGameObject(int xPos, int yPos, HouseDetails inside = null)
	{
		if ((bool)hasFlowers)
		{
			hasFlowers.SetFlowerBedObject(WorldManager.Instance.onTileStatusMap[xPos, yPos]);
			return;
		}
		if ((bool)isBoomBox)
		{
			if (inside == null)
			{
				isBoomBox.SetSongFromStatus(WorldManager.Instance.onTileStatusMap[xPos, yPos]);
			}
			else
			{
				isBoomBox.SetSongFromStatus(inside.houseMapOnTileStatus[xPos, yPos]);
			}
			return;
		}
		if ((bool)toScale)
		{
			float y = (float)WorldManager.Instance.onTileStatusMap[xPos, yPos] / ((float)fullSizeAtNumber * 1f);
			toScale.localScale = new Vector3(1f, y, 1f);
			SoundManager.Instance.playASoundAtPoint(playSoundOnSizeChange, base.transform.position);
			return;
		}
		if ((bool)isClothing)
		{
			if (inside == null)
			{
				isClothing.updateStatus(WorldManager.Instance.onTileStatusMap[xPos, yPos]);
			}
			else
			{
				isClothing.updateStatus(inside.houseMapOnTileStatus[xPos, yPos]);
			}
		}
		if ((bool)isSign)
		{
			if (inside == null)
			{
				isSign.updateStatus(WorldManager.Instance.onTileStatusMap[xPos, yPos]);
			}
			else
			{
				isSign.updateStatus(inside.houseMapOnTileStatus[xPos, yPos]);
			}
			return;
		}
		if ((bool)isChest)
		{
			if (inside != null)
			{
				if (insideLastShowing != -2 && insideLastShowing != inside.houseMapOnTileStatus[xPos, yPos])
				{
					if (inside.houseMapOnTileStatus[xPos, yPos] == 0)
					{
						SoundManager.Instance.playASoundAtPoint(isChest.closeSound, base.transform.position);
					}
					else if (inside.houseMapOnTileStatus[xPos, yPos] == 1)
					{
						SoundManager.Instance.playASoundAtPoint(isChest.openSound, base.transform.position);
					}
				}
				insideLastShowing = inside.houseMapOnTileStatus[xPos, yPos];
				if (insideLastShowing == 0)
				{
					isChest.closeChest();
				}
				else
				{
					isChest.openChest();
				}
			}
			else
			{
				if (lastShowing != -2 && lastShowing != WorldManager.Instance.onTileStatusMap[xPos, yPos])
				{
					if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == 0)
					{
						SoundManager.Instance.playASoundAtPoint(isChest.closeSound, base.transform.position);
					}
					else if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == 1)
					{
						SoundManager.Instance.playASoundAtPoint(isChest.openSound, base.transform.position);
					}
				}
				lastShowing = WorldManager.Instance.onTileStatusMap[xPos, yPos];
				if (lastShowing == 0)
				{
					isChest.closeChest();
				}
				else
				{
					isChest.openChest();
				}
			}
		}
		if ((bool)isChest || inside != null)
		{
			return;
		}
		for (int i = 0; i < objectsToShow.Length; i++)
		{
			if (WorldManager.Instance.onTileStatusMap[xPos, yPos] == i || (WorldManager.Instance.onTileStatusMap[xPos, yPos] == -1 && i == 0))
			{
				if (animatePopUpOnChange && lastShowing != WorldManager.Instance.onTileStatusMap[xPos, yPos])
				{
					StartCoroutine(AnimateAppear(objectsToShow[i]));
				}
				objectsToShow[i].SetActive(value: true);
			}
			else
			{
				objectsToShow[i].SetActive(value: false);
			}
		}
		lastShowing = WorldManager.Instance.onTileStatusMap[xPos, yPos];
	}

	private IEnumerator AnimateAppear(GameObject toPopUp)
	{
		float journey = 0f;
		while (journey <= 0.35f)
		{
			journey += Time.deltaTime;
			float t = UIAnimationManager.manage.windowsOpenCurve.Evaluate(Mathf.Clamp01(journey / 0.35f));
			toPopUp.transform.localScale = new Vector3(Mathf.LerpUnclamped(0.25f, 1f, t), Mathf.LerpUnclamped(0.1f, 1f, t), Mathf.LerpUnclamped(0.25f, 1f, t));
			yield return null;
		}
	}
}
