using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ContainerManager : NetworkBehaviour
{
	public List<Chest> activeChests = new List<Chest>();

	public List<Chest> nonSavedChests = new List<Chest>();

	public List<Chest> privateStashes = new List<Chest>();

	public static ContainerManager manage;

	public InventoryItemLootTable undergroundCrateTable;

	public InventoryItemLootTable undergroundForestChestTable;

	public InventoryItemLootTable reefIslandChestTable;

	public InventoryItemLootTable lavaDungeonChestLootTable;

	public InventoryItemLootTable paintTable;

	public FishPondManager fishPondManager;

	public GameObject autoStackerPrefab;

	public ASound autoStackerFire;

	private void Awake()
	{
		manage = this;
	}

	public override void OnStartServer()
	{
		if (SaveLoad.saveOrLoad.loadingNewSaveFormat)
		{
			SaveLoad.saveOrLoad.newFileSaver.LoadChests();
		}
	}

	public void closeChestFromServer(int xPos, int yPos, int[] itemIds, int[] stacks, HouseDetails inside)
	{
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				activeChest.itemIds = itemIds;
				activeChest.itemStacks = stacks;
				break;
			}
		}
	}

	public void openVehicleStorage(NetworkConnection con, uint vehicleNetId)
	{
		if (NetworkIdentity.spawned.ContainsKey(vehicleNetId))
		{
			VehicleStorage component = NetworkIdentity.spawned[vehicleNetId].GetComponent<VehicleStorage>();
			TargetOpenVehicleStorage(con, vehicleNetId, component.invSlots, component.invSlotStacks);
		}
	}

	public void OpenChestFromMapForSaveConversion(int xPos, int yPos)
	{
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, null);
		activeChests.Add(chestSaveOrCreateNewOne);
	}

	public void OpenChestFromHouseForSaveConversion(int xPos, int yPos, HouseDetails inside)
	{
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, inside);
		activeChests.Add(chestSaveOrCreateNewOne);
	}

	public void openChestFromServer(NetworkConnection con, int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			foreach (Chest nonSavedChest in nonSavedChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, nonSavedChest) && nonSavedChest.xPos == xPos && nonSavedChest.yPos == yPos && nonSavedChest.IsOnCorrectLevel())
				{
					playerOpenedChest(xPos, yPos, inside);
					TargetOpenChest(con, xPos, yPos, nonSavedChest.itemIds, nonSavedChest.itemStacks);
					return;
				}
			}
			Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, inside);
			chestSaveOrCreateNewOne.SetCorrectLevel();
			nonSavedChests.Add(chestSaveOrCreateNewOne);
			playerOpenedChest(xPos, yPos, inside);
			TargetOpenChest(con, xPos, yPos, chestSaveOrCreateNewOne.itemIds, chestSaveOrCreateNewOne.itemStacks);
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				playerOpenedChest(xPos, yPos, inside);
				TargetOpenChest(con, xPos, yPos, activeChest.itemIds, activeChest.itemStacks);
				return;
			}
		}
		Chest chestSaveOrCreateNewOne2 = getChestSaveOrCreateNewOne(xPos, yPos, inside);
		activeChests.Add(chestSaveOrCreateNewOne2);
		playerOpenedChest(xPos, yPos, inside);
		TargetOpenChest(con, xPos, yPos, chestSaveOrCreateNewOne2.itemIds, chestSaveOrCreateNewOne2.itemStacks);
	}

	public void SyncChestFromServerForCrafting(NetworkConnection con, int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				TargetUpdateChestForCrafting(con, xPos, yPos, activeChest.itemIds, activeChest.itemStacks);
				return;
			}
		}
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, inside);
		activeChests.Add(chestSaveOrCreateNewOne);
		TargetUpdateChestForCrafting(con, xPos, yPos, chestSaveOrCreateNewOne.itemIds, chestSaveOrCreateNewOne.itemStacks);
	}

	public void autoStackIntoOpenChest()
	{
	}

	public void openStash(int stashId)
	{
		if (privateStashes.Count == 0)
		{
			loadStashes();
		}
		ChestWindow.chests.openStashInWindow(stashId);
	}

	public void playerCloseChest(int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			foreach (Chest nonSavedChest in nonSavedChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, nonSavedChest) && nonSavedChest.xPos == xPos && nonSavedChest.yPos == yPos && nonSavedChest.IsOnCorrectLevel())
				{
					nonSavedChest.playingLookingInside--;
					if (nonSavedChest.playingLookingInside <= 0)
					{
						nonSavedChest.playingLookingInside = 0;
						if (inside == null)
						{
							NetworkMapSharer.Instance.RpcGiveOnTileStatus(0, xPos, yPos);
						}
						else
						{
							NetworkMapSharer.Instance.RpcGiveOnTileStatusInside(0, xPos, yPos, inside.xPos, inside.yPos);
						}
					}
					break;
				}
			}
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (!checkIfChestIsInsideAndInThisHouse(inside, activeChest) || activeChest.xPos != xPos || activeChest.yPos != yPos)
			{
				continue;
			}
			activeChest.playingLookingInside--;
			if (activeChest.IsAutoSorter())
			{
				StartCoroutine(AutoSortItemsIntoNearbyChests(activeChest));
			}
			if (activeChest.IsMannequin())
			{
				UpdateSignDetailsForManequin(activeChest);
			}
			if (activeChest.IsToolRack())
			{
				UpdateSignDetailsForToolRack(activeChest);
			}
			if (activeChest.IsDisplayStand())
			{
				UpdateSignDetailsForDisplayStand(activeChest);
			}
			if (activeChest.playingLookingInside <= 0)
			{
				activeChest.playingLookingInside = 0;
				if (inside == null)
				{
					NetworkMapSharer.Instance.RpcGiveOnTileStatus(0, xPos, yPos);
				}
				else
				{
					NetworkMapSharer.Instance.RpcGiveOnTileStatusInside(0, xPos, yPos, inside.xPos, inside.yPos);
				}
			}
			break;
		}
	}

	public void playerOpenedChest(int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			foreach (Chest nonSavedChest in nonSavedChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, nonSavedChest) && nonSavedChest.xPos == xPos && nonSavedChest.yPos == yPos && nonSavedChest.IsOnCorrectLevel())
				{
					nonSavedChest.playingLookingInside = Mathf.Clamp(nonSavedChest.playingLookingInside + 1, 0, NetworkNavMesh.nav.getPlayerCount());
					if (inside == null)
					{
						NetworkMapSharer.Instance.RpcGiveOnTileStatus(1, xPos, yPos);
					}
					else
					{
						NetworkMapSharer.Instance.RpcGiveOnTileStatusInside(1, xPos, yPos, inside.xPos, inside.yPos);
					}
					break;
				}
			}
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				activeChest.playingLookingInside = Mathf.Clamp(activeChest.playingLookingInside + 1, 0, NetworkNavMesh.nav.getPlayerCount());
				if (inside == null)
				{
					NetworkMapSharer.Instance.RpcGiveOnTileStatus(1, xPos, yPos);
				}
				else
				{
					NetworkMapSharer.Instance.RpcGiveOnTileStatusInside(1, xPos, yPos, inside.xPos, inside.yPos);
				}
				break;
			}
		}
	}

	public bool checkIfEmpty(int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			foreach (Chest nonSavedChest in nonSavedChests)
			{
				if (!checkIfChestIsInsideAndInThisHouse(inside, nonSavedChest) || nonSavedChest.xPos != xPos || nonSavedChest.yPos != yPos || !nonSavedChest.IsOnCorrectLevel())
				{
					continue;
				}
				for (int i = 0; i < 24; i++)
				{
					if (nonSavedChest.itemIds[i] != -1)
					{
						return false;
					}
				}
				return true;
			}
		}
		else
		{
			foreach (Chest activeChest in activeChests)
			{
				if (!checkIfChestIsInsideAndInThisHouse(inside, activeChest) || activeChest.xPos != xPos || activeChest.yPos != yPos)
				{
					continue;
				}
				if (activeChest.IsFishPond())
				{
					MonoBehaviour.print("checkng fish pond is empty");
					for (int j = 0; j < 5; j++)
					{
						if (activeChest.itemIds[j] != -1)
						{
							return false;
						}
					}
					if (activeChest.itemIds[22] != -1 || activeChest.itemIds[23] != -1)
					{
						return false;
					}
					return true;
				}
				MonoBehaviour.print("Checking non fish pond is empty");
				for (int k = 0; k < 24; k++)
				{
					if (activeChest.itemIds[k] != -1)
					{
						return false;
					}
				}
				return true;
			}
			Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, inside);
			activeChests.Add(chestSaveOrCreateNewOne);
			for (int l = 0; l < 24; l++)
			{
				if (chestSaveOrCreateNewOne.itemIds[l] != -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public Chest getChestForWindow(int xPos, int yPos, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			foreach (Chest nonSavedChest in nonSavedChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, nonSavedChest) && nonSavedChest.xPos == xPos && nonSavedChest.yPos == yPos && nonSavedChest.IsOnCorrectLevel())
				{
					return nonSavedChest;
				}
			}
		}
		else
		{
			foreach (Chest activeChest in activeChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
				{
					return activeChest;
				}
			}
		}
		return null;
	}

	public Chest getChestForRecycling(int xPos, int yPos, HouseDetails inside)
	{
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				return activeChest;
			}
		}
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, inside);
		activeChests.Add(chestSaveOrCreateNewOne);
		return chestSaveOrCreateNewOne;
	}

	public void changeSlotInVehicleStorage(uint vehicleNetId, int slotNo, int newItemId, int newItemStack)
	{
		if (NetworkIdentity.spawned.ContainsKey(vehicleNetId))
		{
			VehicleStorage component = NetworkIdentity.spawned[vehicleNetId].GetComponent<VehicleStorage>();
			component.invSlots[slotNo] = newItemId;
			component.invSlotStacks[slotNo] = newItemStack;
			RpcRefreshOpenedVehicleStorage(vehicleNetId, slotNo, newItemId, newItemStack);
		}
	}

	public void changeSlotInChest(int xPos, int yPos, int slotNo, int newItemId, int newItemStack, HouseDetails inside)
	{
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			foreach (Chest nonSavedChest in nonSavedChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, nonSavedChest) && nonSavedChest.xPos == xPos && nonSavedChest.yPos == yPos && nonSavedChest.IsOnCorrectLevel())
				{
					nonSavedChest.itemIds[slotNo] = newItemId;
					nonSavedChest.itemStacks[slotNo] = newItemStack;
					break;
				}
			}
		}
		else
		{
			foreach (Chest activeChest in activeChests)
			{
				if (checkIfChestIsInsideAndInThisHouse(inside, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
				{
					activeChest.itemIds[slotNo] = newItemId;
					activeChest.itemStacks[slotNo] = newItemStack;
					break;
				}
			}
		}
		if (inside != null)
		{
			RpcRefreshOpenedChest(xPos, yPos, slotNo, newItemId, newItemStack, inside.xPos, inside.yPos);
		}
		else
		{
			RpcRefreshOpenedChest(xPos, yPos, slotNo, newItemId, newItemStack, -1, -1);
		}
	}

	public void clearWholeContainer(int xPos, int yPos, HouseDetails inside)
	{
		if (inside != null)
		{
			RpcClearChest(xPos, yPos, inside.xPos, inside.yPos);
		}
		else
		{
			RpcClearChest(xPos, yPos, -1, -1);
		}
	}

	[ClientRpc]
	public void RpcClearChest(int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(ContainerManager), "RpcClearChest", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRefreshOpenedChest(int xPos, int yPos, int slotNo, int newItemId, int newItemStack, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(slotNo);
		writer.WriteInt(newItemId);
		writer.WriteInt(newItemStack);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(ContainerManager), "RpcRefreshOpenedChest", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRefreshOpenedVehicleStorage(uint vehicleNetId, int slotNo, int newItemId, int newItemStack)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleNetId);
		writer.WriteInt(slotNo);
		writer.WriteInt(newItemId);
		writer.WriteInt(newItemStack);
		SendRPCInternal(typeof(ContainerManager), "RpcRefreshOpenedVehicleStorage", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetOpenVehicleStorage(NetworkConnection con, uint vehicleId, int[] itemIds, int[] itemStack)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleId);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, itemIds);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, itemStack);
		SendTargetRPCInternal(con, typeof(ContainerManager), "TargetOpenVehicleStorage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetOpenChest(NetworkConnection con, int xPos, int yPos, int[] itemIds, int[] itemStack)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, itemIds);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, itemStack);
		SendTargetRPCInternal(con, typeof(ContainerManager), "TargetOpenChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetUpdateChestForCrafting(NetworkConnection con, int xPos, int yPos, int[] itemIds, int[] itemStack)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, itemIds);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, itemStack);
		SendTargetRPCInternal(con, typeof(ContainerManager), "TargetUpdateChestForCrafting", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private Chest getChestSaveOrCreateNewOne(int xPos, int yPos, HouseDetails inside)
	{
		Chest chest = new Chest(xPos, yPos);
		string text = "/Chests/chest";
		if (inside != null)
		{
			chest.inside = true;
			text = text + "h" + inside.xPos + "+" + inside.yPos + "_";
			chest.insideX = inside.xPos;
			chest.insideY = inside.yPos;
		}
		if (!RealWorldTimeLight.time.underGround && File.Exists(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat", FileMode.Open);
				ChestSave chestSave = (ChestSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				for (int i = 0; i < 24; i++)
				{
					chest.itemIds[i] = chestSave.itemId[i];
					chest.itemStacks[i] = chestSave.itemStack[i];
				}
				fileStream.Close();
				SaveLoad.saveOrLoad.makeABackUp(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat", SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".bak");
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading chest");
				fileStream?.Close();
				ChestSave chestSave2 = checkForBackUpChest(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat");
				if (chestSave2 != null)
				{
					for (int j = 0; j < 24; j++)
					{
						chest.itemIds[j] = chestSave2.itemId[j];
						chest.itemStacks[j] = chestSave2.itemStack[j];
					}
				}
				else
				{
					for (int k = 0; k < 24; k++)
					{
						chest.itemIds[k] = -1;
						chest.itemStacks[k] = 0;
					}
				}
			}
		}
		else if (!RealWorldTimeLight.time.underGround)
		{
			ChestSave chestSave3 = checkForBackUpChest(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat");
			if (chestSave3 != null)
			{
				for (int l = 0; l < 24; l++)
				{
					chest.itemIds[l] = chestSave3.itemId[l];
					chest.itemStacks[l] = chestSave3.itemStack[l];
				}
			}
			else
			{
				if (!RealWorldTimeLight.time.underGround && EasyLoadChestExists(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat"))
				{
					return EasyLoadChests(chest, SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat");
				}
				for (int m = 0; m < 24; m++)
				{
					chest.itemIds[m] = -1;
					chest.itemStacks[m] = 0;
				}
			}
		}
		else
		{
			for (int n = 0; n < 24; n++)
			{
				chest.itemIds[n] = -1;
				chest.itemStacks[n] = 0;
			}
		}
		return chest;
	}

	private ChestSave checkForBackUpChest(string path)
	{
		FileStream fileStream = null;
		ChestSave result = null;
		path = path.Replace(".dat", ".bak");
		if (!RealWorldTimeLight.time.underGround && File.Exists(path))
		{
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(path, FileMode.Open);
				result = (ChestSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				Debug.LogWarning("Chest backup found");
				return result;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading chest backup");
				fileStream?.Close();
			}
		}
		return result;
	}

	public void EasySaveChests(Chest chestToSave, string savePath)
	{
		savePath = savePath.Replace(".dat", ".es3");
		ChestSave chestSave = new ChestSave();
		for (int i = 0; i < 24; i++)
		{
			chestSave.itemId[i] = chestToSave.itemIds[i];
			chestSave.itemStack[i] = chestToSave.itemStacks[i];
		}
		try
		{
			ES3.Save("chestInfo", chestSave, savePath);
		}
		catch
		{
			ES3.DeleteFile(savePath);
			ES3.Save("chestInfo", chestSave, savePath);
		}
	}

	public bool EasyLoadChestExists(string savePath)
	{
		savePath = savePath.Replace(".dat", ".es3");
		if (ES3.KeyExists("chestInfo", savePath))
		{
			return true;
		}
		return false;
	}

	public Chest EasyLoadChests(Chest returnChest, string savePath)
	{
		try
		{
			savePath = savePath.Replace(".dat", ".es3");
			if (ES3.KeyExists("chestInfo", savePath))
			{
				ChestSave chestSave = new ChestSave();
				ES3.LoadInto("chestInfo", savePath, chestSave);
				for (int i = 0; i < 24; i++)
				{
					returnChest.itemIds[i] = chestSave.itemId[i];
					returnChest.itemStacks[i] = chestSave.itemStack[i];
				}
				return returnChest;
			}
		}
		catch
		{
		}
		return returnChest;
	}

	public void saveChest(Chest chestToSave)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(SaveLoad.saveOrLoad.saveSlot() + "/Chests");
		if (!directoryInfo.Exists)
		{
			Debug.Log("Creating Chest Folder");
			directoryInfo.Create();
		}
		FileStream fileStream = null;
		try
		{
			string text = "/Chests/chest";
			if (chestToSave.inside)
			{
				text = text + "h" + chestToSave.insideX + "+" + chestToSave.insideY + "_";
			}
			EasySaveChests(chestToSave, SaveLoad.saveOrLoad.saveSlot() + text + chestToSave.xPos + "+" + chestToSave.yPos + ".dat");
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(SaveLoad.saveOrLoad.saveSlot() + text + chestToSave.xPos + "+" + chestToSave.yPos + ".dat");
			ChestSave chestSave = new ChestSave();
			for (int i = 0; i < 24; i++)
			{
				chestSave.itemId[i] = chestToSave.itemIds[i];
				chestSave.itemStack[i] = chestToSave.itemStacks[i];
			}
			binaryFormatter.Serialize(fileStream, chestSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving chest");
			fileStream?.Close();
		}
	}

	public void saveStashes()
	{
		for (int i = 0; i < privateStashes.Count; i++)
		{
			Chest chest = privateStashes[i];
			DirectoryInfo directoryInfo = new DirectoryInfo(SaveLoad.saveOrLoad.saveSlot() + "/Chests");
			if (!directoryInfo.Exists)
			{
				Debug.Log("Creating Chest Folder");
				directoryInfo.Create();
			}
			string text = "/Chests/chest";
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Create(SaveLoad.saveOrLoad.saveSlot() + text + -(i + 1) + "+" + -(i + 1) + ".dat");
				ChestSave chestSave = new ChestSave();
				for (int j = 0; j < 24; j++)
				{
					chestSave.itemId[j] = chest.itemIds[j];
					chestSave.itemStack[j] = chest.itemStacks[j];
				}
				binaryFormatter.Serialize(fileStream, chestSave);
				fileStream.Close();
			}
			catch (Exception)
			{
				Debug.LogWarning("Error saving stash");
				fileStream?.Close();
			}
		}
	}

	public void loadStashes()
	{
		for (int i = 0; i < 2; i++)
		{
			privateStashes.Add(getChestSaveOrCreateNewOne(-(i + 1), -(i + 1), null));
		}
	}

	public bool checkIfChestIsInsideAndInThisHouse(HouseDetails house, Chest checkChest)
	{
		if (checkChest.inside && house != null)
		{
			if (house.xPos == checkChest.insideX && house.yPos == checkChest.insideY)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void moveHousePosForChest(int xPos, int yPos, HouseDetails details, int newHouseX, int newHouseY)
	{
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(details, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				activeChest.insideX = newHouseX;
				activeChest.insideY = newHouseY;
				deleteOldChestSaveIfFound(details, xPos, yPos);
				return;
			}
		}
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, details);
		chestSaveOrCreateNewOne.insideX = newHouseX;
		chestSaveOrCreateNewOne.insideY = newHouseY;
		activeChests.Add(chestSaveOrCreateNewOne);
		deleteOldChestSaveIfFound(details, xPos, yPos);
	}

	public void moveChestInsideHousePositon(HouseDetails details, int xPos, int yPos, int newX, int newY)
	{
		if (!base.isServer)
		{
			return;
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(details, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				activeChest.xPos = newX;
				activeChest.insideY = newY;
				deleteOldChestSaveIfFound(details, xPos, yPos);
				return;
			}
		}
		Chest chestSaveOrCreateNewOne = getChestSaveOrCreateNewOne(xPos, yPos, details);
		chestSaveOrCreateNewOne.xPos = newX;
		chestSaveOrCreateNewOne.insideY = newY;
		activeChests.Add(chestSaveOrCreateNewOne);
		deleteOldChestSaveIfFound(details, xPos, yPos);
	}

	public void deleteOldChestSaveIfFound(HouseDetails house, int xPos, int yPos)
	{
		string text = "/Chests/chest";
		if (house != null)
		{
			text = text + "h" + house.xPos + "+" + house.yPos + "_";
		}
		if (File.Exists(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat"))
		{
			File.Delete(SaveLoad.saveOrLoad.saveSlot() + text + xPos + "+" + yPos + ".dat");
		}
	}

	public void generateUndergroundChest(int xPos, int yPos, InventoryItemLootTable generateFromTable, bool isOffIsland = false)
	{
		if (!base.isServer)
		{
			return;
		}
		Chest chest = new Chest(xPos, yPos);
		for (int i = 0; i < 24; i++)
		{
			chest.itemIds[i] = -1;
			chest.itemStacks[i] = 0;
		}
		int num = UnityEngine.Random.Range(4, 6);
		while (num > 0)
		{
			int num2 = UnityEngine.Random.Range(0, 24);
			if (chest.itemIds[num2] == -1)
			{
				chest.itemIds[num2] = generateFromTable.getRandomDropFromTable().getItemId();
				if (Inventory.Instance.allItems[chest.itemIds[num2]].hasFuel)
				{
					chest.itemStacks[num2] = UnityEngine.Random.Range(10, (int)((float)Inventory.Instance.allItems[chest.itemIds[num2]].fuelMax / 1.5f));
				}
				else
				{
					chest.itemStacks[num2] = 1;
				}
				num--;
			}
		}
		bool flag = false;
		while (!flag)
		{
			int num3 = UnityEngine.Random.Range(0, 24);
			if (chest.itemIds[num3] == -1)
			{
				chest.itemIds[num3] = paintTable.getRandomDropFromTable().getItemId();
				chest.itemStacks[num3] = 1;
				flag = true;
			}
		}
		if (isOffIsland)
		{
			chest.SetToOffIsland();
		}
		else
		{
			chest.SetToUnderGround();
		}
		nonSavedChests.Add(chest);
	}

	public Chest ReturnChestAndRemoveFromListForChestMovement(int xPos, int yPos, int houseX, int houseY)
	{
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			for (int i = 0; i < nonSavedChests.Count; i++)
			{
				if (nonSavedChests[i].xPos == xPos && nonSavedChests[i].yPos == yPos && nonSavedChests[i].insideX == houseX && nonSavedChests[i].insideY == houseY)
				{
					Chest result = nonSavedChests[i];
					nonSavedChests.RemoveAt(i);
					return result;
				}
			}
		}
		else
		{
			for (int j = 0; j < activeChests.Count; j++)
			{
				if (activeChests[j].xPos == xPos && activeChests[j].yPos == yPos && activeChests[j].insideX == houseX && activeChests[j].insideY == houseY)
				{
					Chest result2 = activeChests[j];
					activeChests.RemoveAt(j);
					return result2;
				}
			}
		}
		if (houseX == -1 && houseY == -1)
		{
			return getChestSaveOrCreateNewOne(xPos, yPos, null);
		}
		return getChestSaveOrCreateNewOne(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
	}

	public void PlaceChestBackIntoListAfterMove(Chest placeBackDown)
	{
		placeBackDown.SetCorrectLevel();
		if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
		{
			for (int i = 0; i < nonSavedChests.Count; i++)
			{
				if (nonSavedChests[i].xPos == placeBackDown.xPos && nonSavedChests[i].yPos == placeBackDown.yPos && nonSavedChests[i].insideX == placeBackDown.insideX && nonSavedChests[i].insideY == placeBackDown.insideY)
				{
					nonSavedChests[i] = placeBackDown;
					return;
				}
			}
			nonSavedChests.Add(placeBackDown);
			return;
		}
		for (int j = 0; j < activeChests.Count; j++)
		{
			if (activeChests[j].xPos == placeBackDown.xPos && activeChests[j].yPos == placeBackDown.yPos && activeChests[j].insideX == placeBackDown.insideX && activeChests[j].insideY == placeBackDown.insideY)
			{
				activeChests[j] = placeBackDown;
				return;
			}
		}
		activeChests.Add(placeBackDown);
	}

	public bool CheckIfClientNeedsToRequestChest(int xPos, int yPos)
	{
		HouseDetails insideHouseDetails = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails;
		for (int i = 0; i < activeChests.Count; i++)
		{
			if (checkIfChestIsInsideAndInThisHouse(insideHouseDetails, activeChests[i]) && activeChests[i].xPos == xPos && activeChests[i].yPos == yPos)
			{
				return false;
			}
		}
		return true;
	}

	public int GetAmountOfItemsInChestForTable(int itemId, int xPos, int yPos)
	{
		HouseDetails insideHouseDetails = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails;
		int num = 0;
		for (int i = 0; i < activeChests.Count; i++)
		{
			if (checkIfChestIsInsideAndInThisHouse(insideHouseDetails, activeChests[i]) && activeChests[i].xPos == xPos && activeChests[i].yPos == yPos)
			{
				num += activeChests[i].GetAmountOfItemInside(itemId);
			}
		}
		return num;
	}

	public IEnumerator AutoSortItemsIntoNearbyChests(Chest autoSorterToSortFrom)
	{
		yield return new WaitForSeconds(0.5f);
		HouseDetails inside = null;
		if (autoSorterToSortFrom.insideX != -1 && autoSorterToSortFrom.insideY != -1)
		{
			inside = HouseManager.manage.getHouseInfoIfExists(autoSorterToSortFrom.insideX, autoSorterToSortFrom.insideY);
		}
		_ = autoSorterToSortFrom.xPos;
		_ = autoSorterToSortFrom.yPos;
		if (inside == null)
		{
			if (WorldManager.Instance.onTileStatusMap[autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos] != 0)
			{
				yield break;
			}
		}
		else if (inside.houseMapOnTileStatus[autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos] != 0)
		{
			yield break;
		}
		for (int i = 0; i < autoSorterToSortFrom.itemIds.Length; i++)
		{
			if (autoSorterToSortFrom.itemIds[i] != -1 && Inventory.Instance.allItems[autoSorterToSortFrom.itemIds[i]].checkIfStackable())
			{
				bool flag = false;
				for (int j = 0; j < activeChests.Count; j++)
				{
					if (checkIfChestIsInsideAndInThisHouse(inside, activeChests[j]) && ((!activeChests[j].IsAutoSorter() && !activeChests[j].IsFishPond() && inside == null && activeChests[j].xPos >= autoSorterToSortFrom.xPos - 10 && activeChests[j].xPos <= autoSorterToSortFrom.xPos + 10 && activeChests[j].yPos >= autoSorterToSortFrom.yPos - 10 && activeChests[j].yPos <= autoSorterToSortFrom.yPos + 10) || (inside != null && !activeChests[j].IsAutoSorter() && inside.xPos == activeChests[j].insideX && inside.yPos == activeChests[j].insideY)) && activeChests[j].GetAmountOfItemInside(autoSorterToSortFrom.itemIds[i]) >= 1)
					{
						for (int k = 0; k < activeChests[j].itemIds.Length; k++)
						{
							if (activeChests[j].itemIds[k] == autoSorterToSortFrom.itemIds[i])
							{
								changeSlotInChest(activeChests[j].xPos, activeChests[j].yPos, k, activeChests[j].itemIds[k], activeChests[j].itemStacks[k] + autoSorterToSortFrom.itemStacks[i], inside);
								changeSlotInChest(autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos, i, -1, 0, inside);
								if (inside == null)
								{
									RpcShowObjectGettingSorted(activeChests[j].itemIds[k], autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos, activeChests[j].xPos, activeChests[j].yPos, -1, -1);
								}
								else
								{
									RpcShowObjectGettingSorted(activeChests[j].itemIds[k], autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos, activeChests[j].xPos, activeChests[j].yPos, autoSorterToSortFrom.insideX, autoSorterToSortFrom.insideY);
								}
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						continue;
					}
					yield return new WaitForSeconds(1f);
					if (inside == null)
					{
						if (WorldManager.Instance.onTileStatusMap[autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos] == 0)
						{
							break;
						}
					}
					else if (inside.houseMapOnTileStatus[autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos] == 0)
					{
						break;
					}
					yield break;
				}
			}
			else
			{
				if (autoSorterToSortFrom.itemIds[i] == -1 || Inventory.Instance.allItems[autoSorterToSortFrom.itemIds[i]].checkIfStackable())
				{
					continue;
				}
				bool flag2 = false;
				for (int l = 0; l < activeChests.Count; l++)
				{
					if (checkIfChestIsInsideAndInThisHouse(inside, activeChests[l]) && ((!activeChests[l].IsAutoSorter() && inside == null && activeChests[l].xPos >= autoSorterToSortFrom.xPos - 10 && activeChests[l].xPos <= autoSorterToSortFrom.xPos + 10 && activeChests[l].yPos >= autoSorterToSortFrom.yPos - 10 && activeChests[l].yPos <= autoSorterToSortFrom.yPos + 10) || (inside != null && !activeChests[l].IsAutoSorter() && inside.xPos == activeChests[l].insideX && inside.yPos == activeChests[l].insideY)) && activeChests[l].GetAmountOfItemInside(autoSorterToSortFrom.itemIds[i]) >= 1)
					{
						for (int m = 0; m < activeChests[l].itemIds.Length; m++)
						{
							if (activeChests[l].itemIds[m] == -1)
							{
								changeSlotInChest(activeChests[l].xPos, activeChests[l].yPos, m, autoSorterToSortFrom.itemIds[i], autoSorterToSortFrom.itemStacks[i], inside);
								changeSlotInChest(autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos, i, -1, 0, inside);
								if (inside == null)
								{
									RpcShowObjectGettingSorted(activeChests[l].itemIds[m], autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos, activeChests[l].xPos, activeChests[l].yPos, -1, -1);
								}
								else
								{
									RpcShowObjectGettingSorted(activeChests[l].itemIds[m], autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos, activeChests[l].xPos, activeChests[l].yPos, autoSorterToSortFrom.insideX, autoSorterToSortFrom.insideY);
								}
								flag2 = true;
								break;
							}
						}
					}
					if (!flag2)
					{
						continue;
					}
					yield return new WaitForSeconds(1f);
					if (inside == null)
					{
						if (WorldManager.Instance.onTileStatusMap[autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos] == 0)
						{
							break;
						}
					}
					else if (inside.houseMapOnTileStatus[autoSorterToSortFrom.xPos, autoSorterToSortFrom.yPos] == 0)
					{
						break;
					}
					yield break;
				}
			}
		}
	}

	[ClientRpc]
	public void RpcShowObjectGettingSorted(int itemId, int startX, int startY, int endX, int endY, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(startX);
		writer.WriteInt(startY);
		writer.WriteInt(endX);
		writer.WriteInt(endY);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(ContainerManager), "RpcShowObjectGettingSorted", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void FireProjectileFromSorter(int itemId, Vector3 startPos, Vector3 endPos, int endX, int endY, int houseX, int houseY)
	{
		UnityEngine.Object.Instantiate(autoStackerPrefab, startPos, Quaternion.identity).GetComponent<AutoStackDropFly>().StartFlying(itemId, startPos, endPos, endX, endY, houseX, houseY);
		SoundManager.Instance.playASoundAtPoint(autoStackerFire, startPos);
	}

	public void RemoveAmountOfItemsInChestForTable(int itemId, int removeAmount, int xPos, int yPos, int houseX, int houseY)
	{
		HouseDetails inside = null;
		if (houseX != -1 && houseY != -1)
		{
			inside = HouseManager.manage.getHouseInfoIfExists(houseX, houseY);
		}
		for (int i = 0; i < activeChests.Count; i++)
		{
			if (!checkIfChestIsInsideAndInThisHouse(null, activeChests[i]) || activeChests[i].xPos != xPos || activeChests[i].yPos != yPos || activeChests[i].insideX != houseX || activeChests[i].insideY != houseY)
			{
				continue;
			}
			for (int j = 0; j < activeChests[i].itemIds.Length; j++)
			{
				if (activeChests[i].itemIds[j] == itemId)
				{
					int num = Mathf.Clamp(activeChests[i].itemStacks[j] - removeAmount, 0, activeChests[i].itemStacks[j]);
					int num2 = Mathf.Clamp(activeChests[i].itemStacks[j] - num, 0, activeChests[i].itemStacks[j]);
					if (Inventory.Instance.allItems[activeChests[i].itemIds[j]].isATool || Inventory.Instance.allItems[activeChests[i].itemIds[j]].hasFuel)
					{
						num = 0;
						num2 = 1;
					}
					if (num == 0)
					{
						changeSlotInChest(xPos, yPos, j, -1, 0, inside);
					}
					else
					{
						changeSlotInChest(xPos, yPos, j, activeChests[i].itemIds[j], num, inside);
					}
					removeAmount -= num2;
					if (removeAmount == 0)
					{
						return;
					}
				}
			}
		}
	}

	public int GetAmountOfFishInPond(int xPos, int yPos)
	{
		Chest chestForWindow = getChestForWindow(xPos, yPos, null);
		int num = 0;
		if (chestForWindow != null)
		{
			for (int i = 0; i < 5; i++)
			{
				if (chestForWindow.itemIds[i] != -1)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void AddFishToPond(int fishId, int xPos, int yPos)
	{
		Chest chest = getChestForWindow(xPos, yPos, null);
		if (chest == null)
		{
			chest = getChestSaveOrCreateNewOne(xPos, yPos, null);
			activeChests.Add(chest);
		}
		bool flag = false;
		string text = "";
		for (int i = 0; i < chest.itemIds.Length; i++)
		{
			if (chest.itemIds[i] == -1 && !flag)
			{
				RpcRefreshOpenedChest(xPos, yPos, i, fishId, 1, -1, -1);
				chest.itemIds[i] = fishId;
				chest.itemStacks[i] = 1;
				flag = true;
			}
			if (i < 5)
			{
				text = text + "<" + chest.itemIds[i] + "> ";
			}
		}
		RpcUpdateSignDetails(xPos, yPos, -1, -1, text);
	}

	public int GetFirstFishFromPond(int xPos, int yPos)
	{
		Chest chestForWindow = getChestForWindow(xPos, yPos, null);
		string text = "";
		int num = -1;
		for (int num2 = 4; num2 >= 0; num2--)
		{
			if (chestForWindow.itemIds[num2] != -1 && num == -1)
			{
				num = chestForWindow.itemIds[num2];
				RpcRefreshOpenedChest(xPos, yPos, num2, -1, 0, -1, -1);
				chestForWindow.itemIds[num2] = -1;
				chestForWindow.itemStacks[num2] = 0;
				break;
			}
		}
		for (int i = 0; i < 5; i++)
		{
			text = text + "<" + chestForWindow.itemIds[i] + "> ";
		}
		RpcUpdateSignDetails(xPos, yPos, -1, -1, text);
		return num;
	}

	public int GetBugFromTerrariun(int bugId, int xPos, int yPos)
	{
		Chest chestForWindow = getChestForWindow(xPos, yPos, null);
		string text = "";
		int num = -1;
		for (int num2 = 4; num2 >= 0; num2--)
		{
			if (chestForWindow.itemIds[num2] == bugId && num == -1)
			{
				num = chestForWindow.itemIds[num2];
				RpcRefreshOpenedChest(xPos, yPos, num2, -1, 0, -1, -1);
				chestForWindow.itemIds[num2] = -1;
				chestForWindow.itemStacks[num2] = 0;
				break;
			}
		}
		for (int i = 0; i < 5; i++)
		{
			text = text + "<" + chestForWindow.itemIds[i] + "> ";
		}
		RpcUpdateSignDetails(xPos, yPos, -1, -1, text);
		return num;
	}

	public void DoFishBreeding(int roeId, int xPos, int yPos)
	{
		if (GetAmountOfFishInPond(xPos, yPos) >= 5 || GetAmountOfFishInPond(xPos, yPos) == 0)
		{
			return;
		}
		Chest chestForWindow = getChestForWindow(xPos, yPos, null);
		if (chestForWindow.itemIds[23] == roeId && chestForWindow.itemStacks[23] >= 15)
		{
			chestForWindow.itemStacks[23] = chestForWindow.itemStacks[23] - 15;
			if (chestForWindow.itemStacks[23] <= 0)
			{
				chestForWindow.itemIds[23] = -1;
				chestForWindow.itemStacks[23] = 0;
				RpcRefreshOpenedChest(xPos, yPos, 23, -1, 0, -1, -1);
			}
			else
			{
				RpcRefreshOpenedChest(xPos, yPos, 23, roeId, chestForWindow.itemStacks[23], -1, -1);
			}
			bool flag = false;
			int num = 2000;
			while (!flag)
			{
				int num2 = UnityEngine.Random.Range(0, 5);
				if (chestForWindow.itemIds[num2] != -1 && (bool)Inventory.Instance.allItems[chestForWindow.itemIds[num2]].fish)
				{
					flag = true;
					AddFishToPond(chestForWindow.itemIds[num2], xPos, yPos);
					break;
				}
				if (num <= 0)
				{
					break;
				}
				num--;
			}
		}
		int num3 = 0;
		for (int i = 0; i < 5; i++)
		{
			if (chestForWindow.itemIds[i] != -1 && (bool)Inventory.Instance.allItems[chestForWindow.itemIds[i]].fish)
			{
				num3 += UnityEngine.Random.Range(0, (int)(6 - Inventory.Instance.allItems[chestForWindow.itemIds[i]].fish.mySeason.myRarity));
			}
		}
	}

	public void DoFishPondRoe(int roeId, int xPos, int yPos)
	{
		Chest chestForWindow = getChestForWindow(xPos, yPos, null);
		if (chestForWindow == null || chestForWindow.itemIds[22] == -1)
		{
			return;
		}
		RpcRefreshOpenedChest(xPos, yPos, 22, -1, 0, -1, -1);
		chestForWindow.itemIds[22] = -1;
		chestForWindow.itemStacks[22] = 0;
		int num = 0;
		for (int i = 0; i < 5; i++)
		{
			if (chestForWindow.itemIds[i] != -1 && (bool)Inventory.Instance.allItems[chestForWindow.itemIds[i]].fish)
			{
				int num2 = UnityEngine.Random.Range(0, (int)(6 - Inventory.Instance.allItems[chestForWindow.itemIds[i]].fish.mySeason.myRarity));
				if (num2 == 0 && Inventory.Instance.allItems[chestForWindow.itemIds[i]].fish.mySeason.myRarity == SeasonAndTime.rarity.Common && UnityEngine.Random.Range(0, 2) == 1)
				{
					num2 = 1;
				}
				num += num2;
			}
		}
		if (chestForWindow.itemIds[23] == -1 || chestForWindow.itemIds[23] == roeId)
		{
			chestForWindow.itemIds[23] = roeId;
			chestForWindow.itemStacks[23] = Mathf.Clamp(chestForWindow.itemStacks[23] + num, 0, 15);
			RpcRefreshOpenedChest(xPos, yPos, 22, roeId, chestForWindow.itemStacks[23], -1, -1);
		}
	}

	public void UpdateSignDetailsForManequin(Chest c)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		for (int i = 0; i < c.itemIds.Length; i++)
		{
			if (IsItemEquipable(c.itemIds[i]))
			{
				if (num == -1 && Inventory.Instance.allItems[c.itemIds[i]].equipable.hat)
				{
					num = c.itemIds[i];
				}
				else if (num2 == -1 && Inventory.Instance.allItems[c.itemIds[i]].equipable.face)
				{
					num2 = c.itemIds[i];
				}
				else if (num3 == -1 && Inventory.Instance.allItems[c.itemIds[i]].equipable.shirt)
				{
					num3 = c.itemIds[i];
				}
				else if (num3 == -1 && Inventory.Instance.allItems[c.itemIds[i]].equipable.dress)
				{
					num3 = c.itemIds[i];
				}
				else if (num4 == -1 && Inventory.Instance.allItems[c.itemIds[i]].equipable.pants)
				{
					num4 = c.itemIds[i];
				}
				else if (num5 == -1 && Inventory.Instance.allItems[c.itemIds[i]].equipable.shoes)
				{
					num5 = c.itemIds[i];
				}
			}
		}
		string message = $"<{num}> <{num2}> <{num3}> <{num4}> <{num5}>";
		RpcUpdateSignDetails(c.xPos, c.yPos, c.insideX, c.insideY, message);
	}

	public bool IsItemEquipable(int id)
	{
		if (id <= -1)
		{
			return false;
		}
		return Inventory.Instance.allItems[id].equipable;
	}

	public bool IsItemTool(int id)
	{
		if (id <= -1)
		{
			return false;
		}
		return Inventory.Instance.allItems[id].isATool;
	}

	public void UpdateSignDetailsForToolRack(Chest c)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < c.itemIds.Length; i++)
		{
			if (IsItemTool(c.itemIds[i]))
			{
				if (num == -1)
				{
					num = c.itemIds[i];
				}
				else if (num2 == -1)
				{
					num2 = c.itemIds[i];
				}
				else if (num3 == -1)
				{
					num3 = c.itemIds[i];
				}
			}
		}
		string message = $"<{num}> <{num2}> <{num3}>";
		RpcUpdateSignDetails(c.xPos, c.yPos, c.insideX, c.insideY, message);
	}

	public void UpdateSignDetailsForDisplayStand(Chest c)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < c.itemIds.Length; i++)
		{
			if (num == -1)
			{
				num = c.itemIds[i];
			}
			else if (num2 == -1)
			{
				num2 = c.itemIds[i];
			}
			else if (num3 == -1)
			{
				num3 = c.itemIds[i];
			}
		}
		string message = $"<{num}> <{num2}> <{num3}>";
		RpcUpdateSignDetails(c.xPos, c.yPos, c.insideX, c.insideY, message);
	}

	[ClientRpc]
	public void RpcUpdateSignDetails(int xPos, int yPos, int houseX, int houseY, string message)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		writer.WriteString(message);
		SendRPCInternal(typeof(ContainerManager), "RpcUpdateSignDetails", writer, 0, includeOwner: true);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcClearChest(int xPos, int yPos, int houseX, int houseY)
	{
		HouseDetails house = null;
		if (houseX != -1 && houseY != -1)
		{
			house = HouseManager.manage.getHouseInfo(houseX, houseY);
		}
		foreach (Chest activeChest in activeChests)
		{
			if (checkIfChestIsInsideAndInThisHouse(house, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
			{
				for (int i = 0; i < activeChest.itemIds.Length; i++)
				{
					activeChest.itemIds[i] = -1;
					activeChest.itemStacks[i] = -1;
				}
				break;
			}
		}
	}

	protected static void InvokeUserCode_RpcClearChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClearChest called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_RpcClearChest(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRefreshOpenedChest(int xPos, int yPos, int slotNo, int newItemId, int newItemStack, int houseX, int houseY)
	{
		HouseDetails houseDetails = null;
		if (houseX != -1 && houseY != -1)
		{
			houseDetails = HouseManager.manage.getHouseInfo(houseX, houseY);
		}
		if (!base.isServer)
		{
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
			{
				foreach (Chest nonSavedChest in nonSavedChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(houseDetails, nonSavedChest) && nonSavedChest.xPos == xPos && nonSavedChest.yPos == yPos && nonSavedChest.IsOnCorrectLevel())
					{
						nonSavedChest.itemIds[slotNo] = newItemId;
						nonSavedChest.itemStacks[slotNo] = newItemStack;
						break;
					}
				}
			}
			else
			{
				foreach (Chest activeChest in activeChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(houseDetails, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
					{
						activeChest.itemIds[slotNo] = newItemId;
						activeChest.itemStacks[slotNo] = newItemStack;
						break;
					}
				}
			}
		}
		ChestWindow.chests.refreshOpenWindow(xPos, yPos, houseDetails);
		CraftingManager.manage.RefreshIfCraftingFromChest();
	}

	protected static void InvokeUserCode_RpcRefreshOpenedChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRefreshOpenedChest called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_RpcRefreshOpenedChest(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRefreshOpenedVehicleStorage(uint vehicleNetId, int slotNo, int newItemId, int newItemStack)
	{
		if (NetworkIdentity.spawned.ContainsKey(vehicleNetId))
		{
			VehicleStorage component = NetworkIdentity.spawned[vehicleNetId].GetComponent<VehicleStorage>();
			component.invSlots[slotNo] = newItemId;
			component.invSlotStacks[slotNo] = newItemStack;
			ChestWindow.chests.refreshOpenWindow(vehicleNetId);
		}
	}

	protected static void InvokeUserCode_RpcRefreshOpenedVehicleStorage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRefreshOpenedVehicleStorage called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_RpcRefreshOpenedVehicleStorage(reader.ReadUInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetOpenVehicleStorage(NetworkConnection con, uint vehicleId, int[] itemIds, int[] itemStack)
	{
		if (NetworkIdentity.spawned.ContainsKey(vehicleId))
		{
			VehicleStorage component = NetworkIdentity.spawned[vehicleId].GetComponent<VehicleStorage>();
			component.invSlots = itemIds;
			component.invSlotStacks = itemStack;
			ChestWindow.chests.openVehicleStorage(component);
		}
	}

	protected static void InvokeUserCode_TargetOpenVehicleStorage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOpenVehicleStorage called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_TargetOpenVehicleStorage(NetworkClient.readyConnection, reader.ReadUInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_TargetOpenChest(NetworkConnection con, int xPos, int yPos, int[] itemIds, int[] itemStack)
	{
		HouseDetails insideHouseDetails = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails;
		if (!base.isServer)
		{
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
			{
				foreach (Chest nonSavedChest in nonSavedChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(insideHouseDetails, nonSavedChest) && nonSavedChest.xPos == xPos && nonSavedChest.yPos == yPos && nonSavedChest.IsOnCorrectLevel())
					{
						nonSavedChest.itemIds = itemIds;
						nonSavedChest.itemStacks = itemStack;
						ChestWindow.chests.openChestInWindow(xPos, yPos);
						return;
					}
				}
				Chest chest = new Chest(xPos, yPos);
				chest.SetCorrectLevel();
				chest.itemIds = itemIds;
				chest.itemStacks = itemStack;
				if (insideHouseDetails != null)
				{
					chest.inside = true;
					chest.insideX = insideHouseDetails.xPos;
					chest.insideY = insideHouseDetails.yPos;
				}
				nonSavedChests.Add(chest);
			}
			else
			{
				foreach (Chest activeChest in activeChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(insideHouseDetails, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
					{
						activeChest.itemIds = itemIds;
						activeChest.itemStacks = itemStack;
						ChestWindow.chests.openChestInWindow(xPos, yPos);
						return;
					}
				}
				Chest chest2 = new Chest(xPos, yPos);
				chest2.itemIds = itemIds;
				chest2.itemStacks = itemStack;
				if (insideHouseDetails != null)
				{
					chest2.inside = true;
					chest2.insideX = insideHouseDetails.xPos;
					chest2.insideY = insideHouseDetails.yPos;
				}
				activeChests.Add(chest2);
			}
		}
		ChestWindow.chests.openChestInWindow(xPos, yPos);
	}

	protected static void InvokeUserCode_TargetOpenChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOpenChest called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_TargetOpenChest(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_TargetUpdateChestForCrafting(NetworkConnection con, int xPos, int yPos, int[] itemIds, int[] itemStack)
	{
		HouseDetails insideHouseDetails = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails;
		if (!base.isServer)
		{
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.offIsland)
			{
				foreach (Chest nonSavedChest in nonSavedChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(insideHouseDetails, nonSavedChest) && nonSavedChest.xPos == xPos && nonSavedChest.yPos == yPos && nonSavedChest.IsOnCorrectLevel())
					{
						nonSavedChest.itemIds = itemIds;
						nonSavedChest.itemStacks = itemStack;
						CraftingManager.manage.RefreshIfCraftingFromChest();
						return;
					}
				}
				Chest chest = new Chest(xPos, yPos);
				chest.SetCorrectLevel();
				chest.itemIds = itemIds;
				chest.itemStacks = itemStack;
				if (insideHouseDetails != null)
				{
					chest.inside = true;
					chest.insideX = insideHouseDetails.xPos;
					chest.insideY = insideHouseDetails.yPos;
				}
				nonSavedChests.Add(chest);
			}
			else
			{
				foreach (Chest activeChest in activeChests)
				{
					if (checkIfChestIsInsideAndInThisHouse(insideHouseDetails, activeChest) && activeChest.xPos == xPos && activeChest.yPos == yPos)
					{
						activeChest.itemIds = itemIds;
						activeChest.itemStacks = itemStack;
						CraftingManager.manage.RefreshIfCraftingFromChest();
						return;
					}
				}
				Chest chest2 = new Chest(xPos, yPos);
				chest2.itemIds = itemIds;
				chest2.itemStacks = itemStack;
				if (insideHouseDetails != null)
				{
					chest2.inside = true;
					chest2.insideX = insideHouseDetails.xPos;
					chest2.insideY = insideHouseDetails.yPos;
				}
				activeChests.Add(chest2);
			}
		}
		CraftingManager.manage.RefreshIfCraftingFromChest();
	}

	protected static void InvokeUserCode_TargetUpdateChestForCrafting(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetUpdateChestForCrafting called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_TargetUpdateChestForCrafting(NetworkClient.readyConnection, reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_RpcShowObjectGettingSorted(int itemId, int startX, int startY, int endX, int endY, int houseX, int houseY)
	{
		if (houseX == -1 && houseY == -1)
		{
			Vector3 vector = new Vector3((float)startX * 2f, WorldManager.Instance.heightMap[startX, startY], (float)startY * 2f);
			Vector3 endPos = new Vector3(endX * 2, WorldManager.Instance.heightMap[endX, endY], (float)endY * 2f);
			if (Vector3.Distance(CameraController.control.transform.position, vector) < 200f)
			{
				FireProjectileFromSorter(itemId, vector, endPos, endX, endY, houseX, houseY);
				TileObject tileObject = WorldManager.Instance.findTileObjectInUse(startX, startY);
				if ((bool)tileObject)
				{
					tileObject.tileObjectChest.chestAnim.SetTrigger("Fire");
					ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], vector + Vector3.up * 1.5f, 3);
				}
			}
			return;
		}
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if (!displayPlayerHouseTiles)
		{
			return;
		}
		Vector3 vector2 = new Vector3(displayPlayerHouseTiles.getStartingPosTransform().position.x + (float)startX * 2f, displayPlayerHouseTiles.getStartingPosTransform().position.y, displayPlayerHouseTiles.getStartingPosTransform().position.z + (float)startY * 2f);
		Vector3 endPos2 = new Vector3(displayPlayerHouseTiles.getStartingPosTransform().position.x + (float)endX * 2f, displayPlayerHouseTiles.getStartingPosTransform().position.y, displayPlayerHouseTiles.getStartingPosTransform().position.z + (float)endY * 2f);
		if (Vector3.Distance(CameraController.control.transform.position, vector2) < 200f)
		{
			FireProjectileFromSorter(itemId, vector2, endPos2, endX, endY, houseX, houseY);
			TileObject tileObject2 = displayPlayerHouseTiles.tileObjectsInHouse[startX, startY];
			if ((bool)tileObject2)
			{
				tileObject2.tileObjectChest.chestAnim.SetTrigger("Fire");
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], vector2 + Vector3.up * 1.5f, 3);
			}
		}
	}

	protected static void InvokeUserCode_RpcShowObjectGettingSorted(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowObjectGettingSorted called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_RpcShowObjectGettingSorted(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUpdateSignDetails(int xPos, int yPos, int houseX, int houseY, string message)
	{
		SignManager.manage.changeSignDetails(xPos, yPos, houseX, houseY, message);
	}

	protected static void InvokeUserCode_RpcUpdateSignDetails(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateSignDetails called on server.");
		}
		else
		{
			((ContainerManager)obj).UserCode_RpcUpdateSignDetails(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadString());
		}
	}

	static ContainerManager()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "RpcClearChest", InvokeUserCode_RpcClearChest);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "RpcRefreshOpenedChest", InvokeUserCode_RpcRefreshOpenedChest);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "RpcRefreshOpenedVehicleStorage", InvokeUserCode_RpcRefreshOpenedVehicleStorage);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "RpcShowObjectGettingSorted", InvokeUserCode_RpcShowObjectGettingSorted);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "RpcUpdateSignDetails", InvokeUserCode_RpcUpdateSignDetails);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "TargetOpenVehicleStorage", InvokeUserCode_TargetOpenVehicleStorage);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "TargetOpenChest", InvokeUserCode_TargetOpenChest);
		RemoteCallHelper.RegisterRpcDelegate(typeof(ContainerManager), "TargetUpdateChestForCrafting", InvokeUserCode_TargetUpdateChestForCrafting);
	}
}
