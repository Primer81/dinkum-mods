using System;
using UnityEngine;

[Serializable]
public class Letter
{
	public enum LetterType
	{
		randomLetter,
		thankYouLetter,
		moveInLetter,
		fullInvLetter,
		AnimalResearchLetter,
		AnimalTrapReturn,
		DevLetter,
		CatalogueOrder,
		CraftsmanClosedLetter,
		FishingTips,
		BugTips,
		LicenceUnlock,
		ChrissyTrapDelivered,
		BugCompWin,
		SoldAnimalLetter,
		FishingCompWin,
		SpecialItemTrapDelivery
	}

	public int letterTemplateNo;

	public int sentById;

	public int seasonSent;

	public int itemAttached = -1;

	public int stackOfItemAttached = 1;

	public int itemOriginallAttached = -1;

	public LetterType myType;

	public bool hasBeenRead;

	public bool saved;

	public Letter()
	{
	}

	public Letter(int NPCFrom, LetterType letterType)
	{
		sentById = NPCFrom;
		myType = letterType;
		getRewardFromTemplate();
		itemOriginallAttached = itemAttached;
		seasonSent = WorldManager.Instance.month;
	}

	public Letter(int NPCFrom, LetterType letterType, int rewardId, int rewardStack)
	{
		sentById = NPCFrom;
		itemAttached = rewardId;
		stackOfItemAttached = rewardStack;
		myType = letterType;
		itemOriginallAttached = rewardId;
		seasonSent = WorldManager.Instance.month;
	}

	public Letter(int NPCFrom, LetterType letterType, int itemSubject)
	{
		sentById = NPCFrom;
		itemAttached = -1;
		stackOfItemAttached = -1;
		myType = letterType;
		itemOriginallAttached = itemSubject;
		seasonSent = WorldManager.Instance.month;
	}

	private void getRewardFromTemplate()
	{
		if (getMyTemplate() == null)
		{
			Debug.Log("Template was null");
		}
		if (getMyTemplate() != null && getMyTemplate().gift != null)
		{
			itemAttached = Inventory.Instance.getInvItemId(getMyTemplate().gift);
			stackOfItemAttached = getMyTemplate().stackOfGift;
		}
		else if (getMyTemplate() != null && getMyTemplate().giftFromTable != null)
		{
			Debug.Log(getMyTemplate().giftFromTable.name);
			itemAttached = Inventory.Instance.getInvItemId(getMyTemplate().giftFromTable.getRandomDropFromTable());
			stackOfItemAttached = getMyTemplate().stackOfGift;
		}
		else if (getMyTemplate() != null && getMyTemplate().useRandomFromType)
		{
			itemAttached = getMyTemplate().getRandomGiftFromType();
			stackOfItemAttached = getMyTemplate().stackOfGift;
		}
	}

	public LetterTemplate getMyTemplate()
	{
		if (myType == LetterType.moveInLetter)
		{
			return null;
		}
		if (myType == LetterType.randomLetter)
		{
			if (letterTemplateNo == 0)
			{
				letterTemplateNo = UnityEngine.Random.Range(0, MailManager.manage.randomLetters.Length);
				UnityEngine.Random.InitState(UnityEngine.Random.Range(205050, -209848));
			}
			return MailManager.manage.randomLetters[letterTemplateNo];
		}
		if (myType == LetterType.thankYouLetter)
		{
			if (letterTemplateNo == 0)
			{
				letterTemplateNo = UnityEngine.Random.Range(0, MailManager.manage.thankYouLetters.Length);
				UnityEngine.Random.InitState(UnityEngine.Random.Range(205050, -209848));
			}
			return MailManager.manage.thankYouLetters[letterTemplateNo];
		}
		if (myType == LetterType.fullInvLetter)
		{
			return MailManager.manage.didNotFitInInvLetter[letterTemplateNo];
		}
		if (myType == LetterType.FishingTips)
		{
			return MailManager.manage.fishingTips[letterTemplateNo];
		}
		if (myType == LetterType.LicenceUnlock)
		{
			return MailManager.manage.licenceLevelUp[letterTemplateNo];
		}
		if (myType == LetterType.BugTips)
		{
			return MailManager.manage.bugTips[letterTemplateNo];
		}
		if (myType == LetterType.AnimalResearchLetter)
		{
			return MailManager.manage.animalResearchLetter;
		}
		if (myType == LetterType.AnimalTrapReturn)
		{
			return MailManager.manage.returnTrapLetter;
		}
		if (myType == LetterType.CraftsmanClosedLetter)
		{
			return MailManager.manage.craftmanDayOff;
		}
		if (myType == LetterType.CatalogueOrder)
		{
			return MailManager.manage.catalogueItemLetter;
		}
		if (myType == LetterType.DevLetter)
		{
			return MailManager.manage.devLetter;
		}
		if (myType == LetterType.ChrissyTrapDelivered)
		{
			return MailManager.manage.animalResearchChrissy;
		}
		if (myType == LetterType.SpecialItemTrapDelivery)
		{
			return MailManager.manage.animalResearchSpecialDrop;
		}
		if (myType == LetterType.BugCompWin)
		{
			return MailManager.manage.bugCompPositions[letterTemplateNo];
		}
		if (myType == LetterType.FishingCompWin)
		{
			return MailManager.manage.fishingCompPositions[letterTemplateNo];
		}
		if (myType == LetterType.SoldAnimalLetter)
		{
			return MailManager.manage.soldFarmAnimalWithItem;
		}
		return null;
	}
}
