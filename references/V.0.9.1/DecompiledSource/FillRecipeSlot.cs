using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FillRecipeSlot : MonoBehaviour
{
	public TextMeshProUGUI itemName;

	public TextMeshProUGUI itemAmounts;

	public Image itemImage;

	public InventoryItem itemInSlot;

	public Image lockedIcon;

	public Image background;

	public Color defualtColor;

	public GameObject canBeCraftedImage;

	public Image itemBackgroundImage;

	private int showingItemId;

	private int amountNeeded;

	public void fillRecipeSlot(int itemId)
	{
		showingItemId = itemId;
		itemInSlot = Inventory.Instance.allItems[itemId];
		itemName.text = Inventory.Instance.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.Instance.allItems[itemId].getSprite();
		if (CraftingManager.manage.canBeCraftedInAVariation(itemId))
		{
			canBeCraftedImage.SetActive(value: true);
		}
		else
		{
			canBeCraftedImage.SetActive(value: false);
		}
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	public void updateIfCanBeCrafted()
	{
		if (CraftingManager.manage.canBeCraftedInAVariation(showingItemId))
		{
			canBeCraftedImage.SetActive(value: true);
		}
		else
		{
			canBeCraftedImage.SetActive(value: false);
		}
	}

	public void refreshRecipeSlot()
	{
		fillRecipeSlot(Inventory.Instance.getInvItemId(itemInSlot));
	}

	public void fillRecipeSlotWithCraftAmount(int itemId, int amountBeingCrafted)
	{
		itemInSlot = Inventory.Instance.allItems[itemId];
		itemName.text = Inventory.Instance.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.Instance.allItems[itemId].getSprite();
		itemAmounts.text = amountBeingCrafted.ToString() ?? "";
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	public void fillRecipeSlotWithAmounts(int itemId, int amountHave, int amountNeed)
	{
		if (itemId >= 0)
		{
			itemInSlot = Inventory.Instance.allItems[itemId];
			itemName.text = Inventory.Instance.allItems[itemId].getInvItemName();
			itemImage.sprite = Inventory.Instance.allItems[itemId].getSprite();
			amountNeeded = amountNeed;
			if (amountHave < amountNeed)
			{
				itemAmounts.text = UIAnimationManager.manage.WrapStringInNotEnoughColor(amountHave.ToString() ?? "") + " / " + amountNeed;
			}
			else
			{
				itemAmounts.text = "<b>" + amountHave + "</b> / " + amountNeed;
			}
			if ((bool)itemBackgroundImage)
			{
				itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
			}
		}
	}

	public void fillRecipeSlotForQuestReward(int itemId, int stackAmount)
	{
		itemInSlot = Inventory.Instance.allItems[itemId];
		itemName.text = Inventory.Instance.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.Instance.allItems[itemId].getSprite();
		itemAmounts.text = stackAmount.ToString("n0");
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	public void fillRecipeSlotForCraftUnlock(int itemId, bool isUnlocked)
	{
		itemInSlot = Inventory.Instance.allItems[itemId];
		itemName.text = Inventory.Instance.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.Instance.allItems[itemId].getSprite();
		if (!isUnlocked)
		{
			lockedIcon.enabled = true;
			background.color = Color.Lerp(defualtColor, Color.black, 0.25f);
			itemImage.color = Color.Lerp(Color.white, Color.black, 0.25f);
		}
		else
		{
			background.color = defualtColor;
			lockedIcon.enabled = false;
			itemImage.color = Color.white;
		}
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	public void fillDeedBuySlot(int itemId)
	{
		itemInSlot = Inventory.Instance.allItems[itemId];
		itemName.text = Inventory.Instance.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.Instance.allItems[itemId].getSprite();
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	private void OnEnable()
	{
		Inventory.Instance.buttonsToSnapTo.Add(GetComponent<RectTransform>());
	}

	private void OnDisable()
	{
		Inventory.Instance.buttonsToSnapTo.Remove(GetComponent<RectTransform>());
	}

	public void setSlotFull()
	{
		itemBackgroundImage.color = Color.Lerp(Color.white, Color.black, 0.35f);
		itemImage.color = Color.Lerp(Color.white, Color.black, 0.35f);
		itemAmounts.text = "";
	}

	public void setSlotEmpty()
	{
		itemBackgroundImage.color = Color.white;
		itemImage.color = Color.white;
	}

	public int GetAmountNeededForRefresh()
	{
		return amountNeeded;
	}
}
