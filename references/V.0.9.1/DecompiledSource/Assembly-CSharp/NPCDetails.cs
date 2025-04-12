using UnityEngine;

public class NPCDetails : MonoBehaviour
{
	public enum DressStyle
	{
		None,
		Fletch,
		Clover,
		Sally
	}

	public string NPCName;

	public bool isSpecialGuest;

	public int birthday;

	public int birthSeason;

	public NPCClothingDetails ClothingDetails;

	public GameObject NpcHair;

	public InventoryItem NPCShirt;

	public InventoryItem NPCPants;

	public InventoryItem NPCShoes;

	public Material NpcSkin;

	public Material NpcEyes;

	public Material NpcEyesColor;

	public Material NPCMouth;

	public DressStyle MyDress;

	public int nose;

	public Mesh insideMouthMesh;

	public Mesh npcMesh;

	public bool isChild;

	public ASound NPCVoice;

	public ASound NPCLaugh;

	public NPCSchedual mySchedual;

	public NPCSchedual.Locations workLocation;

	public int spendBeforeMoveIn = 5000;

	public int relationshipBeforeMove = 15;

	public Color npcColor = Color.blue;

	public Sprite npcSprite;

	public InventoryItem favouriteFood;

	public InventoryItem hatedFood;

	public bool hatesAnimalProducts;

	public bool hatesMeat;

	public bool hatesFruits;

	public bool hatesVegitables;

	public bool isAVillager;

	public string[] randomNames;

	public InventoryItem deedOnMoveRequest;

	public AnimatorOverrideController animationOverrride;

	public ConversationObject introductionConvos;

	public ConversationObject moveInRequestConvos;

	public ConversationObject keeperConvos;

	public ConversationObject[] randomChatConvos;

	public ConversationObject[] gossipConvos;

	public ConversationObject[] townMentionConvos;

	public ConversationObject[] friendshipConvos0;

	public ConversationObject[] friendshipConvos25;

	public ConversationObject[] friendshipConvos50;

	public ConversationObject[] friendshipConvos75;

	public ConversationObject[] lowFriendshipGreetings;

	public ConversationObject[] mediumFriendshipGreetings;

	public ConversationObject[] highFriendshipFriendshipGreetings;

	public ConversationObject[] highestFriendshipFriendshipGreetings;

	public ConversationObject[] morningGreetings;

	public ConversationObject[] noonGreetings;

	public ConversationObject[] arvoGreetings;

	public ConversationObject[] nightGreetings;

	public ConversationObject[] coldWeatherGreetings;

	public ConversationObject[] hotWeatherGreetings;

	public ConversationObject[] rainingWeatherGreetings;

	public ConversationObject[] stormingGreetings;

	public ConversationObject[] windyGreetings;

	public ConversationObject[] insidePubGreetings;

	public ConversationObject[] onBugCompGreetings;

	public ConversationObject[] completeRequestConvos;

	public ConversationObject[] completeBirthdayConvos;

	public InventoryItem skyFestKite;

	public void SetRandomName(int nameId)
	{
		NPCName = randomNames[nameId];
	}

	public void SetVoiceGender(bool feminine)
	{
		if (feminine)
		{
			NPCVoice = SoundManager.Instance.highVoice;
		}
		else
		{
			NPCVoice = SoundManager.Instance.medVoice;
		}
	}

	public bool CanProvideRandomConversation()
	{
		if (friendshipConvos0.Length != 0 || randomChatConvos.Length != 0)
		{
			return true;
		}
		return false;
	}

	public ConversationObject GetRandomGreeting(int NPCId)
	{
		if (lowFriendshipGreetings.Length == 0 && mediumFriendshipGreetings.Length == 0 && highFriendshipFriendshipGreetings.Length == 0 && highestFriendshipFriendshipGreetings.Length == 0)
		{
			return null;
		}
		if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 25)
		{
			return lowFriendshipGreetings[Random.Range(0, lowFriendshipGreetings.Length)];
		}
		if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 50)
		{
			if (Random.Range(0, 2) == 0)
			{
				return lowFriendshipGreetings[Random.Range(0, lowFriendshipGreetings.Length)];
			}
			return mediumFriendshipGreetings[Random.Range(0, mediumFriendshipGreetings.Length)];
		}
		if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 75)
		{
			return Random.Range(0, 3) switch
			{
				0 => lowFriendshipGreetings[Random.Range(0, lowFriendshipGreetings.Length)], 
				1 => mediumFriendshipGreetings[Random.Range(0, mediumFriendshipGreetings.Length)], 
				_ => highFriendshipFriendshipGreetings[Random.Range(0, highFriendshipFriendshipGreetings.Length)], 
			};
		}
		return Random.Range(0, 4) switch
		{
			0 => lowFriendshipGreetings[Random.Range(0, lowFriendshipGreetings.Length)], 
			1 => mediumFriendshipGreetings[Random.Range(0, mediumFriendshipGreetings.Length)], 
			2 => highFriendshipFriendshipGreetings[Random.Range(0, highFriendshipFriendshipGreetings.Length)], 
			_ => highestFriendshipFriendshipGreetings[Random.Range(0, highestFriendshipFriendshipGreetings.Length)], 
		};
	}

	public ConversationObject GetRandomChatConversation(int NPCId)
	{
		if (friendshipConvos0.Length != 0)
		{
			if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 25)
			{
				int randomConversationIndexNotUsedYesterday = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), 0, friendshipConvos0.Length);
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday);
				return friendshipConvos0[randomConversationIndexNotUsedYesterday];
			}
			if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 50)
			{
				if (Random.Range(0, 2) == 0)
				{
					int randomConversationIndexNotUsedYesterday2 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), 0, friendshipConvos0.Length);
					NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday2);
					return friendshipConvos0[randomConversationIndexNotUsedYesterday2];
				}
				int randomConversationIndexNotUsedYesterday3 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendshipConvos0.Length, friendshipConvos25.Length + friendshipConvos0.Length);
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday3);
				return friendshipConvos25[randomConversationIndexNotUsedYesterday3 - friendshipConvos0.Length];
			}
			if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 75)
			{
				switch (Random.Range(0, 3))
				{
				case 0:
				{
					int randomConversationIndexNotUsedYesterday6 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), 0, friendshipConvos0.Length);
					NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday6);
					return friendshipConvos0[randomConversationIndexNotUsedYesterday6];
				}
				case 1:
				{
					int randomConversationIndexNotUsedYesterday5 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendshipConvos0.Length, friendshipConvos25.Length + friendshipConvos0.Length);
					NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday5);
					return friendshipConvos25[randomConversationIndexNotUsedYesterday5 - friendshipConvos0.Length];
				}
				default:
				{
					int randomConversationIndexNotUsedYesterday4 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendshipConvos0.Length + friendshipConvos25.Length, friendshipConvos50.Length + (friendshipConvos0.Length + friendshipConvos25.Length));
					NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday4);
					return friendshipConvos50[randomConversationIndexNotUsedYesterday4 - (friendshipConvos0.Length + friendshipConvos25.Length)];
				}
				}
			}
			switch (Random.Range(0, 4))
			{
			case 0:
			{
				int randomConversationIndexNotUsedYesterday10 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), 0, friendshipConvos0.Length);
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday10);
				return friendshipConvos0[randomConversationIndexNotUsedYesterday10];
			}
			case 1:
			{
				int randomConversationIndexNotUsedYesterday9 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendshipConvos0.Length, friendshipConvos25.Length + friendshipConvos0.Length);
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday9);
				return friendshipConvos25[randomConversationIndexNotUsedYesterday9 - friendshipConvos0.Length];
			}
			case 2:
			{
				int randomConversationIndexNotUsedYesterday8 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendshipConvos0.Length + friendshipConvos25.Length, friendshipConvos50.Length + (friendshipConvos0.Length + friendshipConvos25.Length));
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday8);
				return friendshipConvos50[randomConversationIndexNotUsedYesterday8 - (friendshipConvos0.Length + friendshipConvos25.Length)];
			}
			default:
			{
				int randomConversationIndexNotUsedYesterday7 = GetRandomConversationIndexNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendshipConvos0.Length + friendshipConvos25.Length + friendshipConvos50.Length, friendshipConvos75.Length + (friendshipConvos0.Length + friendshipConvos25.Length + friendshipConvos50.Length));
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomConversationIndexNotUsedYesterday7);
				return friendshipConvos75[randomConversationIndexNotUsedYesterday7 - (friendshipConvos0.Length + friendshipConvos25.Length + friendshipConvos50.Length)];
			}
			}
		}
		if (randomChatConvos.Length != 0)
		{
			return randomChatConvos[Random.Range(0, randomChatConvos.Length)];
		}
		return null;
	}

	private int GetRandomConversationIndexNotUsedYesterday(int[] lastused, int min, int max)
	{
		int num = Random.Range(min, max);
		for (int num2 = 2500; num2 > 0; num2--)
		{
			num = Random.Range(min, max);
			if (lastused[0] != num && lastused[1] != num && lastused[2] != num)
			{
				break;
			}
		}
		return num;
	}

	public Sprite GetNPCSprite(int npcNo)
	{
		if ((bool)NPCManager.manage.NPCDetails[npcNo].npcSprite)
		{
			return NPCManager.manage.NPCDetails[npcNo].npcSprite;
		}
		Sprite sprite = CharacterCreatorScript.create.loadNPCPhoto(npcNo);
		if ((bool)sprite)
		{
			NPCManager.manage.NPCDetails[npcNo].npcSprite = sprite;
			return NPCManager.manage.NPCDetails[npcNo].npcSprite;
		}
		CharacterCreatorScript.create.takeNPCPhoto(npcNo);
		return null;
	}

	public bool IsTodayMyBirthday()
	{
		int num = WorldManager.Instance.day + (WorldManager.Instance.week - 1) * 7;
		if (WorldManager.Instance.month == birthSeason && num == birthday)
		{
			return true;
		}
		return false;
	}

	public string GetNPCName()
	{
		for (int i = 0; i < NPCManager.manage.NPCDetails.Length; i++)
		{
			if (NPCManager.manage.NPCDetails[i] == this)
			{
				return ConversationGenerator.generate.GetNPCName(i);
			}
		}
		return "???";
	}
}
