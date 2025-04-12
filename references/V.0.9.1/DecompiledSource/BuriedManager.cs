using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BuriedManager : MonoBehaviour
{
	public static BuriedManager manage;

	public List<BuriedItem> allBuriedItems = new List<BuriedItem>();

	public InventoryItemLootTable randomDrops;

	public InventoryItemLootTable oreDrops;

	public InventoryItem[] oreDropItems;

	public InventoryItem[] normalBarrelDrops;

	public InventoryItemLootTable barrelDrops;

	public InventoryItemLootTable wheelieBinDrops;

	public InventoryItemLootTable shellDrops;

	public InventoryItemLootTable veryCommonDrops;

	public TileObject oldBarrel;

	public TileObject wheelieBin;

	public GameObject amberChunk;

	[Header("Chest Items --------")]
	public TileObject[] chestsToSpawnOnIsland;

	[Header("Giant Trees--------")]
	public InventoryItem fertiliser;

	public TileObject[] objectsToGiantGrow;

	public TileObject[] objectsToGiantGrowInto;

	private void Awake()
	{
		manage = this;
	}

	public void Start()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		List<InventoryItem> list2 = new List<InventoryItem>();
		for (int i = 0; i < Inventory.Instance.allItems.Length; i++)
		{
			if ((bool)Inventory.Instance.allItems[i].relic)
			{
				list.Add(Inventory.Instance.allItems[i]);
				if (Inventory.Instance.allItems[i].relic.myseason.myRarity >= SeasonAndTime.rarity.Uncommon)
				{
					list2.Add(Inventory.Instance.allItems[i]);
				}
			}
		}
		randomDrops.autoFillFromArray(list2.ToArray());
		wheelieBinDrops.autoFillFromArray(list2.ToArray());
		list.Add(normalBarrelDrops[0]);
		barrelDrops.autoFillFromArray(list.ToArray());
	}

	public BuriedItem checkIfBuriedItem(int xPos, int yPos)
	{
		for (int i = 0; i < allBuriedItems.Count; i++)
		{
			if (allBuriedItems[i].matches(xPos, yPos))
			{
				return allBuriedItems[i];
			}
		}
		return null;
	}

	public void buryNewItem(int itemId, int stack, int xPos, int yPos)
	{
		allBuriedItems.Add(new BuriedItem(itemId, stack, xPos, yPos));
	}

	private IEnumerator placeBarrelNextFrame(int xPos, int yPos, NetworkConnection con)
	{
		yield return null;
		NetworkMapSharer.Instance.RpcUpdateOnTileObject(oldBarrel.tileObjectId, xPos, yPos);
		NetworkMapSharer.Instance.TargetGiveDigUpTreasureMilestone(con, -1);
	}

	private IEnumerator placeWheelieBinNextFrame(int xPos, int yPos, NetworkConnection con)
	{
		yield return null;
		NetworkMapSharer.Instance.RpcUpdateOnTileObject(wheelieBin.tileObjectId, xPos, yPos);
		NetworkMapSharer.Instance.TargetGiveDigUpTreasureMilestone(con, -1);
	}

	private IEnumerator placeAmberNextFrame(int xPos, int yPos, NetworkConnection con)
	{
		yield return null;
		Vector3 pos = new Vector3(xPos * 2, WorldManager.Instance.heightMap[xPos, yPos], yPos * 2);
		NetworkMapSharer.Instance.spawnACarryable(amberChunk, pos);
		NetworkMapSharer.Instance.TargetGiveDigUpTreasureMilestone(con, -1);
	}

	private IEnumerator placeTreasureChestNextFrame(int xPos, int yPos, NetworkConnection con)
	{
		yield return null;
		int tileObjectId = chestsToSpawnOnIsland[Random.Range(0, chestsToSpawnOnIsland.Length)].tileObjectId;
		NetworkMapSharer.Instance.RpcUpdateOnTileObject(tileObjectId, xPos, yPos);
		NetworkMapSharer.Instance.RpcGiveOnTileStatus(0, xPos, yPos);
		if (tileObjectId == 200)
		{
			ContainerManager.manage.generateUndergroundChest(xPos, yPos, ContainerManager.manage.undergroundCrateTable, isOffIsland: true);
		}
		if (tileObjectId == 425)
		{
			ContainerManager.manage.generateUndergroundChest(xPos, yPos, ContainerManager.manage.undergroundForestChestTable, isOffIsland: true);
		}
		if (tileObjectId == 422)
		{
			ContainerManager.manage.generateUndergroundChest(xPos, yPos, ContainerManager.manage.reefIslandChestTable, isOffIsland: true);
		}
		NetworkMapSharer.Instance.TargetGiveDigUpTreasureMilestone(con, -1);
	}

	public BuriedItem createARandomItemWhenNotFound(int xPos, int yPos, NetworkConnection con)
	{
		NetworkMapSharer.Instance.RpcDigUpBuriedItemNoise(xPos, yPos);
		if (RealWorldTimeLight.time.offIsland)
		{
			StartCoroutine(placeTreasureChestNextFrame(xPos, yPos, con));
			return null;
		}
		if (Random.Range(0, 11) < 5)
		{
			StartCoroutine(placeBarrelNextFrame(xPos, yPos, con));
			return null;
		}
		if (Random.Range(0, 65) < 6)
		{
			StartCoroutine(placeWheelieBinNextFrame(xPos, yPos, con));
			return null;
		}
		if (Random.Range(0, 50) == 2)
		{
			StartCoroutine(placeAmberNextFrame(xPos, yPos, con));
			return null;
		}
		int stack = 1;
		InventoryItem randomDropFromTable;
		if (Random.Range(0, 3) <= 1)
		{
			randomDropFromTable = randomDrops.getRandomDropFromTable();
		}
		else
		{
			randomDropFromTable = veryCommonDrops.getRandomDropFromTable();
			stack = Random.Range(1, 4);
		}
		return new BuriedItem(Inventory.Instance.getInvItemId(randomDropFromTable), stack, xPos, yPos);
	}

	public bool checkIfShouldTurnIntoBuriedItem(int xPos, int yPos)
	{
		if (WorldManager.Instance.onTileMap[xPos, yPos] > 0 && (bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].dropsItemOnDeath && (bool)WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].dropsItemOnDeath.placeable && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[xPos, yPos]].dropsItemOnDeath.placeable.tileObjectId == WorldManager.Instance.onTileMap[xPos, yPos])
		{
			return false;
		}
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			return false;
		}
		if (Random.Range(0, 45) == 25)
		{
			return true;
		}
		return false;
	}

	public void CheckAllBuriedItemsToGrowGiantTree()
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return;
		}
		List<BuriedItem> list = new List<BuriedItem>();
		for (int i = 0; i < allBuriedItems.Count; i++)
		{
			if (allBuriedItems[i].itemId != fertiliser.getItemId())
			{
				continue;
			}
			int xPos = allBuriedItems[i].xPos;
			int yPos = allBuriedItems[i].yPos;
			for (int j = -2; j <= 2; j++)
			{
				for (int k = -2; k <= 2; k++)
				{
					for (int l = 0; l < objectsToGiantGrow.Length; l++)
					{
						int num = xPos + k;
						int num2 = yPos + j;
						if (WorldManager.Instance.isPositionOnMap(num, num2) && WorldManager.Instance.onTileMap[num, num2] == objectsToGiantGrow[l].tileObjectId)
						{
							if (!list.Contains(allBuriedItems[i]))
							{
								list.Add(allBuriedItems[i]);
							}
							NetworkMapSharer.Instance.RpChangeOnTileObjectNoDrop(objectsToGiantGrowInto[l].tileObjectId, num, num2);
						}
					}
				}
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			if (allBuriedItems.Contains(list[m]))
			{
				if (WorldManager.Instance.onTileMap[list[m].xPos, list[m].yPos] == 30)
				{
					NetworkMapSharer.Instance.RpcUpdateOnTileObject(-1, list[m].xPos, list[m].yPos);
				}
				allBuriedItems.Remove(list[m]);
			}
		}
	}
}
