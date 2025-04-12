using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueButton : MonoBehaviour
{
	private int showingItemId;

	public TextMeshProUGUI itemNameText;

	public TextMeshProUGUI itemPriceText;

	public Image itemIcon;

	public void setUpButton(int itemId)
	{
		showingItemId = itemId;
		itemIcon.sprite = Inventory.Instance.allItems[itemId].getSprite();
		itemNameText.text = Inventory.Instance.allItems[itemId].getInvItemName();
		if (Inventory.Instance.allItems[itemId].isUniqueItem)
		{
			itemPriceText.text = ConversationGenerator.generate.GetJournalNameByTag("Unavailable");
		}
		else
		{
			itemPriceText.text = "<sprite=11>" + ((int)((float)Inventory.Instance.allItems[itemId].value * 2.5f)).ToString("n0");
		}
	}

	public void pressButton()
	{
		CatalogueManager.manage.showItemInfo(showingItemId);
	}

	public InventoryItem getShowingInvItem()
	{
		return Inventory.Instance.allItems[showingItemId];
	}

	public string getShowingInvItemName()
	{
		return Inventory.Instance.allItems[showingItemId].getInvItemName();
	}
}
