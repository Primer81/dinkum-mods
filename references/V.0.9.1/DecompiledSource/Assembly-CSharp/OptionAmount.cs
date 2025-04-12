using System.Collections;
using TMPro;
using UnityEngine;

public class OptionAmount : MonoBehaviour
{
	public TextMeshProUGUI selectedAmountText;

	public TextMeshProUGUI moneyAmount;

	public Transform upButton;

	public Transform downButton;

	private int selectedAmount = 1;

	private int itemIdBeingShown;

	private int valueMultiplier = 1;

	private int maxSelectAmount = 999;

	private bool fillingTileObject;

	private int fillingXPos;

	private int fillingYPos;

	private InventoryItem inventoryItemFillingWith;

	public Transform moneyBox;

	public void selectedAmountUp()
	{
		if (selectedAmount == maxSelectAmount)
		{
			selectedAmount = 1;
		}
		else
		{
			selectedAmount = Mathf.Clamp(selectedAmount + 1, 1, maxSelectAmount);
			StartCoroutine(holdUpOrDown(1));
		}
		selectedAmountText.text = selectedAmount.ToString() ?? "";
		setMoneyAmountText();
		SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
	}

	public void selectedAmountDown()
	{
		if (selectedAmount == 1)
		{
			selectedAmount = maxSelectAmount;
		}
		else
		{
			selectedAmount = Mathf.Clamp(selectedAmount - 1, 1, maxSelectAmount);
			StartCoroutine(holdUpOrDown(-1));
		}
		selectedAmountText.text = selectedAmount.ToString() ?? "";
		setMoneyAmountText();
		SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
	}

	public int getSelectedAmount()
	{
		return selectedAmount;
	}

	public void fillItemDetails(int itemId, int newMultiplier)
	{
		maxSelectAmount = 999;
		valueMultiplier = newMultiplier;
		itemIdBeingShown = itemId;
		selectedAmount = 1;
		selectedAmountText.text = selectedAmount.ToString() ?? "";
		setMoneyAmountText();
	}

	private IEnumerator holdUpOrDown(int dif)
	{
		float increaseCheck = 0f;
		float holdTimer = 0f;
		float multiplierTimer = 0f;
		int multiplier = 1;
		while (InputMaster.input.UISelectHeld())
		{
			if (increaseCheck < 0.15f - holdTimer)
			{
				increaseCheck += Time.deltaTime;
			}
			else
			{
				increaseCheck = 0f;
				int num = dif * multiplier;
				if (selectedAmount + num == Mathf.Clamp(selectedAmount + num, 1, maxSelectAmount))
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
				}
				selectedAmount = Mathf.Clamp(selectedAmount + num, 1, maxSelectAmount);
				selectedAmountText.text = selectedAmount.ToString() ?? "";
				setMoneyAmountText();
				multiplierTimer += 1f;
			}
			if (multiplierTimer < 50f)
			{
				multiplier = 1;
			}
			else if (multiplierTimer < 100f)
			{
				multiplier = 2;
			}
			else if (multiplierTimer < 150f)
			{
				multiplier = 5;
			}
			holdTimer = Mathf.Clamp(holdTimer + Time.deltaTime / 8f, 0f, 0.14f);
			yield return null;
		}
	}

	public void setMoneyAmountText()
	{
		if (fillingTileObject)
		{
			moneyAmount.text = "";
			moneyBox.gameObject.SetActive(value: false);
		}
		else
		{
			moneyAmount.text = "<sprite=11>" + (selectedAmount * (Inventory.Instance.allItems[itemIdBeingShown].value * 2 * valueMultiplier)).ToString("n0");
			moneyBox.gameObject.SetActive(value: true);
		}
	}

	public bool IsFillingTileObject()
	{
		return fillingTileObject;
	}

	public void SetToBuyingItem()
	{
		fillingTileObject = false;
	}

	public void ConfirmFillTileObject()
	{
		if (WorldManager.Instance.onTileMap[fillingXPos, fillingYPos] >= 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[fillingXPos, fillingYPos]].showObjectOnStatusChange)
		{
			NetworkMapSharer.Instance.localChar.myInteract.CmdGiveStatus(selectedAmount + WorldManager.Instance.onTileStatusMap[fillingXPos, fillingYPos], fillingXPos, fillingYPos);
			Inventory.Instance.removeAmountOfItem(inventoryItemFillingWith.getItemId(), selectedAmount);
		}
	}

	public void FillItemDetailsForTileObjectFill(InventoryItem fillingWith, int xPos, int yPos)
	{
		fillingXPos = xPos;
		fillingYPos = yPos;
		inventoryItemFillingWith = fillingWith;
		fillingTileObject = true;
		int max = WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[xPos, yPos]].showObjectOnStatusChange.fullSizeAtNumber - WorldManager.Instance.onTileStatusMap[xPos, yPos];
		int amountOfItemInAllSlots = Inventory.Instance.getAmountOfItemInAllSlots(fillingWith.getItemId());
		maxSelectAmount = Mathf.Clamp(amountOfItemInAllSlots, 0, max);
		valueMultiplier = 0;
		selectedAmount = 1;
		selectedAmountText.text = selectedAmount.ToString() ?? "";
		setMoneyAmountText();
	}
}
