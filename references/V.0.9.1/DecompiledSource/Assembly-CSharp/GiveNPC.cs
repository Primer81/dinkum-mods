using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiveNPC : MonoBehaviour
{
	public enum currentlyGivingTo
	{
		Give,
		Sell,
		SellToTrapper,
		Museum,
		Tech,
		Swapping,
		BulletinBoard,
		Build,
		SellToJimmy,
		SellToTuckshop,
		SellToBugComp,
		SellToFishingComp,
		BoomBoxSwap
	}

	public static GiveNPC give;

	public Transform GiveNPCWindow;

	public Transform GiveNPCSlotWindow;

	public GameObject constructionBoxNeededItems;

	public TextMeshProUGUI trackDeedRequirementsButtonText;

	public OptionAmount optionAmountWindow;

	public InventoryItem raffleTicketItem;

	public ConversationObject wantToBuySomethingConversation;

	public ConversationObject notEnoughMoneyToBuySomethingConversation;

	public ConversationObject hasEnoughMoneyToBuySomethingConversation;

	public ConversationObject notEnoughSpaceInInvToBuySomethingConversation;

	public ConversationObject nothingToGiveConversation;

	public ConversationObject somethingToGiveConversation;

	public ConversationObject somethingToGiveWithRaffleConversation;

	public ConversationObject totalEqualsZeroConversation;

	public ConversationObject nothingToGiveTrapperConversation;

	public ConversationObject somethingToGiveTrapperConversation;

	public ConversationObject totalEqualsZeroTrapperConversation;

	public ConversationObject nothingToGiveJimmyConversation;

	public ConversationObject somethingToGiveJimmyConversation;

	public ConversationObject totalEqualsZeroJimmyConversation;

	public ConversationObject nothingToGiveTuckshopConversation;

	public ConversationObject somethingToGiveTuckshopConversation;

	public ConversationObject totalEqualsZeroTuckshopConversation;

	public ConversationObject nothingToGiveToBugCompConversation;

	public ConversationObject somethingToGiveToBugCompConversation;

	public ConversationObject totalEqualsZeroBugCompConversation;

	public ConversationObject nothingToGiveToFishingCompConversation;

	public ConversationObject somethingToGiveToFishingCompConversation;

	public ConversationObject totalEqualsZeroFishingCompConversation;

	public ConversationObject wantToBuyRecipeConversation;

	public ConversationObject alreadyHaveRecipeConversation;

	public ConversationObject wantToBuyAnimalConversation;

	public ConversationObject wantToBuyAnimalNotEnoughMoneyConversation;

	public ConversationObject wantToBuyAnimalNoHousesConversation;

	public ConversationObject cancleAnimalBuyingConversation;

	public ConversationObject onBuyAnimalConversation;

	public ConversationObject buyAnimalNotLocalConversation;

	public ConversationObject animalAlreadyInShopConversation;

	public ConversationObject noTechToGiveConversation;

	public ConversationObject donateTechItemsConversation;

	public ConversationObject giveItemToSellByWeightConversation;

	public ConversationObject dontHaveLicenceConversation;

	public ConversationObject dontHaveLicenceAnimalConversation;

	public ConversationObject noItemToGiveConversation;

	public ConversationObject noBirthdayGiftGivenConversation;

	public ConversationObject[] givenItemIncorrectConversation;

	public ConversationObject[] givenCorrectItemConversation;

	public ShopBuyDrop dropToBuy;

	public FillRecipeSlot[] requiredItemSlots;

	public bool giveWindowOpen;

	public int moneyOffer;

	public Image giveButton;

	public TextMeshProUGUI giveText;

	public Color giveColour;

	public Color cancleColour;

	private SellByWeight sellingByWeight;

	public PostOnBoard givingPost;

	private int buildingX;

	private int buildingY;

	private int onTopPos = -1;

	private int houseX = -1;

	private int houseY = -1;

	public currentlyGivingTo giveMenuTypeOpen;

	public bool optionWindowOpen;

	private int storedReward = 1000;

	private int raffleTicketAmount;

	private int museumDonateReward;

	private void Awake()
	{
		give = this;
	}

	private void Start()
	{
		GiveNPCWindow.gameObject.SetActive(value: false);
	}

	private IEnumerator UpdateMenu()
	{
		while (giveWindowOpen)
		{
			yield return null;
			if (giveMenuTypeOpen == currentlyGivingTo.Museum || giveMenuTypeOpen == currentlyGivingTo.Sell || giveMenuTypeOpen == currentlyGivingTo.SellToTrapper || giveMenuTypeOpen == currentlyGivingTo.SellToJimmy || giveMenuTypeOpen == currentlyGivingTo.SellToTuckshop || giveMenuTypeOpen == currentlyGivingTo.SellToBugComp || giveMenuTypeOpen == currentlyGivingTo.SellToFishingComp || giveMenuTypeOpen == currentlyGivingTo.Build || giveMenuTypeOpen == currentlyGivingTo.Tech)
			{
				if (getDollarValueOfGiveSlots())
				{
					giveButton.color = giveColour;
					if (giveMenuTypeOpen == currentlyGivingTo.Build)
					{
						giveText.text = ConversationGenerator.generate.GetToolTip("Tip_PlaceDeed");
					}
					else if (giveMenuTypeOpen == currentlyGivingTo.Museum || giveMenuTypeOpen == currentlyGivingTo.Tech)
					{
						giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Donate");
					}
					else
					{
						giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Sell");
					}
				}
				else
				{
					giveButton.color = cancleColour;
					giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Cancel");
				}
				continue;
			}
			if (giveMenuTypeOpen == currentlyGivingTo.Swapping)
			{
				if (getDollarValueOfGiveSlots())
				{
					giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Swap");
					giveButton.color = giveColour;
				}
				else
				{
					giveButton.color = cancleColour;
					giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Cancel");
				}
				continue;
			}
			if (giveMenuTypeOpen == currentlyGivingTo.BoomBoxSwap)
			{
				if (getDollarValueOfGiveSlots())
				{
					giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Swap");
					giveButton.color = giveColour;
				}
				else
				{
					giveButton.color = cancleColour;
					giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Cancel");
				}
				continue;
			}
			int num = 0;
			for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
			{
				if (Inventory.Instance.invSlots[i].isSelectedForGive())
				{
					num++;
				}
			}
			if (num != 0)
			{
				giveButton.color = giveColour;
				giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Give");
			}
			else
			{
				giveButton.color = cancleColour;
				giveText.text = ConversationGenerator.generate.GetToolTip("Tip_Cancel");
			}
		}
	}

	public void openBuildingGiveMenu(int xPos, int yPos)
	{
		buildingX = xPos;
		buildingY = yPos;
		if (!NetworkMapSharer.Instance.isServer)
		{
			NetworkMapSharer.Instance.localChar.CmdGetDeedIngredients(WorldManager.Instance.onTileMap[xPos, yPos]);
		}
		else
		{
			OpenGiveWindow(currentlyGivingTo.Build);
		}
	}

	public void OpenBoomBox(int xPos, int yPos, int houseXNew = -1, int houseYNew = -1, int onTopPosNew = -1)
	{
		buildingX = xPos;
		buildingY = yPos;
		onTopPos = onTopPosNew;
		houseX = houseXNew;
		houseY = houseYNew;
		MonoBehaviour.print("Opening boom box inside = " + houseX + ", " + houseY);
	}

	public void SetFillingXAndYPos(int xPos, int yPos)
	{
		buildingX = xPos;
		buildingY = yPos;
	}

	public void updateDeedGive(int buildingId)
	{
		if (!giveWindowOpen || giveMenuTypeOpen != currentlyGivingTo.Build || buildingId != WorldManager.Instance.onTileMap[buildingX, buildingY])
		{
			return;
		}
		int[] requiredItemsForDeed = DeedManager.manage.getRequiredItemsForDeed(buildingX, buildingY);
		int[] itemsAlreadyGivenForDeed = DeedManager.manage.getItemsAlreadyGivenForDeed(buildingX, buildingY);
		int[] requiredAmountForDeed = DeedManager.manage.getRequiredAmountForDeed(buildingX, buildingY);
		for (int i = 0; i < requiredItemSlots.Length; i++)
		{
			if (i < requiredItemsForDeed.Length)
			{
				requiredItemSlots[i].gameObject.SetActive(value: true);
				requiredItemSlots[i].fillRecipeSlotWithAmounts(requiredItemsForDeed[i], itemsAlreadyGivenForDeed[i], requiredAmountForDeed[i]);
				if (itemsAlreadyGivenForDeed[i] >= requiredAmountForDeed[i])
				{
					requiredItemSlots[i].setSlotFull();
				}
				else
				{
					requiredItemSlots[i].setSlotEmpty();
				}
			}
			else
			{
				requiredItemSlots[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void OpenGiveWindow(currentlyGivingTo giveMenuType = currentlyGivingTo.Give)
	{
		if (ConversationManager.manage.lastConversationTarget == null || ConversationManager.manage.lastConversationTarget.isSign)
		{
			givingPost = null;
		}
		else
		{
			givingPost = BulletinBoard.board.checkMissionsCompletedForNPC(ConversationManager.manage.lastConversationTarget.GetComponent<NPCIdentity>().NPCNo);
		}
		giveMenuTypeOpen = giveMenuType;
		blackOutNonGiveableObjects();
		constructionBoxNeededItems.gameObject.SetActive(value: false);
		if (giveMenuType != currentlyGivingTo.Tech && giveMenuType != currentlyGivingTo.Swapping && giveMenuType != currentlyGivingTo.BoomBoxSwap && giveMenuType != currentlyGivingTo.Sell && giveMenuType != currentlyGivingTo.SellToTrapper && giveMenuTypeOpen != currentlyGivingTo.SellToJimmy && giveMenuTypeOpen != currentlyGivingTo.SellToTuckshop && giveMenuTypeOpen != currentlyGivingTo.SellToBugComp && giveMenuTypeOpen != currentlyGivingTo.SellToFishingComp && giveMenuType == currentlyGivingTo.Build)
		{
			QuestTracker.track.updatePinnedRecipeButton();
			int[] requiredItemsForDeed = DeedManager.manage.getRequiredItemsForDeed(buildingX, buildingY);
			int[] itemsAlreadyGivenForDeed = DeedManager.manage.getItemsAlreadyGivenForDeed(buildingX, buildingY);
			int[] requiredAmountForDeed = DeedManager.manage.getRequiredAmountForDeed(buildingX, buildingY);
			constructionBoxNeededItems.gameObject.SetActive(value: true);
			for (int i = 0; i < requiredItemSlots.Length; i++)
			{
				if (i < requiredItemsForDeed.Length)
				{
					requiredItemSlots[i].gameObject.SetActive(value: true);
					requiredItemSlots[i].fillRecipeSlotWithAmounts(requiredItemsForDeed[i], itemsAlreadyGivenForDeed[i], requiredAmountForDeed[i]);
					if (itemsAlreadyGivenForDeed[i] >= requiredAmountForDeed[i])
					{
						requiredItemSlots[i].setSlotFull();
					}
					else
					{
						requiredItemSlots[i].setSlotEmpty();
					}
				}
				else
				{
					requiredItemSlots[i].gameObject.SetActive(value: false);
				}
			}
		}
		Inventory.Instance.weaponSlot.gameObject.SetActive(value: false);
		giveWindowOpen = true;
		StartCoroutine(UpdateMenu());
		Inventory.Instance.OpenInvForGive();
		GiveNPCWindow.gameObject.SetActive(value: true);
	}

	public InventoryItem GetDeedForCurrentBuilding()
	{
		return DeedManager.manage.GetDeedForBuilding(buildingX, buildingY);
	}

	public void openOptionAmountWindow()
	{
		optionWindowOpen = true;
		optionAmountWindow.gameObject.SetActive(value: true);
		if (!optionAmountWindow.IsFillingTileObject())
		{
			if (dropToBuy.priceMultiplier != 1)
			{
				optionAmountWindow.fillItemDetails(dropToBuy.myItemId, dropToBuy.priceMultiplier);
			}
			else
			{
				optionAmountWindow.fillItemDetails(dropToBuy.myItemId, 1);
			}
		}
	}

	public void closeOptionAmountWindow()
	{
		optionWindowOpen = false;
		optionAmountWindow.gameObject.SetActive(value: false);
	}

	public void CloseGiveWindow()
	{
		GiveNPCWindow.gameObject.SetActive(value: false);
	}

	public void CloseAndCancel()
	{
		returnItemsAndEmptyGiveSlots();
		CloseAndMakeOffer();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void CloseAndMakeOffer()
	{
		GiveNPCWindow.gameObject.SetActive(value: false);
		Inventory.Instance.invOpen = false;
		Inventory.Instance.openAndCloseInv();
		bool flag = false;
		if (giveMenuTypeOpen == currentlyGivingTo.Tech)
		{
			getDollarValueOfGiveSlots();
			if (moneyOffer == 0)
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, noTechToGiveConversation);
			}
			else
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, donateTechItemsConversation);
			}
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Swapping)
		{
			for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
			{
				if (Inventory.Instance.invSlots[i].isSelectedForGive())
				{
					NetworkMapSharer.Instance.localChar.CmdDropItem(Inventory.Instance.invSlots[i].itemNo, Inventory.Instance.invSlots[i].stack, NetworkMapSharer.Instance.localChar.transform.position, NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position);
					Inventory.Instance.invSlots[i].updateSlotContentsAndRefresh(Inventory.Instance.getInvItemId(BugAndFishCelebration.bugAndFishCel.inventoryFullConvo.targetOpenings.talkingAboutItem), 1);
					Inventory.Instance.equipNewSelectedSlot();
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				NetworkMapSharer.Instance.localChar.CmdDropItem(Inventory.Instance.getInvItemId(BugAndFishCelebration.bugAndFishCel.inventoryFullConvo.targetOpenings.talkingAboutItem), 1, NetworkMapSharer.Instance.localChar.transform.position, NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position);
			}
			clearAllSelectedSlots();
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.BoomBoxSwap)
		{
			for (int j = 0; j < Inventory.Instance.invSlots.Length; j++)
			{
				if (Inventory.Instance.invSlots[j].isSelectedForGive())
				{
					NetworkMapSharer.Instance.localChar.CmdSetSongForBoomBox(Inventory.Instance.invSlots[j].itemNo, buildingX, buildingY, houseX, houseY, onTopPos);
					Inventory.Instance.invSlots[j].updateSlotContentsAndRefresh(-1, 0);
					break;
				}
			}
			clearAllSelectedSlots();
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Build)
		{
			bool flag2 = false;
			for (int k = 0; k < Inventory.Instance.invSlots.Length; k++)
			{
				if (Inventory.Instance.invSlots[k].isSelectedForGive() && Inventory.Instance.invSlots[k].itemNo != -1)
				{
					int stack = Inventory.Instance.invSlots[k].stack;
					if (Inventory.Instance.invSlots[k].getGiveAmount() == 0)
					{
						Inventory.Instance.invSlots[k].stack = DeedManager.manage.returnStackAndDonateItemToDeed(Inventory.Instance.invSlots[k].itemNo, Inventory.Instance.invSlots[k].stack, buildingX, buildingY);
					}
					else
					{
						Inventory.Instance.invSlots[k].stack = Inventory.Instance.invSlots[k].stack - Inventory.Instance.invSlots[k].getGiveAmount() + DeedManager.manage.returnStackAndDonateItemToDeed(Inventory.Instance.invSlots[k].itemNo, Inventory.Instance.invSlots[k].getGiveAmount(), buildingX, buildingY);
					}
					if (stack > Inventory.Instance.invSlots[k].stack)
					{
						flag2 = true;
					}
					if (Inventory.Instance.invSlots[k].stack > 0)
					{
						Inventory.Instance.invSlots[k].updateSlotContentsAndRefresh(Inventory.Instance.invSlots[k].itemNo, Inventory.Instance.invSlots[k].stack);
					}
					else
					{
						Inventory.Instance.invSlots[k].updateSlotContentsAndRefresh(-1, 0);
					}
				}
			}
			if (flag2)
			{
				SoundManager.Instance.play2DSound(SoundManager.Instance.dropInBoxSound);
				NetworkMapSharer.Instance.localChar.CmdDonateDeedIngredients(WorldManager.Instance.onTileMap[buildingX, buildingY], DeedManager.manage.getItemsAlreadyGivenForDeed(buildingX, buildingY));
			}
			if (DeedManager.manage.checkIfDeedMaterialsComplete(buildingX, buildingY))
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, ConversationManager.manage.donatingToBuilding.conversationWhenLastItemsDonatedConvo);
			}
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Museum || giveMenuTypeOpen == currentlyGivingTo.Sell || giveMenuTypeOpen == currentlyGivingTo.SellToJimmy || giveMenuTypeOpen == currentlyGivingTo.SellToTuckshop || giveMenuTypeOpen == currentlyGivingTo.SellToBugComp || giveMenuTypeOpen == currentlyGivingTo.SellToFishingComp || giveMenuTypeOpen == currentlyGivingTo.SellToTrapper)
		{
			if (getDollarValueOfGiveSlots())
			{
				if (giveMenuTypeOpen == currentlyGivingTo.Museum)
				{
					List<InventoryItem> list = new List<InventoryItem>();
					bool doublesGiven = false;
					for (int l = 0; l < Inventory.Instance.invSlots.Length; l++)
					{
						if (Inventory.Instance.invSlots[l].isSelectedForGive())
						{
							if (!list.Contains(Inventory.Instance.allItems[Inventory.Instance.invSlots[l].itemNo]))
							{
								list.Add(Inventory.Instance.allItems[Inventory.Instance.invSlots[l].itemNo]);
							}
							else
							{
								doublesGiven = true;
							}
						}
					}
					museumDonateReward = list.Count;
					if (list.Count > 1)
					{
						ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.GetDonationConversationOnMultipleSlots(doublesGiven));
					}
					else if (list.Count == 1)
					{
						ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.GetDonationConversation(list[0]));
					}
					else
					{
						ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationManager.manage.museumConvos.itemCantBeDonated);
					}
				}
				else if (moneyOffer == 0)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, totalEqualsZeroConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToTuckshop)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, somethingToGiveTuckshopConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToFishingComp)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, somethingToGiveToFishingCompConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToBugComp)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, somethingToGiveToBugCompConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToTrapper)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, somethingToGiveTrapperConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, somethingToGiveJimmyConversation);
				}
				else if (raffleTicketAmount == 0)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, somethingToGiveConversation);
				}
				else
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, somethingToGiveWithRaffleConversation);
				}
			}
			else if (giveMenuTypeOpen != currentlyGivingTo.Museum)
			{
				if (giveMenuTypeOpen == currentlyGivingTo.SellToTuckshop)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, nothingToGiveTuckshopConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToTrapper)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, nothingToGiveTrapperConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, nothingToGiveJimmyConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToBugComp)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, nothingToGiveToBugCompConversation);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToFishingComp)
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, nothingToGiveToFishingCompConversation);
				}
				else
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, nothingToGiveConversation);
				}
			}
		}
		else
		{
			InventorySlot inventorySlot = null;
			for (int m = 0; m < Inventory.Instance.invSlots.Length; m++)
			{
				if (Inventory.Instance.invSlots[m].isSelectedForGive())
				{
					inventorySlot = Inventory.Instance.invSlots[m];
					break;
				}
			}
			if (!inventorySlot || inventorySlot.itemNo == -1)
			{
				if (NPCManager.manage.NPCDetails[ConversationManager.manage.lastConversationTarget.myId.NPCNo].IsTodayMyBirthday())
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, noBirthdayGiftGivenConversation);
				}
				else
				{
					ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, noItemToGiveConversation);
				}
			}
			else
			{
				int nPCNo = ConversationManager.manage.lastConversationTarget.GetComponent<NPCIdentity>().NPCNo;
				if (givingPost != null)
				{
					int num = inventorySlot.getGiveAmount();
					if (num == 0)
					{
						num = inventorySlot.stack;
					}
					if (givingPost.isTrade)
					{
						if (givingPost.checkIfTradeItemIsDifferent(inventorySlot.itemNo))
						{
							if (givingPost.checkIfTradeItemIsOk(inventorySlot.itemNo))
							{
								ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givingPost.getPostPostsById().onGivenItemConvo);
								inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, --inventorySlot.stack);
							}
							else
							{
								ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givingPost.getPostPostsById().onGivenWrongTypeForTradeConvo);
							}
						}
						else
						{
							ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givingPost.getPostPostsById().onGivenSameItemForTradeConvo);
						}
					}
					else if (givingPost.requiredItem == inventorySlot.itemNo && givingPost.requireItemAmount <= num)
					{
						ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givingPost.getPostPostsById().onGivenItemConvo);
						inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, inventorySlot.stack - givingPost.requireItemAmount);
					}
					else
					{
						ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givenItemIncorrectConversation[Random.Range(0, givenItemIncorrectConversation.Length)]);
					}
				}
				else
				{
					int num2 = inventorySlot.getGiveAmount();
					if (num2 == 0)
					{
						num2 = inventorySlot.stack;
					}
					if (NPCManager.manage.NPCRequests[nPCNo].checkIfItemMatchesRequest(inventorySlot.itemNo) && NPCManager.manage.NPCRequests[nPCNo].desiredAmount <= num2)
					{
						if (NPCManager.manage.NPCRequests[nPCNo].checkIfNPCAccepts(inventorySlot.itemNo))
						{
							if (Inventory.Instance.getInvItemId(NPCManager.manage.NPCDetails[nPCNo].favouriteFood) == inventorySlot.itemNo)
							{
								ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationGenerator.generate.givenFavouriteFood[Random.Range(0, ConversationGenerator.generate.givenFavouriteFood.Length)]);
							}
							else if (NPCManager.manage.NPCDetails[ConversationManager.manage.lastConversationTarget.myId.NPCNo].IsTodayMyBirthday())
							{
								ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, NPCManager.manage.NPCDetails[ConversationManager.manage.lastConversationTarget.myId.NPCNo].completeBirthdayConvos[0]);
							}
							else if (NPCManager.manage.NPCDetails[ConversationManager.manage.lastConversationTarget.myId.NPCNo].completeRequestConvos.Length != 0)
							{
								ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, NPCManager.manage.NPCDetails[ConversationManager.manage.lastConversationTarget.myId.NPCNo].completeRequestConvos[0]);
							}
							else
							{
								ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givenCorrectItemConversation[Random.Range(0, givenCorrectItemConversation.Length)]);
							}
							inventorySlot.stack -= NPCManager.manage.NPCRequests[nPCNo].desiredAmount;
							inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, inventorySlot.stack);
							storeRewardValue();
						}
						else
						{
							if (Inventory.Instance.getInvItemId(NPCManager.manage.NPCDetails[nPCNo].hatedFood) == inventorySlot.itemNo)
							{
								ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationGenerator.generate.givenHatedFood[Random.Range(0, ConversationGenerator.generate.givenHatedFood.Length)]);
								NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(-1);
							}
							else
							{
								ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, ConversationGenerator.generate.givenDislikeFood[Random.Range(0, ConversationGenerator.generate.givenDislikeFood.Length)]);
							}
							NPCManager.manage.NPCRequests[nPCNo].failRequest(nPCNo);
						}
					}
					else
					{
						ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, givenItemIncorrectConversation[Random.Range(0, givenItemIncorrectConversation.Length)]);
						givingPost = null;
					}
				}
			}
		}
		if (giveMenuTypeOpen == currentlyGivingTo.BulletinBoard || giveMenuTypeOpen == currentlyGivingTo.Give)
		{
			if (givingPost == null || (givingPost.getPostPostsById() != null && givingPost.isTrade))
			{
				clearAllSelectedSlots();
			}
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Build)
		{
			clearAllSelectedSlots();
		}
		clearAllDisabled();
		giveWindowOpen = false;
	}

	private void storeRewardValue()
	{
		InventorySlot inventorySlot = null;
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			if (Inventory.Instance.invSlots[i].isSelectedForGive())
			{
				inventorySlot = Inventory.Instance.invSlots[i];
				break;
			}
		}
		if ((bool)inventorySlot && inventorySlot.itemNo > -1)
		{
			storedReward = Inventory.Instance.allItems[inventorySlot.itemNo].value * 2 + Random.Range(Inventory.Instance.allItems[inventorySlot.itemNo].value / 2, Inventory.Instance.allItems[inventorySlot.itemNo].value * 2);
		}
		else
		{
			storedReward = 1000;
		}
	}

	public int getRewardAmount()
	{
		return storedReward;
	}

	public void completeRequest()
	{
		clearAllSelectedSlots();
	}

	public bool getDollarValueOfGiveSlots()
	{
		moneyOffer = 0;
		raffleTicketAmount = 0;
		bool result = false;
		InventorySlot[] invSlots = Inventory.Instance.invSlots;
		foreach (InventorySlot inventorySlot in invSlots)
		{
			if (inventorySlot.itemNo == -1 || !inventorySlot.isSelectedForGive())
			{
				continue;
			}
			if (TownEventManager.manage.IsJohnsAnniversary() && (bool)inventorySlot.itemInSlot.fish)
			{
				raffleTicketAmount++;
			}
			result = true;
			if (inventorySlot.itemInSlot.hasFuel || inventorySlot.itemInSlot.hasColourVariation)
			{
				moneyOffer += inventorySlot.itemInSlot.value;
			}
			else if (inventorySlot.getGiveAmount() == 0)
			{
				if (giveMenuTypeOpen == currentlyGivingTo.Sell && (bool)inventorySlot.itemInSlot.relic)
				{
					moneyOffer += inventorySlot.itemInSlot.value * inventorySlot.stack / 4;
				}
				else
				{
					moneyOffer += inventorySlot.itemInSlot.value * inventorySlot.stack;
				}
			}
			else if (giveMenuTypeOpen == currentlyGivingTo.Sell && (bool)inventorySlot.itemInSlot.relic)
			{
				moneyOffer += inventorySlot.itemInSlot.value * inventorySlot.getGiveAmount() / 4;
			}
			else
			{
				moneyOffer += inventorySlot.itemInSlot.value * inventorySlot.getGiveAmount();
			}
		}
		moneyOffer += Mathf.RoundToInt((float)moneyOffer / 20f * (float)LicenceManager.manage.allLicences[8].getCurrentLevel());
		if (giveMenuTypeOpen == currentlyGivingTo.SellToBugComp)
		{
			moneyOffer = Mathf.RoundToInt((float)moneyOffer * 2.5f);
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.SellToFishingComp)
		{
			moneyOffer = Mathf.RoundToInt((float)moneyOffer * 2.5f);
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.SellToTuckshop)
		{
			moneyOffer = Mathf.RoundToInt((float)moneyOffer * 2.5f);
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.SellToTrapper)
		{
			moneyOffer = Mathf.RoundToInt((float)moneyOffer * 2f);
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
		{
			moneyOffer = Mathf.RoundToInt((float)moneyOffer * 1.5f);
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Tech)
		{
			moneyOffer = Mathf.RoundToInt((float)moneyOffer * 6f);
		}
		return result;
	}

	public void donateTechItems()
	{
		Inventory.Instance.changeWallet(moneyOffer, addToTownEconomy: false);
		CharLevelManager.manage.todaysMoneyTotal += moneyOffer;
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellItems, moneyOffer);
		InventorySlot[] invSlots = Inventory.Instance.invSlots;
		foreach (InventorySlot inventorySlot in invSlots)
		{
			if ((bool)inventorySlot.itemInSlot && inventorySlot.isSelectedForGive())
			{
				if (inventorySlot.getGiveAmount() == 0)
				{
					CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, inventorySlot.stack, CharLevelManager.manage.skillBoxes.Length);
					inventorySlot.updateSlotContentsAndRefresh(-1, 0);
				}
				else
				{
					CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, inventorySlot.getGiveAmount(), CharLevelManager.manage.skillBoxes.Length);
					inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, inventorySlot.stack - inventorySlot.getGiveAmount());
				}
			}
			inventorySlot.deselectThisSlotForGive();
		}
	}

	public void checkSellSlotForTask(InventorySlot slotToCheck, int stackAmount)
	{
		if (Inventory.Instance.allItems[slotToCheck.itemNo].taskWhenSold != 0)
		{
			DailyTaskGenerator.generate.doATask(Inventory.Instance.allItems[slotToCheck.itemNo].taskWhenSold, stackAmount);
			return;
		}
		if ((bool)Inventory.Instance.allItems[slotToCheck.itemNo].consumeable)
		{
			if (Inventory.Instance.allItems[slotToCheck.itemNo].consumeable.isFruit)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellFruit, stackAmount);
			}
			else if (Inventory.Instance.allItems[slotToCheck.itemNo].consumeable.isVegitable)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellCrops, stackAmount);
			}
		}
		if ((bool)Inventory.Instance.allItems[slotToCheck.itemNo].fish)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellFish, stackAmount);
		}
		if ((bool)Inventory.Instance.allItems[slotToCheck.itemNo].bug)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellBugs, stackAmount);
		}
	}

	public void sellItemsAndEmptyGiveSlots()
	{
		InventorySlot[] invSlots = Inventory.Instance.invSlots;
		foreach (InventorySlot inventorySlot in invSlots)
		{
			if ((bool)inventorySlot.itemInSlot && inventorySlot.isSelectedForGive())
			{
				if (inventorySlot.getGiveAmount() == 0)
				{
					if (!Inventory.Instance.allItems[inventorySlot.itemNo].checkIfStackable())
					{
						CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, 1, CharLevelManager.manage.skillBoxes.Length);
						checkSellSlotForTask(inventorySlot, 1);
					}
					else
					{
						CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, inventorySlot.stack, CharLevelManager.manage.skillBoxes.Length);
						checkSellSlotForTask(inventorySlot, inventorySlot.stack);
					}
					inventorySlot.updateSlotContentsAndRefresh(-1, 0);
				}
				else
				{
					if (!Inventory.Instance.allItems[inventorySlot.itemNo].checkIfStackable())
					{
						CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, 1, CharLevelManager.manage.skillBoxes.Length);
						checkSellSlotForTask(inventorySlot, 1);
					}
					else
					{
						CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, inventorySlot.getGiveAmount(), CharLevelManager.manage.skillBoxes.Length);
						checkSellSlotForTask(inventorySlot, inventorySlot.getGiveAmount());
					}
					inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, inventorySlot.stack - inventorySlot.getGiveAmount());
				}
			}
			inventorySlot.deselectThisSlotForGive();
		}
		if (NetworkMapSharer.Instance.isServer)
		{
			NPCManager.manage.npcStatus[ConversationManager.manage.lastConversationTarget.myId.NPCNo].moneySpentAtStore += moneyOffer;
		}
		if (raffleTicketAmount > 0 && giveMenuTypeOpen == currentlyGivingTo.Sell)
		{
			GiftedItemWindow.gifted.addToListToBeGiven(raffleTicketItem.getItemId(), raffleTicketAmount);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		Inventory.Instance.changeWallet(moneyOffer);
		CharLevelManager.manage.todaysMoneyTotal += moneyOffer;
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellItems, moneyOffer);
		moneyOffer = 0;
	}

	public void returnItemsAndEmptyGiveSlots()
	{
		clearAllSelectedSlots();
		moneyOffer = 0;
	}

	public void askAboutBuyingSomething(ShopBuyDrop myDrop, NPCAI npcTalkTo)
	{
		dropToBuy = myDrop;
		Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
		if (!Inventory.Instance.allItems[dropToBuy.myItemId].checkIfCanBuy())
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, dontHaveLicenceConversation);
		}
		else
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, wantToBuySomethingConversation);
		}
	}

	public void askAboutBuyingAnimal(ShopBuyDrop myDrop, NPCAI npcTalkTo)
	{
		dropToBuy = myDrop;
		Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
		if (!myDrop.sellsAnimal.GetComponent<FarmAnimal>().checkIfCanBuy())
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, dontHaveLicenceAnimalConversation);
			return;
		}
		if (FarmAnimalMenu.menu.checkIfAnimalBoxIsInShop())
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, animalAlreadyInShopConversation);
		}
		if (Inventory.Instance.wallet < myDrop.sellsAnimal.GetComponent<FarmAnimal>().baseValue)
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, wantToBuyAnimalNotEnoughMoneyConversation);
		}
		else if (!NetworkMapSharer.Instance.isServer)
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, buyAnimalNotLocalConversation);
		}
		else
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, wantToBuyAnimalConversation);
		}
	}

	public void askAboutBuyingRecipe(ShopBuyDrop myDrop, NPCAI npcTalkTo)
	{
		dropToBuy = myDrop;
		Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
		if (!CharLevelManager.manage.checkIfUnlocked(myDrop.myItemId))
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, wantToBuyRecipeConversation);
		}
		else
		{
			ConversationManager.manage.TalkToNPC(npcTalkTo, alreadyHaveRecipeConversation);
		}
	}

	public void tryToBuy()
	{
		if (!Inventory.Instance.allItems[dropToBuy.myItemId].checkIfCanBuy())
		{
			if ((bool)dropToBuy.sellsAnimal)
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, dontHaveLicenceAnimalConversation);
			}
			else
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, dontHaveLicenceConversation);
			}
		}
		else if (dropToBuy.usesPermitPoints)
		{
			int num = dropToBuy.getSellPrice() / 500;
			if (PermitPointsManager.manage.getCurrentPoints() < num)
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, notEnoughMoneyToBuySomethingConversation);
			}
			else if (Inventory.Instance.addItemToInventory(dropToBuy.myItemId, 1))
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, hasEnoughMoneyToBuySomethingConversation);
				PermitPointsManager.manage.spendPoints(num);
				if (NetworkMapSharer.Instance.isServer)
				{
					NPCManager.manage.npcStatus[ConversationManager.manage.lastConversationTarget.myId.NPCNo].moneySpentAtStore += dropToBuy.getSellPrice() * optionAmountWindow.getSelectedAmount();
				}
				dropToBuy.buyTheItem();
				dropToBuy = null;
			}
			else
			{
				ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, notEnoughSpaceInInvToBuySomethingConversation);
			}
		}
		else if (dropToBuy.recipesOnly && Inventory.Instance.wallet < dropToBuy.getRecipePrice())
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, notEnoughMoneyToBuySomethingConversation);
		}
		else if (!dropToBuy.recipesOnly && dropToBuy.gives10 && Inventory.Instance.wallet < dropToBuy.getSellPrice() * optionAmountWindow.getSelectedAmount())
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, notEnoughMoneyToBuySomethingConversation);
		}
		else if (!dropToBuy.recipesOnly && !dropToBuy.gives10 && Inventory.Instance.wallet < dropToBuy.getSellPrice())
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, notEnoughMoneyToBuySomethingConversation);
		}
		else if (dropToBuy.recipesOnly)
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, hasEnoughMoneyToBuySomethingConversation);
			Inventory.Instance.changeWallet(-dropToBuy.getRecipePrice());
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyItems, dropToBuy.getRecipePrice());
			GiftedItemWindow.gifted.addRecipeToUnlock(dropToBuy.myItemId);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
			dropToBuy.checkIfTaskCompelted();
			dropToBuy.buyTheItem();
			dropToBuy = null;
		}
		else if (dropToBuy.gives10 && Inventory.Instance.addItemToInventory(dropToBuy.myItemId, give.optionAmountWindow.getSelectedAmount()))
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, hasEnoughMoneyToBuySomethingConversation);
			Inventory.Instance.changeWallet(-dropToBuy.getSellPrice() * optionAmountWindow.getSelectedAmount());
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyItems, dropToBuy.getSellPrice() * optionAmountWindow.getSelectedAmount());
			if (NetworkMapSharer.Instance.isServer)
			{
				NPCManager.manage.npcStatus[ConversationManager.manage.lastConversationTarget.myId.NPCNo].moneySpentAtStore += dropToBuy.getSellPrice() * optionAmountWindow.getSelectedAmount();
			}
			dropToBuy.checkIfTaskCompelted(optionAmountWindow.getSelectedAmount());
			dropToBuy.buyTheItem();
			dropToBuy = null;
		}
		else if ((!Inventory.Instance.allItems[dropToBuy.myItemId].hasFuel && Inventory.Instance.addItemToInventory(dropToBuy.myItemId, 1)) || (Inventory.Instance.allItems[dropToBuy.myItemId].hasFuel && Inventory.Instance.addItemToInventory(dropToBuy.myItemId, Inventory.Instance.allItems[dropToBuy.myItemId].fuelMax)))
		{
			if (dropToBuy.gives5)
			{
				Inventory.Instance.addItemToInventory(dropToBuy.myItemId, 4);
			}
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, hasEnoughMoneyToBuySomethingConversation);
			Inventory.Instance.changeWallet(-dropToBuy.getSellPrice());
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyItems, dropToBuy.getSellPrice());
			if (NetworkMapSharer.Instance.isServer)
			{
				NPCManager.manage.npcStatus[ConversationManager.manage.lastConversationTarget.myId.NPCNo].moneySpentAtStore += dropToBuy.getSellPrice();
			}
			dropToBuy.checkIfTaskCompelted();
			dropToBuy.buyTheItem();
			dropToBuy = null;
		}
		else
		{
			ConversationManager.manage.TalkToNPC(ConversationManager.manage.lastConversationTarget, notEnoughSpaceInInvToBuySomethingConversation);
		}
	}

	public int getMuseumRewardOffer()
	{
		return museumDonateReward * 100;
	}

	public string getDonationName()
	{
		InventorySlot inventorySlot = null;
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			if (Inventory.Instance.invSlots[i].isSelectedForGive())
			{
				inventorySlot = Inventory.Instance.invSlots[i];
				break;
			}
		}
		if ((bool)inventorySlot && (bool)inventorySlot.itemInSlot)
		{
			return inventorySlot.itemInSlot.getInvItemName();
		}
		return "!";
	}

	public void donateItemToMuseum()
	{
		int num = 0;
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			if (Inventory.Instance.invSlots[i].isSelectedForGive() && !list.Contains(Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo]))
			{
				NetworkMapSharer.Instance.localChar.CmdDonateItemToMuseum(Inventory.Instance.invSlots[i].itemNo, Inventory.Instance.playerName);
				num += 100;
				list.Add(Inventory.Instance.allItems[Inventory.Instance.invSlots[i].itemNo]);
				Inventory.Instance.invSlots[i].updateSlotContentsAndRefresh(-1, 0);
			}
		}
		clearAllSelectedSlots();
		PermitPointsManager.manage.addPoints(num);
	}

	public void clearAllSelectedSlots()
	{
		InventorySlot[] invSlots = Inventory.Instance.invSlots;
		for (int i = 0; i < invSlots.Length; i++)
		{
			invSlots[i].deselectThisSlotForGive();
		}
	}

	public void clearAllDisabled()
	{
		Inventory.Instance.weaponSlot.gameObject.SetActive(value: true);
		InventorySlot[] invSlots = Inventory.Instance.invSlots;
		for (int i = 0; i < invSlots.Length; i++)
		{
			invSlots[i].clearDisable();
		}
	}

	public void blackOutNonGiveableObjects()
	{
		InventorySlot[] invSlots;
		if (giveMenuTypeOpen == currentlyGivingTo.Build)
		{
			int[] requiredItemsForDeed = DeedManager.manage.getRequiredItemsForDeed(buildingX, buildingY);
			int[] itemsAlreadyGivenForDeed = DeedManager.manage.getItemsAlreadyGivenForDeed(buildingX, buildingY);
			int[] requiredAmountForDeed = DeedManager.manage.getRequiredAmountForDeed(buildingX, buildingY);
			invSlots = Inventory.Instance.invSlots;
			foreach (InventorySlot inventorySlot in invSlots)
			{
				if (inventorySlot.itemNo == -1 || inventorySlot.itemInSlot.isDeed)
				{
					inventorySlot.disableForGive();
					continue;
				}
				bool flag = false;
				for (int j = 0; j < requiredItemsForDeed.Length; j++)
				{
					if (inventorySlot.itemNo == requiredItemsForDeed[j])
					{
						if (itemsAlreadyGivenForDeed[j] >= requiredAmountForDeed[j])
						{
							inventorySlot.disableForGive();
						}
						flag = true;
					}
				}
				if (!flag)
				{
					inventorySlot.disableForGive();
				}
			}
			return;
		}
		if (giveMenuTypeOpen == currentlyGivingTo.Tech)
		{
			invSlots = Inventory.Instance.invSlots;
			foreach (InventorySlot inventorySlot2 in invSlots)
			{
				if (inventorySlot2.itemNo == -1 || inventorySlot2.itemNo != CraftsmanManager.manage.shinyDiscItem.getItemId())
				{
					inventorySlot2.disableForGive();
				}
			}
			return;
		}
		if (giveMenuTypeOpen == currentlyGivingTo.Museum)
		{
			invSlots = Inventory.Instance.invSlots;
			foreach (InventorySlot inventorySlot3 in invSlots)
			{
				if (inventorySlot3.itemNo == -1 || inventorySlot3.itemInSlot.isDeed)
				{
					inventorySlot3.disableForGive();
				}
				else if (!inventorySlot3.itemInSlot.fish && !inventorySlot3.itemInSlot.bug && !inventorySlot3.itemInSlot.underwaterCreature)
				{
					inventorySlot3.disableForGive();
				}
				else if (!MuseumManager.manage.checkIfDonationNeeded(inventorySlot3.itemInSlot))
				{
					inventorySlot3.disableForGive();
				}
			}
			return;
		}
		if (giveMenuTypeOpen == currentlyGivingTo.BoomBoxSwap)
		{
			invSlots = Inventory.Instance.invSlots;
			foreach (InventorySlot inventorySlot4 in invSlots)
			{
				if (inventorySlot4.itemNo != -1 && !inventorySlot4.itemInSlot.GetComponent<MusicCassette>())
				{
					inventorySlot4.disableForGive();
				}
			}
			return;
		}
		if (giveMenuTypeOpen == currentlyGivingTo.Sell || giveMenuTypeOpen == currentlyGivingTo.Swapping || giveMenuTypeOpen == currentlyGivingTo.SellToTrapper || giveMenuTypeOpen == currentlyGivingTo.SellToJimmy || giveMenuTypeOpen == currentlyGivingTo.SellToTuckshop || giveMenuTypeOpen == currentlyGivingTo.SellToBugComp || giveMenuTypeOpen == currentlyGivingTo.SellToFishingComp)
		{
			Vector3 position = NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position;
			bool flag2 = WorldManager.Instance.checkIfFishCanBeDropped(position);
			invSlots = Inventory.Instance.invSlots;
			foreach (InventorySlot inventorySlot5 in invSlots)
			{
				if (inventorySlot5.itemNo == -1 || inventorySlot5.itemInSlot.isDeed || inventorySlot5.itemInSlot.getItemId() == Inventory.Instance.moneyItem.getItemId())
				{
					inventorySlot5.disableForGive();
				}
				if (giveMenuTypeOpen == currentlyGivingTo.Swapping && inventorySlot5.itemNo != -1 && (bool)inventorySlot5.itemInSlot.fish && !flag2)
				{
					inventorySlot5.disableForGive();
				}
				if (giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
				{
					if (inventorySlot5.itemNo == -1 || inventorySlot5.stack < 50 || Inventory.Instance.allItems[inventorySlot5.itemNo].hasFuel || Inventory.Instance.allItems[inventorySlot5.itemNo].hasColourVariation)
					{
						inventorySlot5.disableForGive();
					}
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToTrapper)
				{
					if (inventorySlot5.itemNo != -1 && (bool)inventorySlot5.itemInSlot.consumeable)
					{
						if (!inventorySlot5.itemInSlot.consumeable.isMeat)
						{
							inventorySlot5.disableForGive();
						}
					}
					else if (inventorySlot5.itemNo != -1 && (bool)inventorySlot5.itemInSlot.itemChange && (bool)inventorySlot5.itemInSlot.itemChange.changesAndTheirChanger[0].changesWhenComplete && (bool)inventorySlot5.itemInSlot.itemChange.changesAndTheirChanger[0].changesWhenComplete.consumeable)
					{
						if (!inventorySlot5.itemInSlot.itemChange.changesAndTheirChanger[0].changesWhenComplete.consumeable.isMeat)
						{
							inventorySlot5.disableForGive();
						}
					}
					else if (inventorySlot5.itemNo != -1 && !inventorySlot5.itemInSlot.fish && !inventorySlot5.itemInSlot.bug && !inventorySlot5.itemInSlot.underwaterCreature)
					{
						inventorySlot5.disableForGive();
					}
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToTuckshop)
				{
					if (inventorySlot5.itemNo != TuckshopManager.manage.specialItemId)
					{
						inventorySlot5.disableForGive();
					}
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToBugComp)
				{
					if (inventorySlot5.itemNo == -1 || !inventorySlot5.itemInSlot.bug)
					{
						inventorySlot5.disableForGive();
					}
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToFishingComp && (inventorySlot5.itemNo == -1 || !inventorySlot5.itemInSlot.fish))
				{
					inventorySlot5.disableForGive();
				}
			}
			return;
		}
		int nPCNo = ConversationManager.manage.lastConversationTarget.GetComponent<NPCIdentity>().NPCNo;
		invSlots = Inventory.Instance.invSlots;
		foreach (InventorySlot inventorySlot6 in invSlots)
		{
			if (inventorySlot6.itemNo == -1 || inventorySlot6.itemInSlot.isDeed)
			{
				inventorySlot6.disableForGive();
			}
			else if (givingPost != null)
			{
				if (givingPost.isTrade)
				{
					if (!givingPost.checkIfTradeItemIsOk(inventorySlot6.itemNo))
					{
						inventorySlot6.disableForGive();
					}
				}
				else if (givingPost.requiredItem != inventorySlot6.itemNo)
				{
					inventorySlot6.disableForGive();
				}
			}
			else if (!NPCManager.manage.NPCRequests[nPCNo].checkIfItemMatchesRequest(inventorySlot6.itemNo))
			{
				inventorySlot6.disableForGive();
			}
		}
	}

	public void setSellingByWeight(SellByWeight selling)
	{
		sellingByWeight = selling;
	}

	public string getSellByWeightName()
	{
		if ((bool)sellingByWeight)
		{
			return sellingByWeight.GetName();
		}
		return "Thing";
	}

	public string getItemWeight()
	{
		if ((bool)sellingByWeight)
		{
			return sellingByWeight.getMyWeight().ToString("0.00") + "Kg";
		}
		return "1";
	}

	public int getSellByWeightMoneyValue()
	{
		return sellingByWeight.getPrice();
	}

	public void sellSellingByWeight()
	{
		CharLevelManager.manage.todaysMoneyTotal += getSellByWeightMoneyValue();
		Inventory.Instance.changeWallet(getSellByWeightMoneyValue());
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellItems, getSellByWeightMoneyValue());
		if (sellingByWeight.taskWhenSold != 0)
		{
			DailyTaskGenerator.generate.doATask(sellingByWeight.taskWhenSold);
		}
		NetworkMapSharer.Instance.localChar.CmdSellByWeight(sellingByWeight.netId);
		sellingByWeight = null;
	}

	public int getRaffleTicketAmount()
	{
		return raffleTicketAmount;
	}
}
