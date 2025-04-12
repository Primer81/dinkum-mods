using UnityEngine;

public class HouseConversationGroup : MonoBehaviour
{
	public ConversationObject startingConversation;

	public ConversationObject startingConversationTent;

	public ConversationObject upgradeVersionTent;

	public ConversationObject upgradeVersionNotEnoughMoneyTent;

	public ConversationObject houseAlreadyBeingUpgradedTent;

	public ConversationObject noHouseVersion;

	public ConversationObject upgradeVersion;

	public ConversationObject upgradeVersionNotEnoughMoney;

	public ConversationObject houseAtMax;

	public ConversationObject houseAlreadyBeingUpgraded;

	public ConversationObject houseIsBeingMoved;

	public ConversationObject notALocal;

	public ConversationObject GetStartingConversation()
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return notALocal;
		}
		if (TownManager.manage.getCurrentHouseStage() == 0)
		{
			if (TownManager.manage.checkIfHouseIsBeingUpgraded())
			{
				return houseAlreadyBeingUpgradedTent;
			}
			return startingConversationTent;
		}
		return startingConversation;
	}

	public ConversationObject GetConversation()
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return notALocal;
		}
		if (TownManager.manage.checkIfHouseIsBeingMoved())
		{
			return houseIsBeingMoved;
		}
		if (TownManager.manage.checkIfHouseIsBeingUpgraded())
		{
			return houseAlreadyBeingUpgraded;
		}
		if (TownManager.manage.getCurrentHouseStage() == -1)
		{
			return noHouseVersion;
		}
		if (TownManager.manage.getCurrentHouseStage() >= 0 && TownManager.manage.getCurrentHouseStage() != TownManager.manage.playerHouseStages.Length - 1)
		{
			if (TownManager.manage.getCurrentHouseStage() == 0)
			{
				if (Inventory.Instance.wallet >= TownManager.manage.getNextHouseCost())
				{
					return upgradeVersionTent;
				}
				return upgradeVersionNotEnoughMoneyTent;
			}
			if (Inventory.Instance.wallet >= TownManager.manage.getNextHouseCost())
			{
				return upgradeVersion;
			}
			return upgradeVersionNotEnoughMoney;
		}
		return houseAtMax;
	}
}
