using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class BugTypes : NetworkBehaviour
{
	[SyncVar(hook = "setUpBug")]
	private int bugType;

	[SyncVar]
	private bool sparkling;

	public GameObject bugLight;

	public GameObject crawling;

	public GameObject wingFlying;

	public GameObject jumping;

	public MeshRenderer crawlingRen;

	public MeshRenderer wingFlyingRen;

	public MeshRenderer jumpingRen;

	public AnimalAI myAi;

	public Animator myAnim;

	public AudioSource bugSoundSource;

	public ItemHitBox myAttackBox;

	private GameObject myBugModel;

	public NameTag myNameTag;

	public Transform groundBugTrans;

	[Header("Underwater Creatures-----")]
	public bool isUnderwaterCreature;

	public GameObject stationaryUnderwaterCreatures;

	public GameObject movingUnderwaterCreatures;

	public GameObject sparklingParticles;

	public bool isASittingBug;

	private static WaitForSeconds soundWait = new WaitForSeconds(0.25f);

	public int NetworkbugType
	{
		get
		{
			return bugType;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref bugType))
			{
				int old = bugType;
				SetSyncVar(value, ref bugType, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					setUpBug(old, value);
					setSyncVarHookGuard(1uL, value: false);
				}
			}
		}
	}

	public bool Networksparkling
	{
		get
		{
			return sparkling;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref sparkling))
			{
				bool flag = sparkling;
				SetSyncVar(value, ref sparkling, 2uL);
			}
		}
	}

	private void OnDisable()
	{
		if (!isUnderwaterCreature)
		{
			AnimalManager.manage.lookAtBugBook.RemoveListener(openBook);
		}
		if (base.isServer)
		{
			WorldManager.Instance.changeDayEvent.RemoveListener(resetBugTypeNewDay);
		}
	}

	private void OnEnable()
	{
		if (!isUnderwaterCreature)
		{
			AnimalManager.manage.lookAtBugBook.AddListener(openBook);
		}
	}

	public void resetBugTypeNewDay()
	{
		getBugType();
	}

	public override void OnStartServer()
	{
		getBugType();
		WorldManager.Instance.changeDayEvent.AddListener(resetBugTypeNewDay);
	}

	public void openBook()
	{
		if (bugType < 0)
		{
			myNameTag.turnOff();
		}
		else if (base.gameObject.activeSelf && AnimalManager.manage.bugBookOpen)
		{
			if (bugType >= 0 && PediaManager.manage.isInPedia(bugType))
			{
				myNameTag.turnOn("<sprite=11>" + Inventory.Instance.allItems[bugType].value + "\n" + Inventory.Instance.allItems[bugType].getInvItemName());
			}
			else
			{
				myNameTag.turnOn("<sprite=11>????\n?????");
			}
			myNameTag.transform.parent = null;
			myNameTag.transform.localScale = Vector3.one;
			myNameTag.transform.parent = base.transform;
		}
		else
		{
			myNameTag.turnOff();
		}
	}

	public override void OnStartClient()
	{
		if (bugType != -1)
		{
			setUpBug(bugType, bugType);
		}
	}

	public int getBugTypeId()
	{
		return bugType;
	}

	private void getBugType()
	{
		int num = (int)base.transform.position.x / 2;
		int num2 = (int)base.transform.position.z / 2;
		_ = WorldManager.Instance.tileTypeMap[num, num2];
		int num3 = GenerateMap.generate.checkBiomType(num, num2);
		InventoryItem inventoryItem;
		if (!isUnderwaterCreature)
		{
			if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.BountifulWish) && RealWorldTimeLight.time.currentHour != 0 && Random.Range(0, 35) == 5)
			{
				Networksparkling = true;
			}
			else
			{
				Networksparkling = false;
			}
			if (RealWorldTimeLight.time.offIsland)
			{
				inventoryItem = AnimalManager.manage.reefIslandBugs.getInventoryItem();
			}
			else if (RealWorldTimeLight.time.underGround)
			{
				inventoryItem = AnimalManager.manage.underGroundBugs.getInventoryItem();
			}
			else
			{
				switch (num3)
				{
				case 1:
					inventoryItem = AnimalManager.manage.topicalBugs.getInventoryItem();
					break;
				case 2:
				case 3:
					inventoryItem = AnimalManager.manage.bushlandBugs.getInventoryItem();
					break;
				case 4:
					inventoryItem = AnimalManager.manage.bushlandBugs.getInventoryItem();
					break;
				case 5:
					inventoryItem = AnimalManager.manage.desertBugs.getInventoryItem();
					break;
				case 6:
					inventoryItem = AnimalManager.manage.pineLandBugs.getInventoryItem();
					break;
				case 7:
					inventoryItem = AnimalManager.manage.plainsBugs.getInventoryItem();
					break;
				default:
					inventoryItem = AnimalManager.manage.bushlandBugs.getInventoryItem();
					break;
				}
			}
		}
		else
		{
			Networksparkling = false;
			inventoryItem = (RealWorldTimeLight.time.offIsland ? AnimalManager.manage.offIslandUnderWaterCreatures.getInventoryItem() : ((WorldManager.Instance.tileTypeMap[num, num2] != 3 && num3 != 8 && num3 != 9 && num3 != 10 && num3 != 11) ? AnimalManager.manage.underWaterRiverCreatures.getInventoryItem() : AnimalManager.manage.underWaterOceanCreatures.getInventoryItem()));
		}
		if (!isUnderwaterCreature && !isASittingBug && inventoryItem.bug.sitsOnTileObject)
		{
			NetworkNavMesh.nav.SpawnASittingBug(inventoryItem, base.transform.position);
			NetworkNavMesh.nav.UnSpawnAnAnimal(myAi, saveToMap: false);
		}
		NetworkbugType = Inventory.Instance.getInvItemId(inventoryItem);
	}

	private void connectBugAnim()
	{
		if (!myBugModel)
		{
			return;
		}
		AnimateAnimalAI component = GetComponent<AnimateAnimalAI>();
		if ((bool)component)
		{
			component.setAnimator(myBugModel.GetComponentInChildren<Animator>());
		}
		else
		{
			GetComponent<AnimateSittingBug>().setAnimator(myBugModel.GetComponentInChildren<Animator>());
		}
		if (isUnderwaterCreature)
		{
			return;
		}
		if (sparkling)
		{
			Transform transform = myBugModel.GetComponentInChildren<BoxCollider>().transform;
			if (transform != null)
			{
				sparklingParticles.transform.position = transform.position;
			}
			else
			{
				sparklingParticles.transform.localPosition = Vector3.zero;
			}
			sparklingParticles.SetActive(value: true);
		}
		else
		{
			sparklingParticles.SetActive(value: false);
		}
	}

	public bool IsSparkling()
	{
		return sparkling;
	}

	private IEnumerator checkForBugInWater()
	{
		while ((bool)groundBugTrans)
		{
			if (groundBugTrans.position.y <= 0.6f && WorldManager.Instance.waterMap[Mathf.RoundToInt(groundBugTrans.position.x / 2f), Mathf.RoundToInt(groundBugTrans.position.z / 2f)])
			{
				NetworkMapSharer.Instance.RpcSplashInWater(groundBugTrans.position);
				NetworkNavMesh.nav.UnSpawnAnAnimal(GetComponent<AnimalAI>(), saveToMap: false);
				break;
			}
			yield return null;
		}
	}

	public void setUpBug(int old, int newBugType)
	{
		NetworkbugType = newBugType;
		if (bugType == -1)
		{
			return;
		}
		if ((bool)myBugModel)
		{
			Object.Destroy(myBugModel);
		}
		if (!isUnderwaterCreature)
		{
			myBugModel = Object.Instantiate(Inventory.Instance.allItems[bugType].bug.insectType, base.transform);
			myBugModel.transform.localPosition = Vector3.zero;
			Invoke("connectBugAnim", 0.25f);
			groundBugTrans = myBugModel.GetComponent<BugAppearance>().setUpBug(Inventory.Instance.allItems[bugType]);
			if ((bool)groundBugTrans && base.isServer && !isASittingBug)
			{
				StartCoroutine(checkForBugInWater());
			}
			if ((bool)Inventory.Instance.allItems[bugType].bug.bugSounds)
			{
				bugSoundSource.clip = Inventory.Instance.allItems[bugType].bug.bugSounds.getSound();
				bugSoundSource.pitch = Inventory.Instance.allItems[bugType].bug.bugSounds.getPitch();
				bugSoundSource.volume = Inventory.Instance.allItems[bugType].bug.bugSounds.volume * SoundManager.Instance.GetGlobalSoundVolume();
				bugSoundSource.loop = Inventory.Instance.allItems[bugType].bug.bugSounds.loop;
				bugSoundSource.maxDistance = Inventory.Instance.allItems[bugType].bug.bugSounds.maxDistance;
				bugSoundSource.enabled = true;
				if (Inventory.Instance.allItems[bugType].bug.bugSounds.loop && base.gameObject.activeInHierarchy)
				{
					bugSoundSource.Play();
					StartCoroutine(changeBugSound());
				}
			}
			else
			{
				bugSoundSource.enabled = false;
			}
			if (Inventory.Instance.allItems[bugType].bug.glows)
			{
				bugLight.gameObject.SetActive(value: true);
			}
			else
			{
				bugLight.gameObject.SetActive(value: false);
			}
			if ((bool)myAi)
			{
				myAi.setBaseSpeed(Inventory.Instance.allItems[bugType].bug.bugBaseSpeed);
				myAi.setAttackType(Inventory.Instance.allItems[bugType].bug.attacksWhenClose);
			}
			if (Inventory.Instance.allItems[bugType].bug.attacksWhenClose)
			{
				if ((bool)myAi)
				{
					myAi.isSkiddish = false;
				}
				myAttackBox.gameObject.SetActive(value: true);
				if (Inventory.Instance.allItems[bugType].bug.poisonOnAttack)
				{
					myAttackBox.poisonDamage = true;
				}
				else
				{
					myAttackBox.poisonDamage = false;
				}
			}
			else
			{
				if ((bool)myAi)
				{
					myAi.isSkiddish = true;
				}
				myAttackBox.gameObject.SetActive(value: false);
			}
		}
		else
		{
			myBugModel = Object.Instantiate(Inventory.Instance.allItems[bugType].underwaterCreature.creatureModel, base.transform);
			myBugModel.transform.localPosition = Vector3.zero;
			Invoke("connectBugAnim", 0.25f);
			groundBugTrans = myBugModel.GetComponent<BugAppearance>().setUpBug(Inventory.Instance.allItems[bugType]);
			if ((bool)groundBugTrans && base.isServer)
			{
				StartCoroutine(checkForBugInWater());
			}
			bugSoundSource.enabled = false;
			if ((bool)myAi)
			{
				myAi.setBaseSpeed(Inventory.Instance.allItems[bugType].underwaterCreature.baseSpeed);
			}
		}
		if (!isUnderwaterCreature)
		{
			if ((bool)myNameTag)
			{
				myNameTag.enableMeshes();
			}
			openBook();
		}
	}

	public void playBugNoiseAnimator()
	{
		if ((bool)Inventory.Instance.allItems[bugType].bug.bugSounds && !Inventory.Instance.allItems[bugType].bug.bugSounds.loop)
		{
			bugSoundSource.PlayOneShot(Inventory.Instance.allItems[bugType].bug.bugSounds.getSound());
		}
	}

	public InventoryItem bugInvItem()
	{
		return Inventory.Instance.allItems[bugType];
	}

	private IEnumerator changeBugSound()
	{
		int startedWithBugId = bugType;
		while (bugType == startedWithBugId)
		{
			yield return soundWait;
			if ((bool)Inventory.Instance.allItems[bugType].bug.bugSounds)
			{
				bugSoundSource.volume = Inventory.Instance.allItems[bugType].bug.bugSounds.volume * SoundManager.Instance.GetGlobalSoundVolume();
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(bugType);
			writer.WriteBool(sparkling);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(bugType);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(sparkling);
			result = true;
		}
		return result;
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = bugType;
			NetworkbugType = reader.ReadInt();
			if (!SyncVarEqual(num, ref bugType))
			{
				setUpBug(num, bugType);
			}
			bool flag = sparkling;
			Networksparkling = reader.ReadBool();
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = bugType;
			NetworkbugType = reader.ReadInt();
			if (!SyncVarEqual(num3, ref bugType))
			{
				setUpBug(num3, bugType);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag2 = sparkling;
			Networksparkling = reader.ReadBool();
		}
	}
}
