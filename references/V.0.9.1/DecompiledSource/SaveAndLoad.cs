using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveAndLoad : MonoBehaviour
{
	public static bool gameHasCompletelyLoaded;

	private string saveFilename = "/Player.es3";

	private string chestSaveFile = "/Container.es3";

	private string mapSaveName = "/MapSave.dat";

	private bool savedSucessfully = true;

	private ES3Settings cacheFileSettings;

	private List<NPCInventory> localInvs = new List<NPCInventory>();

	public bool CheckIfNewSaveAvaliable()
	{
		if (ES3.FileExists(GetSaveSlotDirectory() + saveFilename) && File.Exists(GetSaveSlotDirectory() + "/mapSave.dat"))
		{
			return true;
		}
		return false;
	}

	public bool CheckIfNewSaveAvaliableWithId(int slotId)
	{
		if (ES3.FileExists(GetSaveSlotDirectoryWithId(slotId) + saveFilename))
		{
			return true;
		}
		return false;
	}

	public bool CheckIfNewSaveAvaliableInSlot(int slotId)
	{
		if (ES3.FileExists(GetSaveSlotDirectoryWithId(slotId) + saveFilename))
		{
			return true;
		}
		return false;
	}

	public string GetSaveSlotDirectory()
	{
		return Path.Combine(Application.persistentDataPath + "\\Slot" + SaveLoad.saveOrLoad.currentSaveSlotNo());
	}

	public string GetSaveSlotDirectoryWithId(int id)
	{
		return Path.Combine(Application.persistentDataPath + "\\Slot" + id);
	}

	public void SaveGame(bool isServer, bool takePhoto = true, bool endOfDaySave = true)
	{
		StartCoroutine(SaveCoroutine(isServer, takePhoto, endOfDaySave));
	}

	public void LoadGame()
	{
		StartCoroutine(LoadOverFrames());
	}

	private IEnumerator SaveCoroutine(bool isServer, bool takePhoto = true, bool endOfDaySave = true, bool logOutSave = false)
	{
		savedSucessfully = true;
		cacheFileSettings = new ES3Settings(GetSaveSlotDirectory() + saveFilename, ES3.Location.Cache);
		ES3.CacheFile(GetSaveSlotDirectory() + saveFilename, cacheFileSettings);
		SaveLoad.saveOrLoad.isSaving = true;
		if ((isServer && RealWorldTimeLight.time.underGround) || (isServer && RealWorldTimeLight.time.offIsland))
		{
			yield return null;
		}
		else
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GetSaveSlotDirectory());
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			SaveInv();
			SaveLicences();
			SaveRecipes();
			SaveLevels();
			SavePedia();
			SaveNPCRelations();
			SaveMail();
			SaveStash();
			yield return null;
			if (takePhoto)
			{
				CharacterCreatorScript.create.takeSlotPhotoAndSave();
			}
			if (isServer)
			{
				HouseManager.manage.clearAllFurnitureStatus();
				int[] array = new int[NetworkNavMesh.nav.charsConnected.Count];
				for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
				{
					CharPickUp component = NetworkNavMesh.nav.charsConnected[i].GetComponent<CharPickUp>();
					if ((bool)component && component.sitting && component.myInteract.InsideHouseDetails == null)
					{
						array[i] = WorldManager.Instance.onTileStatusMap[component.sittingXpos, component.sittingYPos];
						WorldManager.Instance.onTileStatusMap[component.sittingXpos, component.sittingYPos] = 0;
					}
				}
				SaveQuests();
				SaveDate();
				yield return null;
				SaveTownManager();
				SaveChests();
				yield return null;
				SaveHouses();
				yield return null;
				SaveTownStatus();
				yield return null;
				SaveBulletinBoard();
				yield return null;
				SaveMuseum();
				SaveVehicles();
				yield return null;
				SaveDeeds();
				yield return null;
				SaveMapIcons();
				SaveCarriables();
				SaveItemsOnTop();
				yield return null;
				SaveWeather();
				yield return StartCoroutine(SaveMapOverFrames(endOfDaySave));
				SaveFencedOffAnimals(endOfDaySave);
				SaveChangers();
				yield return null;
				SaveAnimalHouses();
				yield return null;
				SaveFarmAnimalDetails();
				yield return null;
				SavePhotos(isClient: false);
				yield return null;
				SaveDrops();
				SaveBuried();
				SaveSigns();
				yield return null;
				yield return new WaitForSeconds(1f);
				NetworkMapSharer.Instance.NetworknextDayIsReady = true;
			}
			else
			{
				SavePhotos(isClient: true);
				if (NetworkMapSharer.Instance.nextDayIsReady)
				{
					StartCoroutine(SaveLoad.saveOrLoad.nonServerSave());
				}
			}
			if (savedSucessfully)
			{
				MonoBehaviour.print("Saving cached file");
				ES3.CreateBackup(GetSaveSlotDirectory() + saveFilename);
				ES3.StoreCachedFile(GetSaveSlotDirectory() + saveFilename);
			}
		}
		if (!logOutSave)
		{
			SaveLoad.saveOrLoad.loadingScreen.saveGameConfirmed.gameObject.SetActive(value: true);
			yield return new WaitForSeconds(1f);
			StartCoroutine(SaveLoad.saveOrLoad.loadingScreen.saveGameConfirmed.GetComponent<WindowAnimator>().closeWithMask());
		}
		SaveLoad.saveOrLoad.isSaving = false;
	}

	private IEnumerator SaveMapOverFrames(bool isEndOfDaySave)
	{
		if (NetworkMapSharer.Instance.nextDayIsReady)
		{
			SaveLoad.saveOrLoad.loadingScreen.loadingBarOnlyAppear();
		}
		string path = GetSaveSlotDirectory() + mapSaveName;
		int num = 0;
		using (BinaryWriter w = new BinaryWriter(File.OpenWrite(path)))
		{
			w.Write(GenerateMap.generate.seed);
			for (int y = 0; y < 100; y++)
			{
				for (int x = 0; x < 100; x++)
				{
					w.Write(WorldManager.Instance.chunkChangedMap[x, y]);
					if (!WorldManager.Instance.chunkChangedMap[x, y])
					{
						continue;
					}
					w.Write(WorldManager.Instance.changedMapHeight[x, y]);
					w.Write(WorldManager.Instance.changedMapWater[x, y]);
					w.Write(WorldManager.Instance.changedMapOnTile[x, y]);
					w.Write(WorldManager.Instance.changedMapTileType[x, y]);
					for (int ydif = 0; ydif < 10; ydif++)
					{
						for (int i = 0; i < 10; i++)
						{
							if (WorldManager.Instance.changedMapOnTile[x, y])
							{
								w.Write((short)WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif]);
								if (WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif] > 1 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif]].tileObjectFurniture)
								{
									WorldManager.Instance.onTileStatusMap[x * 10 + i, y * 10 + ydif] = 0;
								}
								w.Write((short)WorldManager.Instance.onTileStatusMap[x * 10 + i, y * 10 + ydif]);
								w.Write((short)WorldManager.Instance.rotationMap[x * 10 + i, y * 10 + ydif]);
							}
							if (WorldManager.Instance.changedMapHeight[x, y])
							{
								w.Write((short)WorldManager.Instance.heightMap[x * 10 + i, y * 10 + ydif]);
							}
							if (WorldManager.Instance.changedMapTileType[x, y])
							{
								w.Write((short)WorldManager.Instance.tileTypeMap[x * 10 + i, y * 10 + ydif]);
								w.Write((short)WorldManager.Instance.tileTypeStatusMap[x * 10 + i, y * 10 + ydif]);
							}
							if (WorldManager.Instance.changedMapWater[x, y])
							{
								w.Write(WorldManager.Instance.waterMap[x * 10 + i, y * 10 + ydif]);
							}
						}
						if (num <= 0)
						{
							SaveLoad.saveOrLoad.loadingScreen.showPercentage((float)(y * 10) + (float)ydif / 1000f);
							yield return null;
							num = 50;
						}
					}
				}
				if (num <= 0)
				{
					SaveLoad.saveOrLoad.loadingScreen.showPercentage((float)(y * 10) / 1000f);
					yield return null;
					num = 50;
				}
			}
		}
		if (!isEndOfDaySave)
		{
			yield return StartCoroutine(WorldManager.Instance.fenceCheck());
		}
	}

	public int ReadIslandSeedFromSaveFile()
	{
		string path = GetSaveSlotDirectory() + "/mapSave.dat";
		if (!File.Exists(path))
		{
			Debug.LogWarning("Save file not found. Cannot read seed.");
			return 0;
		}
		using BinaryReader binaryReader = new BinaryReader(File.OpenRead(path));
		return binaryReader.ReadInt32();
	}

	public IEnumerator LoadOverFrames()
	{
		gameHasCompletelyLoaded = false;
		SaveLoad.saveOrLoad.loadingScreen.appear("Tip_Loading", loadingTipsOn: true);
		yield return new WaitForSeconds(1f);
		DirectoryInfo directoryInfo = new DirectoryInfo(GetSaveSlotDirectory());
		cacheFileSettings = new ES3Settings(GetSaveSlotDirectory() + saveFilename, ES3.Location.Cache);
		ES3.CacheFile(GetSaveSlotDirectory() + saveFilename, cacheFileSettings);
		if (directoryInfo.Exists)
		{
			LoadDate();
			LoadTownManager();
			LoadItemsOnTop();
			yield return null;
			string path = GetSaveSlotDirectory() + "/mapSave.dat";
			int frameCounter = 0;
			using (BinaryReader r = new BinaryReader(File.OpenRead(path)))
			{
				GenerateMap.generate.seed = r.ReadInt32();
				yield return StartCoroutine(GenerateMap.generate.generateNewMap(GenerateMap.generate.seed));
				NewChunkLoader.loader.inside = true;
				yield return null;
				for (int y = 0; y < 100; y++)
				{
					for (int x = 0; x < 100; x++)
					{
						frameCounter--;
						WorldManager.Instance.chunkChangedMap[x, y] = r.ReadBoolean();
						if (!WorldManager.Instance.chunkChangedMap[x, y])
						{
							continue;
						}
						WorldManager.Instance.changedMapHeight[x, y] = r.ReadBoolean();
						WorldManager.Instance.changedMapWater[x, y] = r.ReadBoolean();
						WorldManager.Instance.changedMapOnTile[x, y] = r.ReadBoolean();
						WorldManager.Instance.changedMapTileType[x, y] = r.ReadBoolean();
						for (int ydif = 0; ydif < 10; ydif++)
						{
							for (int i = 0; i < 10; i++)
							{
								if (WorldManager.Instance.changedMapOnTile[x, y])
								{
									WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
									WorldManager.Instance.onTileStatusMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
									WorldManager.Instance.rotationMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
									if (WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif] >= 0 && WorldManager.Instance.allObjectSettings[WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif]].isMultiTileObject && WorldManager.Instance.rotationMap[x * 10 + i, y * 10 + ydif] <= 0)
									{
										WorldManager.Instance.rotationMap[x * 10 + i, y * 10 + ydif] = Mathf.Clamp(WorldManager.Instance.rotationMap[x * 10 + i, y * 10 + ydif], 1, 4);
									}
									WorldManager.Instance.placeFenceInChunk(x * 10 + i, y * 10 + ydif);
									if (WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif] == 132 || WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif] == 318)
									{
										WorldManager.Instance.sprinkerContinuesToWater(x * 10 + i, y * 10 + ydif);
									}
								}
								if (WorldManager.Instance.changedMapHeight[x, y])
								{
									WorldManager.Instance.heightMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
								}
								if (WorldManager.Instance.changedMapTileType[x, y])
								{
									WorldManager.Instance.tileTypeMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
									WorldManager.Instance.tileTypeStatusMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
								}
								if (WorldManager.Instance.changedMapWater[x, y])
								{
									WorldManager.Instance.waterMap[x * 10 + i, y * 10 + ydif] = r.ReadBoolean();
									if (WorldManager.Instance.heightMap[x * 10 + i, y * 10 + ydif] > 0)
									{
										WorldManager.Instance.waterMap[x * 10 + i, y * 10 + ydif] = false;
									}
								}
							}
							if (frameCounter <= 0)
							{
								SaveLoad.saveOrLoad.loadingScreen.showPercentage(0.5f + (float)(y * 10 + ydif) / 1000f / 2f);
								yield return null;
								frameCounter = 3;
							}
						}
					}
					if (frameCounter <= 0)
					{
						SaveLoad.saveOrLoad.loadingScreen.showPercentage(0.5f + (float)y / 100f / 2f);
						yield return null;
						frameCounter = 5;
					}
				}
			}
			LoadHouses();
			MuseumManager.manage.loadMuseum();
			yield return StartCoroutine(WorldManager.Instance.fenceCheck());
			NewChunkLoader.loader.inside = false;
			GenerateMap.generate.onFileLoaded();
			LoadInv();
			yield return null;
			LoadNPCRelations();
			LoadQuests();
			yield return null;
			LoadRecipes();
			LoadLicences();
			yield return null;
			LoadPedia();
			LoadLevels();
			yield return null;
			LoadDeeds();
			LoadTownStatus();
			yield return null;
			LoadFencedOffAnimals();
			yield return null;
			SaveLoad.saveOrLoad.loadingComplete = true;
			LoadChangers();
			yield return null;
			LoadBuried();
			yield return null;
			LoadSigns();
			yield return null;
		}
		SaveLoad.saveOrLoad.DeleteOldSaveFiles();
		gameHasCompletelyLoaded = true;
	}

	private void SaveInv()
	{
		try
		{
			PlayerInv playerInv = new PlayerInv();
			playerInv.SaveInv();
			ES3.Save("playerInfo", playerInv, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Inv: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadInv()
	{
		if (ES3.KeyExists("playerInfo", cacheFileSettings))
		{
			PlayerInv playerInv = new PlayerInv();
			ES3.LoadInto("playerInfo", playerInv, cacheFileSettings);
			playerInv.LoadInv();
			StartCoroutine(EquipWindow.equip.wearingMinersHelmet());
		}
	}

	public void LoadInvForMultiplayerConnect()
	{
		cacheFileSettings = new ES3Settings(GetSaveSlotDirectory() + saveFilename, ES3.Location.Cache);
		ES3.CacheFile(GetSaveSlotDirectory() + saveFilename, cacheFileSettings);
		LoadInv();
		LoadLevels();
		LoadRecipes();
		LoadLicences();
		LoadPedia();
		LoadNPCRelations();
	}

	private void SaveStash()
	{
		try
		{
			for (int i = 0; i < ContainerManager.manage.privateStashes.Count; i++)
			{
				ChestSave chestSave = new ChestSave();
				chestSave.SaveChestDetails(ContainerManager.manage.privateStashes[i]);
				ES3.Save("stash_" + i, chestSave, cacheFileSettings);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Stash: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadStash()
	{
		if (!SaveLoad.saveOrLoad.loadingNewSaveFormat)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (ES3.KeyExists("stash_" + i, cacheFileSettings))
			{
				ChestSave chestSave = new ChestSave();
				ES3.LoadInto("stash_" + i, chestSave, cacheFileSettings);
				chestSave.LoadStash();
			}
		}
	}

	public PlayerInv LoadPlayerDetailsForFileButton(int slotId)
	{
		try
		{
			if (ES3.KeyExists("playerInfo", GetSaveSlotDirectoryWithId(slotId) + saveFilename))
			{
				PlayerInv playerInv = new PlayerInv();
				ES3.LoadInto("playerInfo", GetSaveSlotDirectoryWithId(slotId) + saveFilename, playerInv);
				return playerInv;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to get name for slot id: + " + slotId + ": " + ex.Message);
			if (ES3.RestoreBackup(GetSaveSlotDirectoryWithId(slotId) + saveFilename))
			{
				Debug.Log("Restored a backup for slot " + slotId);
			}
			else
			{
				Debug.Log("No backup to restore for slot" + slotId);
			}
		}
		return null;
	}

	private void SaveLicences()
	{
		try
		{
			LicenceAndPermitPointSave licenceAndPermitPointSave = new LicenceAndPermitPointSave();
			licenceAndPermitPointSave.saveLicencesAndPoints();
			ES3.Save("licences", licenceAndPermitPointSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Licences: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadLicences()
	{
		if (ES3.KeyExists("licences", cacheFileSettings))
		{
			LicenceAndPermitPointSave licenceAndPermitPointSave = new LicenceAndPermitPointSave();
			ES3.LoadInto("licences", licenceAndPermitPointSave, cacheFileSettings);
			licenceAndPermitPointSave.loadLicencesAndPoints();
		}
	}

	private void SaveRecipes()
	{
		try
		{
			RecipesUnlockedSave recipesUnlockedSave = new RecipesUnlockedSave();
			recipesUnlockedSave.SaveRecipes();
			ES3.Save("recipes", recipesUnlockedSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Recipes: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadRecipes()
	{
		if (ES3.KeyExists("recipes", cacheFileSettings))
		{
			RecipesUnlockedSave recipesUnlockedSave = new RecipesUnlockedSave();
			ES3.LoadInto("recipes", recipesUnlockedSave, cacheFileSettings);
			recipesUnlockedSave.loadRecipes();
			CraftsmanManager.manage.currentLevel = recipesUnlockedSave.crafterLevel;
			CraftsmanManager.manage.currentPoints = recipesUnlockedSave.currentPoints;
			CraftsmanManager.manage.craftsmanHasBerkonium = recipesUnlockedSave.crafterHasBerkonium;
			CraftsmanManager.manage.itemCurrentlyCrafting = recipesUnlockedSave.crafterWorkingOnItemId;
			NetworkMapSharer.Instance.NetworkcraftsmanWorking = recipesUnlockedSave.crafterCurrentlyWorking;
			NetworkMapSharer.Instance.NetworkcraftsmanHasBerkonium = CraftsmanManager.manage.craftsmanHasBerkonium;
			CharLevelManager.manage.recipesAlwaysUnlocked();
		}
	}

	private void SaveLevels()
	{
		try
		{
			LevelSave levelSave = new LevelSave();
			levelSave.saveLevels(CharLevelManager.manage.todaysXp, CharLevelManager.manage.currentXp, CharLevelManager.manage.currentLevels);
			ES3.Save("levels", levelSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Levels: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadLevels()
	{
		if (ES3.KeyExists("levels", cacheFileSettings))
		{
			LevelSave levelSave = new LevelSave();
			ES3.LoadInto("levels", levelSave, cacheFileSettings);
			levelSave.loadLevels();
		}
	}

	private void SavePedia()
	{
		try
		{
			PediaSave pediaSave = new PediaSave();
			pediaSave.saveEntries(PediaManager.manage.allEntries.ToArray());
			ES3.Save("pedia", pediaSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Pedia: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadPedia()
	{
		if (ES3.KeyExists("pedia", cacheFileSettings))
		{
			PediaSave pediaSave = new PediaSave();
			ES3.LoadInto("pedia", pediaSave, cacheFileSettings);
			pediaSave.loadEntries();
		}
	}

	private void SaveNPCRelations()
	{
		try
		{
			NPCsave nPCsave = new NPCsave();
			nPCsave.savedStatuses = NPCManager.manage.npcStatus.ToArray();
			if (NetworkMapSharer.Instance.isServer)
			{
				nPCsave.saveInvs = NPCManager.manage.npcInvs.ToArray();
			}
			else
			{
				nPCsave.saveInvs = localInvs.ToArray();
			}
			ES3.Save("npc", nPCsave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save NPC: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadNPCRelations()
	{
		if (ES3.KeyExists("npc", cacheFileSettings))
		{
			NPCsave nPCsave = new NPCsave();
			ES3.LoadInto("npc", nPCsave, cacheFileSettings);
			nPCsave.loadNpcs();
			localInvs = NPCManager.manage.npcInvs;
		}
	}

	private void SaveMail()
	{
		try
		{
			MailSave mailSave = new MailSave();
			mailSave.allMail = MailManager.manage.mailInBox.ToArray();
			mailSave.tomorrowsMail = MailManager.manage.tomorrowsLetters.ToArray();
			ES3.Save("mail", mailSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Mail: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadMail()
	{
		if (!ES3.KeyExists("mail", cacheFileSettings))
		{
			return;
		}
		MailSave mailSave = new MailSave();
		ES3.LoadInto("mail", mailSave, cacheFileSettings);
		for (int i = 0; i < mailSave.allMail.Length; i++)
		{
			MailManager.manage.mailInBox.Add(mailSave.allMail[i]);
		}
		if (mailSave.tomorrowsMail != null)
		{
			for (int j = 0; j < mailSave.tomorrowsMail.Length; j++)
			{
				MailManager.manage.tomorrowsLetters.Add(mailSave.tomorrowsMail[j]);
			}
		}
	}

	private void SaveQuests()
	{
		try
		{
			QuestSave questSave = new QuestSave();
			questSave.accepted = QuestManager.manage.isQuestAccepted;
			questSave.completed = QuestManager.manage.isQuestCompleted;
			ES3.Save("quests", questSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Quests: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadQuests()
	{
		if (ES3.KeyExists("quests", cacheFileSettings))
		{
			QuestSave questSave = new QuestSave();
			ES3.LoadInto("quests", questSave, cacheFileSettings);
			for (int i = 0; i < questSave.completed.Length; i++)
			{
				QuestManager.manage.isQuestCompleted[i] = questSave.completed[i];
				QuestManager.manage.isQuestAccepted[i] = questSave.accepted[i];
			}
		}
	}

	private void SaveDate()
	{
		try
		{
			DateSave dateSave = new DateSave();
			dateSave = WorldManager.Instance.getDateSave();
			dateSave.todaysMineSeed = NetworkMapSharer.Instance.mineSeed;
			dateSave.tomorrowsMineSeed = NetworkMapSharer.Instance.tomorrowsMineSeed;
			dateSave.hour = RealWorldTimeLight.time.currentHour;
			dateSave.minute = RealWorldTimeLight.time.currentMinute;
			ES3.Save("date", dateSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Date: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadDate()
	{
		if (ES3.KeyExists("date", cacheFileSettings))
		{
			DateSave dateSave = new DateSave();
			ES3.LoadInto("date", dateSave, cacheFileSettings);
			WorldManager.Instance.loadDateFromSave(dateSave);
			NetworkMapSharer.Instance.NetworkmineSeed = dateSave.todaysMineSeed;
			NetworkMapSharer.Instance.tomorrowsMineSeed = dateSave.tomorrowsMineSeed;
			RealWorldTimeLight.time.NetworkcurrentHour = dateSave.hour;
		}
	}

	public DateSave LoadDateForSaveSlot(int slotId)
	{
		try
		{
			if (ES3.KeyExists("date", GetSaveSlotDirectoryWithId(slotId) + saveFilename))
			{
				DateSave dateSave = new DateSave();
				ES3.LoadInto("date", GetSaveSlotDirectoryWithId(slotId) + saveFilename, dateSave);
				return dateSave;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to get date for slot button: " + ex.Message);
		}
		return null;
	}

	private void SaveTownManager()
	{
		try
		{
			TownManagerSave townManagerSave = new TownManagerSave();
			townManagerSave.saveTown();
			ES3.Save("townSave", townManagerSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Town Manager: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadTownManager()
	{
		if (ES3.KeyExists("townSave", cacheFileSettings))
		{
			TownManagerSave townManagerSave = new TownManagerSave();
			ES3.LoadInto("townSave", townManagerSave, cacheFileSettings);
			townManagerSave.load();
		}
	}

	private void SaveTownStatus()
	{
		try
		{
			TownStatusSave townStatusSave = new TownStatusSave();
			townStatusSave.saveTownStatus();
			ES3.Save("townStatus", townStatusSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Town Status: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadTownStatus()
	{
		if (ES3.KeyExists("townStatus", cacheFileSettings))
		{
			TownStatusSave townStatusSave = new TownStatusSave();
			ES3.LoadInto("townStatus", townStatusSave, cacheFileSettings);
			townStatusSave.loadTownStatus();
		}
	}

	private void SaveHouses()
	{
		try
		{
			HouseListSave houseListSave = new HouseListSave();
			houseListSave.save();
			ES3.Save("houseSave", houseListSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Houses: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadHouses()
	{
		if (ES3.KeyExists("houseSave", cacheFileSettings))
		{
			HouseListSave houseListSave = new HouseListSave();
			ES3.LoadInto("houseSave", houseListSave, cacheFileSettings);
			houseListSave.load();
		}
	}

	private void SaveBulletinBoard()
	{
		try
		{
			BulletinBoardSave bulletinBoardSave = new BulletinBoardSave();
			bulletinBoardSave.allPosts = BulletinBoard.board.attachedPosts.ToArray();
			ES3.Save("bboard", bulletinBoardSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save bulletin board: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadBulletinBoard()
	{
		if (ES3.KeyExists("bboard", cacheFileSettings))
		{
			BulletinBoardSave bulletinBoardSave = new BulletinBoardSave();
			ES3.LoadInto("bboard", bulletinBoardSave, cacheFileSettings);
			fillBoardSave(bulletinBoardSave);
		}
	}

	private void SaveMuseum()
	{
		try
		{
			MuseumSave museumSave = new MuseumSave();
			museumSave.fishDonated = MuseumManager.manage.fishDonated;
			museumSave.bugDonated = MuseumManager.manage.bugsDonated;
			museumSave.underWaterCreatures = MuseumManager.manage.underWaterCreaturesDonated;
			ES3.Save("museumSave", museumSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Museum: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadMuseum()
	{
		if (!ES3.KeyExists("museumSave", cacheFileSettings))
		{
			return;
		}
		MuseumSave museumSave = new MuseumSave();
		ES3.LoadInto("museumSave", museumSave, cacheFileSettings);
		for (int i = 0; i < museumSave.fishDonated.Length; i++)
		{
			MuseumManager.manage.fishDonated[i] = museumSave.fishDonated[i];
		}
		for (int j = 0; j < museumSave.bugDonated.Length; j++)
		{
			MuseumManager.manage.bugsDonated[j] = museumSave.bugDonated[j];
		}
		if (museumSave.underWaterCreatures != null)
		{
			for (int k = 0; k < museumSave.underWaterCreatures.Length; k++)
			{
				MuseumManager.manage.underWaterCreaturesDonated[k] = museumSave.underWaterCreatures[k];
			}
		}
	}

	private void SaveVehicles()
	{
		try
		{
			List<VehicleSavable> list = new List<VehicleSavable>();
			for (int num = SaveLoad.saveOrLoad.vehiclesToSave.Count - 1; num > -1; num--)
			{
				if (SaveLoad.saveOrLoad.vehiclesToSave[num] != null)
				{
					if (SaveLoad.saveOrLoad.vehiclesToSave[num].WorldAreaLocation != 0)
					{
						SaveLoad.saveOrLoad.vehiclesToSave[num].destroyServerSelf();
					}
					else
					{
						list.Add(new VehicleSavable(SaveLoad.saveOrLoad.vehiclesToSave[num]));
					}
				}
			}
			VehicleSave vehicleSave = new VehicleSave();
			vehicleSave.allVehicles = list.ToArray();
			ES3.Save("vehicleInfo", vehicleSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save vehicles: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadVehicles()
	{
		if (!ES3.KeyExists("vehicleInfo", cacheFileSettings))
		{
			return;
		}
		VehicleSave vehicleSave = new VehicleSave();
		ES3.LoadInto("vehicleInfo", vehicleSave, cacheFileSettings);
		VehicleSavable[] allVehicles = vehicleSave.allVehicles;
		foreach (VehicleSavable vehicleSavable in allVehicles)
		{
			if (vehicleSavable.vehicleId == 3)
			{
				MonoBehaviour.print("spawning mu mount instead of mu vehicle");
				StartCoroutine(SaveLoad.saveOrLoad.delayReplaceMuMount(vehicleSavable.getPosition()));
			}
			else if (vehicleSavable.vehicleId < SaveLoad.saveOrLoad.vehiclePrefabs.Length)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(SaveLoad.saveOrLoad.vehiclePrefabs[vehicleSavable.vehicleId], vehicleSavable.getPosition(), vehicleSavable.getRotation());
				gameObject.GetComponent<Vehicle>().setVariation(vehicleSavable.colourVaration);
				VehicleStorage component = gameObject.GetComponent<VehicleStorage>();
				if ((bool)component && vehicleSavable.chestItems != null && vehicleSavable.chestItemStack != null)
				{
					component.loadStorage(vehicleSavable.chestItems, vehicleSavable.chestItemStack);
				}
				NetworkMapSharer.Instance.spawnGameObject(gameObject);
			}
		}
	}

	private void SaveDeeds()
	{
		try
		{
			DeedSave deedSave = new DeedSave();
			deedSave.saveDeeds(DeedManager.manage.deedDetails.ToArray());
			ES3.Save("deeds", deedSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Deeds: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadDeeds()
	{
		if (ES3.KeyExists("deeds", cacheFileSettings))
		{
			DeedSave deedSave = new DeedSave();
			ES3.LoadInto("deeds", deedSave, cacheFileSettings);
			deedSave.loadDeeds();
		}
	}

	private void SaveMapIcons()
	{
		try
		{
			MapIconSave mapIconSave = new MapIconSave();
			mapIconSave.saveIcons();
			ES3.Save("mapIcons", mapIconSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save map Icons: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadMapIcons()
	{
		if (ES3.KeyExists("mapIcons", cacheFileSettings))
		{
			MapIconSave mapIconSave = new MapIconSave();
			ES3.LoadInto("mapIcons", mapIconSave, cacheFileSettings);
			mapIconSave.LoadPlayerPlacedIcons();
		}
	}

	private void SaveCarriables()
	{
		try
		{
			CarrySave carrySave = new CarrySave();
			carrySave.saveAllCarryable();
			ES3.Save("carry", carrySave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Carriables: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadCarriables()
	{
		if (ES3.KeyExists("carry", cacheFileSettings))
		{
			CarrySave carrySave = new CarrySave();
			ES3.LoadInto("carry", carrySave, cacheFileSettings);
			carrySave.loadAllCarryable();
		}
	}

	private void SaveItemsOnTop()
	{
		try
		{
			ItemOnTopSave itemOnTopSave = new ItemOnTopSave();
			itemOnTopSave.saveObjectsOnTop();
			ES3.Save("itemsOnTop", itemOnTopSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save On Top: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadItemsOnTop()
	{
		if (ES3.KeyExists("itemsOnTop", cacheFileSettings))
		{
			ItemOnTopSave itemOnTopSave = new ItemOnTopSave();
			ES3.LoadInto("itemsOnTop", itemOnTopSave, cacheFileSettings);
			itemOnTopSave.loadObjectsOnTop();
		}
		else
		{
			MonoBehaviour.print("NO ITEMS ON TOP FOUND");
		}
	}

	private void SaveWeather()
	{
		try
		{
			WeatherSave weatherSave = new WeatherSave();
			weatherSave.SaveWeather();
			ES3.Save("weather", weatherSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Weather: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadWeather()
	{
		if (ES3.KeyExists("weather", cacheFileSettings))
		{
			WeatherSave weatherSave = new WeatherSave();
			ES3.LoadInto("weather", weatherSave, cacheFileSettings);
			weatherSave.LoadWeather();
		}
		else
		{
			WeatherManager.Instance.CreateNewWeatherPatterns();
		}
	}

	private void SaveFencedOffAnimals(bool endOfDaySave)
	{
		try
		{
			FencedOffAnimalSave fencedOffAnimalSave = new FencedOffAnimalSave();
			fencedOffAnimalSave.saveAnimals(endOfDaySave);
			ES3.Save("animalDetails", fencedOffAnimalSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Fenced Animals: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadFencedOffAnimals()
	{
		if (ES3.KeyExists("animalDetails", cacheFileSettings))
		{
			FencedOffAnimalSave fencedOffAnimalSave = new FencedOffAnimalSave();
			ES3.LoadInto("animalDetails", fencedOffAnimalSave, cacheFileSettings);
			fencedOffAnimalSave.loadAnimals();
		}
	}

	private void SaveChangers()
	{
		try
		{
			ChangerSave changerSave = new ChangerSave();
			changerSave.saveChangers();
			ES3.Save("changers", changerSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Changers: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadChangers()
	{
		if (ES3.KeyExists("changers", cacheFileSettings))
		{
			ChangerSave changerSave = new ChangerSave();
			ES3.LoadInto("changers", changerSave, cacheFileSettings);
			changerSave.loadChangers();
		}
	}

	private void SaveDrops()
	{
		try
		{
			DropSaves dropSaves = new DropSaves();
			dropSaves.saveDrops();
			ES3.Save("drops", dropSaves, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save drops: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadDrops()
	{
		if (ES3.KeyExists("drops", cacheFileSettings))
		{
			DropSaves dropSaves = new DropSaves();
			ES3.LoadInto("drops", dropSaves, cacheFileSettings);
			dropSaves.loadDrops();
		}
	}

	private void SavePhotos(bool isClient)
	{
		try
		{
			PhotoSave photoSave = new PhotoSave(PhotoManager.manage.savedPhotos, null);
			if (isClient)
			{
				if (ES3.KeyExists("photos", GetSaveSlotDirectory() + saveFilename))
				{
					PhotoSave photoSave2 = new PhotoSave();
					ES3.LoadInto("photos", GetSaveSlotDirectory() + saveFilename, photoSave2);
					photoSave = new PhotoSave(PhotoManager.manage.savedPhotos, photoSave2.displayedPhotosSave);
				}
				else
				{
					photoSave = new PhotoSave(PhotoManager.manage.savedPhotos, null);
				}
			}
			else
			{
				photoSave = new PhotoSave(PhotoManager.manage.savedPhotos, PhotoManager.manage.displayedPhotos);
			}
			ES3.Save("photos", photoSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save photos: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadPhotos(bool isClient = false)
	{
		if (ES3.KeyExists("photos", GetSaveSlotDirectory() + saveFilename))
		{
			PhotoSave photoSave = new PhotoSave();
			ES3.LoadInto("photos", GetSaveSlotDirectory() + saveFilename, photoSave);
			photoSave.loadPhotos(isClient);
			StartCoroutine(SaveLoad.saveOrLoad.populateJournalPhotos());
		}
	}

	private void SaveAnimalHouses()
	{
		try
		{
			AnimalHouseSave animalHouseSave = new AnimalHouseSave();
			List<AnimalHouse> list = new List<AnimalHouse>();
			for (int i = 0; i < FarmAnimalManager.manage.animalHouses.Count; i++)
			{
				if (FarmAnimalManager.manage.animalHouses[i] != null)
				{
					list.Add(FarmAnimalManager.manage.animalHouses[i]);
				}
			}
			animalHouseSave.animalHouses = list.ToArray();
			ES3.Save("animalHouseSave", animalHouseSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Inv: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadAnimalHouses()
	{
		if (ES3.KeyExists("animalHouseSave", cacheFileSettings))
		{
			AnimalHouseSave animalHouseSave = new AnimalHouseSave();
			ES3.LoadInto("animalHouseSave", animalHouseSave, cacheFileSettings);
			for (int i = 0; i < animalHouseSave.animalHouses.Length; i++)
			{
				FarmAnimalManager.manage.animalHouses.Add(animalHouseSave.animalHouses[i]);
				FarmAnimalManager.manage.placeNavmeshOnHouseFloor(animalHouseSave.animalHouses[i].xPos, animalHouseSave.animalHouses[i].yPos);
			}
		}
	}

	private void SaveFarmAnimalDetails()
	{
		try
		{
			FarmAnimalSave farmAnimalSave = new FarmAnimalSave();
			for (int i = 0; i < FarmAnimalManager.manage.farmAnimalDetails.Count; i++)
			{
				if (FarmAnimalManager.manage.activeAnimalAgents[FarmAnimalManager.manage.farmAnimalDetails[i].agentListId] != null)
				{
					FarmAnimalManager.manage.farmAnimalDetails[i].setPosition(FarmAnimalManager.manage.activeAnimalAgents[FarmAnimalManager.manage.farmAnimalDetails[i].agentListId].transform.position);
				}
			}
			farmAnimalSave.farmAnimalsToSave = FarmAnimalManager.manage.farmAnimalDetails.ToArray();
			ES3.Save("farmAnimalSave", farmAnimalSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Farm Animals: " + ex.Message);
			savedSucessfully = false;
		}
	}

	public void LoadFarmAnimals()
	{
		if (ES3.KeyExists("farmAnimalSave", cacheFileSettings))
		{
			FarmAnimalSave farmAnimalSave = new FarmAnimalSave();
			ES3.LoadInto("farmAnimalSave", farmAnimalSave, cacheFileSettings);
			for (int i = 0; i < farmAnimalSave.farmAnimalsToSave.Length; i++)
			{
				FarmAnimalManager.manage.farmAnimalDetails.Add(farmAnimalSave.farmAnimalsToSave[i]);
			}
		}
	}

	private void SaveBuried()
	{
		try
		{
			BuriedSave buriedSave = new BuriedSave();
			buriedSave.saveBuriedItems();
			ES3.Save("buried", buriedSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save burried: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadBuried()
	{
		if (ES3.KeyExists("buried", cacheFileSettings))
		{
			BuriedSave buriedSave = new BuriedSave();
			ES3.LoadInto("buried", buriedSave, cacheFileSettings);
			buriedSave.loadBuriedItems();
		}
	}

	private void SaveSigns()
	{
		try
		{
			SignSave signSave = new SignSave();
			signSave.saveSigns();
			ES3.Save("signs", signSave, cacheFileSettings);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save Signs: " + ex.Message);
			savedSucessfully = false;
		}
	}

	private void LoadSigns()
	{
		if (ES3.KeyExists("signs", cacheFileSettings))
		{
			SignSave signSave = new SignSave();
			ES3.LoadInto("signs", signSave, cacheFileSettings);
			signSave.loadSigns();
		}
	}

	private void SaveChests()
	{
		ES3.CreateBackup(GetSaveSlotDirectory() + chestSaveFile);
		AllChestSaves allChestSaves = new AllChestSaves();
		allChestSaves.SaveAllChests();
		ES3.Save("chests", allChestSaves, GetSaveSlotDirectory() + chestSaveFile);
	}

	public void LoadChests()
	{
		try
		{
			if (ES3.KeyExists("chests", GetSaveSlotDirectory() + chestSaveFile))
			{
				AllChestSaves allChestSaves = new AllChestSaves();
				ES3.LoadInto("chests", GetSaveSlotDirectory() + chestSaveFile, allChestSaves);
				allChestSaves.LoadAllChests();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load chests: " + ex.Message);
			if (ES3.RestoreBackup(GetSaveSlotDirectory() + chestSaveFile))
			{
				Debug.Log("Chest Backup restored.");
				LoadChests();
			}
			else
			{
				Debug.Log("No Chest Backup exists.");
			}
		}
	}

	private void fillBoardSave(BulletinBoardSave boardSave)
	{
		for (int i = 0; i < boardSave.allPosts.Length; i++)
		{
			boardSave.allPosts[i].populateOnLoad();
			BulletinBoard.board.attachedPosts.Add(boardSave.allPosts[i]);
		}
		if (BulletinBoard.board.attachedPosts.Count > 4)
		{
			List<PostOnBoard> list = new List<PostOnBoard>();
			for (int j = 0; j < 4; j++)
			{
				list.Add(BulletinBoard.board.attachedPosts[BulletinBoard.board.attachedPosts.Count - 1 - j]);
			}
			BulletinBoard.board.attachedPosts = list;
		}
		for (int k = 0; k < BulletinBoard.board.attachedPosts.Count; k++)
		{
			if (BulletinBoard.board.attachedPosts[k].checkIfAccepted() && !BulletinBoard.board.attachedPosts[k].checkIfExpired() && !BulletinBoard.board.attachedPosts[k].completed)
			{
				RenderMap.Instance.CreateTaskIcon(BulletinBoard.board.attachedPosts[k]);
			}
		}
	}
}
