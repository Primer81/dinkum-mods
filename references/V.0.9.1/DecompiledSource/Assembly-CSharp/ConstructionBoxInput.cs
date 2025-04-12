using UnityEngine;

public class ConstructionBoxInput : MonoBehaviour
{
	public ConversationObject constructionBoxNeedsMatsConvo;

	public ConversationObject constructionBoxFullConvo;

	public ConversationObject conversationWhenLastItemsDonatedConvo;

	public TileObject myTileObject;

	private bool isTileObjectToFill;

	public InventoryItem canBeFilledWith;

	public ConversationObject conversationWhenNotHoldingItemToFillWith;

	public bool isWishingWell;

	public void Start()
	{
		myTileObject = GetComponentInParent<TileObject>();
		if ((bool)myTileObject.showObjectOnStatusChange && (bool)myTileObject.showObjectOnStatusChange.toScale)
		{
			isTileObjectToFill = true;
		}
	}

	public ConversationObject getConversation()
	{
		if (isWishingWell)
		{
			if (Inventory.Instance.wallet < 150000)
			{
				return conversationWhenNotHoldingItemToFillWith;
			}
			if (NetworkMapSharer.Instance.wishManager.wishMadeToday)
			{
				return constructionBoxFullConvo;
			}
			return constructionBoxNeedsMatsConvo;
		}
		if (isTileObjectToFill)
		{
			if (Inventory.Instance.getAmountOfItemInAllSlots(canBeFilledWith.getItemId()) <= 0)
			{
				return conversationWhenNotHoldingItemToFillWith;
			}
			if (WorldManager.Instance.onTileStatusMap[myTileObject.xPos, myTileObject.yPos] >= myTileObject.showObjectOnStatusChange.fullSizeAtNumber)
			{
				return constructionBoxFullConvo;
			}
			GiveNPC.give.optionAmountWindow.FillItemDetailsForTileObjectFill(canBeFilledWith, myTileObject.xPos, myTileObject.yPos);
			return constructionBoxNeedsMatsConvo;
		}
		if (DeedManager.manage.checkIfDeedMaterialsComplete(myTileObject.xPos, myTileObject.yPos))
		{
			return constructionBoxFullConvo;
		}
		return constructionBoxNeedsMatsConvo;
	}

	public void openForGivingMenus()
	{
		GiveNPC.give.openBuildingGiveMenu(myTileObject.xPos, myTileObject.yPos);
	}
}
