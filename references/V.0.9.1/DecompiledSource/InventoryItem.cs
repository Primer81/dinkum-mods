using I2.Loc;
using UnityEngine;

public class InventoryItem : MonoBehaviour
{
	public enum staminaType
	{
		None,
		Farming,
		Foraging,
		Mining,
		Fishing,
		BugCatching,
		Hunting
	}

	public enum typeOfAnimation
	{
		ShovelAnimation,
		Pickaxe,
		Axe,
		BugNet,
		FishingRod,
		Bat,
		Scyth,
		Spear,
		MetalDetector,
		Hammer,
		WateringCan,
		UpgradedWateringCan,
		UpgradedHoe,
		Knife,
		Glider,
		UpgradedScyth,
		Whip
	}

	public new string tag;

	public string itemName;

	public string itemDescription;

	public int value;

	public Sprite itemSprite;

	public GameObject itemPrefab;

	public GameObject altDropPrefab;

	[Header("Special Options-------")]
	public InventoryItem changeToWhenUsed;

	public bool changeToAndStillUseFuel;

	public bool hideHighlighter;

	[Header("Animation Settings")]
	public bool hasUseAnimationStance = true;

	public bool useRightHandAnim;

	public typeOfAnimation myAnimType;

	[Header("Placeable --------------")]
	public TileObject placeable;

	public bool burriedPlaceable;

	public bool ignoreOnTileObject;

	public int[] canBePlacedOntoTileType;

	public int placeableTileType = -1;

	[Header("Placeable On To OTher Tile Object --------------")]
	public TileObject[] canBePlacedOnToTileObject;

	public int statusToChangeToWhenPlacedOnTop;

	[Header("Item Type --------------")]
	public bool isStackable = true;

	public int maxStack = -1;

	public bool isATool;

	public bool isPowerTool;

	public bool isFurniture;

	public bool canBePlacedInHouse;

	public bool canBeUsedInShops;

	public bool isRequestable;

	public bool isUniqueItem;

	public bool isOneOfKindUniqueItem;

	public bool ignoreDurabilityBuff;

	[Header("Fuel and Stamina Options-------")]
	public staminaType staminaTypeUse;

	public bool hasFuel;

	public int fuelMax;

	public int fuelOnUse = 5;

	public Color customFuelColour;

	[Header("Weapon Info --------------")]
	public float weaponDamage = 1f;

	public float weaponKnockback = 2.5f;

	public bool canBlock;

	[Header("Damage Tile Object Info --------------")]
	public float damagePerAttack = 1f;

	public bool damageWood;

	public bool damageHardWood;

	public bool damageMetal;

	public bool damageStone;

	public bool damageHardStone;

	public bool damageSmallPlants;

	public int changeToHeightTiles;

	public bool onlyChangeHeightPaths = true;

	public bool anyHeight;

	[Header("Damage Tile Types --------------")]
	public bool grassGrowable;

	public bool canDamagePath;

	public bool canDamageDirt;

	public bool canDamageStone;

	public bool canDamageTilledDirt;

	public bool canDamageWetTilledDirt;

	public bool canDamageFertilizedSoil;

	public bool canDamageWetFertilizedSoil;

	public bool placeOnWaterOnly;

	public int[] resultingTileType;

	public bool ignoreTwoArmAnim;

	public bool isDeed;

	[Header("Other Settings---------")]
	public bool hasColourVariation;

	public bool isRepairable;

	public bool canUseUnderWater;

	[Header("Spawn a world object or vehicle ----")]
	public GameObject spawnPlaceable;

	[Header("Milestone & Licence ----")]
	public LicenceManager.LicenceTypes requiredToBuy;

	public int requiredLicenceLevel = 1;

	public DailyTaskGenerator.genericTaskType assosiatedTask;

	public DailyTaskGenerator.genericTaskType taskWhenSold;

	[Header("Other Scripts --------------")]
	public Equipable equipable;

	public Recipe craftable;

	public Consumeable consumeable;

	public ItemChange itemChange;

	public BugIdentity bug;

	public FishIdentity fish;

	public UnderWaterCreature underwaterCreature;

	public Relic relic;

	private int itemId = -1;

	public bool showInCreativeMenu = true;

	public void setItemId(int setTo)
	{
		itemId = setTo;
	}

	public void setFurnitureSprite(int newId)
	{
		itemSprite = EquipWindow.equip.furnitureSprites[newId];
	}

	public void setClothingSprite(int newId)
	{
		if (!equipable.useOwnSprite)
		{
			itemSprite = EquipWindow.equip.clothingSprites[newId];
		}
	}

	public int getItemId()
	{
		if (itemId == -1)
		{
			int num = Inventory.Instance.itemIdBackUp(this);
			if (num != -1)
			{
				return num;
			}
			Debug.LogError("Item Id of -1 was given");
		}
		return itemId;
	}

	public string getLicenceLevelText()
	{
		if (requiredLicenceLevel <= 1)
		{
			return "";
		}
		return string.Format(LicenceManager.manage.GetLicenceStatusDesc("LicenceLevelAbbreviated"), requiredLicenceLevel);
	}

	public bool checkIfCanBuy()
	{
		if (requiredToBuy == LicenceManager.LicenceTypes.None)
		{
			return true;
		}
		if (LicenceManager.manage.allLicences[(int)requiredToBuy].getCurrentLevel() >= requiredLicenceLevel)
		{
			return true;
		}
		return false;
	}

	public Sprite getSprite()
	{
		return itemSprite;
	}

	public bool checkIfStackable()
	{
		if (isStackable && !isATool && !hasColourVariation && !hasFuel)
		{
			return true;
		}
		return false;
	}

	public bool canDamageTileTypes()
	{
		if (canDamageStone || canDamagePath || canDamageDirt || canDamageTilledDirt)
		{
			return true;
		}
		return false;
	}

	public int getResultingPlaceableTileType(int placingOnToType)
	{
		if (resultingTileType.Length != 0)
		{
			for (int i = 0; i < resultingTileType.Length; i++)
			{
				if (canBePlacedOntoTileType[i] == placingOnToType)
				{
					return resultingTileType[i];
				}
			}
			return 0;
		}
		return placeableTileType;
	}

	public string getInvItemName(int amountOfItem = 1)
	{
		LocalizedString localizedString = "InventoryItemNames/InvItem_" + Inventory.Instance.getInvItemId(this);
		if ((string)localizedString == null)
		{
			return itemName;
		}
		return LocalisationMarkUp.ProcessNameTag(localizedString, amountOfItem);
	}

	public LocalisationMarkUp.LanguageGender GetLanguageGender()
	{
		LocalizedString localizedString = "InventoryItemNames/InvItem_" + Inventory.Instance.getInvItemId(this);
		if ((string)localizedString == null)
		{
			return LocalisationMarkUp.LanguageGender.neutral;
		}
		if (localizedString.ToString().Contains("{M}"))
		{
			return LocalisationMarkUp.LanguageGender.masculine;
		}
		if (localizedString.ToString().Contains("{F}"))
		{
			return LocalisationMarkUp.LanguageGender.feminine;
		}
		return LocalisationMarkUp.LanguageGender.neutral;
	}

	public string getItemDescription(int itemId)
	{
		LocalizedString localizedString = "InventoryItemDescriptions/InvDesc_" + itemId;
		if ((string)localizedString == null)
		{
			return itemDescription;
		}
		return localizedString;
	}

	public float getStaminaCost()
	{
		return CharLevelManager.manage.getStaminaCost((int)(staminaTypeUse - 1));
	}

	public void checkForTask()
	{
		if (assosiatedTask != 0)
		{
			DailyTaskGenerator.generate.doATask(assosiatedTask);
		}
	}
}
