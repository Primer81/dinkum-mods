using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheatMenuButton : MonoBehaviour
{
	public Image icon;

	public TextMeshProUGUI text;

	public int myItemNo;

	public bool isCreativeButton;

	public bool isMinimisedCreativeButton;

	public Color dulledColour;

	public Color defaultColour;

	public void setUpButton(int itemNo)
	{
		myItemNo = itemNo;
		if (itemNo >= Inventory.Instance.allItems.Length || itemNo <= -1)
		{
			text.text = "";
			icon.enabled = false;
		}
		else
		{
			text.text = Inventory.Instance.allItems[itemNo].getInvItemName();
			icon.sprite = Inventory.Instance.allItems[itemNo].getSprite();
			icon.enabled = true;
		}
	}

	public void pressButton()
	{
		if (myItemNo >= Inventory.Instance.allItems.Length || myItemNo == -1)
		{
			return;
		}
		if (Inventory.Instance.allItems[myItemNo].hasFuel)
		{
			if (isMinimisedCreativeButton)
			{
				if (Inventory.Instance.checkIfItemCanFit(myItemNo, Inventory.Instance.allItems[myItemNo].fuelMax))
				{
					Inventory.Instance.dragSlot.updateSlotContentsAndRefresh(myItemNo, Inventory.Instance.allItems[myItemNo].fuelMax);
				}
				else
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.pocketsFull);
				}
			}
			else if (!Inventory.Instance.addItemToInventory(myItemNo, Inventory.Instance.allItems[myItemNo].fuelMax))
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.pocketsFull);
			}
		}
		else if (Inventory.Instance.allItems[myItemNo].isDeed || !Inventory.Instance.allItems[myItemNo].checkIfStackable())
		{
			if (isMinimisedCreativeButton)
			{
				if (Inventory.Instance.checkIfItemCanFit(myItemNo, 1))
				{
					Inventory.Instance.dragSlot.updateSlotContentsAndRefresh(myItemNo, 1);
				}
				else
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.pocketsFull);
				}
			}
			else if (!Inventory.Instance.addItemToInventory(myItemNo, 1))
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.pocketsFull);
			}
		}
		else if (isMinimisedCreativeButton)
		{
			if (Inventory.Instance.checkIfItemCanFit(myItemNo, CheatScript.cheat.amountToGive))
			{
				if (Inventory.Instance.getAmountOfItemInAllSlots(myItemNo) == 0)
				{
					Inventory.Instance.dragSlot.updateSlotContentsAndRefresh(myItemNo, CheatScript.cheat.amountToGive);
				}
				else
				{
					Inventory.Instance.addItemToInventory(myItemNo, CheatScript.cheat.amountToGive);
				}
			}
			else
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.pocketsFull);
			}
		}
		else if ((isCreativeButton && !Inventory.Instance.addItemToInventory(myItemNo, 99)) || !Inventory.Instance.addItemToInventory(myItemNo, CheatScript.cheat.amountToGive))
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.pocketsFull);
		}
	}
}
