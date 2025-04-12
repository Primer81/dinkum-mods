using I2.Loc;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
	public ShopBuyDrop isShopBuyDrop;

	public ReadableSign isSign;

	public MailBoxShowsMail isMailBox;

	public BulletinBoardShowNewMessage isBulletinBoard;

	public ChestPlaceable isChest;

	public FurnitureStatus isFurniture;

	public WorkTable isWorkTable;

	public MineControls isMineControls;

	public Vehicle isVehicle;

	public TileObjectAnimalHouse isAnimalHouse;

	public PickUpAndCarry isPickUpAndCarry;

	public ItemDepositAndChanger isItemChanger;

	public FarmAnimal isFarmAnimal;

	public MuseumPainting isPainting;

	public ItemSign isItemSign;

	public TileObjectGrowthStages isGrowable;

	public CustomiseHouseZone customiseHouse;

	public WritableSign isWritableSign;

	public CamerTripod isTripodCamera;

	public BoomBox isBoomBox;

	public bool showingToolTip(Transform rayPos, CharPickUp myPickUp)
	{
		if ((bool)isShopBuyDrop)
		{
			if (isShopBuyDrop.canTalkToKeeper() && !isShopBuyDrop.isKeeperWorking())
			{
				if (isShopBuyDrop.canTalkToKeeper() && (bool)isShopBuyDrop.closedConvo)
				{
					NotificationManager.manage.showButtonPrompt(ConversationGenerator.generate.GetToolTipWithFormat("Tip_Buy", Inventory.Instance.allItems[isShopBuyDrop.myItemId].getInvItemName()), "B", rayPos.position);
				}
				else
				{
					NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_ShopClosed", "no", rayPos.position);
				}
			}
			else if (isShopBuyDrop.canTalkToKeeper())
			{
				if ((bool)isShopBuyDrop.sellsAnimal && isShopBuyDrop.dummyAnimal.activeSelf)
				{
					NotificationManager.manage.showButtonPrompt(ConversationGenerator.generate.GetToolTipWithFormat("Tip_Buy", isShopBuyDrop.sellsAnimal.GetAnimalName()), "B", rayPos.position);
				}
				else if (isShopBuyDrop.myItemId != -1)
				{
					NotificationManager.manage.showButtonPrompt(ConversationGenerator.generate.GetToolTipWithFormat("Tip_Buy", Inventory.Instance.allItems[isShopBuyDrop.myItemId].getInvItemName()), "B", rayPos.position);
				}
			}
			else
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_ClerkBusy", "no", rayPos.position);
			}
			return true;
		}
		if ((bool)isSign)
		{
			if (isSign.usePromptInsteadOfReadPromt)
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Use", "B", rayPos.position);
			}
			else
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Read", "B", rayPos.position);
			}
			return true;
		}
		if ((bool)isWritableSign)
		{
			if ((bool)isWritableSign.toSpin)
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Spin", "B", rayPos.position);
				return true;
			}
			if (myPickUp.myEquip.itemCurrentlyHolding == SignManager.manage.signWritingPen)
			{
				if (!isWritableSign.isAnimalTank && !isWritableSign.isMannequin && !isWritableSign.isToolRack && !isWritableSign.isDisplayCase)
				{
					NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Write", "X", rayPos.position);
				}
				return true;
			}
		}
		if ((bool)isMailBox)
		{
			if (isMailBox.hasMail)
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Read", "B", rayPos.position);
			}
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Read", "B", rayPos.position);
			return true;
		}
		if ((bool)isBulletinBoard)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Read", "B", rayPos.position);
			return true;
		}
		if ((bool)isChest)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Open", "B", rayPos.position);
			return true;
		}
		if ((bool)isFurniture)
		{
			if (isFurniture.isSeat)
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Sit", "B", rayPos.position);
			}
			else
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_LayDown", "B", rayPos.position);
			}
			return true;
		}
		if ((bool)isWorkTable)
		{
			NotificationManager.manage.showButtonPrompt(ConversationGenerator.generate.GetToolTipWithFormat("Tip_UseSubject", isWorkTable.GetWorkTableName()), "B", rayPos.position);
			return true;
		}
		if ((bool)isMineControls)
		{
			if (NetworkMapSharer.Instance.canUseMineControls)
			{
				if (isMineControls.forEntrance)
				{
					NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Use", "B", rayPos.position);
				}
				else
				{
					NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Use", "B", rayPos.position);
				}
				return true;
			}
			return true;
		}
		if ((bool)isVehicle)
		{
			if (rayPos.tag == "VehicleStorage")
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Open", "B", rayPos.position);
				return true;
			}
			if (rayPos.tag == "DropOffSpot")
			{
				NotificationManager.manage.hideButtonPrompt();
				return true;
			}
			if (rayPos.tag == "Passenger")
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Sit", "B", rayPos.position);
				return true;
			}
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Drive", "B", rayPos.position);
			return true;
		}
		_ = (bool)isAnimalHouse;
		if ((bool)isPickUpAndCarry && isPickUpAndCarry.canBePickedUp && !isPickUpAndCarry.IsCarriedByPlayer)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_PickUp", "B", rayPos.position);
			return true;
		}
		if ((bool)customiseHouse && (bool)myPickUp.myInteract.myEquip.itemCurrentlyHolding && myPickUp.myInteract.myEquip.itemCurrentlyHolding == HouseEditor.edit.houseKitItem)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_CustomiseHouse", "X", rayPos.position);
			return true;
		}
		if ((bool)isItemChanger)
		{
			if ((bool)myPickUp.myInteract.myEquip.itemCurrentlyHolding && isItemChanger.canDepositThisItem(myPickUp.myInteract.myEquip.itemCurrentlyHolding, myPickUp.myInteract.InsideHouseDetails))
			{
				NotificationManager.manage.showButtonPrompt(isItemChanger.GetLocalisedVerb(), "X", rayPos.position);
			}
			else if ((bool)myPickUp.myInteract.myEquip.itemCurrentlyHolding && isItemChanger.returnAmountNeeded(myPickUp.myInteract.myEquip.itemCurrentlyHolding) > 0 && !isItemChanger.getIfProcessing())
			{
				NotificationManager.manage.showButtonPrompt(string.Concat((LocalizedString)"ToolTips/Tip_Requires", " ", isItemChanger.returnAmountNeeded(myPickUp.myInteract.myEquip.itemCurrentlyHolding).ToString()), "no", rayPos.position);
			}
			else
			{
				NotificationManager.manage.hideButtonPrompt();
			}
			return true;
		}
		if ((bool)isFarmAnimal)
		{
			if ((bool)isFarmAnimal.canBeHarvested && isFarmAnimal.canBeHarvested.checkIfCanHarvest(myPickUp.myEquip.itemCurrentlyHolding))
			{
				NotificationManager.manage.showButtonPrompt(isFarmAnimal.canBeHarvested.getToolTip(myPickUp.myEquip.itemCurrentlyHolding), "X", rayPos.position);
				return true;
			}
			NotificationManager.manage.showButtonPrompt(ConversationGenerator.generate.GetToolTipWithFormat("Tip_Pet", isFarmAnimal.getAnimalName()), "B", rayPos.position);
			return true;
		}
		if ((bool)isTripodCamera)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_UseCamera", "B", rayPos.position);
			return true;
		}
		if ((bool)isBoomBox)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Use", "B", rayPos.position);
			return true;
		}
		if ((bool)isPainting)
		{
			if (isPainting.checkIfMuseumKeeperCanBeTalkedTo() && !isPainting.checkIfMuseumKeeperIsAtWork())
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_ShopClosed", "no", rayPos.position);
			}
			else if (isPainting.checkIfMuseumKeeperCanBeTalkedTo())
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Check", "B", rayPos.position);
			}
			else
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_ClerkBusy", "no", rayPos.position);
			}
			return true;
		}
		if ((bool)isItemSign)
		{
			if (isItemSign.isSilo)
			{
				if (myPickUp.myInteract.myEquip.itemCurrentlyHolding == isItemSign.itemCanPlaceIn)
				{
					TileObject componentInParent = isItemSign.GetComponentInParent<TileObject>();
					if (WorldManager.Instance.onTileStatusMap[componentInParent.xPos, componentInParent.yPos] < 200)
					{
						NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Insert", "X", rayPos.position);
					}
					else
					{
						NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_TooFull", "no", rayPos.position);
					}
					return true;
				}
			}
			else if ((bool)myPickUp.myInteract.myEquip.itemCurrentlyHolding)
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Display", "B", rayPos.position);
				return true;
			}
		}
		if ((bool)isGrowable && (bool)myPickUp.myInteract.myEquip.itemCurrentlyHolding)
		{
			for (int i = 0; i < isGrowable.itemsToPlace.Length; i++)
			{
				if (myPickUp.myInteract.myEquip.itemCurrentlyHolding == isGrowable.itemsToPlace[i])
				{
					TileObject componentInParent2 = isGrowable.GetComponentInParent<TileObject>();
					if (WorldManager.Instance.onTileStatusMap[componentInParent2.xPos, componentInParent2.yPos] < isGrowable.maxStageToReachByPlacing)
					{
						NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Bait", "X", rayPos.position);
						return true;
					}
					NotificationManager.manage.hideButtonPrompt();
					return false;
				}
			}
		}
		return false;
	}
}
