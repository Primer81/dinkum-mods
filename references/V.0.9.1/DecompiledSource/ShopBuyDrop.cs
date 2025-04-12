using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

public class ShopBuyDrop : MonoBehaviour
{
	public ShopManager.stallTypes myStallType;

	public int shopStallNo = -1;

	public int myItemId;

	private int previouslyStocked;

	public InventoryItem onlySells;

	public InventoryItemLootTable canSell;

	public GameObject spriteDrop;

	private GameObject itemPrefab;

	public bool gives10;

	public bool gives5;

	public bool disapearwhenBuy = true;

	public Transform createObjectHere;

	public Transform FourSquareFurniture;

	public Transform TwoSquareFurniture;

	public Transform TwoSquareFurnitureVert;

	public MeshRenderer objectMaterialHere;

	public bool hatsOnly;

	public bool faceAccessoriesOnly;

	public bool shirtOnly;

	public bool pantsOnly;

	public bool shoesOnly;

	public bool furnitureOnly;

	public bool furnitureOnTop;

	public bool flooringOnly;

	public bool wallPaperOnly;

	public bool recipesOnly;

	public bool sellsSeasonsCrops;

	public bool usesPermitPoints;

	public int priceMultiplier = 1;

	public NPCSchedual.Locations insideShop;

	public AnimalAI sellsAnimal;

	public GameObject dummyAnimal;

	public CopyShopItem copyTo;

	public TextMeshPro priceTag;

	public ConversationObject uniqueConvo;

	public ConversationObject closedConvo;

	private NetStall myNetStall;

	public void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isShopBuyDrop = this;
		if ((bool)sellsAnimal)
		{
			WorldManager.Instance.changeDayEvent.AddListener(refreshAnimal);
			return;
		}
		if (!onlySells)
		{
			WorldManager.Instance.changeDayEvent.AddListener(changeItem);
			changeItem();
			return;
		}
		WorldManager.Instance.changeDayEvent.AddListener(refreshItem);
		if ((bool)onlySells)
		{
			myItemId = Inventory.Instance.getInvItemId(onlySells);
		}
		createItem(myItemId);
		previouslyStocked = myItemId;
	}

	private void OnEnable()
	{
		Invoke("refreshConnectionToShopManager", Random.Range(0.1f, 0.25f));
		if ((bool)createObjectHere)
		{
			createObjectHere.gameObject.SetActive(value: true);
		}
		if ((bool)priceTag)
		{
			LocalizationManager.OnLocalizeEvent += OnChangeLanguage;
		}
	}

	private void OnChangeLanguage()
	{
		priceTag.text = "<color=red>" + ConversationGenerator.generate.GetToolTip("Tip_Sold") + "</color>";
	}

	private void OnDisable()
	{
		if ((bool)createObjectHere)
		{
			createObjectHere.gameObject.SetActive(value: false);
		}
		if ((bool)priceTag)
		{
			LocalizationManager.OnLocalizeEvent -= OnChangeLanguage;
		}
	}

	public int getRecipePrice()
	{
		return 5000;
	}

	private void refreshConnectionToShopManager()
	{
		if (shopStallNo == -1)
		{
			return;
		}
		myNetStall = ShopManager.manage.connectStall(this);
		if (myNetStall.hasBeenSold)
		{
			sold(partsShown: false);
		}
		else
		{
			if ((bool)sellsAnimal)
			{
				return;
			}
			if (!onlySells)
			{
				changeItem();
				return;
			}
			if ((bool)onlySells)
			{
				myItemId = Inventory.Instance.getInvItemId(onlySells);
			}
			Object.Destroy(itemPrefab);
			createItem(myItemId);
			previouslyStocked = myItemId;
		}
	}

	public void generateRandomItem()
	{
		bool flag = false;
		if (shirtOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomShirtOrDressForShop().getItemId();
			flag = true;
		}
		if (hatsOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomHat().getItemId();
			flag = true;
		}
		if (pantsOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomPants().getItemId();
			flag = true;
		}
		if (shoesOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomShoes().getItemId();
			flag = true;
		}
		if (faceAccessoriesOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomFaceItem().getItemId();
			flag = true;
		}
		if (furnitureOnly)
		{
			if (furnitureOnTop)
			{
				myItemId = RandomObjectGenerator.generate.getRandomOnTopFurniture().getItemId();
				flag = true;
			}
			else
			{
				myItemId = RandomObjectGenerator.generate.getRandomFurnitureForShop().getItemId();
				flag = true;
			}
		}
		if (flooringOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomFlooring().getItemId();
			flag = true;
		}
		if (wallPaperOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomWallPaper().getItemId();
			flag = true;
		}
		while (!flag)
		{
			myItemId = Random.Range(0, Inventory.Instance.allItems.Length);
			if (recipesOnly)
			{
				if ((bool)Inventory.Instance.allItems[myItemId].craftable && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.CookingTable && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.RaffleBox && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.CraftingShop && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.Blocked && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.NickShop && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.KiteTable && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.SkyFestRaffleBox && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.IceCraftingTable && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.TrapperShop && Inventory.Instance.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.AgentCrafting && !Inventory.Instance.allItems[myItemId].craftable.isDeed && !Inventory.Instance.allItems[myItemId].craftable.learnThroughQuest && !Inventory.Instance.allItems[myItemId].craftable.learnThroughLevels && !Inventory.Instance.allItems[myItemId].craftable.learnThroughLicence && !CharLevelManager.manage.checkIfIsInStartingRecipes(myItemId))
				{
					flag = true;
				}
			}
			else if (wallPaperOnly || flooringOnly)
			{
				if ((bool)Inventory.Instance.allItems[myItemId].equipable && ((flooringOnly && Inventory.Instance.allItems[myItemId].equipable.flooring) || (wallPaperOnly && Inventory.Instance.allItems[myItemId].equipable.wallpaper)))
				{
					flag = true;
				}
			}
			else if (furnitureOnly)
			{
				if (Inventory.Instance.allItems[myItemId].isFurniture)
				{
					flag = true;
				}
			}
			else if (hatsOnly)
			{
				if ((bool)Inventory.Instance.allItems[myItemId].equipable && Inventory.Instance.allItems[myItemId].equipable.hat)
				{
					flag = true;
				}
			}
			else if (faceAccessoriesOnly)
			{
				if ((bool)Inventory.Instance.allItems[myItemId].equipable && Inventory.Instance.allItems[myItemId].equipable.face)
				{
					flag = true;
				}
			}
			else if (shoesOnly || pantsOnly || shirtOnly)
			{
				if (shoesOnly && (bool)Inventory.Instance.allItems[myItemId].equipable && Inventory.Instance.allItems[myItemId].equipable.shoes)
				{
					flag = true;
				}
				if (pantsOnly && (bool)Inventory.Instance.allItems[myItemId].equipable && Inventory.Instance.allItems[myItemId].equipable.pants)
				{
					flag = true;
				}
				if (shirtOnly && (bool)Inventory.Instance.allItems[myItemId].equipable && Inventory.Instance.allItems[myItemId].equipable.shirt)
				{
					flag = true;
				}
			}
			else if (Inventory.Instance.allItems[myItemId].isATool)
			{
				flag = true;
			}
		}
	}

	public void createItem(int itemId)
	{
		myItemId = itemId;
		if ((bool)createObjectHere || furnitureOnly)
		{
			if (furnitureOnly && Inventory.Instance.allItems[itemId].placeable.IsMultiTileObject())
			{
				Transform parent;
				if (Inventory.Instance.allItems[itemId].placeable.GetXSize() == 2 && Inventory.Instance.allItems[itemId].placeable.GetYSize() == 2)
				{
					parent = FourSquareFurniture;
					FourSquareFurniture.gameObject.SetActive(value: true);
					createObjectHere.gameObject.SetActive(value: false);
					TwoSquareFurniture.gameObject.SetActive(value: false);
					TwoSquareFurnitureVert.gameObject.SetActive(value: false);
				}
				else if (Inventory.Instance.allItems[itemId].placeable.GetXSize() == 2)
				{
					parent = TwoSquareFurniture;
					FourSquareFurniture.gameObject.SetActive(value: false);
					createObjectHere.gameObject.SetActive(value: false);
					TwoSquareFurniture.gameObject.SetActive(value: true);
					TwoSquareFurnitureVert.gameObject.SetActive(value: false);
				}
				else
				{
					parent = TwoSquareFurnitureVert;
					FourSquareFurniture.gameObject.SetActive(value: false);
					createObjectHere.gameObject.SetActive(value: false);
					TwoSquareFurniture.gameObject.SetActive(value: false);
					TwoSquareFurnitureVert.gameObject.SetActive(value: true);
				}
				itemPrefab = Object.Instantiate(Inventory.Instance.allItems[itemId].placeable.gameObject, parent);
			}
			else if (furnitureOnly)
			{
				FourSquareFurniture.gameObject.SetActive(value: false);
				createObjectHere.gameObject.SetActive(value: true);
				TwoSquareFurniture.gameObject.SetActive(value: false);
				itemPrefab = Object.Instantiate(Inventory.Instance.allItems[itemId].placeable.gameObject, createObjectHere);
			}
			else if (hatsOnly || faceAccessoriesOnly)
			{
				itemPrefab = Object.Instantiate(Inventory.Instance.allItems[itemId].equipable.hatPrefab, createObjectHere);
			}
			else if ((bool)Inventory.Instance.allItems[itemId].altDropPrefab)
			{
				itemPrefab = Object.Instantiate(Inventory.Instance.allItems[itemId].altDropPrefab, createObjectHere);
			}
			else
			{
				itemPrefab = Object.Instantiate(Inventory.Instance.allItems[itemId].itemPrefab, createObjectHere);
			}
			if (furnitureOnly && (bool)itemPrefab.GetComponent<FurnitureStatus>())
			{
				FurnitureStatus component = itemPrefab.GetComponent<FurnitureStatus>();
				if ((bool)component.seatPosition1)
				{
					Object.Destroy(component.seatPosition1.gameObject);
				}
				if ((bool)component.seatPosition2)
				{
					Object.Destroy(component.seatPosition2.gameObject);
				}
				Object.Destroy(component);
			}
			if ((bool)itemPrefab.GetComponent<Animator>())
			{
				Object.Destroy(itemPrefab.GetComponent<Animator>());
			}
			if ((bool)itemPrefab.GetComponent<SetItemTexture>())
			{
				itemPrefab.GetComponent<SetItemTexture>().setTexture(Inventory.Instance.allItems[itemId]);
			}
			Light[] componentsInChildren = itemPrefab.GetComponentsInChildren<Light>();
			if (componentsInChildren.Length != 0)
			{
				Light[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = false;
				}
			}
			itemPrefab.transform.localPosition = Vector3.zero;
			itemPrefab.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else if ((bool)objectMaterialHere)
		{
			if (shirtOnly && Inventory.Instance.allItems[myItemId].equipable.dress)
			{
				if (Inventory.Instance.allItems[myItemId].equipable.longDress)
				{
					objectMaterialHere.gameObject.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultLongDress;
				}
				else
				{
					objectMaterialHere.gameObject.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultDress;
				}
			}
			else if (shirtOnly && (bool)Inventory.Instance.allItems[myItemId].equipable.shirtMesh)
			{
				objectMaterialHere.gameObject.GetComponent<MeshFilter>().mesh = Inventory.Instance.allItems[myItemId].equipable.shirtMesh;
				if ((bool)copyTo)
				{
					copyTo.changeMesh(Inventory.Instance.allItems[myItemId].equipable.shirtMesh);
				}
			}
			else if (shirtOnly)
			{
				objectMaterialHere.gameObject.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultShirtMesh;
			}
			objectMaterialHere.material = Inventory.Instance.allItems[myItemId].equipable.material;
			objectMaterialHere.enabled = true;
			if ((bool)copyTo)
			{
				copyTo.changeMaterial(Inventory.Instance.allItems[myItemId].equipable.material);
			}
		}
		else if ((bool)createObjectHere)
		{
			if (hatsOnly || faceAccessoriesOnly)
			{
				itemPrefab = Object.Instantiate(Inventory.Instance.allItems[itemId].equipable.hatPrefab, createObjectHere);
			}
			else if ((bool)Inventory.Instance.allItems[itemId].altDropPrefab)
			{
				itemPrefab = Object.Instantiate(Inventory.Instance.allItems[itemId].altDropPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, 0f), createObjectHere);
			}
			else
			{
				itemPrefab = Object.Instantiate(Inventory.Instance.allItems[itemId].itemPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, 0f), createObjectHere);
				Object.Destroy(itemPrefab.GetComponent<Animator>());
			}
		}
		else
		{
			itemPrefab = Object.Instantiate(spriteDrop, base.transform);
			itemPrefab.GetComponent<SpriteLookAtCam>().changeSprite(itemId);
			if (recipesOnly)
			{
				itemPrefab.GetComponent<SpriteLookAtCam>().setAsBluePrint();
			}
			itemPrefab.transform.localPosition = new Vector3(0f, 0f, 1.7f);
		}
		if ((bool)priceTag)
		{
			priceTag.text = ((float)Inventory.Instance.allItems[myItemId].value * 2f).ToString("n0");
		}
	}

	private void changeItem()
	{
		Object.Destroy(itemPrefab);
		Random.InitState(NetworkMapSharer.Instance.mineSeed + (int)base.transform.position.x + (int)base.transform.position.z + (int)base.transform.position.y + (int)((float)((int)base.transform.position.x / (int)base.transform.position.z) + base.transform.position.y) + WorldManager.Instance.day * WorldManager.Instance.week + WorldManager.Instance.year);
		if ((bool)canSell)
		{
			if (sellsSeasonsCrops)
			{
				List<InventoryItem> list = new List<InventoryItem>();
				for (int i = 0; i < ShopManager.manage.allSeeds.Count; i++)
				{
					if (ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.canAppearInShops)
					{
						if (WorldManager.Instance.month == 1 && ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.growsInSummer)
						{
							list.Add(ShopManager.manage.allSeeds[i]);
						}
						else if (WorldManager.Instance.month == 2 && ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.growsInAutum)
						{
							list.Add(ShopManager.manage.allSeeds[i]);
						}
						if (WorldManager.Instance.month == 3 && ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.growsInWinter)
						{
							list.Add(ShopManager.manage.allSeeds[i]);
						}
						if (WorldManager.Instance.month == 4 && ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.growsInSpring)
						{
							list.Add(ShopManager.manage.allSeeds[i]);
						}
					}
				}
				canSell.autoFillFromArray(list.ToArray());
			}
			myItemId = Inventory.Instance.getInvItemId(canSell.getRandomDropFromTable());
		}
		else
		{
			generateRandomItem();
		}
		createItem(myItemId);
	}

	private void refreshItem()
	{
		if (myItemId == -1)
		{
			myItemId = previouslyStocked;
			Object.Destroy(itemPrefab);
			createItem(myItemId);
		}
	}

	public void TryAndBuyItem()
	{
		if ((canTalkToKeeper() && myItemId != -1) || (canTalkToKeeper() && (bool)sellsAnimal && dummyAnimal.activeSelf))
		{
			if (canTalkToKeeper() && !isKeeperWorking() && (bool)closedConvo)
			{
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, closedConvo);
			}
			else if ((bool)uniqueConvo)
			{
				GiveNPC.give.dropToBuy = this;
				ConversationManager.manage.TalkToNPC(NPCManager.manage.getVendorNPC(insideShop), uniqueConvo);
			}
			else if ((bool)sellsAnimal)
			{
				GiveNPC.give.askAboutBuyingAnimal(this, NPCManager.manage.getVendorNPC(insideShop));
			}
			else if (recipesOnly)
			{
				GiveNPC.give.askAboutBuyingRecipe(this, NPCManager.manage.getVendorNPC(insideShop));
			}
			else
			{
				GiveNPC.give.askAboutBuyingSomething(this, NPCManager.manage.getVendorNPC(insideShop));
			}
		}
	}

	public bool isKeeperWorking()
	{
		if (!NPCManager.manage.getVendorNPC(insideShop))
		{
			return false;
		}
		return NPCManager.manage.getVendorNPC(insideShop).isAtWork();
	}

	public bool canTalkToKeeper()
	{
		if (!NPCManager.manage.getVendorNPC(insideShop))
		{
			if ((bool)closedConvo)
			{
				return true;
			}
			return false;
		}
		return NPCManager.manage.getVendorNPC(insideShop).IsValidConversationTargetForAnyPlayer();
	}

	public void checkIfTaskCompelted(int amount = 1)
	{
		if (furnitureOnly)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyFurniture, amount);
		}
		else if (shirtOnly || hatsOnly || pantsOnly || shoesOnly || faceAccessoriesOnly)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyShirt, amount);
		}
		else if (wallPaperOnly)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyWallpaper, amount);
		}
		else if (flooringOnly)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyFlooring, amount);
		}
		else if (!recipesOnly)
		{
			if (myItemId > -1 && (bool)Inventory.Instance.allItems[myItemId].placeable && (bool)Inventory.Instance.allItems[myItemId].placeable.tileObjectGrowthStages && Inventory.Instance.allItems[myItemId].placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuySeeds, amount);
			}
			else if (myItemId > -1 && Inventory.Instance.allItems[myItemId].isATool)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyATool);
			}
		}
	}

	public void buyTheItem()
	{
		if (disapearwhenBuy)
		{
			if (shopStallNo > -1)
			{
				NetworkMapSharer.Instance.localChar.CmdBuyItemFromStall((int)myStallType, shopStallNo);
			}
			sold();
		}
	}

	public void sold(bool partsShown = true)
	{
		if ((bool)sellsAnimal && (bool)dummyAnimal)
		{
			dummyAnimal.SetActive(value: false);
		}
		if ((bool)objectMaterialHere)
		{
			objectMaterialHere.enabled = false;
			if ((bool)copyTo)
			{
				copyTo.disable();
			}
		}
		else
		{
			Object.Destroy(itemPrefab);
		}
		myItemId = -1;
		if (partsShown && (bool)createObjectHere)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], createObjectHere.position, 10);
		}
		if ((bool)priceTag)
		{
			priceTag.text = "<color=red>" + ConversationGenerator.generate.GetToolTip("Tip_Sold") + "</color>";
		}
	}

	public void refreshAnimal()
	{
		dummyAnimal.SetActive(value: true);
	}

	public int getSellPrice()
	{
		if (priceMultiplier == 1)
		{
			return Inventory.Instance.allItems[myItemId].value * 2;
		}
		return Inventory.Instance.allItems[myItemId].value * 2 * priceMultiplier;
	}
}
