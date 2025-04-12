using I2.Loc;
using UnityEngine;

public class ConversationGenerator : MonoBehaviour
{
	public static ConversationGenerator generate;

	public ConversationObject[] greetingsMorning;

	public ConversationObject[] greetingsMiddag;

	public ConversationObject[] greetingsAfternoon;

	public ConversationObject[] greetingsNightTime;

	public ConversationObject[] greetingsRaining;

	public ConversationObject[] greetingsStormy;

	public ConversationObject[] greetingsWindy;

	public ConversationObject[] greetingsFoggy;

	public ConversationObject[] greetingsHot;

	public ConversationObject[] greetingsCold;

	public ConversationObject[] greetingsRandom;

	public ConversationObject[] requestSpecificFishOrBugRequest;

	public ConversationObject[] requestSomethingToEatConvo;

	public ConversationObject[] requestSomeLoggingItems;

	public ConversationObject[] requestMiningItems;

	public ConversationObject[] requestSellingItem;

	public ConversationObject[] requestSellingItemNotEnoughMoney;

	public ConversationObject[] requestNewClothes;

	public ConversationObject[] requestNewFurniture;

	public ConversationObject[] requestItemFromInv;

	public ConversationObject[] requestItemAccept;

	public ConversationObject[] noRequestConversations;

	public ConversationObject[] myBirthdayConvos;

	public ConversationObject[] hangOutNotGoodEnoughFriends;

	public ConversationObject[] hangOutNoDayOff;

	public ConversationObject[] hangOutAccept;

	public ConversationObject[] hangOutAlreadyWithSomeone;

	public ConversationObject[] givenDislikeFood;

	public ConversationObject[] givenHatedFood;

	public ConversationObject[] givenFavouriteFood;

	public ConversationObject[] randomConvos;

	public ConversationObject[] commentUndies;

	public ConversationObject[] commentClothing;

	public ConversationObject[] commentStamina;

	public ConversationObject[] commentHealth;

	public ConversationObject[] commentFire;

	public ConversationObject[] disguiseGroup;

	public ConversationObject[] commentPoisoned;

	public ConversationObject[] commentAnimal;

	public ConversationObject[] commentActivities;

	public ConversationObject[] commentCrops;

	public ConversationObject[] commentVisitorInTent;

	public InventoryItem balloon;

	public InventoryItem otherBalloon;

	public InventoryItem partyHorn;

	public ConversationObject islandDayGeneric;

	public ConversationObject[] islandDayGreetingMorning;

	public ConversationObject[] islandDayGreetingNoon;

	public ConversationObject[] islandDayGreetingAfternoon;

	public ConversationObject[] islandDayGreetingEvening;

	public ConversationObject offeringBalloon;

	public ConversationObject offeringPartyHorn;

	public ConversationObject skyFestGeneric;

	public ConversationObject[] skyFestGreetingMorning;

	public ConversationObject[] skyFestGreetingNoon;

	public ConversationObject[] skyFestGreetingAfternoon;

	public ConversationObject[] skyFestGreetingEvening;

	private void Awake()
	{
		generate = this;
	}

	public ConversationObject GetRandomGreeting(int npcId)
	{
		if (TownEventManager.manage.townEventOn == TownEventManager.TownEventType.IslandDay)
		{
			return GetIslandDayConvo(npcId);
		}
		if (TownEventManager.manage.townEventOn == TownEventManager.TownEventType.SkyFest)
		{
			return GetSkyFestConvo(npcId);
		}
		if (CatchingCompetitionManager.manage.IsBugCompToday() && CatchingCompetitionManager.manage.competitionActive() && NPCManager.manage.NPCDetails[npcId].onBugCompGreetings.Length != 0)
		{
			return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].onBugCompGreetings);
		}
		if (WeatherManager.Instance.stormMgr.IsActive && Random.Range(0, 3) == 2)
		{
			if (NPCManager.manage.NPCDetails[npcId].stormingGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].stormingGreetings);
			}
			return GetRandomConversationFromGroup(greetingsStormy);
		}
		if (WeatherManager.Instance.rainMgr.IsActive && Random.Range(0, 3) == 2)
		{
			if (NPCManager.manage.NPCDetails[npcId].rainingWeatherGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].rainingWeatherGreetings);
			}
			return GetRandomConversationFromGroup(greetingsStormy);
		}
		if (!WeatherManager.Instance.stormMgr.IsActive && WeatherManager.Instance.windMgr.IsActive && RealWorldTimeLight.time.currentHour < 18 && Random.Range(0, 3) == 2)
		{
			if (NPCManager.manage.NPCDetails[npcId].windyGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].windyGreetings);
			}
			return GetRandomConversationFromGroup(greetingsWindy);
		}
		if (RealWorldTimeLight.time.currentHour != 0 && RealWorldTimeLight.time.currentHour < 10 && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].morningGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].morningGreetings);
			}
			return GetRandomConversationFromGroup(greetingsMorning);
		}
		if (RealWorldTimeLight.time.currentHour != 0 && RealWorldTimeLight.time.currentHour >= 11 && RealWorldTimeLight.time.currentHour < RealWorldTimeLight.time.getSunSetTime() - 3 && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].noonGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].noonGreetings);
			}
			return GetRandomConversationFromGroup(greetingsMiddag);
		}
		if (RealWorldTimeLight.time.currentHour != 0 && RealWorldTimeLight.time.currentHour >= 15 && RealWorldTimeLight.time.currentHour <= RealWorldTimeLight.time.getSunSetTime() && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].arvoGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].arvoGreetings);
			}
			return GetRandomConversationFromGroup(greetingsAfternoon);
		}
		if ((RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour > RealWorldTimeLight.time.getSunSetTime()) && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].nightGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].nightGreetings);
			}
			return GetRandomConversationFromGroup(greetingsNightTime);
		}
		if ((float)GenerateMap.generate.getPlaceTemperature(CameraController.control.transform.position) < 10f && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].coldWeatherGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].coldWeatherGreetings);
			}
			return GetRandomConversationFromGroup(greetingsCold);
		}
		if (WeatherManager.Instance.heatWaveMgr.IsActive && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].hotWeatherGreetings.Length != 0)
			{
				return GetRandomConversationFromGroup(NPCManager.manage.NPCDetails[npcId].hotWeatherGreetings);
			}
			return GetRandomConversationFromGroup(greetingsHot);
		}
		ConversationObject randomGreeting = NPCManager.manage.NPCDetails[npcId].GetRandomGreeting(npcId);
		if (randomGreeting != null)
		{
			return randomGreeting;
		}
		return greetingsRandom[Random.Range(0, greetingsRandom.Length)];
	}

	public ConversationObject GetRandomRequestConversation(int NPCNo)
	{
		if (NPCManager.manage.NPCRequests[NPCNo].wantToSellSomething())
		{
			if (Inventory.Instance.wallet >= NPCManager.manage.NPCRequests[NPCNo].getSellPrice())
			{
				return GetRandomConversationFromGroup(requestSellingItem);
			}
			return GetRandomConversationFromGroup(requestSellingItemNotEnoughMoney);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsAnItemInYourInv())
		{
			return GetRandomConversationFromGroup(requestItemFromInv);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsCertainFishOrBug())
		{
			return GetRandomConversationFromGroup(requestSpecificFishOrBugRequest);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomethingToEat())
		{
			return GetRandomConversationFromGroup(requestSomethingToEatConvo);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomeFurniture())
		{
			return GetRandomConversationFromGroup(requestNewFurniture);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomeClothing())
		{
			return GetRandomConversationFromGroup(requestNewClothes);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomeLogging())
		{
			return GetRandomConversationFromGroup(requestSomeLoggingItems);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomeMining())
		{
			return GetRandomConversationFromGroup(requestMiningItems);
		}
		return GetRandomConversationFromGroup(noRequestConversations);
	}

	public ConversationObject GetRandomRequestItemAcceptedConversation()
	{
		return GetRandomConversationFromGroup(requestItemAccept);
	}

	public ConversationObject GetRandomBirthdayConversation()
	{
		return GetRandomConversationFromGroup(myBirthdayConvos);
	}

	public ConversationObject GetAskToHangOutConversation(int NPCNo)
	{
		if (NetworkMapSharer.Instance.localChar.followedBy != -1)
		{
			return GetRandomConversationFromGroup(hangOutAlreadyWithSomeone);
		}
		if (NPCManager.manage.npcStatus[NPCNo].relationshipLevel < 45)
		{
			return GetRandomConversationFromGroup(hangOutNotGoodEnoughFriends);
		}
		if (NPCManager.manage.NPCDetails[NPCNo].mySchedual.dayOff[WorldManager.Instance.day - 1])
		{
			return GetRandomConversationFromGroup(hangOutAccept);
		}
		return GetRandomConversationFromGroup(hangOutNoDayOff);
	}

	public ConversationObject GetRandomComment()
	{
		if (MarketPlaceManager.manage.someoneVisiting() && NPCManager.manage.npcStatus[ConversationManager.manage.lastConversationTarget.myId.NPCNo].checkIfHasMovedIn() && Random.Range(0, 9) == 3)
		{
			return GetRandomConversationFromGroup(commentVisitorInTent);
		}
		if (!StatusManager.manage.IsStaminaAbove(30f) && Random.Range(0, 9) == 3)
		{
			return GetRandomConversationFromGroup(commentStamina);
		}
		if (StatusManager.manage.connectedDamge.health < 30 && Random.Range(0, 9) == 3)
		{
			return GetRandomConversationFromGroup(commentHealth);
		}
		if (Random.Range(0, 6) == 3)
		{
			if (IsMyCharacterWearingNothing())
			{
				return GetRandomConversationFromGroup(commentUndies);
			}
			return GetRandomConversationFromGroup(commentClothing);
		}
		return GetRandomConversationFromGroup(randomConvos);
	}

	public ConversationObject GetRandomCommentOnPlayerBeingOnFire()
	{
		return GetRandomConversationFromGroup(commentFire);
	}

	public ConversationObject GetRandomCommentOnDisguise()
	{
		return GetRandomConversationFromGroup(disguiseGroup);
	}

	public ConversationObject GetRandomNoRquestConversation()
	{
		return GetRandomConversationFromGroup(noRequestConversations);
	}

	public InventoryItem GetRandomClothingName()
	{
		EquipItemToChar myEquip = NetworkMapSharer.Instance.localChar.myEquip;
		while (true)
		{
			switch (Random.Range(0, 5))
			{
			case 0:
				if (myEquip.shirtId != -1)
				{
					return Inventory.Instance.allItems[myEquip.shirtId];
				}
				break;
			case 1:
				if (myEquip.pantsId != -1)
				{
					return Inventory.Instance.allItems[myEquip.pantsId];
				}
				break;
			case 2:
				if (myEquip.shoeId != -1)
				{
					return Inventory.Instance.allItems[myEquip.shoeId];
				}
				break;
			case 3:
				if (myEquip.faceId != -1)
				{
					return Inventory.Instance.allItems[myEquip.faceId];
				}
				break;
			case 4:
				if (myEquip.headId != -1)
				{
					return Inventory.Instance.allItems[myEquip.headId];
				}
				break;
			}
		}
	}

	private ConversationObject GetRandomConversationFromGroup(ConversationObject[] group)
	{
		return group[Random.Range(0, group.Length)];
	}

	private bool IsMyCharacterWearingNothing()
	{
		if (NetworkMapSharer.Instance.localChar.myEquip.shirtId == -1 && NetworkMapSharer.Instance.localChar.myEquip.pantsId == -1)
		{
			return true;
		}
		return false;
	}

	public ConversationObject GetIslandDayConvo(int npcId)
	{
		if ((bool)ConversationManager.manage.lastConversationTarget && ConversationManager.manage.lastConversationTarget.myId.NPCNo == npcId)
		{
			if (Inventory.Instance.checkIfItemCanFit(balloon.getItemId(), 1) && ConversationManager.manage.lastConversationTarget.doesTask.npcHolds.currentlyHolding == balloon)
			{
				if (Random.Range(0, 2) == 1)
				{
					GiftedItemWindow.gifted.addToListToBeGiven(balloon.getItemId(), 1);
				}
				else
				{
					GiftedItemWindow.gifted.addToListToBeGiven(otherBalloon.getItemId(), 1);
				}
				GiftedItemWindow.gifted.openWindowAndGiveItems();
				NetworkMapSharer.Instance.localChar.CmdTakeItemFromNPC(ConversationManager.manage.lastConversationTarget.netId);
				return offeringBalloon;
			}
			if (Inventory.Instance.checkIfItemCanFit(partyHorn.getItemId(), 1) && ConversationManager.manage.lastConversationTarget.doesTask.npcHolds.currentlyHolding == partyHorn)
			{
				GiftedItemWindow.gifted.addToListToBeGiven(partyHorn.getItemId(), 1);
				GiftedItemWindow.gifted.openWindowAndGiveItems();
				NetworkMapSharer.Instance.localChar.CmdTakeItemFromNPC(ConversationManager.manage.lastConversationTarget.netId);
				return offeringPartyHorn;
			}
		}
		if (RealWorldTimeLight.time.currentHour > 0 && RealWorldTimeLight.time.currentHour < 12)
		{
			if (npcId >= islandDayGreetingMorning.Length)
			{
				return islandDayGeneric;
			}
			return islandDayGreetingMorning[npcId];
		}
		if (RealWorldTimeLight.time.currentHour > 0 && RealWorldTimeLight.time.currentHour < 15)
		{
			if (npcId >= islandDayGreetingNoon.Length)
			{
				return islandDayGeneric;
			}
			return islandDayGreetingNoon[npcId];
		}
		if (RealWorldTimeLight.time.currentHour > 0 && RealWorldTimeLight.time.currentHour < 20)
		{
			if (npcId >= islandDayGreetingAfternoon.Length)
			{
				return islandDayGeneric;
			}
			return islandDayGreetingAfternoon[npcId];
		}
		if (npcId >= islandDayGreetingEvening.Length)
		{
			return islandDayGeneric;
		}
		return islandDayGreetingEvening[npcId];
	}

	public ConversationObject GetSkyFestConvo(int npcId)
	{
		if (RealWorldTimeLight.time.currentHour > 0 && RealWorldTimeLight.time.currentHour < 11)
		{
			if (npcId >= skyFestGreetingMorning.Length)
			{
				return skyFestGeneric;
			}
			return skyFestGreetingMorning[npcId];
		}
		if (RealWorldTimeLight.time.currentHour > 0 && RealWorldTimeLight.time.currentHour < 15)
		{
			if (npcId >= skyFestGreetingNoon.Length)
			{
				return skyFestGeneric;
			}
			return skyFestGreetingNoon[npcId];
		}
		if (RealWorldTimeLight.time.currentHour > 0 && RealWorldTimeLight.time.currentHour < 20)
		{
			if (npcId >= skyFestGreetingAfternoon.Length)
			{
				return skyFestGeneric;
			}
			return skyFestGreetingAfternoon[npcId];
		}
		if (npcId >= skyFestGreetingEvening.Length)
		{
			return skyFestGeneric;
		}
		return skyFestGreetingEvening[npcId];
	}

	public string GetAnimalName(int animalId)
	{
		return (LocalizedString)("AnimalNames/Animal_" + animalId);
	}

	public string GetAnimalVariationAdjective(int animalId, int adjectiveId)
	{
		if (adjectiveId == 0)
		{
			return "";
		}
		return (LocalizedString)("AnimalNames/Animal_" + animalId + "_VariationAdjective_" + adjectiveId);
	}

	public string GetCarriableName(int carriableId)
	{
		return (LocalizedString)("AnimalNames/Carriable_" + carriableId);
	}

	public string GetNPCName(int npcId)
	{
		return (LocalizedString)("NPCNames/NPC_" + npcId);
	}

	public string GetBuildingName(string tag)
	{
		return (LocalizedString)("BuildingNames/" + tag);
	}

	public string GetToolTip(string tag)
	{
		return (LocalizedString)("ToolTips/" + tag);
	}

	public string GetToolTipWithFormat(string tag, string toFormat)
	{
		string text = (LocalizedString)("ToolTips/" + tag);
		if (text != null)
		{
			return string.Format(text, toFormat);
		}
		return "";
	}

	public string GetNotificationText(string tag)
	{
		return (LocalizedString)("Notifications/" + tag);
	}

	public string GetBiomeNameText(string tag)
	{
		return (LocalizedString)("BiomeNames/" + tag);
	}

	public string GetTimeNameByTag(string tag)
	{
		return (LocalizedString)("Time/" + tag);
	}

	public string GetJournalNameByTag(string tag)
	{
		return (LocalizedString)("JournalText/" + tag);
	}

	public string GetOptionNameByTag(string tag)
	{
		return (LocalizedString)("OptionsWindow/" + tag);
	}

	public string GetLanguageNameByTag(int languageID)
	{
		if (LocalizationManager.Sources == null || LocalizationManager.Sources.Count == 0)
		{
			Debug.LogWarning("No localization sources found.");
			return string.Empty;
		}
		LanguageSourceData languageSourceData = LocalizationManager.Sources[0];
		if (languageID < 0 || languageID >= languageSourceData.mLanguages.Count)
		{
			Debug.LogWarning($"Invalid language ID: {languageID}");
			return string.Empty;
		}
		TermData termData = languageSourceData.GetTermData("OptionsWindow/Language_Name");
		if (termData == null)
		{
			Debug.LogWarning("Term 'OptionsWindow/Language_Name' not found in localization source.");
			return string.Empty;
		}
		return termData.GetTranslation(languageID) ?? string.Empty;
	}

	public string GetDailyChallengeByTag(string tag, int points, string name = "")
	{
		if ((string)(LocalizedString)("DailyChallenges/" + tag) != null)
		{
			return string.Format((LocalizedString)("DailyChallenges/" + tag), "<b>" + points.ToString("n0") + "</b>", name);
		}
		return tag + " " + points + " " + name;
	}

	public string GetNPCBubbleText(string tag)
	{
		return (LocalizedString)("NPCBubbleDialogue/" + tag);
	}

	public string GetNPCChattingBubbleText(string tag)
	{
		return (LocalizedString)tag;
	}

	public string GetLetterText(string tag)
	{
		return (LocalizedString)("MailLettersText/" + tag);
	}

	public string GetBulletinBoardText(string tag)
	{
		return (LocalizedString)("Bulletinboard/" + tag);
	}

	public string GetMenuText(string tag)
	{
		return (LocalizedString)("MenuText/" + tag);
	}

	public string GetDescriptionDetails(string tag)
	{
		return (LocalizedString)("DescriptionDetails/" + tag);
	}

	public string GetLocStringByTag(string tag)
	{
		return (LocalizedString)tag;
	}

	public string GetQuestTrackerText(string tag)
	{
		return (LocalizedString)("QuestTrackerText/" + tag);
	}

	public string GetQuestTrackerTextWithCompletedCheck(string tag, bool completed)
	{
		if (completed)
		{
			return "<sprite=13> " + (LocalizedString)("QuestTrackerText/" + tag);
		}
		return "<sprite=12> " + (LocalizedString)("QuestTrackerText/" + tag);
	}

	public string GetBuffNameText(int buffNo)
	{
		return (LocalizedString)("Buffs/Buff_Name_" + buffNo);
	}

	public string GetBuffDescText(int buffNo)
	{
		return (LocalizedString)("Buffs/Buff_Desc_" + buffNo);
	}

	public string GetTimeText(string tag)
	{
		return (LocalizedString)("Time/" + tag);
	}

	public string GetItemAmount(int amount, string name)
	{
		return string.Format((LocalizedString)"QuestTrackerText/AmountItemName", amount, name);
	}

	public string GetMoneyFormat(int amount)
	{
		return string.Format((LocalizedString)"JournalText/MoneyFormat", "<sprite=11>", amount);
	}
}
