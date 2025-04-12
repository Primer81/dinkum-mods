using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
	public enum stallTypes
	{
		JohnsGoods,
		Clothing,
		Furniture,
		Weapon,
		Animal,
		Plants,
		Crafting,
		Museum,
		Jimmy,
		Tuckshop,
		Marketplace,
		Airport,
		Bank
	}

	public static ShopManager manage;

	public bool shopsStarSync;

	public NetStall[] JohnsGoodsStalls;

	public NetStall[] ClothingStalls;

	public NetStall[] FurnitureStalls;

	public NetStall[] WeaponStalls;

	public NetStall[] AnimalStalls;

	public NetStall[] PlantStalls;

	public NetStall[] CrafterStalls;

	public NetStall[] MuseumStalls;

	public NetStall[] JimmyStalls;

	public NetStall[] TuckshopStalls;

	public NetStall[] MarketPlaceStalls;

	public NetStall[] AirPortStalls;

	public NetStall[] BankStall;

	public List<InventoryItem> allSeeds = new List<InventoryItem>();

	private List<NetStall[]> quickGetList = new List<NetStall[]>();

	private void Awake()
	{
		manage = this;
		refreshNewDay();
		quickGetList.Add(JohnsGoodsStalls);
		quickGetList.Add(ClothingStalls);
		quickGetList.Add(FurnitureStalls);
		quickGetList.Add(WeaponStalls);
		quickGetList.Add(AnimalStalls);
		quickGetList.Add(PlantStalls);
		quickGetList.Add(CrafterStalls);
		quickGetList.Add(MuseumStalls);
		quickGetList.Add(JimmyStalls);
		quickGetList.Add(TuckshopStalls);
		quickGetList.Add(MarketPlaceStalls);
		quickGetList.Add(AirPortStalls);
		quickGetList.Add(BankStall);
	}

	private void Start()
	{
		WorldManager.Instance.changeDayEvent.AddListener(refreshNewDay);
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].placeable && (bool)Inventory.Instance.allItems[i].placeable.tileObjectGrowthStages && Inventory.Instance.allItems[i].placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				allSeeds.Add(Inventory.Instance.allItems[i]);
			}
		}
	}

	public void sellStall(int type, int id)
	{
		quickGetList[type][id].hasBeenSold = true;
		quickGetList[type][id].sellIfConnected();
	}

	public NetStall connectStall(ShopBuyDrop toConnect)
	{
		quickGetList[(int)toConnect.myStallType][toConnect.shopStallNo].connectedStall = toConnect;
		return quickGetList[(int)toConnect.myStallType][toConnect.shopStallNo];
	}

	public void refreshNewDay()
	{
		for (int i = 0; i < quickGetList.Count; i++)
		{
			for (int j = 0; j < quickGetList[i].Length; j++)
			{
				quickGetList[i][j].hasBeenSold = false;
			}
		}
	}

	public void fillStallsFromRequest(bool[] requestedDetails)
	{
		int num = 0;
		for (int i = 0; i < quickGetList.Count; i++)
		{
			for (int j = 0; j < quickGetList[i].Length; j++)
			{
				quickGetList[i][j].hasBeenSold = requestedDetails[num];
				quickGetList[i][j].sellIfConnected();
				num++;
			}
		}
	}

	public bool[] getBoolArrayForSync()
	{
		List<bool> list = new List<bool>();
		for (int i = 0; i < quickGetList.Count; i++)
		{
			for (int j = 0; j < quickGetList[i].Length; j++)
			{
				list.Add(quickGetList[i][j].hasBeenSold);
			}
		}
		return list.ToArray();
	}
}
