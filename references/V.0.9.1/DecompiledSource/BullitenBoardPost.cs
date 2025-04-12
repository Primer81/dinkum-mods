using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPost", menuName = "BoardPost")]
public class BullitenBoardPost : ScriptableObject
{
	public string title;

	[TextArea(8, 9)]
	public string contentText;

	public ConversationObject onCompleteConvo;

	public ConversationObject onGivenItemConvo;

	public ConversationObject onGivenWrongPhotoConvo;

	public ConversationObject onGivenSameItemForTradeConvo;

	public ConversationObject onGivenWrongTypeForTradeConvo;

	public TileObject transformativeTileObject;

	public InventoryItem[] itemsRequiredPool;

	public string originalTemplateName;

	public string getBoardRewardItem(int attachedPostId)
	{
		if (BulletinBoard.board.attachedPosts[attachedPostId].rewardId > -1)
		{
			if (BulletinBoard.board.attachedPosts[attachedPostId].rewardAmount > 1)
			{
				return BulletinBoard.board.attachedPosts[attachedPostId].rewardAmount + " " + Inventory.Instance.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].getInvItemName(BulletinBoard.board.attachedPosts[attachedPostId].rewardAmount);
			}
			return Inventory.Instance.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].getInvItemName(BulletinBoard.board.attachedPosts[attachedPostId].rewardAmount);
		}
		return "";
	}

	public string getBoardHuntRequestAnimal(int attachedPostId)
	{
		if (BulletinBoard.board.attachedPosts[attachedPostId].isHuntingTask)
		{
			return BulletinBoard.board.attachedPosts[attachedPostId].myHuntingChallenge.getAnimalName();
		}
		if (BulletinBoard.board.attachedPosts[attachedPostId].isCaptureTask)
		{
			if (BulletinBoard.board.attachedPosts[attachedPostId].captureVariation != -1)
			{
				return AnimalManager.manage.allAnimals[BulletinBoard.board.attachedPosts[attachedPostId].animalToCapture].GetAnimalVariationAdjective(BulletinBoard.board.attachedPosts[attachedPostId].captureVariation) + AnimalManager.manage.allAnimals[BulletinBoard.board.attachedPosts[attachedPostId].animalToCapture].GetAnimalName();
			}
			return AnimalManager.manage.allAnimals[BulletinBoard.board.attachedPosts[attachedPostId].animalToCapture].GetAnimalName();
		}
		return "";
	}

	public void copyPostContents(BullitenBoardPost copyTo)
	{
		copyTo.title = title;
		copyTo.contentText = contentText;
		copyTo.onCompleteConvo = onCompleteConvo;
		copyTo.onGivenItemConvo = onGivenItemConvo;
		copyTo.onGivenWrongPhotoConvo = onGivenWrongPhotoConvo;
		copyTo.transformativeTileObject = transformativeTileObject;
		copyTo.onGivenSameItemForTradeConvo = onGivenSameItemForTradeConvo;
		copyTo.onGivenWrongTypeForTradeConvo = onGivenWrongTypeForTradeConvo;
		copyTo.itemsRequiredPool = itemsRequiredPool;
		copyTo.originalTemplateName = base.name;
	}

	public string GetPhotoRequestFormatting(int postId)
	{
		if (BulletinBoard.board.attachedPosts[postId].isPhotoTask)
		{
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Animal)
			{
				return ConversationGenerator.generate.GetBulletinBoardText("PhotoRequestOf_Animals").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Npc)
			{
				return ConversationGenerator.generate.GetBulletinBoardText("PhotoRequestOf_NPC").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Biome)
			{
				return ConversationGenerator.generate.GetBulletinBoardText("PhotoRequestOf_Biome").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Location)
			{
				return ConversationGenerator.generate.GetBulletinBoardText("PhotoRequestOf_NearLocation").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Carryable)
			{
				return ConversationGenerator.generate.GetBulletinBoardText("PhotoRequestOf_Carriable").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId));
			}
		}
		return "";
	}

	public string GetPhotoRequestMissionText(int postId)
	{
		if (BulletinBoard.board.attachedPosts[postId].isPhotoTask)
		{
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Animal)
			{
				return ConversationGenerator.generate.GetQuestTrackerText("MissionText_PhotoOf_Animals").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId, isMissionText: true));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Npc)
			{
				return ConversationGenerator.generate.GetQuestTrackerText("MissionText_PhotoOf_NPC").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId, isMissionText: true));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Biome)
			{
				return ConversationGenerator.generate.GetQuestTrackerText("MissionText_PhotoOf_Biome").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId, isMissionText: true));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Location)
			{
				return ConversationGenerator.generate.GetQuestTrackerText("MissionText_PhotoOf_NearLocation").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId, isMissionText: true));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Carryable)
			{
				return ConversationGenerator.generate.GetQuestTrackerText("MissionText_PhotoOf_Carriable").Replace("<getPhotoTarget>", getRequirementsNeededInPhoto(postId, isMissionText: true));
			}
		}
		return "";
	}

	public string getRequirementsNeededInPhoto(int postId, bool isMissionText = false)
	{
		if (!BulletinBoard.board.attachedPosts[postId].isPhotoTask)
		{
			return "";
		}
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		string text = "";
		if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Animal)
		{
			for (int i = 0; i < BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto().Length; i++)
			{
				string item = BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto()[i].GetAnimalName();
				if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto()[i].animalId == 1)
				{
					item = AnimalManager.manage.allAnimals[1].GetComponent<FishType>().getFishInvItem().getInvItemName();
				}
				else if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto()[i].animalId == 2)
				{
					item = BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto()[i].GetComponent<BugTypes>().bugInvItem().itemName;
				}
				if (!list.Contains(item))
				{
					list.Add(item);
					list2.Add(1);
				}
				else
				{
					list2[list.IndexOf(item)]++;
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				text = ((list2[j] > 1) ? ((!isMissionText) ? (text + list2[j] + " x " + UIAnimationManager.manage.GetCharacterNameTag(list[j])) : (text + list2[j] + " x " + list[j])) : ((!isMissionText) ? (text + " " + UIAnimationManager.manage.GetCharacterNameTag(list[j])) : (text + " " + list[j])));
				if (j != list.Count - 1)
				{
					text = ((j != list.Count - 2 || list.Count <= 1) ? (text + ", ") : (text + " & "));
				}
			}
		}
		else
		{
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Npc)
			{
				if (isMissionText)
				{
					return NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetNPCName();
				}
				return UIAnimationManager.manage.GetCharacterNameTag(NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetNPCName());
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Location)
			{
				if (isMissionText)
				{
					return ConversationGenerator.generate.GetQuestTrackerText("TakenNearThisLocation");
				}
				return UIAnimationManager.manage.GetCharacterNameTag(ConversationGenerator.generate.GetQuestTrackerText("TakenNearThisLocation"));
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Carryable)
			{
				if ((bool)SaveLoad.saveOrLoad.carryablePrefabs[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetComponent<SellByWeight>())
				{
					if (isMissionText)
					{
						return SaveLoad.saveOrLoad.carryablePrefabs[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetComponent<SellByWeight>().GetName();
					}
					return UIAnimationManager.manage.GetItemColorTag(SaveLoad.saveOrLoad.carryablePrefabs[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetComponent<SellByWeight>().GetName());
				}
				if (isMissionText)
				{
					return SaveLoad.saveOrLoad.carryablePrefabs[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetComponent<PickUpAndCarry>().GetName();
				}
				return UIAnimationManager.manage.GetItemColorTag(SaveLoad.saveOrLoad.carryablePrefabs[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetComponent<PickUpAndCarry>().GetName());
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Biome)
			{
				if (isMissionText)
				{
					return BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnRequiredLocationBiomeName();
				}
				return UIAnimationManager.manage.GetCharacterNameTag(BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnRequiredLocationBiomeName());
			}
		}
		return text;
	}

	public string getBoardRequestItem(int attachedPostId, int amountNeeded)
	{
		if (BulletinBoard.board.attachedPosts[attachedPostId].requiredItem > -1)
		{
			if (BulletinBoard.board.attachedPosts[attachedPostId].requireItemAmount <= 1)
			{
				return Inventory.Instance.allItems[BulletinBoard.board.attachedPosts[attachedPostId].requiredItem].getInvItemName(amountNeeded);
			}
			return ConversationGenerator.generate.GetItemAmount(BulletinBoard.board.attachedPosts[attachedPostId].requireItemAmount, Inventory.Instance.allItems[BulletinBoard.board.attachedPosts[attachedPostId].requiredItem].getInvItemName(amountNeeded));
		}
		if (BulletinBoard.board.attachedPosts[attachedPostId].isTrade)
		{
			if (Inventory.Instance.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].isFurniture)
			{
				return ConversationGenerator.generate.GetQuestTrackerText("AnyOtherFurniture");
			}
			if ((bool)Inventory.Instance.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].equipable && Inventory.Instance.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].equipable.cloths)
			{
				return ConversationGenerator.generate.GetQuestTrackerText("AnyOtherClothing");
			}
		}
		return "";
	}

	public void randomiseHuntingConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.isHuntingTask = true;
		postToRandomise.myHuntingChallenge = HuntingChallengeManager.manage.createNewChallengeAndAttachToPost();
		postToRandomise.rewardId = Inventory.Instance.moneyItem.getItemId();
		postToRandomise.rewardAmount = Mathf.RoundToInt((float)AnimalManager.manage.allAnimals[postToRandomise.myHuntingChallenge.getAnimalId()].dangerValue * 1.5f);
	}

	public void randomiseCaptureConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.isCaptureTask = true;
		TrapActivate componentInChildren = WorldManager.Instance.allObjects[141].tileObjectAnimalHouse.houseNavTileFloor.GetComponentInChildren<TrapActivate>();
		int num = Random.Range(0, componentInChildren.canCatch.Length - 1);
		while (componentInChildren.canCatch[num].animalId == 29)
		{
			num = Random.Range(0, componentInChildren.canCatch.Length - 1);
		}
		postToRandomise.animalToCapture = componentInChildren.canCatch[num].animalId;
		if ((bool)AnimalManager.manage.allAnimals[postToRandomise.animalToCapture].hasVariation)
		{
			postToRandomise.captureVariation = AnimalManager.manage.allAnimals[postToRandomise.animalToCapture].getRandomVariationNo();
		}
		else
		{
			postToRandomise.captureVariation = -1;
		}
		postToRandomise.rewardId = Inventory.Instance.moneyItem.getItemId();
		postToRandomise.rewardAmount = (int)((float)AnimalManager.manage.allAnimals[postToRandomise.animalToCapture].dangerValue * Random.Range(15f, 20f));
	}

	public void randomiseTradeConditions(PostOnBoard postToRandomise)
	{
		new List<InventoryItem>();
		postToRandomise.rewardId = RandomObjectGenerator.generate.getRandomClothing();
		postToRandomise.rewardAmount = 1;
		postToRandomise.isTrade = true;
	}

	public void randomisePhotoConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.isPhotoTask = true;
		postToRandomise.rewardId = Inventory.Instance.moneyItem.getItemId();
		postToRandomise.myPhotoChallenge = PhotoChallengeManager.manage.createRandomPhotoChallengeAndAttachToPost();
		postToRandomise.rewardId = Inventory.Instance.moneyItem.getItemId();
		postToRandomise.rewardAmount = postToRandomise.myPhotoChallenge.getReward();
	}

	public void randomiseCookingConditions(PostOnBoard postToRandomise)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].craftable && Inventory.Instance.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CookingTable)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		int index = Random.Range(0, list.Count - 1);
		int num = Random.Range(0, list[index].craftable.itemsInRecipe.Length);
		postToRandomise.requiredItem = list[index].craftable.itemsInRecipe[num].getItemId();
		postToRandomise.requireItemAmount = 1;
		postToRandomise.rewardId = list[index].getItemId();
		postToRandomise.rewardAmount = 1;
	}

	public void randomiseSmeltingCoditions(PostOnBoard postToRandomise)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].itemChange && Inventory.Instance.allItems[i].itemChange.getChangerResultId(transformativeTileObject.tileObjectId) != -1)
			{
				list.Add(Inventory.Instance.allItems[i]);
			}
		}
		InventoryItem inventoryItem = list[Random.Range(0, list.Count)];
		postToRandomise.requiredItem = inventoryItem.getItemId();
		postToRandomise.requireItemAmount = inventoryItem.itemChange.getAmountNeeded(transformativeTileObject.tileObjectId);
		postToRandomise.rewardId = Inventory.Instance.allItems[inventoryItem.itemChange.getChangerResultId(transformativeTileObject.tileObjectId)].getItemId();
		if (inventoryItem.itemChange.getCycles(transformativeTileObject.tileObjectId) > 1)
		{
			postToRandomise.rewardAmount = 2 * inventoryItem.itemChange.getCycles(transformativeTileObject.tileObjectId);
		}
		else
		{
			postToRandomise.rewardAmount = Random.Range(2, 4);
		}
	}

	public void randomiseCraftingConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.requiredItem = itemsRequiredPool[Random.Range(0, itemsRequiredPool.Length)].getItemId();
		postToRandomise.requireItemAmount = Random.Range(1, 4);
		if ((bool)Inventory.Instance.allItems[postToRandomise.requiredItem].craftable)
		{
			postToRandomise.requireItemAmount *= Inventory.Instance.allItems[postToRandomise.requiredItem].craftable.recipeGiveThisAmount;
		}
		postToRandomise.rewardId = Inventory.Instance.moneyItem.getItemId();
		postToRandomise.rewardAmount = (int)((float)Random.Range(3000, 6000) + (float)(Inventory.Instance.allItems[postToRandomise.requiredItem].value * postToRandomise.requireItemAmount) * 1.5f);
	}

	public void randomiseShippingConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.requiredItem = itemsRequiredPool[Random.Range(0, itemsRequiredPool.Length)].getItemId();
		postToRandomise.requireItemAmount = Random.Range(20, 31);
		postToRandomise.rewardId = Inventory.Instance.moneyItem.getItemId();
		postToRandomise.rewardAmount = (int)((float)Random.Range(4000, 6000) + (float)(Inventory.Instance.allItems[postToRandomise.requiredItem].value * postToRandomise.requireItemAmount) * 2f);
	}

	public void randomiseSateliteConditions(PostOnBoard postToRandomise)
	{
		bool flag = false;
		int num = 2000;
		int num2 = Random.Range(200, 800);
		int num3 = Random.Range(200, 800);
		while (!flag)
		{
			if (tileIsEmptyOrHasGrass(num2, num3) && tileIsEmptyOrHasGrass(num2 + 1, num3) && tileIsEmptyOrHasGrass(num2, num3 + 1) && tileIsEmptyOrHasGrass(num2 + 1, num3 + 1))
			{
				flag = true;
			}
			else
			{
				num2 = Random.Range(200, 800);
				num3 = Random.Range(200, 800);
				num--;
			}
			if (num <= 0)
			{
				Debug.LogError("No location found for satelite");
				break;
			}
		}
		if (flag)
		{
			Debug.Log("Spawned a satelite");
			NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[9], new Vector3(num2 * 2 + 1, WorldManager.Instance.heightMap[num2, num3], num3 * 2 + 1));
		}
		postToRandomise.isInvestigation = true;
		postToRandomise.location = new int[3]
		{
			num2,
			WorldManager.Instance.heightMap[num2, num3],
			num3
		};
	}

	public void randomisePresentConditions(PostOnBoard postToRandomise)
	{
		for (int i = 0; i < WorldManager.Instance.allCarriables.Count; i++)
		{
			if (WorldManager.Instance.allCarriables[i] != null && WorldManager.Instance.allCarriables[i].prefabId == 10)
			{
				postToRandomise.isInvestigation = true;
				postToRandomise.location = new int[3]
				{
					(int)WorldManager.Instance.allCarriables[i].transform.position.x / 2,
					WorldManager.Instance.heightMap[(int)WorldManager.Instance.allCarriables[i].transform.position.x / 2, (int)WorldManager.Instance.allCarriables[i].transform.position.z / 2],
					(int)WorldManager.Instance.allCarriables[i].transform.position.z / 2
				};
				return;
			}
		}
		bool flag = false;
		int num = 2000;
		int num2 = Random.Range(200, 800);
		int num3 = Random.Range(200, 800);
		while (!flag)
		{
			if (tileIsEmptyOrHasGrass(num2, num3) && tileIsEmptyOrHasGrass(num2 + 1, num3) && tileIsEmptyOrHasGrass(num2, num3 + 1) && tileIsEmptyOrHasGrass(num2 + 1, num3 + 1))
			{
				flag = true;
			}
			else
			{
				num2 = Random.Range(200, 800);
				num3 = Random.Range(200, 800);
				num--;
			}
			if (num <= 0)
			{
				Debug.LogError("No location found for satelite");
				break;
			}
		}
		if (flag)
		{
			Debug.Log("Spawned a satelite");
			NetworkMapSharer.Instance.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[10], new Vector3(num2 * 2 + 1, WorldManager.Instance.heightMap[num2, num3], num3 * 2 + 1));
		}
		postToRandomise.isInvestigation = true;
		postToRandomise.location = new int[3]
		{
			num2,
			WorldManager.Instance.heightMap[num2, num3],
			num3
		};
	}

	public bool tileIsEmptyOrHasGrass(int xPos, int yPos)
	{
		if (WorldManager.Instance.waterMap[xPos, yPos])
		{
			return false;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] == -1)
		{
			return true;
		}
		if (WorldManager.Instance.onTileMap[xPos, yPos] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].isGrass)
		{
			return true;
		}
		return false;
	}

	public string GetTranslatedTitleText()
	{
		if (originalTemplateName == "")
		{
			return title;
		}
		string bulletinBoardText = ConversationGenerator.generate.GetBulletinBoardText(originalTemplateName + "_Title");
		if (bulletinBoardText == null || bulletinBoardText == "")
		{
			return title;
		}
		return bulletinBoardText;
	}

	public string GetTranslatedContentText()
	{
		if (originalTemplateName == "")
		{
			return contentText;
		}
		string bulletinBoardText = ConversationGenerator.generate.GetBulletinBoardText(originalTemplateName);
		if (bulletinBoardText == null || bulletinBoardText == "")
		{
			return contentText;
		}
		return bulletinBoardText;
	}
}
