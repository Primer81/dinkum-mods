using System.Collections;
using System.Text;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationManager : MonoBehaviour
{
	public static ConversationManager manage;

	public MuseumConvoGroup museumConvos;

	public HouseConversationGroup houseConvos;

	public TownConversation townConvos;

	public ConversationObject signMissingText;

	public ConversationObject[] randomFollowStops;

	public ConversationObject atWorkChatConversations;

	public ConversationObject playerFillTileObjectConversation;

	[SerializeField]
	private GameObject optionsWindow;

	[SerializeField]
	private Transform conversationWindow;

	[SerializeField]
	private Transform OptionWindow;

	[SerializeField]
	private VerticalLayoutGroup optionLayout;

	[SerializeField]
	private HeartContainer[] relationHearts;

	[SerializeField]
	private Image nameBox;

	[SerializeField]
	private Image backBox;

	[SerializeField]
	private TextMeshProUGUI npcNameText;

	[SerializeField]
	private TextMeshProUGUI conversationText;

	[SerializeField]
	private GameObject OptionButton;

	[SerializeField]
	private GameObject nextArrowBounce;

	[SerializeField]
	private Image arrowBounceColour;

	public ASound nextTextSound;

	public ASound skipTextSound;

	public ASound showOptionsSound;

	[SerializeField]
	private InventoryItem blobFish;

	public string playerName;

	public bool ready;

	public NPCAI lastConversationTarget;

	public bool inOptionScreen;

	[HideInInspector]
	public ConstructionBoxInput donatingToBuilding;

	private ConversationObject customConversation;

	private ConversationObject conversationToReadThrough;

	private OptionWindow[] optionButtons;

	private readonly WaitForSeconds waitForNext = new WaitForSeconds(0.15f);

	private readonly WaitForSeconds waitForNextSpeedUp = new WaitForSeconds(0.006f);

	private string teledir = "null";

	private float customStartConversationDelay;

	private float speedWait;

	private bool addDefaultStartConversationDelay;

	private bool speedUpConversation;

	private bool optionButtonHasBeenClicked;

	private bool showOptionAmountWindow;

	private int wantsToShowEmotion;

	private int currentlyShowingEmotion;

	private int talkingAboutPhotoId = -1;

	private int petVariationNo;

	private int selectedOption;

	public bool IsConversationActive { get; private set; }

	private void Awake()
	{
		manage = this;
	}

	public void TalkToNPC(NPCAI targetNPC, ConversationObject newCustomConversation = null, bool hasStartDelay = false, bool forceUseCustom = false)
	{
		if (!IsConversationActive)
		{
			if (targetNPC.IsValidConversationTargetForAnyPlayer())
			{
				StartConversationWithAvailableNPC(targetNPC, newCustomConversation, hasStartDelay, forceUseCustom);
			}
			else if (IsNPCFollowingLocalPlayer(targetNPC))
			{
				StartConversationWithNPCFollowingLocalPlayer(targetNPC, hasStartDelay);
			}
		}
	}

	private void StartConversationWithAvailableNPC(NPCAI targetNPC, ConversationObject newCustomConvo = null, bool hasStartDelay = false, bool forceUseCustom = false)
	{
		addDefaultStartConversationDelay = hasStartDelay;
		lastConversationTarget = targetNPC;
		int nPCNo = targetNPC.GetComponent<NPCIdentity>().NPCNo;
		Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
		NetworkMapSharer.Instance.localChar.CmdChangeTalkTo(targetNPC.netId, isTalking: true);
		if ((bool)newCustomConvo && forceUseCustom)
		{
			customConversation = newCustomConvo;
		}
		else if (nPCNo != -1 && NPCManager.manage.shouldAskToMoveIn(nPCNo))
		{
			NotificationManager.manage.makeTopNotification(ConversationGenerator.generate.GetNotificationText("NewDeedAvailable"), ConversationGenerator.generate.GetNotificationText("TalkToFletchForDeeds"), SoundManager.Instance.notificationSound);
			customConversation = NPCManager.manage.NPCDetails[nPCNo].moveInRequestConvos;
			DeedManager.manage.unlockDeed(NPCManager.manage.NPCDetails[nPCNo].deedOnMoveRequest);
		}
		else if (nPCNo == -1 || NPCManager.manage.npcStatus[nPCNo].hasMet)
		{
			customConversation = QuestManager.manage.checkIfThereIsAQuestToGive(nPCNo);
			if (customConversation == null && (bool)newCustomConvo)
			{
				customConversation = newCustomConvo;
			}
		}
		else
		{
			customConversation = NPCManager.manage.NPCDetails[nPCNo].introductionConvos;
			NPCManager.manage.npcStatus[nPCNo].hasMet = true;
		}
		if (nPCNo == -1)
		{
			npcNameText.SetText(string.Empty);
			HideRelationshipHearts();
			nameBox.color = Color.white;
			backBox.color = Color.grey;
			arrowBounceColour.color = Color.grey;
			nameBox.enabled = false;
		}
		else
		{
			FillNameBoxDetails(targetNPC, nPCNo);
		}
		StartCoroutine(ReadThroughConversationContent());
	}

	private void StartConversationWithNPCFollowingLocalPlayer(NPCAI targetNPC, bool hasStartDelay = false)
	{
		int nPCNo = targetNPC.GetComponent<NPCIdentity>().NPCNo;
		addDefaultStartConversationDelay = hasStartDelay;
		NetworkMapSharer.Instance.localChar.CmdChangeTalkTo(targetNPC.netId, isTalking: true);
		lastConversationTarget = targetNPC;
		customConversation = QuestManager.manage.checkIfThereIsAQuestToGive(nPCNo);
		if (customConversation == null)
		{
			customConversation = randomFollowStops[Random.Range(0, randomFollowStops.Length)];
		}
		FillNameBoxDetails(targetNPC, nPCNo);
		StartCoroutine(ReadThroughConversationContent());
	}

	public void FillNameBoxDetails(NPCAI targetNPC, int npcIndex)
	{
		nameBox.enabled = true;
		npcNameText.SetText(NPCManager.manage.NPCDetails[targetNPC.GetComponent<NPCIdentity>().NPCNo].GetNPCName());
		if (npcNameText.text.Length <= 4)
		{
			npcNameText.alignment = TextAlignmentOptions.Center;
		}
		else
		{
			npcNameText.alignment = TextAlignmentOptions.Left;
		}
		nameBox.color = NPCManager.manage.NPCDetails[targetNPC.GetComponent<NPCIdentity>().NPCNo].npcColor;
		backBox.color = NPCManager.manage.NPCDetails[targetNPC.GetComponent<NPCIdentity>().NPCNo].npcColor;
		arrowBounceColour.color = NPCManager.manage.NPCDetails[targetNPC.GetComponent<NPCIdentity>().NPCNo].npcColor;
		ShowRelationshipHearts(npcIndex);
	}

	private void Update()
	{
		if (IsConversationActive && !TownManager.manage.firstConnect)
		{
			if (nameBox.gameObject.activeInHierarchy)
			{
				HandleSpeedingUpConversation();
				return;
			}
			speedUpConversation = false;
			speedWait = 0f;
		}
	}

	private void HandleSpeedingUpConversation()
	{
		bool flag = InputMaster.input.UISelectHeld();
		if (!flag)
		{
			flag = InputMaster.input.UICancelHeld();
		}
		if (flag)
		{
			if (speedWait <= 0.5f)
			{
				speedWait += Time.deltaTime;
				speedUpConversation = false;
			}
			else
			{
				speedUpConversation = true;
			}
		}
		else
		{
			speedUpConversation = false;
			speedWait = 0f;
		}
	}

	private IEnumerator SelectPlayerOptionByKeys()
	{
		int currentlySelected = 0;
		if (!Inventory.Instance.usingMouse)
		{
			while (optionButtons == null)
			{
				yield return null;
			}
			yield return null;
			Inventory.Instance.cursor.transform.position = optionButtons[0].transform.position + new Vector3(optionButtons[0].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
		}
		while (inOptionScreen)
		{
			float vertical = 0f - InputMaster.input.UINavigation().y;
			if (InputMaster.input.UICancel() && (!conversationToReadThrough.HasOptionsSequence || conversationToReadThrough.targetResponses[optionButtons.Length - 1].branchToConversation == null))
			{
				currentlySelected = optionButtons.Length - 1;
				selectWithButton(currentlySelected);
				Inventory.Instance.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
				yield return null;
				yield return null;
				yield return null;
				yield return null;
				buttonClick();
			}
			if (vertical >= 0.45f)
			{
				if (GiveNPC.give.optionWindowOpen && currentlySelected <= -1)
				{
					currentlySelected = Mathf.Clamp(currentlySelected + 1, -2, 0);
					SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
					switch (currentlySelected)
					{
					case -1:
						Inventory.Instance.cursor.transform.position = GiveNPC.give.optionAmountWindow.downButton.position;
						break;
					case -2:
						Inventory.Instance.cursor.transform.position = GiveNPC.give.optionAmountWindow.upButton.position;
						break;
					case 0:
						selectWithButton(currentlySelected);
						Inventory.Instance.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
						break;
					}
				}
				else
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
					currentlySelected = Mathf.Clamp(currentlySelected + 1, 0, optionButtons.Length - 1);
					selectWithButton(currentlySelected);
					Inventory.Instance.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
				}
				yield return new WaitForSeconds(0.15f);
			}
			else if (vertical <= -0.45f)
			{
				if (GiveNPC.give.optionWindowOpen && currentlySelected <= 0)
				{
					currentlySelected = Mathf.Clamp(currentlySelected - 1, -2, 0);
					SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
					switch (currentlySelected)
					{
					case -1:
						Inventory.Instance.cursor.transform.position = GiveNPC.give.optionAmountWindow.downButton.position;
						break;
					case -2:
						Inventory.Instance.cursor.transform.position = GiveNPC.give.optionAmountWindow.upButton.position;
						break;
					case 0:
						selectWithButton(currentlySelected);
						Inventory.Instance.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
						break;
					}
				}
				else
				{
					SoundManager.Instance.play2DSound(SoundManager.Instance.inventorySound);
					currentlySelected = Mathf.Clamp(currentlySelected - 1, 0, optionButtons.Length - 1);
					selectWithButton(currentlySelected);
					Inventory.Instance.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
				}
				yield return new WaitForSeconds(0.15f);
			}
			yield return null;
		}
	}

	public void selectWithButton(int selectionInt)
	{
		selectedOption = selectionInt;
	}

	public void buttonClick()
	{
		optionButtonHasBeenClicked = true;
	}

	private void tryAndTalk(char thisCharacter)
	{
		if ((bool)lastConversationTarget && !lastConversationTarget.isSign && !lastConversationTarget.myAudio.isPlaying)
		{
			lastConversationTarget.myAudio.pitch = NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].NPCVoice.pitchLow + getLetterPitchDif(thisCharacter);
			lastConversationTarget.myAudio.volume = NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].NPCVoice.volume * SoundManager.Instance.GetGlobalSoundVolume();
			lastConversationTarget.myAudio.PlayOneShot(NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].NPCVoice.myClips[Random.Range(0, NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].NPCVoice.myClips.Length)]);
		}
	}

	public float getLetterPitchDif(char inputLetter)
	{
		char c = 'a';
		inputLetter = char.ToLower(inputLetter);
		for (float num = 0f; num <= 0.1f; num += 0.0038f)
		{
			if (c == inputLetter)
			{
				return num;
			}
		}
		return 0.1003f;
	}

	public void SetStartTalkDelay(float delay)
	{
		customStartConversationDelay = delay;
	}

	public void CheckIfLocalPlayerWasTalkingToNPCAndSetNetworkStopTalkingAfterConversationEnds()
	{
		if ((bool)lastConversationTarget && lastConversationTarget.talkingTo == NetworkMapSharer.Instance.localChar.netId)
		{
			NetworkMapSharer.Instance.localChar.CmdChangeTalkTo(lastConversationTarget.netId, isTalking: false);
		}
	}

	private IEnumerator ReadThroughConversationContent()
	{
		IsConversationActive = true;
		Inventory.Instance.quickSlotBar.gameObject.SetActive(value: false);
		MenuButtonsTop.menu.closed = false;
		Inventory.Instance.checkIfWindowIsNeeded();
		optionsWindow.SetActive(value: false);
		conversationText.SetText(string.Empty);
		nextArrowBounce.SetActive(value: false);
		if (addDefaultStartConversationDelay)
		{
			addDefaultStartConversationDelay = false;
			yield return new WaitForSeconds(0.3f);
		}
		if (customStartConversationDelay != 0f)
		{
			yield return new WaitForSeconds(customStartConversationDelay);
			customStartConversationDelay = 0f;
		}
		conversationWindow.gameObject.SetActive(value: true);
		while (!nameBox.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.25f);
		SetNewConversation();
		if (conversationToReadThrough == null)
		{
			yield break;
		}
		yield return StartCoroutine(ReadConversationSegment());
		ExecuteSpecialActionWithoutDelay();
		yield return waitForNext;
		if (!conversationToReadThrough.HasOptionsSequence && GetWillExitConversationAfterThisSegment())
		{
			CheckIfLocalPlayerWasTalkingToNPCAndSetNetworkStopTalkingAfterConversationEnds();
		}
		ExecuteSpecialFunctionWithDelay();
		if (conversationToReadThrough.HasOptionsSequence)
		{
			nextArrowBounce.SetActive(value: false);
			if (showOptionAmountWindow)
			{
				GiveNPC.give.openOptionAmountWindow();
				showOptionAmountWindow = false;
			}
			else
			{
				GiveNPC.give.closeOptionAmountWindow();
			}
			CreatePlayerOptions();
			yield return null;
			yield return StartCoroutine(WaitForPlayerToSelectOption());
			yield return waitForNext;
			inOptionScreen = false;
			yield return StartCoroutine(ReadConversationSegment(selectedOption));
			ExecuteSpecialActionWithoutDelay(selectedOption);
			nextArrowBounce.SetActive(value: false);
			if (GetWillExitConversationAfterThisSegment())
			{
				CheckIfLocalPlayerWasTalkingToNPCAndSetNetworkStopTalkingAfterConversationEnds();
			}
			ExecuteSpecialFunctionWithDelay(selectedOption);
		}
		if (!lastConversationTarget.isSign)
		{
			lastConversationTarget.faceAnim.stopEmotions();
		}
		Inventory.Instance.equipNewSelectedSlot();
		MenuButtonsTop.menu.closeButtonDelay();
		IsConversationActive = false;
		if (conversationToReadThrough.HasOptionsSequence && conversationToReadThrough.targetResponses[selectedOption].branchToConversation != null)
		{
			TalkToNPC(lastConversationTarget, conversationToReadThrough.targetResponses[selectedOption].branchToConversation, hasStartDelay: false, forceUseCustom: true);
			yield break;
		}
		ExecuteSpecialActionAfterConversationEnded(selectedOption);
		if (!IsConversationActive)
		{
			conversationWindow.gameObject.SetActive(value: false);
			Inventory.Instance.checkIfWindowIsNeeded();
		}
	}

	private void SetNewConversation()
	{
		if ((bool)customConversation)
		{
			conversationToReadThrough = customConversation;
			customConversation = null;
		}
		else if (lastConversationTarget.isSign)
		{
			conversationToReadThrough = signMissingText;
		}
		else if (NetworkMapSharer.Instance.localChar.myEquip.disguiseId != -1)
		{
			conversationToReadThrough = ConversationGenerator.generate.GetRandomCommentOnDisguise();
		}
		else if (StatusManager.manage.connectedDamge.onFire)
		{
			conversationToReadThrough = ConversationGenerator.generate.GetRandomCommentOnPlayerBeingOnFire();
		}
		else
		{
			conversationToReadThrough = ConversationGenerator.generate.GetRandomGreeting(lastConversationTarget.myId.NPCNo);
		}
	}

	private void CreatePlayerOptions()
	{
		optionButtons = new OptionWindow[conversationToReadThrough.playerOptions.Length];
		for (int i = 0; i < conversationToReadThrough.playerOptions.Length; i++)
		{
			OptionWindow component = Object.Instantiate(OptionButton, OptionWindow).GetComponent<OptionWindow>();
			component.setOptionText(buttonTextChange(conversationToReadThrough, i));
			component.GetComponent<InvButton>().isConverstationOption = true;
			component.GetComponent<InvButton>().craftRecipeNumber = i;
			optionButtons[i] = component;
		}
		for (int j = 0; j < conversationToReadThrough.playerOptions.Length; j++)
		{
			optionButtons[j].showOptionBox();
		}
		optionsWindow.SetActive(value: true);
		SoundManager.Instance.play2DSound(showOptionsSound);
		optionLayout.enabled = false;
		optionLayout.enabled = true;
	}

	private IEnumerator WaitForPlayerToSelectOption()
	{
		bool optionNotSelected = true;
		ready = false;
		selectedOption = 0;
		inOptionScreen = true;
		StartCoroutine(SelectPlayerOptionByKeys());
		while (optionNotSelected)
		{
			for (int i = 0; i < conversationToReadThrough.playerOptions.Length; i++)
			{
				if (selectedOption == i)
				{
					optionButtons[i].selectedOrNot(selected: true);
				}
				else
				{
					optionButtons[i].selectedOrNot(selected: false);
				}
			}
			if (optionButtonHasBeenClicked)
			{
				optionButtonHasBeenClicked = false;
				optionNotSelected = false;
				GiveNPC.give.closeOptionAmountWindow();
				OptionWindow[] array = optionButtons;
				for (int j = 0; j < array.Length; j++)
				{
					Object.Destroy(array[j].gameObject);
				}
			}
			yield return null;
		}
		optionsWindow.SetActive(value: false);
	}

	private void ExecuteSpecialActionWithoutDelay(int responseNo = -1)
	{
		switch (conversationToReadThrough.GetSpecialAction(responseNo))
		{
		case CONVERSATION_SPECIAL_ACTION.RepairItems:
			CraftingManager.manage.repairItemsInPockets();
			break;
		case CONVERSATION_SPECIAL_ACTION.ConfirmSleep:
			NetworkMapSharer.Instance.localChar.myPickUp.confirmSleep();
			break;
		case CONVERSATION_SPECIAL_ACTION.ConfirmMineControls:
			NetworkMapSharer.Instance.localChar.myPickUp.ConfirmUseMineElevator();
			break;
		case CONVERSATION_SPECIAL_ACTION.ConfirmFlyOffIsland:
			NetworkMapSharer.Instance.localChar.myPickUp.ConfirmUseAirshipTakeOff();
			break;
		case CONVERSATION_SPECIAL_ACTION.FillTileObject:
			GiveNPC.give.optionAmountWindow.ConfirmFillTileObject();
			break;
		case CONVERSATION_SPECIAL_ACTION.PlaceDeed:
			NetworkMapSharer.Instance.localChar.myInteract.doDamage();
			break;
		case CONVERSATION_SPECIAL_ACTION.ChargeForHouseUpgrade:
			TownManager.manage.payForUpgradeAndSetBuildForTomorrow();
			break;
		case CONVERSATION_SPECIAL_ACTION.SignUpForComp:
			CatchingCompetitionManager.manage.signUpForComp();
			break;
		case CONVERSATION_SPECIAL_ACTION.DonateToMuseum:
			GiveNPC.give.donateItemToMuseum();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenAnimalMenuForPetDog:
			petVariationNo = conversationToReadThrough.targetResponses[responseNo].talkingAboutAnimal.animalId;
			break;
		case CONVERSATION_SPECIAL_ACTION.ShowWeatherForecast:
			BookWindow.book.OpenWeatherForecast();
			break;
		case CONVERSATION_SPECIAL_ACTION.SellItemByWeight:
			GiveNPC.give.sellSellingByWeight();
			break;
		case CONVERSATION_SPECIAL_ACTION.SetMineLevelToJungle:
			Inventory.Instance.removeAmountOfItem(MineEnterExit.mineEntrance.rubyShard.getItemId(), 4);
			NetworkMapSharer.Instance.localChar.CmdSetMinelayer(1);
			break;
		case CONVERSATION_SPECIAL_ACTION.SetMineLevelToLava:
			Inventory.Instance.removeAmountOfItem(MineEnterExit.mineEntrance.emeraldShard.getItemId(), 4);
			NetworkMapSharer.Instance.localChar.CmdSetMinelayer(2);
			break;
		case CONVERSATION_SPECIAL_ACTION.CallTower:
			RenderMap.Instance.canTele = true;
			MenuButtonsTop.menu.switchToMap();
			break;
		case CONVERSATION_SPECIAL_ACTION.SendSignal:
			Inventory.Instance.useItemWithFuel();
			NetworkMapSharer.Instance.localChar.CmdSendTeleSignal();
			break;
		case CONVERSATION_SPECIAL_ACTION.CallSignal:
			if (TeleSignal.currentSignal != null)
			{
				Inventory.Instance.useItemWithFuel();
				NetworkMapSharer.Instance.localChar.CmdTeleportToSignal();
			}
			break;
		case CONVERSATION_SPECIAL_ACTION.RingTownBell:
			NetworkMapSharer.Instance.localChar.CmdRingTownBell();
			break;
		case CONVERSATION_SPECIAL_ACTION.GiveSnag:
			GiftedItemWindow.gifted.addToListToBeGiven(TownEventManager.manage.snag.getItemId(), 1);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
			TownEventManager.manage.snagAvaliable = false;
			break;
		case CONVERSATION_SPECIAL_ACTION.GiveKiteKit:
			GiftedItemWindow.gifted.addToListToBeGiven(TownEventManager.manage.kiteKit.getItemId(), 1);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
			TownEventManager.manage.snagAvaliable = false;
			break;
		case CONVERSATION_SPECIAL_ACTION.DeclineRequest:
			NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].failRequest(lastConversationTarget.GetComponent<NPCIdentity>().NPCNo);
			break;
		case CONVERSATION_SPECIAL_ACTION.DropSwapItem:
			NetworkMapSharer.Instance.localChar.CmdDropItem(Inventory.Instance.getInvItemId(BugAndFishCelebration.bugAndFishCel.inventoryFullConvo.targetOpenings.talkingAboutItem), 1, NetworkMapSharer.Instance.localChar.transform.position, NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position);
			break;
		case CONVERSATION_SPECIAL_ACTION.GiveTalkedAboutItem:
			if (!Inventory.Instance.addItemToInventory(Inventory.Instance.getInvItemId(conversationToReadThrough.targetOpenings.talkingAboutItem), 1))
			{
				NotificationManager.manage.turnOnPocketsFullNotification();
			}
			break;
		case CONVERSATION_SPECIAL_ACTION.RewardForBulletinBoard:
		{
			if (conversationToReadThrough.GetSpecialAction(responseNo) != CONVERSATION_SPECIAL_ACTION.RewardForBulletinBoard)
			{
				break;
			}
			int nPCNo = lastConversationTarget.GetComponent<NPCIdentity>().NPCNo;
			PostOnBoard givingPost = GiveNPC.give.givingPost;
			if (givingPost == null)
			{
				break;
			}
			if (givingPost.rewardId == Inventory.Instance.moneyItem.getItemId())
			{
				GiftedItemWindow.gifted.addToListToBeGiven(givingPost.rewardId, givingPost.rewardAmount);
				GiftedItemWindow.gifted.openWindowAndGiveItems();
				NetworkMapSharer.Instance.localChar.CmdCompleteBulletinBoardPost(BulletinBoard.board.attachedPosts.IndexOf(givingPost));
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CompleteABulletinBoardRequest);
				PermitPointsManager.manage.addPoints(100);
				GiveNPC.give.completeRequest();
				NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(Random.Range(4, 7));
				QuestTracker.track.updateTasksEvent.Invoke();
				if (givingPost.isPhotoTask)
				{
					PhotoManager.manage.letNPCKeepPhoto();
				}
			}
			else
			{
				if (givingPost.isTrade)
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
					if ((bool)inventorySlot)
					{
						inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, inventorySlot.stack - 1);
					}
					GiveNPC.give.clearAllSelectedSlots();
				}
				GiftedItemWindow.gifted.addToListToBeGiven(givingPost.rewardId, givingPost.rewardAmount);
				GiftedItemWindow.gifted.openWindowAndGiveItems();
				NetworkMapSharer.Instance.localChar.CmdCompleteBulletinBoardPost(BulletinBoard.board.attachedPosts.IndexOf(givingPost));
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CompleteABulletinBoardRequest);
				PermitPointsManager.manage.addPoints(100);
				NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(Random.Range(4, 7));
				GiveNPC.give.completeRequest();
				if (givingPost.isPhotoTask)
				{
					PhotoManager.manage.letNPCKeepPhoto();
				}
			}
			GiveNPC.give.givingPost = null;
			break;
		}
		case CONVERSATION_SPECIAL_ACTION.DeclineNpcRequestAndRemoveRequest:
			NPCManager.manage.NPCRequests[lastConversationTarget.myId.NPCNo].completeRequest(lastConversationTarget.myId.NPCNo);
			break;
		case CONVERSATION_SPECIAL_ACTION.StartTeleport:
			if (teledir != "null")
			{
				NetworkMapSharer.Instance.localChar.CmdTeleport(teledir);
			}
			teledir = "null";
			break;
		case CONVERSATION_SPECIAL_ACTION.MakePeacefulWish:
			NetworkMapSharer.Instance.localChar.TryAndMakeWish(1);
			break;
		case CONVERSATION_SPECIAL_ACTION.MakePowerfulWish:
			NetworkMapSharer.Instance.localChar.TryAndMakeWish(2);
			break;
		case CONVERSATION_SPECIAL_ACTION.MakeBountifulWish:
			NetworkMapSharer.Instance.localChar.TryAndMakeWish(3);
			break;
		case CONVERSATION_SPECIAL_ACTION.MakeRegretfulWish:
			NetworkMapSharer.Instance.localChar.TryAndMakeWish(0);
			break;
		case CONVERSATION_SPECIAL_ACTION.MakeFortuitusWish:
			NetworkMapSharer.Instance.localChar.TryAndMakeWish(5);
			break;
		case CONVERSATION_SPECIAL_ACTION.MakeSpotlessWish:
			NetworkMapSharer.Instance.localChar.TryAndMakeWish(4);
			break;
		}
	}

	private void ExecuteSpecialFunctionWithDelay(int respondeIndex = -1)
	{
		switch (conversationToReadThrough.GetSpecialAction(respondeIndex))
		{
		case CONVERSATION_SPECIAL_ACTION.CompleteNpcRequest:
			CompleteRequest();
			break;
		case CONVERSATION_SPECIAL_ACTION.AcceptQuest:
			QuestManager.manage.acceptQuestLastTalkedAbout();
			break;
		case CONVERSATION_SPECIAL_ACTION.AcceptRequest:
			NPCManager.manage.NPCRequests[lastConversationTarget.myId.NPCNo].acceptRequest(lastConversationTarget.myId.NPCNo);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenAnimalMenu:
			FarmAnimalMenu.menu.openAnimalMenu(GiveNPC.give.dropToBuy.sellsAnimal.animalId);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenAnimalMenuForPetDog:
			FarmAnimalMenu.menu.openAnimalMenu(19, petVariationNo);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenMirror:
			HairDresserMenu.menu.openMirror();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenBankMenu:
			BankMenu.menu.open();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenAtmMenu:
			BankMenu.menu.OpenAsATM();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenHouseEditor:
			HouseEditor.edit.openWindow();
			break;
		case CONVERSATION_SPECIAL_ACTION.ConfirmUpgradeGuestHouse:
			HouseEditor.edit.TakePaymentAndSetBuildingToUpgrade();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenLicenceWindow:
			LicenceManager.manage.openLicenceWindow();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenTownManager:
			TownManager.manage.openTownManager(TownManager.windowType.Awards);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenDonateWindow:
			BankMenu.menu.openAsDonations();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenGiveMenu:
			GiveNPC.give.OpenGiveWindow();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenGiveMenuMuseum:
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Museum);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenGiveForSwap:
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Swapping);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenGiveForBoombox:
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.BoomBoxSwap);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenTechDonateWindow:
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Tech);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenConstructionBox:
			donatingToBuilding.openForGivingMenus();
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenGiveForBulletinBoard:
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.BulletinBoard);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenCraftsManMenu:
			CraftingManager.manage.openCloseCraftMenu(isMenuOpen: true, CraftingManager.CraftingMenuType.CraftingShop);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenTrapperCraftMenu:
			CraftingManager.manage.openCloseCraftMenu(isMenuOpen: true, CraftingManager.CraftingMenuType.TrapperShop);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenAgentCraftMenu:
			CraftingManager.manage.openCloseCraftMenu(isMenuOpen: true, CraftingManager.CraftingMenuType.AgentCrafting);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenNickCraftMenu:
			CraftingManager.manage.openCloseCraftMenu(isMenuOpen: true, CraftingManager.CraftingMenuType.NickShop);
			break;
		case CONVERSATION_SPECIAL_ACTION.GivePhotoNpc:
			PhotoManager.manage.openPhotoTab(showingNPC: true);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenDeedMenu:
			DeedManager.manage.openDeedWindow();
			break;
		case CONVERSATION_SPECIAL_ACTION.SellGivenItems:
			GiveNPC.give.sellItemsAndEmptyGiveSlots();
			break;
		case CONVERSATION_SPECIAL_ACTION.ReturnItemsInGiveMenu:
			GiveNPC.give.returnItemsAndEmptyGiveSlots();
			break;
		case CONVERSATION_SPECIAL_ACTION.CompleteRequestAndGiveReward:
			if (!NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].IsTodayMyBirthday())
			{
				findReward();
			}
			CompleteRequest();
			break;
		case CONVERSATION_SPECIAL_ACTION.RewardForRequest:
			if (!NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].IsTodayMyBirthday())
			{
				findReward();
			}
			break;
		case CONVERSATION_SPECIAL_ACTION.AcceptAndCompleteQuest:
			QuestManager.manage.acceptQuestLastTalkedAbout();
			QuestManager.manage.completeQuestLastTalkedAbout();
			break;
		case CONVERSATION_SPECIAL_ACTION.StartFollowing:
			NetworkMapSharer.Instance.localChar.CmdNPCStartFollow(lastConversationTarget.netId, NetworkMapSharer.Instance.localChar.netId);
			NetworkMapSharer.Instance.localChar.followedBy = lastConversationTarget.myId.NPCNo;
			if (!NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].hasHungOutToday)
			{
				NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].hasHungOutToday = true;
				NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].addToRelationshipLevel(2);
			}
			break;
		case CONVERSATION_SPECIAL_ACTION.StopFollowing:
			NetworkMapSharer.Instance.localChar.CmdNPCStartFollow(lastConversationTarget.netId, 0u);
			NetworkMapSharer.Instance.localChar.followedBy = -1;
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenGiveMenuToSell:
			if (lastConversationTarget.myId.NPCNo == 5)
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.SellToTrapper);
			}
			else if (lastConversationTarget.myId.NPCNo == 11)
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.SellToJimmy);
			}
			else if (lastConversationTarget.myId.NPCNo == 12)
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.SellToTuckshop);
			}
			else if (lastConversationTarget.myId.NPCNo == 13)
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.SellToBugComp);
			}
			else if (lastConversationTarget.myId.NPCNo == 14)
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.SellToFishingComp);
			}
			else
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Sell);
			}
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenHaircutMenu:
			HairDresserMenu.menu.openHairCutMenu(colorSelector: false);
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenHairCutColorMenu:
			HairDresserMenu.menu.openHairCutMenu(colorSelector: true);
			break;
		case CONVERSATION_SPECIAL_ACTION.GivePhotoMuseum:
			PhotoManager.manage.openPhotoTab(showingNPC: true, talkingAboutPhotoId);
			talkingAboutPhotoId = -1;
			break;
		case CONVERSATION_SPECIAL_ACTION.OpenCatalogue:
			if (lastConversationTarget.myId.NPCNo == 3)
			{
				CatalogueManager.manage.openCatalogue(CatalogueManager.EntryType.Furniture);
			}
			else
			{
				CatalogueManager.manage.openCatalogue(CatalogueManager.EntryType.Clothing);
			}
			break;
		case CONVERSATION_SPECIAL_ACTION.DonateTechItems:
			CraftsmanManager.manage.giveCraftsmanXp();
			GiveNPC.give.donateTechItems();
			break;
		}
		void CompleteRequest()
		{
			int nPCNo = lastConversationTarget.myId.NPCNo;
			NPCManager.manage.NPCRequests[nPCNo].CheckForOtherActionsOnComplete();
			NPCManager.manage.NPCRequests[nPCNo].completeRequest(nPCNo);
			if (NPCManager.manage.NPCDetails[nPCNo].IsTodayMyBirthday())
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.GiveBirthdayGift);
				PermitPointsManager.manage.addPoints(50);
				NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(10);
			}
			else
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DoAFavourSomeone);
				PermitPointsManager.manage.addPoints(50);
				if (NPCManager.manage.npcStatus[nPCNo].relationshipLevel < 45)
				{
					NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(Random.Range(3, 5));
				}
				else
				{
					NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(Random.Range(2, 4));
				}
			}
			MonoBehaviour.print("Completeting birthday: " + NPCManager.manage.NPCRequests[nPCNo].completedToday);
			GiveNPC.give.completeRequest();
		}
	}

	private void ExecuteSpecialActionAfterConversationEnded(int optionIndex)
	{
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.BuyItem, optionIndex))
		{
			GiveNPC.give.tryToBuy();
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.DonateTechItems, optionIndex))
		{
			if (CharLevelManager.manage.isCraftsmanRecipeUnlockedThisLevel())
			{
				TalkToNPC(lastConversationTarget, CraftsmanManager.manage.hasLearnedANewRecipeIconConvos);
			}
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.RequestHouseUpgrade, optionIndex))
		{
			TalkToNPC(lastConversationTarget, houseConvos.GetConversation());
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.AskAboutHouseGeneral, optionIndex))
		{
			TalkToNPC(lastConversationTarget, houseConvos.GetStartingConversation());
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.StartLookingForTechConvo, optionIndex))
		{
			TalkToNPC(lastConversationTarget, CraftsmanManager.manage.lookingForTechConvo);
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenTownConvo, optionIndex))
		{
			TalkToNPC(lastConversationTarget, townConvos.openingConversation);
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.AskAboutDeeds, optionIndex))
		{
			TalkToNPC(lastConversationTarget, townConvos.AskAboutDeeds());
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.ConfirmDeedInConvo, optionIndex))
		{
			DeedManager.manage.confirmDeedInConvo();
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.GiveCompletedTech, optionIndex))
		{
			CraftsmanManager.manage.tryAndGiveCompletedItem();
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.ChatToNPCAtWork, optionIndex))
		{
			TalkToNPC(lastConversationTarget, atWorkChatConversations);
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenFillTileObjectText, optionIndex))
		{
			showOptionAmountWindow = true;
			TalkToNPC(lastConversationTarget, playerFillTileObjectConversation);
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.UpdateCompScore, optionIndex))
		{
			if (CatchingCompetitionManager.manage.competitionActive())
			{
				TalkToNPC(lastConversationTarget, CatchingCompetitionManager.manage.loggedScoreInBookConvo);
				NetworkMapSharer.Instance.localChar.myEquip.CmdSetNewCompScore(NetworkMapSharer.Instance.localChar.myEquip.localCompScore);
			}
			else
			{
				TalkToNPC(lastConversationTarget, CatchingCompetitionManager.manage.loggedScoreTooLateConvo);
			}
			return;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.GetRandomConvo, optionIndex))
		{
			if (!NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].hasBeenTalkedToToday)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TalkToPeople);
			}
			int num = Random.Range(0, NPCManager.manage.getNoOfNPCsMovedIn());
			if (!NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].hasBeenTalkedToToday && NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].CanProvideRandomConversation())
			{
				ConversationObject randomChatConversation = NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].GetRandomChatConversation(lastConversationTarget.myId.NPCNo);
				int num2 = 0;
				while (randomChatConversation.targetOpenings.sequence[0].Contains("Need Text Here") || randomChatConversation.targetOpenings.sequence[0].Contains("Need Text"))
				{
					num2++;
					randomChatConversation = NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].GetRandomChatConversation(lastConversationTarget.myId.NPCNo);
					if (num2 >= 100)
					{
						break;
					}
				}
				TalkToNPC(lastConversationTarget, randomChatConversation);
				NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].hasBeenTalkedToToday = true;
				NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].addToRelationshipLevel(1);
				return;
			}
			if (NetworkMapSharer.Instance.isServer && NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].checkIfHasMovedIn() && !NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].hasGossipedToday && NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].gossipConvos.Length != 0 && num != lastConversationTarget.myId.NPCNo && NPCManager.manage.npcStatus[num].checkIfHasMovedIn())
			{
				TalkToNPC(lastConversationTarget, NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].gossipConvos[num]);
				NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].hasGossipedToday = true;
				if (NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].relationshipLevel <= 25)
				{
					NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].addToRelationshipLevel(1);
				}
				return;
			}
			if (NetworkMapSharer.Instance.isServer && NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].checkIfHasMovedIn() && NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].townMentionConvos.Length != 0 && Random.Range(0, 5) == 3)
			{
				int noOfNPCsMovedIn = NPCManager.manage.getNoOfNPCsMovedIn();
				int num3 = 0;
				if (noOfNPCsMovedIn <= 2)
				{
					num3 = 0;
				}
				else if (noOfNPCsMovedIn <= 4)
				{
					num3 = 1;
				}
				else if (noOfNPCsMovedIn <= 6)
				{
					num3 = 2;
				}
				else if (noOfNPCsMovedIn <= 8)
				{
					num3 = 3;
				}
				TalkToNPC(lastConversationTarget, NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].townMentionConvos[num3]);
				return;
			}
			TalkToNPC(lastConversationTarget, ConversationGenerator.generate.GetRandomComment());
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.AskToHangOut, optionIndex))
		{
			TalkToNPC(lastConversationTarget, ConversationGenerator.generate.GetAskToHangOutConversation(lastConversationTarget.myId.NPCNo));
		}
		else if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.DonateToMuseum, optionIndex))
		{
			TalkToNPC(lastConversationTarget, museumConvos.askIfHasAnotherDonation);
		}
		else if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.AgreeToCraftsman, optionIndex))
		{
			CraftsmanManager.manage.agreeToCrafting();
		}
		else if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.AgreeToMoveBuilding, optionIndex))
		{
			BuildingManager.manage.confirmWantToMoveBuilding();
		}
		else if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenBuildingManager, optionIndex))
		{
			BuildingManager.manage.openWindow();
		}
		else if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.AskToMoveHouse, optionIndex))
		{
			BuildingManager.manage.getWantToMovePlayerHouseConvo();
		}
		else if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.UpgradeGuestHouse, optionIndex))
		{
			TalkToNPC(NPCManager.manage.sign, HouseEditor.edit.ReturnUpgradeText());
		}
		else
		{
			if (!conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.GetRandomJobRequest, optionIndex))
			{
				return;
			}
			PostOnBoard postOnBoard = BulletinBoard.board.checkMissionsCompletedForNPC(lastConversationTarget.myId.NPCNo);
			if (postOnBoard != null)
			{
				TalkToNPC(lastConversationTarget, postOnBoard.getPostPostsById().onCompleteConvo);
				return;
			}
			if (!NPCManager.manage.NPCRequests[lastConversationTarget.myId.NPCNo].generatedToday)
			{
				NPCManager.manage.NPCRequests[lastConversationTarget.myId.NPCNo].GetNewRequest(lastConversationTarget.myId.NPCNo);
			}
			if (NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].IsTodayMyBirthday())
			{
				if (!NPCManager.manage.NPCRequests[lastConversationTarget.myId.NPCNo].completedToday)
				{
					TalkToNPC(lastConversationTarget, ConversationGenerator.generate.GetRandomBirthdayConversation());
				}
				else
				{
					TalkToNPC(lastConversationTarget, ConversationGenerator.generate.GetRandomNoRquestConversation());
				}
			}
			else if (NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].completedRequest)
			{
				TalkToNPC(lastConversationTarget, ConversationGenerator.generate.GetRandomNoRquestConversation());
			}
			else if (!NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].acceptedRequest)
			{
				TalkToNPC(lastConversationTarget, ConversationGenerator.generate.GetRandomRequestConversation(lastConversationTarget.myId.NPCNo));
			}
			else
			{
				TalkToNPC(lastConversationTarget, ConversationGenerator.generate.GetRandomRequestItemAcceptedConversation());
			}
		}
	}

	private string buttonTextChange(ConversationObject convo, int buttonNo)
	{
		string text = convo.playerOptions[buttonNo];
		if (!text.Contains("<"))
		{
			return convo.GetPlayerOptionTranslated(buttonNo);
		}
		switch (text)
		{
		case "<chat>":
			return Random.Range(0, 4) switch
			{
				0 => (LocalizedString)"ToolTips/AskToChat_0", 
				1 => (LocalizedString)"ToolTips/AskToChat_1", 
				2 => (LocalizedString)"ToolTips/AskToChat_2", 
				_ => (LocalizedString)"ToolTips/AskToChat_3", 
			};
		case "<cancel>":
			return Random.Range(0, 4) switch
			{
				0 => (LocalizedString)"ToolTips/CancelConvo_0", 
				1 => (LocalizedString)"ToolTips/CancelConvo_1", 
				2 => (LocalizedString)"ToolTips/CancelConvo_2", 
				_ => (LocalizedString)"ToolTips/CancelConvo_3", 
			};
		case "<hangOut>":
			return Random.Range(0, 2) switch
			{
				0 => (LocalizedString)"ToolTips/AskToHangOut_0", 
				1 => (LocalizedString)"ToolTips/AskToHangOut_1", 
				_ => (LocalizedString)"ToolTips/AskToHangOut_2", 
			};
		case "<jobCondition>":
		{
			PostOnBoard postOnBoard = BulletinBoard.board.checkMissionsCompletedForNPC(lastConversationTarget.GetComponent<NPCIdentity>().NPCNo);
			if (postOnBoard != null)
			{
				GiveNPC.give.givingPost = postOnBoard;
			}
			if (postOnBoard != null)
			{
				if (Random.Range(0, 2) == 0)
				{
					return (LocalizedString)"ToolTips/CompleteBoardRequest_0";
				}
				return (LocalizedString)"ToolTips/CompleteBoardRequest_1";
			}
			if (NPCManager.manage.NPCDetails[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].IsTodayMyBirthday() && (NPCManager.manage.NPCRequests[lastConversationTarget.myId.NPCNo].isMyBirthday() || !NPCManager.manage.NPCRequests[lastConversationTarget.myId.NPCNo].generatedToday))
			{
				return ConversationGenerator.generate.GetToolTip("Tip_HappyBirthday");
			}
			if (NPCManager.manage.npcStatus[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].acceptedRequest)
			{
				if (Random.Range(0, 2) == 0)
				{
					return (LocalizedString)"ToolTips/GiveItem_0";
				}
				return (LocalizedString)"ToolTips/GiveItem_1";
			}
			if (Random.Range(0, 2) == 0)
			{
				return (LocalizedString)"ToolTips/AskForJob_0";
			}
			return (LocalizedString)"ToolTips/AskForJob_1";
		}
		case "<donateButton>":
			return "Donate";
		default:
			return convo.GetPlayerOptionTranslated(buttonNo);
		}
	}

	public void SetTalkingAboutPhotoId(int newId)
	{
		talkingAboutPhotoId = newId;
	}

	private string CheckLineForReplacement(string inString, ConversationObject convo, int response)
	{
		new StringBuilder().Append(inString);
		string text = inString;
		wantsToShowEmotion = 0;
		if (text == null)
		{
			return text;
		}
		if (!text.Contains("<"))
		{
			return text;
		}
		text = text.Replace("<PlayerName>", UIAnimationManager.manage.GetCharacterNameTag(Inventory.Instance.playerName));
		if (text.Contains("<NPCName>"))
		{
			text = ((response == -1) ? text.Replace("<NPCName>", UIAnimationManager.manage.GetCharacterNameTag(convo.targetOpenings.talkingAboutNPC.GetNPCName())) : text.Replace("<NPCName>", UIAnimationManager.manage.GetCharacterNameTag(convo.targetResponses[response].talkingAboutNPC.GetNPCName())));
		}
		if (text.Contains("<getOpeningHours>"))
		{
			text = ((response == -1) ? text.Replace("<getOpeningHours>", convo.targetOpenings.talkingAboutNPC.mySchedual.getOpeningHours()) : text.Replace("<getOpeningHours>", convo.targetResponses[response].talkingAboutNPC.mySchedual.getOpeningHours()));
		}
		if (text.Contains("<getClosedDays>"))
		{
			text = ((response == -1) ? text.Replace("<getClosedDays>", convo.targetOpenings.talkingAboutNPC.mySchedual.getDaysClosed()) : text.Replace("<getClosedDays>", convo.targetResponses[response].talkingAboutNPC.mySchedual.getDaysClosed()));
		}
		if (text.Contains("<getTime>"))
		{
			text = text.Replace("<getTime>", UIAnimationManager.manage.GetCharacterNameTag(RealWorldTimeLight.time.currentHour + ":" + RealWorldTimeLight.time.currentMinute.ToString("00")));
		}
		if (text.Contains("<followedByName>"))
		{
			text = text.Replace("<followedByName>", NPCManager.manage.NPCDetails[NetworkMapSharer.Instance.localChar.followedBy].GetNPCName());
		}
		if (text.Contains("<IslandName>"))
		{
			text = text.Replace("<IslandName>", UIAnimationManager.manage.GetCharacterNameTag(NetworkMapSharer.Instance.islandName));
		}
		if (text.Contains("<SouthCity>"))
		{
			text = text.Replace("<SouthCity>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("SouthCity")));
		}
		if (text.Contains("<favouriteFood>"))
		{
			text = text.Replace("<favouriteFood>", UIAnimationManager.manage.GetItemColorTag(NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].favouriteFood.getInvItemName()));
			text = LocalisationMarkUp.RunMarkupCheck(text, NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].favouriteFood);
		}
		if (text.Contains("<hatedFood>"))
		{
			text = text.Replace("<hatedFood>", UIAnimationManager.manage.GetItemColorTag(NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].hatedFood.getInvItemName()));
			text = LocalisationMarkUp.RunMarkupCheck(text, NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].hatedFood);
		}
		if (text.Contains("<myName>"))
		{
			text = text.Replace("<myName>", UIAnimationManager.manage.GetCharacterNameTag(NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].GetNPCName()));
		}
		if (text.Contains("<FarmingLicence>"))
		{
			text = text.Replace("<FarmingLicence>", UIAnimationManager.manage.GetCharacterNameTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Farming)));
		}
		if (text.Contains("<MiningLicence>"))
		{
			text = text.Replace("<MiningLicence>", UIAnimationManager.manage.GetCharacterNameTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Mining)));
		}
		if (text.Contains("<AnimalHandlingLicence>"))
		{
			text = text.Replace("<AnimalHandlingLicence>", UIAnimationManager.manage.GetCharacterNameTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.AnimalHandling)));
		}
		if (text.Contains("<licenceType>"))
		{
			if ((bool)GiveNPC.give.dropToBuy.sellsAnimal)
			{
				text = text.Replace("<licenceType>", UIAnimationManager.manage.GetCharacterNameTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.AnimalHandling) + " " + GiveNPC.give.dropToBuy.sellsAnimal.GetComponent<FarmAnimal>().getLicenceLevelText()));
			}
			else
			{
				int requiredToBuy = (int)Inventory.Instance.allItems[GiveNPC.give.dropToBuy.myItemId].requiredToBuy;
				string licenceLevelText = Inventory.Instance.allItems[GiveNPC.give.dropToBuy.myItemId].getLicenceLevelText();
				text = text.Replace("<licenceType>", UIAnimationManager.manage.GetCharacterNameTag(licenceLevelText + LicenceManager.manage.getLicenceName((LicenceManager.LicenceTypes)requiredToBuy)));
			}
		}
		if (text.Contains("<itemName>"))
		{
			if (response != -1)
			{
				if ((bool)convo.targetResponses[response].talkingAboutItem)
				{
					text = text.Replace("<itemName>", UIAnimationManager.manage.GetItemColorTag(convo.targetResponses[response].talkingAboutItem.getInvItemName()));
					text = LocalisationMarkUp.RunMarkupCheck(text, convo.targetResponses[response].talkingAboutItem);
				}
			}
			else if ((bool)convo.targetOpenings.talkingAboutItem)
			{
				text = text.Replace("<itemName>", UIAnimationManager.manage.GetItemColorTag(convo.targetOpenings.talkingAboutItem.getInvItemName()));
				text = LocalisationMarkUp.RunMarkupCheck(text, convo.targetOpenings.talkingAboutItem);
			}
		}
		if (text.Contains("<sellByWeightName>"))
		{
			text = text.Replace("<sellByWeightName>", UIAnimationManager.manage.GetItemColorTag(GiveNPC.give.getSellByWeightName()));
		}
		if (text.Contains("<getItemWeight>"))
		{
			text = text.Replace("<getItemWeight>", GiveNPC.give.getItemWeight());
		}
		if (text.Contains("<getItemByWeightPrice>"))
		{
			text = text.Replace("<getItemByWeightPrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + GiveNPC.give.getSellByWeightMoneyValue().ToString("n0")));
		}
		if (text.Contains("<animalType>"))
		{
			text = ((response == -1) ? text.Replace("<animalType>", UIAnimationManager.manage.GetCharacterNameTag(convo.targetOpenings.talkingAboutAnimal.GetAnimalName())) : text.Replace("<animalType>", UIAnimationManager.manage.GetCharacterNameTag(convo.targetResponses[response].talkingAboutAnimal.GetAnimalName())));
		}
		if (text.Contains("<animalName>"))
		{
			if (response != -1)
			{
				text = text.Replace("<animalName>", UIAnimationManager.manage.GetCharacterNameTag(convo.targetResponses[response].talkingAboutAnimal.GetAnimalName()));
				text = LocalisationMarkUp.RunMarkupCheckAnimal(text, convo.targetResponses[response].talkingAboutAnimal);
			}
			else
			{
				text = text.Replace("<animalName>", UIAnimationManager.manage.GetCharacterNameTag(convo.targetOpenings.talkingAboutAnimal.GetAnimalName()));
				text = LocalisationMarkUp.RunMarkupCheckAnimal(text, convo.targetOpenings.talkingAboutAnimal);
			}
		}
		if (text.Contains("<donationName>"))
		{
			text = text.Replace("<donationName>", UIAnimationManager.manage.GetItemColorTag(GiveNPC.give.getDonationName()));
		}
		if (text.Contains("<givenItem>"))
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
			text = text.Replace("<givenItem>", UIAnimationManager.manage.GetItemColorTag(NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].getDesiredItemNameByNumber(inventorySlot.itemNo, inventorySlot.stack)));
		}
		if (text.Contains("<requestItem>"))
		{
			PostOnBoard postOnBoard = BulletinBoard.board.checkMissionsCompletedForNPC(lastConversationTarget.GetComponent<NPCIdentity>().NPCNo);
			if (postOnBoard != null)
			{
				GiveNPC.give.givingPost = postOnBoard;
			}
			if (postOnBoard != null)
			{
				text = text.Replace("<requestItem>", UIAnimationManager.manage.GetItemColorTag(ConversationGenerator.generate.GetItemAmount(postOnBoard.requireItemAmount, Inventory.Instance.allItems[postOnBoard.requiredItem].getInvItemName())));
				text = LocalisationMarkUp.RunMarkupCheck(text, Inventory.Instance.allItems[postOnBoard.requiredItem], postOnBoard.requireItemAmount);
			}
			else
			{
				text = text.Replace("<requestItem>", UIAnimationManager.manage.GetItemColorTag(NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].getDesiredItemName()));
				text = ((NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].specificDesiredItem < 0) ? LocalisationMarkUp.ScrubGenderPlaceholders(text) : LocalisationMarkUp.RunMarkupCheck(text, NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].specificDesiredItem, NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].desiredAmount));
			}
		}
		if (text.Contains("<buyRequestAmount>"))
		{
			text = text.Replace("<buyRequestAmount>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].getDesiredPriceToPay().ToString("n0")));
		}
		if (text.Contains("<requestItemLocation>"))
		{
			PostOnBoard postOnBoard2 = BulletinBoard.board.checkMissionsCompletedForNPC(lastConversationTarget.GetComponent<NPCIdentity>().NPCNo);
			if (postOnBoard2 != null)
			{
				GiveNPC.give.givingPost = postOnBoard2;
			}
			if (postOnBoard2 == null)
			{
				text = text.Replace("<requestItemLocation>", NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].itemFoundInLocation);
			}
		}
		if (text.Contains("<marketPlaceNPCName>"))
		{
			text = text.Replace("<marketPlaceNPCName>", UIAnimationManager.manage.GetCharacterNameTag(MarketPlaceManager.manage.getCurrentVisitorsName()));
		}
		if (text.Contains("<currentItemInHand>"))
		{
			text = text.Replace("<currentItemInHand>", UIAnimationManager.manage.GetItemColorTag(NetworkMapSharer.Instance.localChar.myEquip.itemCurrentlyHolding.getInvItemName()));
		}
		if (text.Contains("<farmAnimalType>"))
		{
			text = text.Replace("<farmAnimalType>", UIAnimationManager.manage.GetItemColorTag("Animal Type"));
		}
		if (text.Contains("<farmAnimalName>"))
		{
			text = text.Replace("<farmAnimalName>", UIAnimationManager.manage.GetItemColorTag("Animal Name"));
		}
		if (text.Contains("<randomClothing>"))
		{
			InventoryItem randomClothingName = ConversationGenerator.generate.GetRandomClothingName();
			text = text.Replace("<randomClothing>", UIAnimationManager.manage.GetItemColorTag(randomClothingName.getInvItemName()));
			text = LocalisationMarkUp.RunMarkupCheck(text, randomClothingName);
		}
		if (text.Contains("<$>"))
		{
			text = text.Replace("<$>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + GiveNPC.give.moneyOffer.ToString("n0")));
		}
		if (text.Contains("Wish"))
		{
			if (text.Contains("<WishingWellPrice>"))
			{
				text = text.Replace("<WishingWellPrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + NetworkMapSharer.Instance.wishManager.GetWishCost().ToString("n0")));
			}
			if (text.Contains("<GetCurrentWish>"))
			{
				text = text.Replace("<GetCurrentWish>", NetworkMapSharer.Instance.wishManager.GetCurrentWishName());
			}
			if (text.Contains("<DangerousWish>"))
			{
				text = text.Replace("<DangerousWish>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Dangerous_Wish")));
			}
			if (text.Contains("<BountifulWish>"))
			{
				text = text.Replace("<BountifulWish>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Bountiful_Wish")));
			}
			if (text.Contains("<PeacefulWish>"))
			{
				text = text.Replace("<PeacefulWish>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Peaceful_Wish")));
			}
			if (text.Contains("<RegretfulWish>"))
			{
				text = text.Replace("<RegretfulWish>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Regretful_Wish")));
			}
			if (text.Contains("<FortuitousWish>"))
			{
				text = text.Replace("<FortuitousWish>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Fortuitous_Wish")));
			}
			if (text.Contains("<SpotlessWish>"))
			{
				text = text.Replace("<SpotlessWish>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Spotless_Wish")));
			}
		}
		if (text.Contains("<TeleJumperFee>"))
		{
			text = text.Replace("<TeleJumperFee>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + 10000.ToString("n0")));
		}
		if (text.Contains("<upgradeHouseCost>"))
		{
			text = text.Replace("<upgradeHouseCost>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + HouseEditor.edit.GetCurrentGuestHouseUpdgradeCost().ToString("n0")));
		}
		if (text.Contains("<ticketAmount>"))
		{
			text = ((GiveNPC.give.getRaffleTicketAmount() <= 1) ? text.Replace("<ticketAmount>", UIAnimationManager.manage.GetItemColorTag("<sprite=31>" + GiveNPC.give.getRaffleTicketAmount() + " " + GiveNPC.give.raffleTicketItem.getInvItemName())) : text.Replace("<ticketAmount>", UIAnimationManager.manage.GetItemColorTag("<sprite=31>" + GiveNPC.give.getRaffleTicketAmount() + " " + GiveNPC.give.raffleTicketItem.getInvItemName(2))));
		}
		if (text.Contains("<Dinks>"))
		{
			text = text.Replace("<Dinks>", UIAnimationManager.manage.MoneyAmountColorTag(GetLocByTag("Dinks")));
		}
		if (text.Contains("<Dink>"))
		{
			text = text.Replace("<Dink>", UIAnimationManager.manage.MoneyAmountColorTag(GetLocByTag("Dink")));
		}
		if (text.Contains("<Journal>"))
		{
			text = text.Replace("<Journal>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Journal")));
		}
		if (text.Contains("<Shift>"))
		{
			text = text.Replace("<Shift>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Shift")));
		}
		if (text.Contains("<Shifts>"))
		{
			text = text.Replace("<Shifts>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Shifts")));
		}
		if (text.Contains("<Licence>"))
		{
			text = text.Replace("<Licence>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Licence")));
		}
		if (text.Contains("<Licences>"))
		{
			text = text.Replace("<Licences>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Licences")));
		}
		if (text.Contains("<CommerceLicence>"))
		{
			text = text.Replace("<CommerceLicence>", UIAnimationManager.manage.GetItemColorTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Commerce)));
		}
		if (text.Contains("<FarmingLicence>"))
		{
			text = text.Replace("<CommerceLicence>", UIAnimationManager.manage.GetItemColorTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Farming)));
		}
		if (text.Contains("<LoggingLicence>"))
		{
			text = text.Replace("<LoggingLicence>", UIAnimationManager.manage.GetItemColorTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Logging)));
		}
		if (text.Contains("<AirShip>"))
		{
			text = text.Replace("<AirShip>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Airship")));
		}
		if (text.Contains("<AirShips>"))
		{
			text = text.Replace("<AirShips>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Airships")));
		}
		if (text.Contains("<Nomad>"))
		{
			text = text.Replace("<Nomad>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Nomad")));
		}
		if (text.Contains("<birthday>"))
		{
			text = text.Replace("<birthday>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Birthday")));
		}
		if (text.Contains("<Nomads>"))
		{
			text = text.Replace("<Nomads>", UIAnimationManager.manage.GetCharacterNameTag("Nomads"));
		}
		if (text.Contains("<BlobFish>"))
		{
			text = text.Replace("<BlobFish>", UIAnimationManager.manage.GetItemColorTag(blobFish.getInvItemName()));
		}
		if (text.Contains("<Blobfish>"))
		{
			text = text.Replace("<Blobfish>", UIAnimationManager.manage.GetItemColorTag(blobFish.getInvItemName()));
		}
		if (text.Contains("<PermitPoints>"))
		{
			text = text.Replace("<PermitPoints>", UIAnimationManager.manage.PointsAmountColorTag(GetLocByTag("Permit Points")));
		}
		if (text.Contains("<SnagSizzle>"))
		{
			text = text.Replace("<SnagSizzle>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Snag Sizzle")));
		}
		if (text.Contains("<SnagSizzles>"))
		{
			text = text.Replace("<SnagSizzles>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Snag Sizzles")));
		}
		if (text.Contains("<Snag>"))
		{
			text = text.Replace("<Snag>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Snag")));
		}
		if (text.Contains("<Snags>"))
		{
			text = text.Replace("<Snags>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Snags")));
		}
		if (text.Contains("<IslandDay>"))
		{
			text = text.Replace("<IslandDay>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Island Day").Replace("<IslandName>", NetworkMapSharer.Instance.islandName)));
		}
		if (text.Contains("<SkyFest>"))
		{
			text = text.Replace("<SkyFest>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Sky Fest")));
		}
		if (text.Contains("<Kite>"))
		{
			text = text.Replace("<Kite>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Kite")));
		}
		if (text.Contains("<PaperLantern>"))
		{
			text = text.Replace("<PaperLantern>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Flying Lantern")));
		}
		if (text.Contains("<PaperLanterns>"))
		{
			text = text.Replace("<PaperLanterns>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Flying Lanterns")));
		}
		if (text.Contains("<KiteMakingTable>"))
		{
			text = text.Replace("<KiteMakingTable>", UIAnimationManager.manage.GetItemColorTag(ConversationGenerator.generate.GetToolTip("Tip_KiteMakingTable")));
		}
		if (text.Contains("<Aurora>"))
		{
			text = text.Replace("<Aurora>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Aurora")));
		}
		if (text.Contains("<timeSinceArriving>"))
		{
			text = text.Replace("<timeSinceArriving>", UIAnimationManager.manage.GetItemColorTag(string.Format(GetLocByTag("YearSinceArriving"), TownEventManager.manage.AddOrdinal(WorldManager.Instance.year))));
		}
		if (text.Contains("<timeSinceArrivingInteger>"))
		{
			text = text.Replace("<timeSinceArrivingInteger>", UIAnimationManager.manage.GetItemColorTag(WorldManager.Instance.year.ToString()));
		}
		if (text.Contains("<itemLicence>"))
		{
			InventoryItem inventoryItem = null;
			if (response != -1)
			{
				if ((bool)convo.targetResponses[response].talkingAboutItem)
				{
					inventoryItem = convo.targetResponses[response].talkingAboutItem;
				}
			}
			else if ((bool)convo.targetOpenings.talkingAboutItem)
			{
				inventoryItem = convo.targetOpenings.talkingAboutItem;
			}
			text = ((!(inventoryItem != null)) ? text.Replace("<itemLicence>", UIAnimationManager.manage.GetCharacterNameTag(GetLocByTag("Licence"))) : text.Replace("<itemLicence>", UIAnimationManager.manage.GetCharacterNameTag(LicenceManager.manage.getLicenceName(inventoryItem.requiredToBuy) + inventoryItem.getLicenceLevelText())));
		}
		if (text.Contains("<requestSellPrice>"))
		{
			text = text.Replace("<requestSellPrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + NPCManager.manage.NPCRequests[lastConversationTarget.GetComponent<NPCIdentity>().NPCNo].getSellPrice().ToString("n0")));
		}
		if (text.Contains("<pointsAmount>"))
		{
			text = text.Replace("<pointsAmount>", UIAnimationManager.manage.PointsAmountColorTag("<sprite=15>" + Mathf.Clamp(GiveNPC.give.moneyOffer / 250, 1f, 1E+11f).ToString("n0")));
		}
		if (text.Contains("<donationReward>"))
		{
			text = text.Replace("<donationReward>", UIAnimationManager.manage.PointsAmountColorTag("<sprite=15>" + GiveNPC.give.getMuseumRewardOffer()));
		}
		if (text.Contains("<buyItemName>"))
		{
			if ((bool)GiveNPC.give.dropToBuy.sellsAnimal)
			{
				text = text.Replace("<buyItemName>", UIAnimationManager.manage.GetItemColorTag(GiveNPC.give.dropToBuy.sellsAnimal.GetAnimalName()));
				text = LocalisationMarkUp.RunMarkupCheckAnimal(text, GiveNPC.give.dropToBuy.sellsAnimal);
			}
			else
			{
				text = text.Replace("<buyItemName>", UIAnimationManager.manage.GetItemColorTag(Inventory.Instance.allItems[GiveNPC.give.dropToBuy.myItemId].getInvItemName()));
				text = LocalisationMarkUp.RunMarkupCheck(text, Inventory.Instance.allItems[GiveNPC.give.dropToBuy.myItemId]);
			}
		}
		if (text.Contains("<getHousePrice>"))
		{
			text = text.Replace("<getHousePrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + TownManager.manage.getNextHouseCost().ToString("n0")));
		}
		if (text.Contains("<buyItemPrice>"))
		{
			if (GiveNPC.give.dropToBuy.usesPermitPoints)
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.PointsAmountColorTag("<sprite=15>" + (GiveNPC.give.dropToBuy.getSellPrice() / 500).ToString("n0")));
			}
			else if ((bool)GiveNPC.give.dropToBuy.sellsAnimal)
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + GiveNPC.give.dropToBuy.sellsAnimal.GetComponent<FarmAnimal>().baseValue.ToString("n0")));
			}
			else if (GiveNPC.give.dropToBuy.recipesOnly)
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + GiveNPC.give.dropToBuy.getRecipePrice().ToString("n0")));
			}
			else if (GiveNPC.give.dropToBuy.gives10)
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + GiveNPC.give.dropToBuy.getSellPrice().ToString("n0")));
				GiveNPC.give.optionAmountWindow.SetToBuyingItem();
				showOptionAmountWindow = true;
			}
			else
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + GiveNPC.give.dropToBuy.getSellPrice().ToString("n0")));
			}
		}
		if (text.Contains("<craftItemPrice>"))
		{
			text = text.Replace("<craftItemPrice>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + CraftsmanManager.manage.getCraftingPrice().ToString("n0")));
		}
		if (text.Contains("<BulletinBoardReward>"))
		{
			PostOnBoard postOnBoard3 = BulletinBoard.board.checkMissionsCompletedForNPC(lastConversationTarget.GetComponent<NPCIdentity>().NPCNo);
			if (postOnBoard3 == null)
			{
				postOnBoard3 = GiveNPC.give.givingPost;
			}
			if (postOnBoard3 != null)
			{
				if (postOnBoard3.rewardId == Inventory.Instance.moneyItem.getItemId())
				{
					text = text.Replace("<BulletinBoardReward>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + postOnBoard3.rewardAmount.ToString("n0")));
				}
				else if (postOnBoard3.rewardAmount > 1)
				{
					text = text.Replace("<BulletinBoardReward>", UIAnimationManager.manage.GetItemColorTag(ConversationGenerator.generate.GetItemAmount(postOnBoard3.rewardAmount, Inventory.Instance.allItems[postOnBoard3.rewardId].getInvItemName())));
					text = LocalisationMarkUp.RunMarkupCheckForRewardOnly(text, Inventory.Instance.allItems[postOnBoard3.rewardId], postOnBoard3.rewardAmount);
				}
				else
				{
					text = text.Replace("<BulletinBoardReward>", UIAnimationManager.manage.GetItemColorTag(Inventory.Instance.allItems[postOnBoard3.rewardId].getInvItemName()));
					text = LocalisationMarkUp.RunMarkupCheckForRewardOnly(text, Inventory.Instance.allItems[postOnBoard3.rewardId], postOnBoard3.rewardAmount);
				}
			}
			else
			{
				text = text.Replace("<BulletinBoardReward>", "reward");
			}
		}
		if (text.Contains("<requestedHuntTarget>"))
		{
			PostOnBoard postOnBoard4 = BulletinBoard.board.checkMissionsCompletedForNPC(lastConversationTarget.GetComponent<NPCIdentity>().NPCNo);
			text = ((postOnBoard4 == null) ? text.Replace("<requestedHuntTarget>", "Animal") : text.Replace("<requestedHuntTarget>", postOnBoard4.getPostPostsById().getBoardHuntRequestAnimal(postOnBoard4.getPostIdOnBoard())));
		}
		if (text.Contains("<getPhotoDetails>"))
		{
			text = ((Random.Range(0, 2) != 0) ? text.Replace("<getPhotoDetails>", GetLocByTag("AskAboutMuseumPhoto_1")) : text.Replace("<getPhotoDetails>", GetLocByTag("AskAboutMuseumPhoto_0")));
		}
		if (text.Contains("<animalJustBoughtName>"))
		{
			text = text.Replace("<animalJustBoughtName>", UIAnimationManager.manage.GetCharacterNameTag(FarmAnimalMenu.menu.getLastAnimalName()));
		}
		if (text.Contains("<specialBuyItemDescription>"))
		{
			string locByTag = GetLocByTag("SpecialItemDescription_InvItem_" + GiveNPC.give.dropToBuy.myItemId);
			if (locByTag != null && locByTag != "")
			{
				MonoBehaviour.print("giving special tag here");
				text = text.Replace("<specialBuyItemDescription>", locByTag);
			}
			else
			{
				MonoBehaviour.print("replacing with normal tag");
				text = text.Replace("<specialBuyItemDescription>", "<buyItemDescription>");
			}
		}
		if (text.Contains("<buyItemDescription>"))
		{
			text = ((Inventory.Instance.getExtraDetails(GiveNPC.give.dropToBuy.myItemId) != "") ? text.Replace("<buyItemDescription>", Inventory.Instance.getExtraDetails(GiveNPC.give.dropToBuy.myItemId)) : (GiveNPC.give.dropToBuy.furnitureOnly ? ((Random.Range(0, 2) != 0) ? text.Replace("<buyItemDescription>", GetLocByTag("AskAboutFurniture_1")) : text.Replace("<buyItemDescription>", GetLocByTag("AskAboutFurniture_0"))) : ((!Inventory.Instance.allItems[GiveNPC.give.dropToBuy.myItemId].equipable || !Inventory.Instance.allItems[GiveNPC.give.dropToBuy.myItemId].equipable.cloths) ? text.Replace("<buyItemDescription>", Inventory.Instance.allItems[GiveNPC.give.dropToBuy.myItemId].getItemDescription(GiveNPC.give.dropToBuy.myItemId)) : ((Random.Range(0, 2) != 0) ? text.Replace("<buyItemDescription>", GetLocByTag("AskAboutClothing_1")) : text.Replace("<buyItemDescription>", GetLocByTag("AskAboutClothing_0"))))));
		}
		if (text.Contains("<deedDebtAmount>"))
		{
			text = text.Replace("<deedDebtAmount>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + DeedManager.manage.getDeedCost().ToString("n0")));
		}
		if (text.Contains("<deedName>"))
		{
			text = text.Replace("<deedName>", UIAnimationManager.manage.GetItemColorTag(DeedManager.manage.getDeedName()));
		}
		if (text.Contains("<questRequiredItems>"))
		{
			text = text.Replace("<questRequiredItems>", UIAnimationManager.manage.GetItemColorTag(QuestManager.manage.listRequiredItemsInQuestLastTalkedAbout()));
		}
		if (text.Contains("<nextDayOff>"))
		{
			text = text.Replace("<nextDayOff>", NPCManager.manage.NPCDetails[lastConversationTarget.myId.NPCNo].mySchedual.getNextDayOffName());
		}
		if (text.Contains("<getWeatherPrediction>"))
		{
			text = text.Replace("<getWeatherPrediction>", WeatherManager.Instance.GetDescriptionOfTomorrowsWeatherPredictions());
		}
		if (text.Contains("<getCurrentWeather>"))
		{
			text = text.Replace("<getCurrentWeather>", WeatherManager.Instance.GetCurrentWeatherDescription());
		}
		if (text.Contains("<TownDonateText>"))
		{
			text = text.Replace("<TownDonateText>", UIAnimationManager.manage.MoneyAmountColorTag("<sprite=11>" + NetworkMapSharer.Instance.townDebt.ToString("n0")));
		}
		if (text.Contains("<relocateBuildingName>"))
		{
			text = text.Replace("<relocateBuildingName>", UIAnimationManager.manage.GetItemColorTag(BuildingManager.manage.getTalkingAboutBuildingName()));
		}
		if (text.Contains("<petName>"))
		{
			text = Random.Range(0, 4) switch
			{
				1 => text.Replace("<petName>", GetLocByTag("PetName1")), 
				2 => text.Replace("<petName>", GetLocByTag("PetName2")), 
				_ => text.Replace("<petName>", GetLocByTag("PetName3")), 
			};
		}
		if (text.Contains("<haha>"))
		{
			wantsToShowEmotion = 1;
			text = text.Replace("<haha>", "");
		}
		if (text.Contains("<hoho>"))
		{
			wantsToShowEmotion = 1;
			text = text.Replace("Ho! Ho! Ho!", "");
		}
		if (text.Contains("<angry>"))
		{
			wantsToShowEmotion = 2;
			text = text.Replace("<angry>", "");
		}
		if (text.Contains("<boohoo>"))
		{
			wantsToShowEmotion = 3;
			text = text.Replace("<boohoo>", "");
		}
		if (text.Contains("<wave>"))
		{
			wantsToShowEmotion = 4;
			text = text.Replace("<wave>", "");
		}
		if (text.Contains("<thinking>"))
		{
			wantsToShowEmotion = 5;
			text = text.Replace("<thinking>", "");
		}
		if (text.Contains("<shocked>"))
		{
			wantsToShowEmotion = 6;
			text = text.Replace("<shocked>", "");
		}
		if (text.Contains("<shock>"))
		{
			wantsToShowEmotion = 6;
			text = text.Replace("<shock>", "");
		}
		if (text.Contains("<pumped>"))
		{
			wantsToShowEmotion = 7;
			text = text.Replace("<pumped>", "");
		}
		if (text.Contains("<glee>"))
		{
			wantsToShowEmotion = 8;
			text = text.Replace("<glee>", "");
		}
		if (text.Contains("<shy>"))
		{
			wantsToShowEmotion = 13;
			text = text.Replace("<shy>", "");
		}
		if (text.Contains("?") && wantsToShowEmotion == 0)
		{
			wantsToShowEmotion = 9;
		}
		if (text.Contains("<yes>"))
		{
			wantsToShowEmotion = 10;
			text = text.Replace("<yes>", "");
		}
		if (text.Contains("<no>"))
		{
			wantsToShowEmotion = 11;
			text = text.Replace("<no>", "");
		}
		if (text.Contains("<proud>"))
		{
			wantsToShowEmotion = 14;
			text = text.Replace("<proud>", "");
		}
		if (text.Contains("<worried>"))
		{
			wantsToShowEmotion = 15;
			text = text.Replace("<worried>", "");
		}
		if (text.Contains("<scared>"))
		{
			wantsToShowEmotion = 15;
			text = text.Replace("<scared>", "");
		}
		if (text.Contains("<sigh>"))
		{
			wantsToShowEmotion = 16;
			text = text.Replace("<sigh>", "");
		}
		if (text.Contains("<dance>"))
		{
			wantsToShowEmotion = 17;
			text = text.Replace("<dance>", "");
		}
		if (text.Contains("<bugComp>"))
		{
			text = text.Replace("<bugComp>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Bug Catching Comp")));
		}
		if (text.Contains("<FishingComp>"))
		{
			text = text.Replace("<FishingComp>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Fishing Comp")));
		}
		if (text.Contains("<compBook>"))
		{
			text = text.Replace("<compBook>", UIAnimationManager.manage.GetItemColorTag(GetLocByTag("Comp Log Book")));
		}
		if (text.Contains("<compScore>"))
		{
			string inString2 = string.Format(ConversationGenerator.generate.GetDescriptionDetails("CompPoints"), NetworkMapSharer.Instance.localChar.myEquip.localCompScore.ToString());
			text = text.Replace("<compScore>", UIAnimationManager.manage.GetItemColorTag(inString2));
		}
		if (text.Contains("<bugCompStartTime>"))
		{
			text = text.Replace("<bugCompStartTime>", UIAnimationManager.manage.GetItemColorTag(CatchingCompetitionManager.manage.getStartTime()));
		}
		if (text.Contains("<bugCompEndTime>"))
		{
			text = text.Replace("<bugCompEndTime>", UIAnimationManager.manage.GetItemColorTag(CatchingCompetitionManager.manage.getEndTime()));
		}
		if (text.Contains("<getCompScoreboard>"))
		{
			text = text.Replace("<getCompScoreboard>", CatchingCompetitionManager.manage.currentHighScoreHolders);
		}
		if (text.Contains("<tele-"))
		{
			if (text.Contains("<tele-north>"))
			{
				if (NetworkMapSharer.Instance.northOn)
				{
					teledir = "north";
					text = text.Replace("<tele-north>", "Beginning...");
				}
				else
				{
					text = text.Replace("<tele-north>", "...Nothing happened...");
				}
			}
			if (text.Contains("<tele-east>"))
			{
				if (NetworkMapSharer.Instance.eastOn)
				{
					teledir = "east";
					text = text.Replace("<tele-east>", "Beginning...");
				}
				else
				{
					text = text.Replace("<tele-east>", "...Nothing happened...");
				}
			}
			if (text.Contains("<tele-south>"))
			{
				if (NetworkMapSharer.Instance.southOn)
				{
					teledir = "south";
					text = text.Replace("<tele-south>", "Beginning...");
				}
				else
				{
					text = text.Replace("<tele-south>", "...Nothing happened...");
				}
			}
			if (text.Contains("<tele-west>"))
			{
				if (NetworkMapSharer.Instance.westOn)
				{
					teledir = "west";
					text = text.Replace("<tele-west>", "Beginning...");
				}
				else
				{
					text = text.Replace("<tele-west>", "...Nothing happened...");
				}
			}
		}
		return text.Replace("\r", "");
	}

	public string GetLocByTag(string tag)
	{
		return (LocalizedString)("ConversationTagReplacements/" + tag);
	}

	public bool checkPunctuation(string punc)
	{
		switch (punc)
		{
		case "?":
		case "!":
		case ".":
			return true;
		default:
			return false;
		}
	}

	public void ShowRelationshipHearts(int npcNo, bool isSign = false)
	{
		HeartContainer[] array = relationHearts;
		foreach (HeartContainer heartContainer in array)
		{
			heartContainer.gameObject.SetActive(value: true);
			if (NPCManager.manage.npcStatus[npcNo].relationshipLevel >= 96)
			{
				heartContainer.updateHealth(100);
			}
			else
			{
				heartContainer.updateHealth(NPCManager.manage.npcStatus[npcNo].relationshipLevel);
			}
		}
	}

	private void HideRelationshipHearts()
	{
		HeartContainer[] array = relationHearts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
	}

	private IEnumerator spellOutWord(int wordNo)
	{
		for (int i = 0; i < conversationText.textInfo.wordInfo[wordNo].characterCount; i++)
		{
			conversationText.maxVisibleCharacters = 1;
			if (speedUpConversation)
			{
				_ = i % 8;
			}
			if (i % 4 != 0)
			{
				yield return null;
			}
		}
	}

	private void findReward()
	{
		int num = Random.Range(0, 27);
		if (!lastConversationTarget.isSign)
		{
			num += NPCManager.manage.npcStatus[lastConversationTarget.myId.NPCNo].relationshipLevel / 10;
			num = Mathf.Clamp(num, 0, 27);
		}
		if (num <= 10)
		{
			int num2 = GiveNPC.give.getRewardAmount();
			if (num2 < 5000)
			{
				num2 += Random.Range(8000, 12000);
			}
			else if (num2 <= 20000)
			{
				num2 += Random.Range(3000, 7000);
			}
			GiftedItemWindow.gifted.addToListToBeGiven(Inventory.Instance.getInvItemId(Inventory.Instance.moneyItem), num2);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (num <= 15)
		{
			int randomClothing = RandomObjectGenerator.generate.getRandomClothing(resetSeed: true);
			GiftedItemWindow.gifted.addToListToBeGiven(randomClothing, 1);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (num <= 20)
		{
			int randomCookedDish = RandomObjectGenerator.generate.getRandomCookedDish();
			GiftedItemWindow.gifted.addToListToBeGiven(randomCookedDish, 1);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (num <= 25)
		{
			int itemId = RandomObjectGenerator.generate.getRandomFurniture(resetSeed: true).getItemId();
			GiftedItemWindow.gifted.addToListToBeGiven(itemId, 1);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (Random.Range(0, 3) != 1 || !RandomObjectGenerator.generate.giveRandomRecipeFromBluePrint())
		{
			findReward();
		}
	}

	private IEnumerator ReadConversationSegment(int responseNo = -1)
	{
		speedUpConversation = false;
		string[] sequences = ((responseNo != -1) ? conversationToReadThrough.GetTargetResponseSequences(responseNo) : conversationToReadThrough.GetTargetOpeningSequences());
		currentlyShowingEmotion = 0;
		for (int i = 0; i < sequences.Length; i++)
		{
			nextArrowBounce.SetActive(value: false);
			conversationText.SetText(CheckLineForReplacement(sequences[i], conversationToReadThrough, responseNo));
			conversationText.maxVisibleCharacters = 0;
			if (!lastConversationTarget.isSign && (wantsToShowEmotion == 0 || wantsToShowEmotion != currentlyShowingEmotion))
			{
				lastConversationTarget.faceAnim.stopEmotions();
			}
			if (i != 0)
			{
				SoundManager.Instance.play2DSound(nextTextSound);
				if (!speedUpConversation)
				{
					yield return waitForNext;
				}
			}
			for (int c = 0; c < conversationText.textInfo.characterCount + 1; c++)
			{
				int num = c - 1;
				conversationText.maxVisibleCharacters = c + 1;
				lastConversationTarget.playingTalkingAnimation(isPlaying: true);
				if (num == -1 || conversationText.textInfo.characterInfo[num].character != ' ')
				{
					if ((bool)lastConversationTarget.faceAnim)
					{
						lastConversationTarget.faceAnim.setTriggerTalk();
					}
					if (!lastConversationTarget.isSign && OptionsMenu.options.voiceOn)
					{
						tryAndTalk(conversationText.textInfo.characterInfo[Mathf.Clamp(num + 1, 0, conversationText.textInfo.characterInfo.Length - 1)].character);
					}
					else
					{
						SoundManager.Instance.play2DSound(SoundManager.Instance.signTalk);
					}
				}
				if (OptionsMenu.options.textSpeed == 1)
				{
					if (c % 2 == 0)
					{
						yield return null;
					}
				}
				else if (OptionsMenu.options.textSpeed == 2)
				{
					if (c % 5 == 0)
					{
						yield return null;
					}
				}
				else if ((!speedUpConversation || c % 2 != 0) && (!speedUpConversation || c % 3 != 0) && (!speedUpConversation || c % 4 != 0))
				{
					yield return null;
				}
				if (speedUpConversation)
				{
					continue;
				}
				if (c + 1 < conversationText.textInfo.characterCount && conversationText.textInfo.characterInfo[c + 1].character == ' ' && conversationText.textInfo.characterInfo[c].character == ',')
				{
					if (speedUpConversation)
					{
						yield return waitForNextSpeedUp;
					}
					else
					{
						yield return waitForNext;
					}
				}
				if (c < conversationText.textInfo.characterCount && conversationText.textInfo.characterInfo[c].character == ' ')
				{
					yield return null;
				}
			}
			if (!lastConversationTarget.isSign)
			{
				if (wantsToShowEmotion != 0)
				{
					lastConversationTarget.faceAnim.setEmotionNo(wantsToShowEmotion);
				}
				currentlyShowingEmotion = wantsToShowEmotion;
			}
			lastConversationTarget.playingTalkingAnimation(isPlaying: false);
			if (responseNo != -1)
			{
				if (speedUpConversation)
				{
					yield return waitForNextSpeedUp;
				}
				else
				{
					yield return waitForNext;
				}
				nextArrowBounce.SetActive(value: true);
				while (!ready && !IsReadyToClickReadyButton())
				{
					yield return null;
				}
				speedWait = 0f;
				yield return null;
				yield return null;
			}
			else if (conversationToReadThrough.playerOptions.Length == 0 || i < sequences.Length - 1)
			{
				nextArrowBounce.SetActive(value: true);
				while (!ready && !IsReadyToClickReadyButton())
				{
					yield return null;
				}
				speedWait = 0f;
				yield return null;
				yield return null;
			}
		}
	}

	private bool IsNPCFollowingLocalPlayer(NPCAI npc)
	{
		return npc.followingNetId == NetworkMapSharer.Instance.localChar.netId;
	}

	private bool IsReadyToClickReadyButton()
	{
		if ((InputMaster.input.UISelect() && !inOptionScreen) || (InputMaster.input.UICancel() && !inOptionScreen))
		{
			return true;
		}
		return false;
	}

	private bool GetWillExitConversationAfterThisSegment()
	{
		if (conversationToReadThrough.IsConversationGoingToBranchOut(selectedOption))
		{
			return false;
		}
		if (conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenGiveMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenGiveMenuToSell, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenGiveMenuMuseum, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenHaircutMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenHairCutColorMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenCraftsManMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenNickCraftMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenTrapperCraftMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenAgentCraftMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenDeedMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenAnimalMenu, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenGiveForBulletinBoard, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.GivePhotoNpc, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.GivePhotoMuseum, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenTechDonateWindow, selectedOption) || conversationToReadThrough.HasSpecialActionAddedToAnyPointOfConversation(CONVERSATION_SPECIAL_ACTION.OpenLicenceWindow, selectedOption))
		{
			return false;
		}
		return true;
	}
}
