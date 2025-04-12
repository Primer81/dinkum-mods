using UnityEngine;

public class MineControls : MonoBehaviour
{
	public bool forEntrance;

	public bool forLevelSelect;

	public bool forAirShip;

	public ConversationObject useControlConversationSO;

	public ConversationObject noMinePassConversationSO;

	public ConversationObject nonLocalConversationSO;

	public ConversationObject allPlayersInLiftSO;

	public ConversationObject normalMinesAlreadyGeneratedSO;

	public ConversationObject forestMinesAlreadyGeneratedSO;

	public ConversationObject noLevelSetSO;

	public ConversationObject notEnoughRubySO;

	public ConversationObject notEnoughEmeraldSO;

	public ConversationObject enoughRubySO;

	public ConversationObject enoughEmeraldSO;

	public ConversationObject startFlyingWithBoardingPass;

	public ConversationObject startFlyingNoBoardingPass;

	public ConversationObject nonLocalTriesToFly;

	public ConversationObject allPlayersArentInAirship;

	public GameObject[] levelGameObject;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isMineControls = this;
	}

	private void OnEnable()
	{
		if (forLevelSelect)
		{
			showObjects();
			RealWorldTimeLight.time.mineLevelChangeEvent.AddListener(showObjects);
		}
	}

	private void OnDisable()
	{
		if (forLevelSelect)
		{
			RealWorldTimeLight.time.mineLevelChangeEvent.RemoveListener(showObjects);
		}
	}

	public void showObjects()
	{
		for (int i = 0; i < levelGameObject.Length; i++)
		{
			if (i == RealWorldTimeLight.time.mineLevel)
			{
				levelGameObject[i].SetActive(value: true);
			}
			else
			{
				levelGameObject[i].SetActive(value: false);
			}
		}
	}

	public void useControls()
	{
		if (!NetworkMapSharer.Instance.canUseMineControls)
		{
			return;
		}
		if (forAirShip)
		{
			if (!NetworkMapSharer.Instance.isServer)
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, nonLocalConversationSO);
			}
			else if (Inventory.Instance.getAmountOfItemInAllSlots(Inventory.Instance.getInvItemId(Inventory.Instance.boardingPass)) <= 0)
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, startFlyingNoBoardingPass);
			}
			else
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, startFlyingWithBoardingPass);
			}
		}
		else if (forLevelSelect)
		{
			if (RealWorldTimeLight.time.mineLevel == -1)
			{
				if (Inventory.Instance.getAmountOfItemInAllSlots(MineEnterExit.mineEntrance.rubyShard.getItemId()) >= 4)
				{
					noLevelSetSO.targetResponses[1].branchToConversation = enoughRubySO;
				}
				else
				{
					noLevelSetSO.targetResponses[1].branchToConversation = notEnoughRubySO;
				}
				if (Inventory.Instance.getAmountOfItemInAllSlots(MineEnterExit.mineEntrance.emeraldShard.getItemId()) >= 4)
				{
					noLevelSetSO.targetResponses[2].branchToConversation = enoughEmeraldSO;
				}
				else
				{
					noLevelSetSO.targetResponses[2].branchToConversation = notEnoughEmeraldSO;
				}
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, noLevelSetSO);
			}
			else if (RealWorldTimeLight.time.mineLevel != -1)
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, normalMinesAlreadyGeneratedSO);
			}
			else if (RealWorldTimeLight.time.mineLevel == 1)
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, forestMinesAlreadyGeneratedSO);
			}
		}
		else if (!NetworkMapSharer.Instance.isServer)
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, nonLocalConversationSO);
		}
		else if (forEntrance)
		{
			if (Inventory.Instance.getAmountOfItemInAllSlots(Inventory.Instance.getInvItemId(Inventory.Instance.minePass)) <= 0)
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, noMinePassConversationSO);
			}
			else
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, useControlConversationSO);
			}
		}
		else
		{
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, useControlConversationSO);
		}
	}
}
