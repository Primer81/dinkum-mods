using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class FishType : NetworkBehaviour
{
	[SyncVar(hook = "setFishType")]
	private int myFishType = -1;

	[SyncVar]
	private bool sparkling;

	public SkinnedMeshRenderer fishRen;

	public Transform fishSizeTrans;

	public FishType fishTypeForDummy;

	public GameObject fishSparkle;

	private Mesh defualtMesh;

	public NameTag myNameTag;

	public int NetworkmyFishType
	{
		get
		{
			return myFishType;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref myFishType))
			{
				int old = myFishType;
				SetSyncVar(value, ref myFishType, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, value: true);
					setFishType(old, value);
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

	private void Awake()
	{
		defualtMesh = fishRen.sharedMesh;
	}

	public override void OnStartClient()
	{
		if (myFishType != -1)
		{
			setFishType(myFishType, myFishType);
			openBook();
			fishSparkle.SetActive(sparkling);
		}
	}

	public override void OnStartServer()
	{
		WorldManager.Instance.changeDayEvent.AddListener(resetFishTypeOnNewDay);
	}

	private void OnEnable()
	{
		AnimalManager.manage.lookAtFishBook.AddListener(openBook);
	}

	private void OnDisable()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(resetFishTypeOnNewDay);
		AnimalManager.manage.lookAtFishBook.RemoveListener(openBook);
	}

	public void resetFishTypeOnNewDay()
	{
		if (base.isServer)
		{
			generateFishForEnviroment();
		}
	}

	public int getFishTypeId()
	{
		return myFishType;
	}

	public void generateFishForEnviroment()
	{
		int num = (int)base.transform.position.x / 2;
		int num2 = (int)base.transform.position.z / 2;
		int num3 = WorldManager.Instance.tileTypeMap[num, num2];
		InventoryItem item = (RealWorldTimeLight.time.underGround ? AnimalManager.manage.undergroundFish.getInventoryItem() : (RealWorldTimeLight.time.offIsland ? AnimalManager.manage.reefIslandFish.getInventoryItem() : ((num3 == 14) ? AnimalManager.manage.mangroveFish.getInventoryItem() : ((GenerateMap.generate.checkBiomType(num, num2) == 11) ? ((Random.Range(0, 6) == 5 && CatchingCompetitionManager.manage.IsFishCompToday() && RealWorldTimeLight.time.currentHour > 0 && RealWorldTimeLight.time.currentHour < RealWorldTimeLight.time.getSunSetTime()) ? CatchingCompetitionManager.manage.desiredFishItem : ((num2 <= 500) ? AnimalManager.manage.southernOceanFish.getInventoryItem() : AnimalManager.manage.northernOceanFish.getInventoryItem())) : ((GenerateMap.generate.checkBiomType(num, num2) == 9) ? AnimalManager.manage.northernOceanFish.getInventoryItem() : ((GenerateMap.generate.checkBiomType(num, num2) == 10 || GenerateMap.generate.checkBiomType(num, num2) == 8) ? AnimalManager.manage.southernOceanFish.getInventoryItem() : ((GenerateMap.generate.checkBiomType(num, num2) != 2) ? AnimalManager.manage.riverFish.getInventoryItem() : AnimalManager.manage.billabongFish.getInventoryItem())))))));
		NetworkmyFishType = Inventory.Instance.getInvItemId(item);
		if (RealWorldTimeLight.time.currentHour == 0)
		{
			if (Inventory.Instance.allItems[myFishType].fish.mySeason.myRarity == SeasonAndTime.rarity.SuperRare && Random.Range(0, 30) != 4)
			{
				generateFishForEnviroment();
				return;
			}
			if (Inventory.Instance.allItems[myFishType].fish.mySeason.myRarity == SeasonAndTime.rarity.VeryRare && Random.Range(0, 25) != 2)
			{
				generateFishForEnviroment();
				return;
			}
			if (Inventory.Instance.allItems[myFishType].fish.mySeason.myRarity == SeasonAndTime.rarity.Rare && Random.Range(0, 20) != 2)
			{
				generateFishForEnviroment();
				return;
			}
		}
		else
		{
			if (Inventory.Instance.allItems[myFishType].fish.mySeason.myRarity == SeasonAndTime.rarity.SuperRare && Random.Range(0, 5) != 4)
			{
				generateFishForEnviroment();
				return;
			}
			if (Inventory.Instance.allItems[myFishType].fish.mySeason.myRarity == SeasonAndTime.rarity.VeryRare && Random.Range(0, 3) == 1)
			{
				generateFishForEnviroment();
				return;
			}
		}
		if (NetworkMapSharer.Instance.wishManager.IsWishActive(WishManager.WishType.BountifulWish) && RealWorldTimeLight.time.currentHour != 0 && Random.Range(0, 75) == 5)
		{
			Networksparkling = true;
		}
		else
		{
			Networksparkling = false;
		}
	}

	public bool IsSparkling()
	{
		return sparkling;
	}

	public void setFishType(int old, int fishType)
	{
		NetworkmyFishType = fishType;
		if (myFishType == -1)
		{
			return;
		}
		if (!fishTypeForDummy)
		{
			fishSizeTrans.localScale = Inventory.Instance.allItems[fishType].fish.fishScale();
			fishRen.material = Inventory.Instance.allItems[fishType].equipable.material;
			if ((bool)Inventory.Instance.allItems[fishType].equipable.useAltMesh)
			{
				fishRen.sharedMesh = Inventory.Instance.allItems[fishType].equipable.useAltMesh;
			}
			else
			{
				fishRen.sharedMesh = defualtMesh;
			}
		}
		else
		{
			fishSizeTrans.localScale = Inventory.Instance.allItems[fishType].fish.fishScale();
			fishRen.material = Inventory.Instance.allItems[fishType].equipable.material;
			if ((bool)Inventory.Instance.allItems[fishType].equipable.useAltMesh)
			{
				fishRen.sharedMesh = Inventory.Instance.allItems[fishType].equipable.useAltMesh;
			}
			else
			{
				fishRen.sharedMesh = defualtMesh;
			}
		}
		if ((bool)GetComponent<AnimateAnimalAI>())
		{
			GetComponent<AnimateAnimalAI>().setScaleSwimDif();
		}
		if ((bool)myNameTag)
		{
			myNameTag.enableMeshes();
		}
	}

	public InventoryItem getFishInvItem()
	{
		return Inventory.Instance.allItems[myFishType];
	}

	public int getFishInvId()
	{
		return myFishType;
	}

	public void openBook()
	{
		if (base.gameObject.activeSelf && AnimalManager.manage.fishBookOpen)
		{
			if (myFishType >= 0 && PediaManager.manage.isInPedia(myFishType))
			{
				myNameTag.turnOn("<sprite=11>" + Inventory.Instance.allItems[myFishType].value + "\n" + Inventory.Instance.allItems[myFishType].getInvItemName());
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

	private void MirrorProcessed()
	{
	}

	public override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(myFishType);
			writer.WriteBool(sparkling);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(myFishType);
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
			int num = myFishType;
			NetworkmyFishType = reader.ReadInt();
			if (!SyncVarEqual(num, ref myFishType))
			{
				setFishType(num, myFishType);
			}
			bool flag = sparkling;
			Networksparkling = reader.ReadBool();
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = myFishType;
			NetworkmyFishType = reader.ReadInt();
			if (!SyncVarEqual(num3, ref myFishType))
			{
				setFishType(num3, myFishType);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag2 = sparkling;
			Networksparkling = reader.ReadBool();
		}
	}
}
