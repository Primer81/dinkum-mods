using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoad : MonoBehaviour
{
	public static SaveLoad saveOrLoad;

	public SaveAndLoad newFileSaver;

	public bool loadingComplete;

	private bool quitAfterSave;

	public List<Vehicle> vehiclesToSave = new List<Vehicle>();

	public GameObject[] vehiclePrefabs;

	public GameObject[] carryablePrefabs;

	private int saveSlotToLoad = 1;

	public LoadingScreen loadingScreen;

	public List<NPCInventory> localInvs = new List<NPCInventory>();

	public bool isSaving;

	public bool loadingNewSaveFormat;

	public ContainerManager managerToLoadOldChestSaves;

	private Coroutine returnToMenuCoroutine;

	private int[] sittingPosOriginals;

	private void Awake()
	{
		saveOrLoad = this;
	}

	public string saveSlot()
	{
		return Path.Combine(Application.persistentDataPath + "\\Slot" + saveSlotToLoad);
	}

	public int currentSaveSlotNo()
	{
		return saveSlotToLoad;
	}

	public void findAFreeSlotForNewSave()
	{
		new DirectoryInfo(Application.persistentDataPath + "/Slot" + 0);
		for (int i = 0; i < 100; i++)
		{
			if (!new DirectoryInfo(Application.persistentDataPath + "/Slot" + i).Exists)
			{
				saveSlotToLoad = i;
				break;
			}
		}
	}

	public bool isASaveSlot()
	{
		new DirectoryInfo(Application.persistentDataPath + "/Slot" + 0);
		for (int i = 0; i < 100; i++)
		{
			if (new DirectoryInfo(Application.persistentDataPath + "/Slot" + i).Exists)
			{
				return true;
			}
		}
		return false;
	}

	public void quitGame()
	{
		if (isSaving)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			return;
		}
		if ((bool)SteamLobby.instance)
		{
			SteamLobby.instance.LeaveGameLobby();
		}
		Application.Quit();
	}

	public void returnToMenu()
	{
		if (isSaving)
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
		}
		else if (returnToMenuCoroutine == null)
		{
			returnToMenuCoroutine = StartCoroutine(returnToMenuDelay());
		}
	}

	public IEnumerator returnToMenuDelay()
	{
		if ((bool)SteamLobby.instance)
		{
			SteamLobby.instance.LeaveGameLobbyButton();
			yield return null;
		}
		if (NetworkMapSharer.Instance.isServer)
		{
			CustomNetworkManager.manage.StopHost();
		}
		else if (!NetworkMapSharer.Instance.isServer)
		{
			yield return StartCoroutine(saveRoutine(isServer: false, takePhoto: false, endOfDaySave: false, logOutSave: true));
			CustomNetworkManager.manage.StopClient();
			yield return null;
			SceneManager.LoadScene(1);
		}
	}

	public void SaveChests()
	{
		if (!NetworkMapSharer.Instance.isServer)
		{
			return;
		}
		foreach (Chest activeChest in ContainerManager.manage.activeChests)
		{
			ContainerManager.manage.saveChest(activeChest);
		}
	}

	public void SaveGame(bool isServer, bool takePhoto = true, bool endOfDaySave = true)
	{
		StartCoroutine(saveRoutine(isServer, takePhoto, endOfDaySave));
	}

	public IEnumerator saveRoutine(bool isServer, bool takePhoto = true, bool endOfDaySave = true, bool logOutSave = false)
	{
		isSaving = true;
		if ((isServer && RealWorldTimeLight.time.underGround) || (isServer && RealWorldTimeLight.time.offIsland))
		{
			yield return null;
		}
		else
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(saveSlot());
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			saveVersionNumber();
			SaveInv();
			saveLicences();
			saveRecipesUnlocked();
			saveLevels();
			savePedia();
			saveNpcRelations();
			saveMail();
			yield return null;
			if (takePhoto)
			{
				CharacterCreatorScript.create.takeSlotPhotoAndSave();
			}
			if (isServer)
			{
				HouseManager.manage.clearAllFurnitureStatus();
				sittingPosOriginals = new int[NetworkNavMesh.nav.charsConnected.Count];
				for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
				{
					CharPickUp component = NetworkNavMesh.nav.charsConnected[i].GetComponent<CharPickUp>();
					if ((bool)component && component.sitting && component.myInteract.InsideHouseDetails == null)
					{
						sittingPosOriginals[i] = WorldManager.Instance.onTileStatusMap[component.sittingXpos, component.sittingYPos];
						WorldManager.Instance.onTileStatusMap[component.sittingXpos, component.sittingYPos] = 0;
					}
				}
				saveQuests();
				saveDate();
				saveTownManager();
				SaveChests();
				saveHouse();
				saveTownStatus();
				saveBulletinBoard();
				saveMuseum();
				saveVehicles();
				saveDeeds();
				saveMapIcons();
				saveCarriables();
				saveItemsOnTop();
				SaveWeather();
				yield return StartCoroutine(saveOverFrames(endOfDaySave));
				saveFencedOffAnimals(endOfDaySave);
				saveChangers();
				FarmAnimalManager.manage.saveAnimalHouses();
				FarmAnimalManager.manage.saveFarmAnimalDetails();
				savePhotos(isClient: false);
				yield return new WaitForSeconds(3f);
				yield return null;
				saveDrops();
				yield return new WaitForSeconds(1f);
				NetworkMapSharer.Instance.NetworknextDayIsReady = true;
			}
			else
			{
				savePhotos(isClient: true);
				if (NetworkMapSharer.Instance.nextDayIsReady)
				{
					StartCoroutine(nonServerSave());
				}
			}
		}
		EasySaveAfter(isServer);
		if (!logOutSave)
		{
			saveOrLoad.loadingScreen.saveGameConfirmed.gameObject.SetActive(value: true);
			yield return new WaitForSeconds(1f);
			StartCoroutine(saveOrLoad.loadingScreen.saveGameConfirmed.GetComponent<WindowAnimator>().closeWithMask());
		}
		isSaving = false;
	}

	public void EasySaveAfter(bool isServer)
	{
		if (!RealWorldTimeLight.time.underGround && !RealWorldTimeLight.time.offIsland)
		{
			EasySaveInv();
			EasySaveLicences();
			EasySaveRecipes();
			EasySaveLevels();
			EasySavePedia();
			EasySaveNPCRelations();
			EasySaveMail();
			if (isServer)
			{
				EasySaveQuests();
				EasySaveDate();
				EasySaveTownManager();
				EasySaveHouses();
				EasySaveTownStatus();
				EasySaveBulletinBoard();
				EasySaveMuseum();
				EasySaveVehicles();
				EasySaveDeeds();
				EasySaveMapIcons();
				EasySaveDrops();
				EasySaveCarriables();
				EasySaveOnTop();
				FarmAnimalManager.manage.EasySaveAnimalHouses();
				FarmAnimalManager.manage.EasySaveFarmAnimals();
				EasySaveBuried();
				EasySaveSigns();
			}
		}
	}

	public IEnumerator nonServerSave()
	{
		loadingScreen.loadingBarOnlyAppear();
		yield return new WaitForSeconds(1f);
		loadingScreen.completed();
	}

	public void saveVehiclesForUnderGround()
	{
		for (int i = 0; i < vehiclesToSave.Count; i++)
		{
			if (vehiclesToSave[i] != null)
			{
				NetworkMapSharer.Instance.unSpawnGameObject(vehiclesToSave[i].gameObject);
				vehiclesToSave[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void loadVehiclesForAboveGround()
	{
		for (int i = 0; i < vehiclesToSave.Count; i++)
		{
			if (vehiclesToSave[i] != null)
			{
				vehiclesToSave[i].gameObject.SetActive(value: true);
				NetworkMapSharer.Instance.spawnGameObject(vehiclesToSave[i].gameObject);
				vehiclesToSave[i].stopVehicleFallingOnMapChange();
			}
		}
	}

	public void EasySaveVehicles()
	{
		List<VehicleSavable> list = new List<VehicleSavable>();
		for (int i = 0; i < vehiclesToSave.Count; i++)
		{
			if (vehiclesToSave[i] != null)
			{
				list.Add(new VehicleSavable(vehiclesToSave[i]));
			}
		}
		VehicleSave vehicleSave = new VehicleSave();
		vehicleSave.allVehicles = list.ToArray();
		try
		{
			ES3.Save("vehicleInfo", vehicleSave, saveSlot() + "/vehicleInfo.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/vehicleInfo.es3");
			ES3.Save("vehicleInfo", vehicleSave, saveSlot() + "/vehicleInfo.es3");
		}
	}

	public bool EasyLoadVehicles()
	{
		try
		{
			if (ES3.KeyExists("vehicleInfo", saveSlot() + "/vehicleInfo.es3"))
			{
				VehicleSave vehicleSave = new VehicleSave();
				ES3.LoadInto("vehicleInfo", saveSlot() + "/vehicleInfo.es3", vehicleSave);
				VehicleSavable[] allVehicles = vehicleSave.allVehicles;
				foreach (VehicleSavable vehicleSavable in allVehicles)
				{
					if (vehicleSavable.vehicleId == 3)
					{
						MonoBehaviour.print("spawning mu mount instead of mu vehicle");
						StartCoroutine(delayReplaceMuMount(vehicleSavable.getPosition()));
					}
					else if (vehicleSavable.vehicleId < vehiclePrefabs.Length)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(vehiclePrefabs[vehicleSavable.vehicleId], vehicleSavable.getPosition(), vehicleSavable.getRotation());
						gameObject.GetComponent<Vehicle>().setVariation(vehicleSavable.colourVaration);
						NetworkMapSharer.Instance.spawnGameObject(gameObject);
					}
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveVehicles()
	{
		List<VehicleSavable> list = new List<VehicleSavable>();
		for (int i = 0; i < vehiclesToSave.Count; i++)
		{
			if (vehiclesToSave[i] != null)
			{
				list.Add(new VehicleSavable(vehiclesToSave[i]));
			}
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			VehicleSave graph = new VehicleSave
			{
				allVehicles = list.ToArray()
			};
			fileStream = File.Create(saveSlot() + "/vehicleInfo.dat");
			binaryFormatter.Serialize(fileStream, graph);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving vehicles");
			fileStream?.Close();
		}
	}

	public void savePhotos(bool isClient)
	{
		PhotoSave graph = new PhotoSave(PhotoManager.manage.savedPhotos, null);
		if (isClient)
		{
			if (File.Exists(saveSlot() + "/photoDetails.dat"))
			{
				FileStream fileStream = null;
				try
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					fileStream = File.Open(saveSlot() + "/photoDetails.dat", FileMode.Open);
					PhotoSave photoSave = (PhotoSave)binaryFormatter.Deserialize(fileStream);
					fileStream.Close();
					graph = new PhotoSave(PhotoManager.manage.savedPhotos, photoSave.displayedPhotosSave);
				}
				catch (Exception)
				{
					Debug.LogWarning("error saving photos");
					fileStream?.Close();
				}
			}
			else
			{
				graph = new PhotoSave(PhotoManager.manage.savedPhotos, null);
			}
		}
		else
		{
			graph = new PhotoSave(PhotoManager.manage.savedPhotos, PhotoManager.manage.displayedPhotos);
		}
		FileStream fileStream2 = null;
		try
		{
			BinaryFormatter binaryFormatter2 = new BinaryFormatter();
			fileStream2 = File.Create(saveSlot() + "/photoDetails.dat");
			binaryFormatter2.Serialize(fileStream2, graph);
			fileStream2.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving photos");
			fileStream2?.Close();
		}
	}

	public void loadPhotos(bool isClient = false)
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadPhotos(isClient);
			return;
		}
		if (File.Exists(saveSlot() + "/photoDetails.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/photoDetails.dat", FileMode.Open);
				((PhotoSave)binaryFormatter.Deserialize(fileStream)).loadPhotos(isClient);
				StartCoroutine(populateJournalPhotos());
				fileStream.Close();
				makeABackUp(saveSlot() + "/photoDetails.dat", saveSlot() + "/photoDetails.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading photo save.");
				fileStream?.Close();
				loadBackupPhotos();
				return;
			}
		}
		loadBackupPhotos();
	}

	public void loadBackupPhotos(bool isClient = false)
	{
		if (File.Exists(saveSlot() + "/photoDetails.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/photoDetails.bak", FileMode.Open);
				((PhotoSave)binaryFormatter.Deserialize(fileStream)).loadPhotos(isClient);
				StartCoroutine(populateJournalPhotos());
				fileStream.Close();
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading photo backup.");
				fileStream?.Close();
			}
		}
	}

	public IEnumerator populateJournalPhotos()
	{
		yield return null;
		PhotoManager.manage.populatePhotoButtons();
	}

	public void makeABackUp(string fileName, string backupName)
	{
		try
		{
			File.Copy(fileName, backupName, overwrite: true);
		}
		catch (Exception)
		{
			Debug.LogWarning("Error backing up file: " + fileName);
		}
	}

	public IEnumerator delayReplaceMuMount(Vector3 spawnPos)
	{
		while (!NetworkMapSharer.Instance.serverActive())
		{
			yield return null;
		}
		while (!FarmAnimalManager.manage)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		FarmAnimalManager.manage.spawnNewFarmAnimalWithDetails(33, 0, "Mu", spawnPos);
	}

	public void loadVehicles()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadVehicles();
		}
		if (File.Exists(saveSlot() + "/vehicleInfo.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/vehicleInfo.dat", FileMode.Open);
				VehicleSavable[] allVehicles = ((VehicleSave)binaryFormatter.Deserialize(fileStream)).allVehicles;
				foreach (VehicleSavable vehicleSavable in allVehicles)
				{
					if (vehicleSavable.vehicleId == 3)
					{
						MonoBehaviour.print("spawning mu mount instead of mu vehicle");
						StartCoroutine(delayReplaceMuMount(vehicleSavable.getPosition()));
					}
					else if (vehicleSavable.vehicleId < vehiclePrefabs.Length)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(vehiclePrefabs[vehicleSavable.vehicleId], vehicleSavable.getPosition(), vehicleSavable.getRotation());
						gameObject.GetComponent<Vehicle>().setVariation(vehicleSavable.colourVaration);
						VehicleStorage component = gameObject.GetComponent<VehicleStorage>();
						if ((bool)component && vehicleSavable.chestItems != null && vehicleSavable.chestItemStack != null)
						{
							component.loadStorage(vehicleSavable.chestItems, vehicleSavable.chestItemStack);
						}
						NetworkMapSharer.Instance.spawnGameObject(gameObject);
					}
				}
				fileStream.Close();
				makeABackUp(saveSlot() + "/vehicleInfo.dat", saveSlot() + "/vehicleInfo.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading vehicles");
				fileStream?.Close();
				loadBackupVehicles();
				return;
			}
		}
		loadBackupVehicles();
	}

	public void loadBackupVehicles()
	{
		if (EasyLoadVehicles() || !File.Exists(saveSlot() + "/vehicleInfo.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/vehicleInfo.bak", FileMode.Open);
			VehicleSavable[] allVehicles = ((VehicleSave)binaryFormatter.Deserialize(fileStream)).allVehicles;
			foreach (VehicleSavable vehicleSavable in allVehicles)
			{
				if (vehicleSavable.vehicleId == 3)
				{
					MonoBehaviour.print("spawning mu mount instead of mu vehicle");
					StartCoroutine(delayReplaceMuMount(vehicleSavable.getPosition()));
				}
				else if (vehicleSavable.vehicleId < vehiclePrefabs.Length)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(vehiclePrefabs[vehicleSavable.vehicleId], vehicleSavable.getPosition(), vehicleSavable.getRotation());
					gameObject.GetComponent<Vehicle>().setVariation(vehicleSavable.colourVaration);
					NetworkMapSharer.Instance.spawnGameObject(gameObject);
				}
			}
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading vehicles backup");
			fileStream?.Close();
		}
	}

	public void EasySaveInv()
	{
		PlayerInv playerInv = new PlayerInv();
		playerInv.money = Inventory.Instance.wallet;
		playerInv.bankBalance = BankMenu.menu.accountBalance;
		playerInv.accountOverflow = BankMenu.menu.accountOverflow;
		playerInv.playerName = Inventory.Instance.playerName;
		playerInv.islandName = Inventory.Instance.islandName;
		playerInv.eyeStyle = Inventory.Instance.playerEyes;
		playerInv.eyeColour = Inventory.Instance.playerEyeColor;
		playerInv.skinTone = Inventory.Instance.skinTone;
		playerInv.nose = Inventory.Instance.nose;
		playerInv.mouth = Inventory.Instance.mouth;
		playerInv.face = EquipWindow.equip.faceSlot.itemNo;
		playerInv.hair = Inventory.Instance.playerHair;
		playerInv.hairColour = Inventory.Instance.playerHairColour;
		playerInv.head = EquipWindow.equip.hatSlot.itemNo;
		playerInv.body = EquipWindow.equip.shirtSlot.itemNo;
		playerInv.pants = EquipWindow.equip.pantsSlot.itemNo;
		playerInv.shoes = EquipWindow.equip.shoeSlot.itemNo;
		playerInv.health = StatusManager.manage.connectedDamge.health;
		playerInv.healthMax = StatusManager.manage.connectedDamge.maxHealth;
		playerInv.stamina = StatusManager.manage.getStamina();
		playerInv.staminaMax = StatusManager.manage.getStaminaMax();
		playerInv.snagsEaten = StatusManager.manage.snagsEaten;
		playerInv.catalogue = CatalogueManager.manage.collectedItem;
		playerInv.savedTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
		playerInv.itemsInInvSlots = new int[Inventory.Instance.invSlots.Length];
		playerInv.stacksInSlots = new int[Inventory.Instance.invSlots.Length];
		for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
		{
			playerInv.itemsInInvSlots[i] = Inventory.Instance.invSlots[i].itemNo;
			playerInv.stacksInSlots[i] = Inventory.Instance.invSlots[i].stack;
		}
		try
		{
			ES3.Save("playerInfo", playerInv, saveSlot() + "/playerInfo.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/playerInfo.es3");
			ES3.Save("playerInfo", playerInv, saveSlot() + "/playerInfo.es3");
		}
	}

	public bool EasyInvExists()
	{
		if (ES3.KeyExists("playerInfo", saveSlot() + "/playerInfo.es3"))
		{
			return true;
		}
		return false;
	}

	public PlayerInv EasyInvForLoadSlot()
	{
		try
		{
			if (ES3.KeyExists("playerInfo", saveSlot() + "/playerInfo.es3"))
			{
				PlayerInv playerInv = new PlayerInv();
				ES3.LoadInto("playerInfo", saveSlot() + "/playerInfo.es3", playerInv);
				return playerInv;
			}
		}
		catch
		{
		}
		return null;
	}

	public bool EasyLoadInv()
	{
		try
		{
			if (ES3.KeyExists("playerInfo", saveSlot() + "/playerInfo.es3"))
			{
				PlayerInv playerInv = new PlayerInv();
				ES3.LoadInto("playerInfo", saveSlot() + "/playerInfo.es3", playerInv);
				Inventory.Instance.changeWalletToLoad(playerInv.money);
				BankMenu.menu.accountBalance = playerInv.bankBalance;
				BankMenu.menu.accountOverflow = playerInv.accountOverflow;
				if (playerInv.hair < 0)
				{
					playerInv.hair = Mathf.Abs(playerInv.hair + 1);
				}
				Inventory.Instance.playerHair = playerInv.hair;
				Inventory.Instance.playerHairColour = playerInv.hairColour;
				Inventory.Instance.playerEyes = playerInv.eyeStyle;
				Inventory.Instance.nose = playerInv.nose;
				Inventory.Instance.mouth = playerInv.mouth;
				Inventory.Instance.playerEyeColor = playerInv.eyeColour;
				Inventory.Instance.skinTone = playerInv.skinTone;
				Inventory.Instance.playerName = playerInv.playerName;
				Inventory.Instance.islandName = playerInv.islandName;
				EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(playerInv.head, 1);
				EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(playerInv.face, 1);
				EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(playerInv.body, 1);
				EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(playerInv.pants, 1);
				EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(playerInv.shoes, 1);
				StatusManager.manage.snagsEaten = playerInv.snagsEaten;
				StartCoroutine(EquipWindow.equip.wearingMinersHelmet());
				if (playerInv.catalogue != null)
				{
					for (int i = 0; i < playerInv.catalogue.Length; i++)
					{
						CatalogueManager.manage.collectedItem[i] = playerInv.catalogue[i];
					}
				}
				StatusManager.manage.loadStatus(playerInv.health, playerInv.healthMax, playerInv.stamina, playerInv.staminaMax);
				for (int j = 0; j < playerInv.itemsInInvSlots.Length; j++)
				{
					Inventory.Instance.invSlots[j].itemNo = playerInv.itemsInInvSlots[j];
					Inventory.Instance.invSlots[j].stack = playerInv.stacksInSlots[j];
					Inventory.Instance.invSlots[j].updateSlotContentsAndRefresh(playerInv.itemsInInvSlots[j], playerInv.stacksInSlots[j]);
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void SaveInv()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/playerInfo.dat");
			PlayerInv playerInv = new PlayerInv();
			playerInv.money = Inventory.Instance.wallet;
			playerInv.bankBalance = BankMenu.menu.accountBalance;
			playerInv.accountOverflow = BankMenu.menu.accountOverflow;
			playerInv.playerName = Inventory.Instance.playerName;
			playerInv.islandName = Inventory.Instance.islandName;
			playerInv.eyeStyle = Inventory.Instance.playerEyes;
			playerInv.eyeColour = Inventory.Instance.playerEyeColor;
			playerInv.skinTone = Inventory.Instance.skinTone;
			playerInv.nose = Inventory.Instance.nose;
			playerInv.mouth = Inventory.Instance.mouth;
			playerInv.face = EquipWindow.equip.faceSlot.itemNo;
			playerInv.hair = Inventory.Instance.playerHair;
			playerInv.hairColour = Inventory.Instance.playerHairColour;
			playerInv.head = EquipWindow.equip.hatSlot.itemNo;
			playerInv.body = EquipWindow.equip.shirtSlot.itemNo;
			playerInv.pants = EquipWindow.equip.pantsSlot.itemNo;
			playerInv.shoes = EquipWindow.equip.shoeSlot.itemNo;
			playerInv.health = StatusManager.manage.connectedDamge.health;
			playerInv.healthMax = StatusManager.manage.connectedDamge.maxHealth;
			playerInv.stamina = StatusManager.manage.getStamina();
			playerInv.staminaMax = StatusManager.manage.getStaminaMax();
			playerInv.snagsEaten = StatusManager.manage.snagsEaten;
			playerInv.catalogue = CatalogueManager.manage.collectedItem;
			playerInv.savedTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
			playerInv.itemsInInvSlots = new int[Inventory.Instance.invSlots.Length];
			playerInv.stacksInSlots = new int[Inventory.Instance.invSlots.Length];
			for (int i = 0; i < Inventory.Instance.invSlots.Length; i++)
			{
				playerInv.itemsInInvSlots[i] = Inventory.Instance.invSlots[i].itemNo;
				playerInv.stacksInSlots[i] = Inventory.Instance.invSlots[i].stack;
			}
			binaryFormatter.Serialize(fileStream, playerInv);
			fileStream.Close();
			ContainerManager.manage.saveStashes();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving player ");
			fileStream?.Close();
		}
	}

	public void EasySaveLicences()
	{
		LicenceAndPermitPointSave licenceAndPermitPointSave = new LicenceAndPermitPointSave();
		licenceAndPermitPointSave.saveLicencesAndPoints();
		try
		{
			ES3.Save("licences", licenceAndPermitPointSave, saveSlot() + "/licences.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/licences.es3");
			ES3.Save("licences", licenceAndPermitPointSave, saveSlot() + "/licences.es3");
		}
	}

	public bool EasyLoadLicences()
	{
		try
		{
			if (ES3.KeyExists("licences", saveSlot() + "/licences.es3"))
			{
				LicenceAndPermitPointSave licenceAndPermitPointSave = new LicenceAndPermitPointSave();
				ES3.LoadInto("licences", saveSlot() + "/licences.es3", licenceAndPermitPointSave);
				licenceAndPermitPointSave.loadLicencesAndPoints();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void saveLicences()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/licences.dat");
			LicenceAndPermitPointSave licenceAndPermitPointSave = new LicenceAndPermitPointSave();
			licenceAndPermitPointSave.saveLicencesAndPoints();
			binaryFormatter.Serialize(fileStream, licenceAndPermitPointSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving licences.");
			fileStream?.Close();
		}
	}

	public void loadLicences()
	{
		if (File.Exists(saveSlot() + "/licences.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/licences.dat", FileMode.Open);
				LicenceAndPermitPointSave obj = (LicenceAndPermitPointSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadLicencesAndPoints();
				makeABackUp(saveSlot() + "/licences.dat", saveSlot() + "/licences.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading licences.");
				fileStream?.Close();
				loadLicencesBackup();
				return;
			}
		}
		loadLicencesBackup();
	}

	public void loadLicencesBackup()
	{
		if (EasyLoadLicences())
		{
			return;
		}
		Debug.LogWarning("Loading licences backup");
		if (!File.Exists(saveSlot() + "/licences.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/licences.bak", FileMode.Open);
			LicenceAndPermitPointSave obj = (LicenceAndPermitPointSave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			obj.loadLicencesAndPoints();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving licences backup");
			fileStream?.Close();
		}
	}

	public void EasySaveQuests()
	{
		QuestSave questSave = new QuestSave();
		questSave.accepted = QuestManager.manage.isQuestAccepted;
		questSave.completed = QuestManager.manage.isQuestCompleted;
		try
		{
			ES3.Save("quests", questSave, saveSlot() + "/quests.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/quests.es3");
			ES3.Save("quests", questSave, saveSlot() + "/quests.es3");
		}
	}

	public bool EasyLoadQuests()
	{
		try
		{
			if (ES3.KeyExists("quests", saveSlot() + "/quests.es3"))
			{
				QuestSave questSave = new QuestSave();
				ES3.LoadInto("quests", saveSlot() + "/quests.es3", questSave);
				for (int i = 0; i < questSave.completed.Length; i++)
				{
					QuestManager.manage.isQuestCompleted[i] = questSave.completed[i];
					QuestManager.manage.isQuestAccepted[i] = questSave.accepted[i];
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveQuests()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/quests.dat");
			binaryFormatter.Serialize(fileStream, new QuestSave
			{
				accepted = QuestManager.manage.isQuestAccepted,
				completed = QuestManager.manage.isQuestCompleted
			});
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving quests");
			fileStream?.Close();
		}
	}

	private void loadQuests()
	{
		if (File.Exists(saveSlot() + "/quests.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/quests.dat", FileMode.Open);
				QuestSave questSave = (QuestSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				makeABackUp(saveSlot() + "/quests.dat", saveSlot() + "/quests.bak");
				for (int i = 0; i < questSave.completed.Length; i++)
				{
					QuestManager.manage.isQuestCompleted[i] = questSave.completed[i];
					QuestManager.manage.isQuestAccepted[i] = questSave.accepted[i];
				}
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading quest save.");
				fileStream?.Close();
				loadQuestsBackup();
				return;
			}
		}
		loadQuestsBackup();
	}

	private void loadQuestsBackup()
	{
		if (EasyLoadQuests())
		{
			return;
		}
		Debug.LogWarning("Loading quest save backup");
		if (!File.Exists(saveSlot() + "/quests.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/quests.bak", FileMode.Open);
			QuestSave questSave = (QuestSave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			for (int i = 0; i < questSave.completed.Length; i++)
			{
				QuestManager.manage.isQuestCompleted[i] = questSave.completed[i];
				QuestManager.manage.isQuestAccepted[i] = questSave.accepted[i];
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading quest save backup");
			fileStream?.Close();
		}
	}

	public void clearSavedStatsForClient()
	{
	}

	public void EasySaveTownManager()
	{
		TownManagerSave townManagerSave = new TownManagerSave();
		townManagerSave.saveTown();
		try
		{
			ES3.Save("townSave", townManagerSave, saveSlot() + "/townSave.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/townSave.es3");
			ES3.Save("townSave", townManagerSave, saveSlot() + "/townSave.es3");
		}
	}

	public bool EasyLoadTownManager()
	{
		try
		{
			if (ES3.KeyExists("townSave", saveSlot() + "/townSave.es3"))
			{
				TownManagerSave townManagerSave = new TownManagerSave();
				ES3.LoadInto("townSave", saveSlot() + "/townSave.es3", townManagerSave);
				townManagerSave.load();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveTownManager()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/townSave.dat");
			TownManagerSave townManagerSave = new TownManagerSave();
			townManagerSave.saveTown();
			binaryFormatter.Serialize(fileStream, townManagerSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving quests");
			fileStream?.Close();
		}
	}

	public void EasySaveDate()
	{
		DateSave dateSave = new DateSave();
		dateSave = WorldManager.Instance.getDateSave();
		dateSave.todaysMineSeed = NetworkMapSharer.Instance.mineSeed;
		dateSave.tomorrowsMineSeed = NetworkMapSharer.Instance.tomorrowsMineSeed;
		dateSave.hour = RealWorldTimeLight.time.currentHour;
		dateSave.minute = RealWorldTimeLight.time.currentMinute;
		try
		{
			ES3.Save("date", dateSave, saveSlot() + "/date.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/date.es3");
			ES3.Save("date", dateSave, saveSlot() + "/date.es3");
		}
	}

	public bool EasyLoadDate()
	{
		try
		{
			if (ES3.KeyExists("date", saveSlot() + "/date.es3"))
			{
				DateSave dateSave = new DateSave();
				ES3.LoadInto("date", saveSlot() + "/date.es3", dateSave);
				WorldManager.Instance.loadDateFromSave(dateSave);
				NetworkMapSharer.Instance.NetworkmineSeed = dateSave.todaysMineSeed;
				NetworkMapSharer.Instance.tomorrowsMineSeed = dateSave.tomorrowsMineSeed;
				RealWorldTimeLight.time.NetworkcurrentHour = dateSave.hour;
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void EasySaveBuried()
	{
		BuriedSave buriedSave = new BuriedSave();
		buriedSave.saveBuriedItems();
		try
		{
			ES3.Save("buried", buriedSave, saveSlot() + "/buried.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/buried.es3");
			ES3.Save("buried", buriedSave, saveSlot() + "/buried.es3");
		}
	}

	public bool EasyLoadBuried()
	{
		try
		{
			if (ES3.KeyExists("buried", saveSlot() + "/buried.es3"))
			{
				BuriedSave buriedSave = new BuriedSave();
				ES3.LoadInto("buried", saveSlot() + "/buried.es3", buriedSave);
				buriedSave.loadBuriedItems();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void EasySaveSigns()
	{
		SignSave signSave = new SignSave();
		signSave.saveSigns();
		try
		{
			ES3.Save("signs", signSave, saveSlot() + "/signs.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/signs.es3");
			ES3.Save("signs", signSave, saveSlot() + "/signs.es3");
		}
	}

	public bool EasyLoadSigns()
	{
		try
		{
			if (ES3.KeyExists("signs", saveSlot() + "/signs.es3"))
			{
				SignSave signSave = new SignSave();
				ES3.LoadInto("signs", saveSlot() + "/signs.es3", signSave);
				signSave.loadSigns();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveDate()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/date.dat");
			DateSave dateSave = new DateSave();
			dateSave = WorldManager.Instance.getDateSave();
			dateSave.todaysMineSeed = NetworkMapSharer.Instance.mineSeed;
			dateSave.tomorrowsMineSeed = NetworkMapSharer.Instance.tomorrowsMineSeed;
			dateSave.hour = RealWorldTimeLight.time.currentHour;
			dateSave.minute = RealWorldTimeLight.time.currentMinute;
			binaryFormatter.Serialize(fileStream, dateSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving quests");
			fileStream?.Close();
		}
	}

	private void loadDate()
	{
		if (DoesSaveExist() && File.Exists(saveSlot() + "/date.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/date.dat", FileMode.Open);
				DateSave dateSave = (DateSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				WorldManager.Instance.loadDateFromSave(dateSave);
				NetworkMapSharer.Instance.NetworkmineSeed = dateSave.todaysMineSeed;
				NetworkMapSharer.Instance.tomorrowsMineSeed = dateSave.tomorrowsMineSeed;
				RealWorldTimeLight.time.NetworkcurrentHour = dateSave.hour;
				RealWorldTimeLight.time.currentMinute = dateSave.minute;
				makeABackUp(saveSlot() + "/date.dat", saveSlot() + "/date.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading date file.");
				fileStream?.Close();
				loadDateBackup();
				return;
			}
		}
		loadDateBackup();
	}

	private void loadDateBackup()
	{
		if (EasyLoadDate())
		{
			return;
		}
		Debug.LogWarning("Loading date backup file.");
		if (!DoesSaveExist() || !File.Exists(saveSlot() + "/date.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/date.bak", FileMode.Open);
			DateSave dateSave = (DateSave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			WorldManager.Instance.loadDateFromSave(dateSave);
			NetworkMapSharer.Instance.NetworkmineSeed = dateSave.todaysMineSeed;
			NetworkMapSharer.Instance.tomorrowsMineSeed = dateSave.tomorrowsMineSeed;
			RealWorldTimeLight.time.NetworkcurrentHour = dateSave.hour;
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading date backup file.");
			fileStream?.Close();
		}
	}

	private void loadTown()
	{
		if (DoesSaveExist() && File.Exists(saveSlot() + "/townSave.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/townSave.dat", FileMode.Open);
				((TownManagerSave)binaryFormatter.Deserialize(fileStream)).load();
				fileStream.Close();
				makeABackUp(saveSlot() + "/townSave.dat", saveSlot() + "/townSave.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading town save");
				fileStream?.Close();
				loadTownBackup();
				return;
			}
		}
		loadTownBackup();
	}

	private void loadTownBackup()
	{
		if (EasyLoadTownManager())
		{
			return;
		}
		Debug.LogWarning("Loading town save backup");
		if (!DoesSaveExist() || !File.Exists(saveSlot() + "/townSave.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/townSave.bak", FileMode.Open);
			((TownManagerSave)binaryFormatter.Deserialize(fileStream)).load();
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading town save backup");
			fileStream?.Close();
		}
	}

	public void EasySaveOnTop()
	{
		ItemOnTopSave itemOnTopSave = new ItemOnTopSave();
		itemOnTopSave.saveObjectsOnTop();
		try
		{
			ES3.Save("onTop", itemOnTopSave, saveSlot() + "/onTop.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/onTop.es3");
			ES3.Save("onTop", itemOnTopSave, saveSlot() + "/onTop.es3");
		}
	}

	public bool EasyLoadOnTop()
	{
		try
		{
			if (ES3.KeyExists("onTop", saveSlot() + "/onTop.es3"))
			{
				ItemOnTopSave itemOnTopSave = new ItemOnTopSave();
				ES3.LoadInto("onTop", saveSlot() + "/onTop.es3", itemOnTopSave);
				itemOnTopSave.loadObjectsOnTop();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void loadItemsOnTop()
	{
		if (DoesSaveExist() && File.Exists(saveSlot() + "/onTop.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/onTop.dat", FileMode.Open);
				((ItemOnTopSave)binaryFormatter.Deserialize(fileStream)).loadObjectsOnTop();
				fileStream.Close();
				makeABackUp(saveSlot() + "/onTop.dat", saveSlot() + "/onTop.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading on top files.");
				fileStream?.Close();
				loadItemsOnTopBackup();
				return;
			}
		}
		loadItemsOnTopBackup();
	}

	private void loadItemsOnTopBackup()
	{
		if (EasyLoadOnTop())
		{
			return;
		}
		Debug.LogWarning("Loading on top files backup");
		if (!DoesSaveExist() || !File.Exists(saveSlot() + "/onTop.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/onTop.bak", FileMode.Open);
			((ItemOnTopSave)binaryFormatter.Deserialize(fileStream)).loadObjectsOnTop();
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading on top files backup");
			fileStream?.Close();
		}
	}

	private void saveItemsOnTop()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/onTop.dat");
			ItemOnTopSave itemOnTopSave = new ItemOnTopSave();
			itemOnTopSave.saveObjectsOnTop();
			binaryFormatter.Serialize(fileStream, itemOnTopSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving items on top");
			fileStream?.Close();
		}
	}

	private void saveNpcRelations()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/npc.dat");
			NPCsave nPCsave = new NPCsave
			{
				savedStatuses = NPCManager.manage.npcStatus.ToArray()
			};
			if (NetworkMapSharer.Instance.isServer)
			{
				nPCsave.saveInvs = NPCManager.manage.npcInvs.ToArray();
			}
			else
			{
				nPCsave.saveInvs = localInvs.ToArray();
			}
			binaryFormatter.Serialize(fileStream, nPCsave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving NPC relationship");
			fileStream?.Close();
		}
	}

	public void EasySaveNPCRelations()
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
		try
		{
			ES3.Save("npc", nPCsave, saveSlot() + "/npc.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/npc.es3");
			ES3.Save("npc", nPCsave, saveSlot() + "/npc.es3");
		}
	}

	public bool EasyLoadNPCRelations()
	{
		try
		{
			if (ES3.KeyExists("npc", saveSlot() + "/npc.es3"))
			{
				NPCsave nPCsave = new NPCsave();
				ES3.LoadInto("npc", saveSlot() + "/npc.es3", nPCsave);
				nPCsave.loadNpcs();
				localInvs = NPCManager.manage.npcInvs;
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void loadNpcRelations()
	{
		if (File.Exists(saveSlot() + "/npc.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/npc.dat", FileMode.Open);
				NPCsave obj = (NPCsave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadNpcs();
				localInvs = NPCManager.manage.npcInvs;
				makeABackUp(saveSlot() + "/npc.dat", saveSlot() + "/npc.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading on NPC status.");
				fileStream?.Close();
				loadNpcRelationsBackup();
				return;
			}
		}
		loadNpcRelationsBackup();
	}

	private void loadNpcRelationsBackup()
	{
		if (EasyLoadNPCRelations())
		{
			return;
		}
		Debug.LogWarning("Loading on NPC status backup");
		if (!File.Exists(saveSlot() + "/npc.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/npc.bak", FileMode.Open);
			NPCsave obj = (NPCsave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			obj.loadNpcs();
			localInvs = NPCManager.manage.npcInvs;
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading on NPC status backup");
			fileStream?.Close();
		}
	}

	private void EasySaveRecipes()
	{
		RecipesUnlockedSave recipesUnlockedSave = new RecipesUnlockedSave();
		recipesUnlockedSave.crafterLevel = CraftsmanManager.manage.currentLevel;
		recipesUnlockedSave.currentPoints = CraftsmanManager.manage.currentPoints;
		recipesUnlockedSave.crafterHasBerkonium = CraftsmanManager.manage.craftsmanHasBerkonium;
		recipesUnlockedSave.crafterWorkingOnItemId = CraftsmanManager.manage.itemCurrentlyCrafting;
		recipesUnlockedSave.crafterCurrentlyWorking = NetworkMapSharer.Instance.craftsmanWorking;
		recipesUnlockedSave.recipesUnlocked = CharLevelManager.manage.recipes.ToArray();
		try
		{
			ES3.Save("unlocked", recipesUnlockedSave, saveSlot() + "/unlocked.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/unlocked.es3");
			ES3.Save("unlocked", recipesUnlockedSave, saveSlot() + "/unlocked.es3");
		}
	}

	private void saveRecipesUnlocked()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/unlocked.dat");
			binaryFormatter.Serialize(fileStream, new RecipesUnlockedSave
			{
				crafterLevel = CraftsmanManager.manage.currentLevel,
				currentPoints = CraftsmanManager.manage.currentPoints,
				crafterHasBerkonium = CraftsmanManager.manage.craftsmanHasBerkonium,
				crafterWorkingOnItemId = CraftsmanManager.manage.itemCurrentlyCrafting,
				crafterCurrentlyWorking = NetworkMapSharer.Instance.craftsmanWorking,
				recipesUnlocked = CharLevelManager.manage.recipes.ToArray()
			});
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving recipes");
			fileStream?.Close();
		}
	}

	public bool EasyLoadRecipes()
	{
		try
		{
			if (ES3.KeyExists("unlocked", saveSlot() + "/unlocked.es3"))
			{
				RecipesUnlockedSave recipesUnlockedSave = new RecipesUnlockedSave();
				ES3.LoadInto("unlocked", saveSlot() + "/unlocked.es3", recipesUnlockedSave);
				recipesUnlockedSave.loadRecipes();
				CraftsmanManager.manage.currentLevel = recipesUnlockedSave.crafterLevel;
				CraftsmanManager.manage.currentPoints = recipesUnlockedSave.currentPoints;
				CraftsmanManager.manage.craftsmanHasBerkonium = recipesUnlockedSave.crafterHasBerkonium;
				CraftsmanManager.manage.itemCurrentlyCrafting = recipesUnlockedSave.crafterWorkingOnItemId;
				NetworkMapSharer.Instance.NetworkcraftsmanWorking = recipesUnlockedSave.crafterCurrentlyWorking;
				NetworkMapSharer.Instance.NetworkcraftsmanHasBerkonium = CraftsmanManager.manage.craftsmanHasBerkonium;
				CharLevelManager.manage.recipesAlwaysUnlocked();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void loadRecipesUnlocked()
	{
		if (File.Exists(saveSlot() + "/unlocked.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/unlocked.dat", FileMode.Open);
				RecipesUnlockedSave recipesUnlockedSave = (RecipesUnlockedSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				recipesUnlockedSave.loadRecipes();
				CraftsmanManager.manage.currentLevel = recipesUnlockedSave.crafterLevel;
				CraftsmanManager.manage.currentPoints = recipesUnlockedSave.currentPoints;
				CraftsmanManager.manage.craftsmanHasBerkonium = recipesUnlockedSave.crafterHasBerkonium;
				CraftsmanManager.manage.itemCurrentlyCrafting = recipesUnlockedSave.crafterWorkingOnItemId;
				NetworkMapSharer.Instance.NetworkcraftsmanWorking = recipesUnlockedSave.crafterCurrentlyWorking;
				NetworkMapSharer.Instance.NetworkcraftsmanHasBerkonium = CraftsmanManager.manage.craftsmanHasBerkonium;
				CharLevelManager.manage.recipesAlwaysUnlocked();
				makeABackUp(saveSlot() + "/unlocked.dat", saveSlot() + "/unlocked.bak");
				return;
			}
			catch (Exception message)
			{
				Debug.LogWarning("Error loading recipes");
				Debug.LogError(message);
				fileStream?.Close();
				loadRecipesUnlockBackup();
				return;
			}
		}
		loadRecipesUnlockBackup();
	}

	private void loadRecipesUnlockBackup()
	{
		if (EasyLoadRecipes() || !File.Exists(saveSlot() + "/unlocked.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/unlocked.bak", FileMode.Open);
			RecipesUnlockedSave recipesUnlockedSave = (RecipesUnlockedSave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			recipesUnlockedSave.loadRecipes();
			CraftsmanManager.manage.currentLevel = recipesUnlockedSave.crafterLevel;
			CraftsmanManager.manage.currentPoints = recipesUnlockedSave.currentPoints;
			CraftsmanManager.manage.craftsmanHasBerkonium = recipesUnlockedSave.crafterHasBerkonium;
			CraftsmanManager.manage.itemCurrentlyCrafting = recipesUnlockedSave.crafterWorkingOnItemId;
			NetworkMapSharer.Instance.NetworkcraftsmanWorking = recipesUnlockedSave.crafterCurrentlyWorking;
			NetworkMapSharer.Instance.NetworkcraftsmanHasBerkonium = CraftsmanManager.manage.craftsmanHasBerkonium;
			CharLevelManager.manage.recipesAlwaysUnlocked();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading recipes");
			fileStream?.Close();
		}
	}

	private void EasySaveDeeds()
	{
		DeedSave deedSave = new DeedSave();
		deedSave.saveDeeds(DeedManager.manage.deedDetails.ToArray());
		try
		{
			ES3.Save("deeds", deedSave, saveSlot() + "/deeds.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/deeds.es3");
			ES3.Save("deeds", deedSave, saveSlot() + "/deeds.es3");
		}
	}

	public bool EasyLoadDeeds()
	{
		try
		{
			if (ES3.KeyExists("deeds", saveSlot() + "/deeds.es3"))
			{
				DeedSave deedSave = new DeedSave();
				ES3.LoadInto("deeds", saveSlot() + "/deeds.es3", deedSave);
				deedSave.loadDeeds();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveDeeds()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/deeds.dat");
			DeedSave deedSave = new DeedSave();
			deedSave.saveDeeds(DeedManager.manage.deedDetails.ToArray());
			binaryFormatter.Serialize(fileStream, deedSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving deeds.");
			fileStream?.Close();
		}
	}

	private void loadDeeds()
	{
		if (File.Exists(saveSlot() + "/deeds.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/deeds.dat", FileMode.Open);
				DeedSave obj = (DeedSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadDeeds();
				makeABackUp(saveSlot() + "/deeds.dat", saveSlot() + "/deeds.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading deeds.");
				fileStream?.Close();
				loadDeedsBackUp();
				return;
			}
		}
		loadDeedsBackUp();
	}

	private void loadDeedsBackUp()
	{
		if (EasyLoadDeeds())
		{
			return;
		}
		Debug.LogWarning("Loading backup deeds");
		if (!File.Exists(saveSlot() + "/deeds.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/deeds.bak", FileMode.Open);
			DeedSave obj = (DeedSave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			obj.loadDeeds();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading deeds backup");
			fileStream?.Close();
		}
	}

	private void EasySavePedia()
	{
		PediaSave pediaSave = new PediaSave();
		pediaSave.saveEntries(PediaManager.manage.allEntries.ToArray());
		try
		{
			ES3.Save("pedia", pediaSave, saveSlot() + "/pedia.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/pedia.es3");
			ES3.Save("pedia", pediaSave, saveSlot() + "/pedia.es3");
		}
	}

	public bool EasyLoadPedia()
	{
		try
		{
			if (ES3.KeyExists("pedia", saveSlot() + "/pedia.es3"))
			{
				PediaSave pediaSave = new PediaSave();
				ES3.LoadInto("pedia", saveSlot() + "/pedia.es3", pediaSave);
				pediaSave.loadEntries();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void savePedia()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/pedia.dat");
			PediaSave pediaSave = new PediaSave();
			pediaSave.saveEntries(PediaManager.manage.allEntries.ToArray());
			binaryFormatter.Serialize(fileStream, pediaSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving pedia");
			fileStream?.Close();
		}
	}

	private void loadPedia()
	{
		if (File.Exists(saveSlot() + "/pedia.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/pedia.dat", FileMode.Open);
				PediaSave obj = (PediaSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadEntries();
				makeABackUp(saveSlot() + "/pedia.dat", saveSlot() + "/pedia.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading Pedia");
				fileStream?.Close();
				loadPediaBackup();
				return;
			}
		}
		loadPediaBackup();
	}

	private void loadPediaBackup()
	{
		if (EasyLoadPedia())
		{
			return;
		}
		Debug.LogWarning("Loading Pedia backup");
		if (!File.Exists(saveSlot() + "/pedia.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/pedia.bak", FileMode.Open);
			PediaSave obj = (PediaSave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			obj.loadEntries();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading Pedia backup");
			fileStream?.Close();
		}
	}

	private void EasySaveTownStatus()
	{
		TownStatusSave townStatusSave = new TownStatusSave();
		townStatusSave.saveTownStatus();
		try
		{
			ES3.Save("townStatus", townStatusSave, saveSlot() + "/townStatus.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/townStatus.es3");
			ES3.Save("townStatus", townStatusSave, saveSlot() + "/townStatus.es3");
		}
	}

	private bool EasyLoadTownStatus()
	{
		try
		{
			if (ES3.KeyExists("townStatus", saveSlot() + "/townStatus.es3"))
			{
				TownStatusSave townStatusSave = new TownStatusSave();
				ES3.LoadInto("townStatus", saveSlot() + "/townStatus.es3", townStatusSave);
				townStatusSave.loadTownStatus();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveTownStatus()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/townStatus.dat");
			TownStatusSave townStatusSave = new TownStatusSave();
			townStatusSave.saveTownStatus();
			binaryFormatter.Serialize(fileStream, townStatusSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving town status.");
			fileStream?.Close();
			loadTownStatusBackup();
		}
	}

	private void loadTownStatus()
	{
		if (File.Exists(saveSlot() + "/townStatus.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/townStatus.dat", FileMode.Open);
				TownStatusSave obj = (TownStatusSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadTownStatus();
				makeABackUp(saveSlot() + "/townStatus.dat", saveSlot() + "/townStatus.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading town status.");
				fileStream?.Close();
				loadTownStatusBackup();
				return;
			}
		}
		loadTownStatusBackup();
	}

	private void loadTownStatusBackup()
	{
		if (EasyLoadTownStatus())
		{
			return;
		}
		Debug.LogWarning("Loading town status backup");
		if (!File.Exists(saveSlot() + "/townStatus.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/townStatus.bak", FileMode.Open);
			TownStatusSave obj = (TownStatusSave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			obj.loadTownStatus();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading town status backup");
			fileStream?.Close();
		}
	}

	public void EasySaveChangers()
	{
		ChangerSave changerSave = new ChangerSave();
		changerSave.saveChangers();
		try
		{
			ES3.Save("changers", changerSave, saveSlot() + "/changers.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/changers.es3");
			ES3.Save("changers", changerSave, saveSlot() + "/changers.es3");
		}
	}

	public bool EasyLoadChangers()
	{
		try
		{
			if (ES3.KeyExists("changers", saveSlot() + "/changers.es3"))
			{
				ChangerSave changerSave = new ChangerSave();
				ES3.LoadInto("changers", saveSlot() + "/changers.es3", changerSave);
				changerSave.loadChangers();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveChangers()
	{
		EasySaveChangers();
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/changers.dat");
			ChangerSave changerSave = new ChangerSave();
			changerSave.saveChangers();
			binaryFormatter.Serialize(fileStream, changerSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving changers");
			fileStream?.Close();
		}
	}

	private void loadChangers()
	{
		if (File.Exists(saveSlot() + "/changers.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/changers.dat", FileMode.Open);
				ChangerSave obj = (ChangerSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadChangers();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading changers from file.");
				fileStream?.Close();
				EasyLoadChangers();
				return;
			}
		}
		EasyLoadChangers();
	}

	private void saveLevels()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/levels.dat");
			LevelSave levelSave = new LevelSave();
			levelSave.saveLevels(CharLevelManager.manage.todaysXp, CharLevelManager.manage.currentXp, CharLevelManager.manage.currentLevels);
			binaryFormatter.Serialize(fileStream, levelSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving levels");
			fileStream?.Close();
		}
	}

	public void EasySaveLevels()
	{
		LevelSave levelSave = new LevelSave();
		levelSave.saveLevels(CharLevelManager.manage.todaysXp, CharLevelManager.manage.currentXp, CharLevelManager.manage.currentLevels);
		try
		{
			ES3.Save("levels", levelSave, saveSlot() + "/levels.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/levels.es3");
			ES3.Save("levels", levelSave, saveSlot() + "/levels.es3");
		}
	}

	public bool EasyLoadLevels()
	{
		try
		{
			if (ES3.KeyExists("levels", saveSlot() + "/levels.es3"))
			{
				LevelSave levelSave = new LevelSave();
				ES3.LoadInto("levels", saveSlot() + "/levels.es3", levelSave);
				levelSave.loadLevels();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void loadLevels()
	{
		if (File.Exists(saveSlot() + "/levels.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/levels.dat", FileMode.Open);
				LevelSave obj = (LevelSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadLevels();
				makeABackUp(saveSlot() + "/levels.dat", saveSlot() + "/levels.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading levels");
				fileStream?.Close();
				loadLevelsBackup();
				return;
			}
		}
		loadLevelsBackup();
	}

	private void loadLevelsBackup()
	{
		if (EasyLoadLevels())
		{
			return;
		}
		Debug.LogWarning("Loading levels backup");
		if (!File.Exists(saveSlot() + "/levels.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/levels.bak", FileMode.Open);
			LevelSave obj = (LevelSave)binaryFormatter.Deserialize(fileStream);
			fileStream.Close();
			obj.loadLevels();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading levels backup");
			fileStream?.Close();
		}
	}

	public void EasySaveMuseum()
	{
		MuseumSave museumSave = new MuseumSave();
		museumSave.fishDonated = MuseumManager.manage.fishDonated;
		museumSave.bugDonated = MuseumManager.manage.bugsDonated;
		museumSave.underWaterCreatures = MuseumManager.manage.underWaterCreaturesDonated;
		try
		{
			ES3.Save("museumSave", museumSave, saveSlot() + "/museumSave.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/museumSave.es3");
			ES3.Save("museumSave", museumSave, saveSlot() + "/museumSave.es3");
		}
	}

	public bool EasyLoadMuseum()
	{
		try
		{
			if (ES3.KeyExists("museumSave", saveSlot() + "/museumSave.es3"))
			{
				MuseumSave museumSave = new MuseumSave();
				ES3.LoadInto("museumSave", saveSlot() + "/museumSave.es3", museumSave);
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
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveMuseum()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/museumSave.dat");
			binaryFormatter.Serialize(fileStream, new MuseumSave
			{
				fishDonated = MuseumManager.manage.fishDonated,
				bugDonated = MuseumManager.manage.bugsDonated,
				underWaterCreatures = MuseumManager.manage.underWaterCreaturesDonated
			});
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving museum");
			fileStream?.Close();
		}
	}

	public void loadMuseum()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadMuseum();
			return;
		}
		if (File.Exists(saveSlot() + "/museumSave.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/museumSave.dat", FileMode.Open);
				MuseumSave museumSave = (MuseumSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				makeABackUp(saveSlot() + "/museumSave.dat", saveSlot() + "/museumSave.bak");
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
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading museum save.");
				fileStream?.Close();
				loadMuseumBackup();
				return;
			}
		}
		loadMuseumBackup();
	}

	public void loadMuseumBackup()
	{
		if (EasyLoadMuseum() || !File.Exists(saveSlot() + "/museumSave.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/museumSave.bak", FileMode.Open);
			MuseumSave museumSave = (MuseumSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
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
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading museum backup");
			fileStream?.Close();
		}
	}

	public void EasySaveBulletinBoard()
	{
		BulletinBoardSave bulletinBoardSave = new BulletinBoardSave();
		bulletinBoardSave.allPosts = BulletinBoard.board.attachedPosts.ToArray();
		try
		{
			ES3.Save("bboard", bulletinBoardSave, saveSlot() + "/bboard.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/bboard.es3");
			ES3.Save("bboard", bulletinBoardSave, saveSlot() + "/bboard.es3");
		}
	}

	public bool EasyLoadBulletinBoard()
	{
		try
		{
			if (ES3.KeyExists("bboard", saveSlot() + "/bboard.es3"))
			{
				BulletinBoardSave bulletinBoardSave = new BulletinBoardSave();
				ES3.LoadInto("bboard", saveSlot() + "/bboard.es3", bulletinBoardSave);
				fillBoardSave(bulletinBoardSave);
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveBulletinBoard()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/bboard.dat");
			binaryFormatter.Serialize(fileStream, new BulletinBoardSave
			{
				allPosts = BulletinBoard.board.attachedPosts.ToArray()
			});
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading bulletinboard save.");
			fileStream?.Close();
		}
	}

	public void loadBulletinBoard()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadBulletinBoard();
			return;
		}
		if (File.Exists(saveSlot() + "/bboard.dat"))
		{
			BulletinBoard.board.onLocalConnect();
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/bboard.dat", FileMode.Open);
				BulletinBoardSave boardSave = (BulletinBoardSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				fillBoardSave(boardSave);
				makeABackUp(saveSlot() + "/bboard.dat", saveSlot() + "/bboard.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading bulletinboard save.");
				fileStream?.Close();
				loadBulletinBoardBackUp();
				return;
			}
		}
		loadBulletinBoardBackUp();
	}

	public void loadBulletinBoardBackUp()
	{
		if (EasyLoadBulletinBoard())
		{
			return;
		}
		Debug.LogWarning("Looking for bulletinboard backup.");
		if (!File.Exists(saveSlot() + "/bboard.bak"))
		{
			return;
		}
		BulletinBoard.board.onLocalConnect();
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/bboard.bak", FileMode.Open);
			BulletinBoardSave boardSave = (BulletinBoardSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
			fileStream.Close();
			fillBoardSave(boardSave);
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading backup bulletinboard save.");
			fileStream?.Close();
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

	private void EasySaveMail()
	{
		MailSave mailSave = new MailSave();
		mailSave.allMail = MailManager.manage.mailInBox.ToArray();
		mailSave.tomorrowsMail = MailManager.manage.tomorrowsLetters.ToArray();
		try
		{
			ES3.Save("mail", mailSave, saveSlot() + "/mail.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/mail.es3");
			ES3.Save("mail", mailSave, saveSlot() + "/mail.es3");
		}
	}

	public bool EasyLoadMail()
	{
		try
		{
			if (ES3.KeyExists("mail", saveSlot() + "/mail.es3"))
			{
				MailSave mailSave = new MailSave();
				ES3.LoadInto("mail", saveSlot() + "/mail.es3", mailSave);
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
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveMail()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/mail.dat");
			binaryFormatter.Serialize(fileStream, new MailSave
			{
				allMail = MailManager.manage.mailInBox.ToArray(),
				tomorrowsMail = MailManager.manage.tomorrowsLetters.ToArray()
			});
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("error saving mail");
			fileStream?.Close();
		}
	}

	public void loadMail()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadMail();
			return;
		}
		if (File.Exists(saveSlot() + "/mail.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/mail.dat", FileMode.Open);
				MailSave mailSave = (MailSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
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
				fileStream.Close();
				makeABackUp(saveSlot() + "/mail.dat", saveSlot() + "/mail.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("error loading mail");
				fileStream?.Close();
				loadBackupMail();
				return;
			}
		}
		loadBackupMail();
	}

	public void loadBackupMail()
	{
		if (EasyLoadMail() || !File.Exists(saveSlot() + "/mail.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/mail.bak", FileMode.Open);
			MailSave mailSave = (MailSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
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
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("error loading backup mail");
			fileStream?.Close();
		}
	}

	private void EasySaveHouses()
	{
		HouseListSave houseListSave = new HouseListSave();
		houseListSave.save();
		try
		{
			ES3.Save("houseSave", houseListSave, saveSlot() + "/houseSave.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/houseSave.es3");
			ES3.Save("houseSave", houseListSave, saveSlot() + "/houseSave.es3");
		}
	}

	public bool EasyLoadHouses()
	{
		try
		{
			if (ES3.KeyExists("houseSave", saveSlot() + "/houseSave.es3"))
			{
				HouseListSave houseListSave = new HouseListSave();
				ES3.LoadInto("houseSave", saveSlot() + "/houseSave.es3", houseListSave);
				houseListSave.load();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void saveHouse()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/houseSave.dat");
			HouseListSave houseListSave = new HouseListSave();
			houseListSave.save();
			binaryFormatter.Serialize(fileStream, houseListSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving house ");
			fileStream?.Close();
		}
	}

	public void loadHouse()
	{
		if (File.Exists(saveSlot() + "/houseSave.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/houseSave.dat", FileMode.Open);
				((HouseListSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).load();
				fileStream.Close();
				makeABackUp(saveSlot() + "/houseSave.dat", saveSlot() + "/houseSave.bak");
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading house from file.");
				fileStream?.Close();
				loadHouseBackup();
			}
		}
		else
		{
			loadHouseBackup();
		}
		HouseManager.manage.OpenAllChestsInHousesForSaveConversion();
	}

	public void loadHouseBackup()
	{
		if (EasyLoadHouses())
		{
			return;
		}
		Debug.LogWarning("Loading house backup");
		if (!File.Exists(saveSlot() + "/houseSave.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/houseSave.bak", FileMode.Open);
			((HouseListSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).load();
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading house backup.");
			fileStream?.Close();
		}
	}

	private void EasySaveMapIcons()
	{
		MapIconSave mapIconSave = new MapIconSave();
		mapIconSave.saveIcons();
		try
		{
			ES3.Save("mapIcons", mapIconSave, saveSlot() + "/mapIcons.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/mapIcons.es3");
			ES3.Save("mapIcons", mapIconSave, saveSlot() + "/mapIcons.es3");
		}
	}

	public bool EasyLoadMapIcons()
	{
		try
		{
			if (ES3.KeyExists("mapIcons", saveSlot() + "/mapIcons.es3"))
			{
				MapIconSave mapIconSave = new MapIconSave();
				ES3.LoadInto("mapIcons", saveSlot() + "/mapIcons.es3", mapIconSave);
				mapIconSave.LoadPlayerPlacedIcons();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveMapIcons()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/mapIcons.dat");
			MapIconSave mapIconSave = new MapIconSave();
			mapIconSave.saveIcons();
			binaryFormatter.Serialize(fileStream, mapIconSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving map icons ");
			fileStream?.Close();
		}
	}

	public void loadMapIcons()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadMapIcons();
			return;
		}
		FileStream fileStream = null;
		try
		{
			if (File.Exists(saveSlot() + "/mapIcons.dat"))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/mapIcons.dat", FileMode.Open);
				((MapIconSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).LoadPlayerPlacedIcons();
				fileStream.Close();
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading map icons ");
			fileStream?.Close();
			EasyLoadMapIcons();
		}
	}

	private void SaveWeather()
	{
		WeatherSave weatherSave = new WeatherSave();
		weatherSave.SaveWeather();
		try
		{
			ES3.Save("weather", weatherSave, saveSlot() + "/weather.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/weather.es3");
			ES3.Save("weather", weatherSave, saveSlot() + "/weather.es3");
		}
	}

	public bool LoadWeather()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadWeather();
			return false;
		}
		try
		{
			if (ES3.KeyExists("weather", saveSlot() + "/weather.es3"))
			{
				WeatherSave weatherSave = new WeatherSave();
				ES3.LoadInto("weather", saveSlot() + "/weather.es3", weatherSave);
				weatherSave.LoadWeather();
				return true;
			}
			WeatherManager.Instance.CreateNewWeatherPatterns();
		}
		catch
		{
		}
		return false;
	}

	private void EasySaveDrops()
	{
		DropSaves dropSaves = new DropSaves();
		dropSaves.saveDrops();
		try
		{
			ES3.Save("drops", dropSaves, saveSlot() + "/drops.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/drops.es3");
			ES3.Save("drops", dropSaves, saveSlot() + "/drops.es3");
		}
	}

	public bool EasyLoadDrops()
	{
		try
		{
			if (ES3.KeyExists("drops", saveSlot() + "/drops.es3"))
			{
				DropSaves dropSaves = new DropSaves();
				ES3.LoadInto("drops", saveSlot() + "/drops.es3", dropSaves);
				dropSaves.loadDrops();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveDrops()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/drops.dat");
			DropSaves dropSaves = new DropSaves();
			dropSaves.saveDrops();
			binaryFormatter.Serialize(fileStream, dropSaves);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error Saving Drops");
			fileStream?.Close();
		}
	}

	public void loadDrops()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadDrops();
			return;
		}
		if (File.Exists(saveSlot() + "/drops.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/drops.dat", FileMode.Open);
				((DropSaves)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadDrops();
				fileStream.Close();
				makeABackUp(saveSlot() + "/drops.dat", saveSlot() + "/drops.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading drops");
				fileStream?.Close();
				loadDropsBackup();
				return;
			}
		}
		loadDropsBackup();
	}

	public void loadDropsBackup()
	{
		if (EasyLoadDrops())
		{
			return;
		}
		Debug.LogWarning("Loading backup Drops");
		if (File.Exists(saveSlot() + "/drops.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/drops.bak", FileMode.Open);
				((DropSaves)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadDrops();
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading drops backup");
				fileStream?.Close();
				EasyLoadDrops();
				return;
			}
		}
		EasyLoadDrops();
	}

	private void EasySaveCarriables()
	{
		CarrySave carrySave = new CarrySave();
		carrySave.saveAllCarryable();
		try
		{
			ES3.Save("carry", carrySave, saveSlot() + "/carry.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/carry.es3");
			ES3.Save("carry", carrySave, saveSlot() + "/carry.es3");
		}
	}

	public bool EasyLoadCarriables()
	{
		try
		{
			if (ES3.KeyExists("carry", saveSlot() + "/carry.es3"))
			{
				CarrySave carrySave = new CarrySave();
				ES3.LoadInto("carry", saveSlot() + "/carry.es3", carrySave);
				carrySave.loadAllCarryable();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveCarriables()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/carry.dat");
			CarrySave carrySave = new CarrySave();
			carrySave.saveAllCarryable();
			binaryFormatter.Serialize(fileStream, carrySave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving carrables");
			fileStream?.Close();
		}
	}

	public void loadCarriables()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadCarriables();
			return;
		}
		if (File.Exists(saveSlot() + "/carry.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/carry.dat", FileMode.Open);
				CarrySave obj = (CarrySave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				obj.loadAllCarryable();
				makeABackUp(saveSlot() + "/carry.dat", saveSlot() + "/carry.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading carryables");
				if (fileStream != null)
				{
					Debug.LogWarning("File closed");
					fileStream.Close();
				}
				loadCarriablesBackup();
				return;
			}
		}
		loadCarriablesBackup();
	}

	public void loadCarriablesBackup()
	{
		if (EasyLoadCarriables())
		{
			return;
		}
		Debug.LogWarning("Loading carryables backup");
		if (!File.Exists(saveSlot() + "/carry.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/carry.bak", FileMode.Open);
			CarrySave obj = (CarrySave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
			fileStream.Close();
			obj.loadAllCarryable();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading carryables backup");
			if (fileStream != null)
			{
				Debug.LogWarning("File closed");
				fileStream.Close();
			}
		}
	}

	public void EasySaveFencedOffAnimals(bool endOfDaySave)
	{
		FencedOffAnimalSave fencedOffAnimalSave = new FencedOffAnimalSave();
		fencedOffAnimalSave.saveAnimals(endOfDaySave);
		try
		{
			ES3.Save("animalDetails", fencedOffAnimalSave, saveSlot() + "/animalDetails.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/animalDetails.es3");
			ES3.Save("animalDetails", fencedOffAnimalSave, saveSlot() + "/animalDetails.es3");
		}
	}

	public bool EasyLoadFencedOffAnimals()
	{
		try
		{
			if (ES3.KeyExists("animalDetails", saveSlot() + "/animalDetails.es3"))
			{
				FencedOffAnimalSave fencedOffAnimalSave = new FencedOffAnimalSave();
				ES3.LoadInto("animalDetails", saveSlot() + "/animalDetails.es3", fencedOffAnimalSave);
				fencedOffAnimalSave.loadAnimals();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void saveFencedOffAnimals(bool endOfDaySave)
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/animalDetails.dat");
			FencedOffAnimalSave fencedOffAnimalSave = new FencedOffAnimalSave();
			fencedOffAnimalSave.saveAnimals(endOfDaySave);
			binaryFormatter.Serialize(fileStream, fencedOffAnimalSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving animals");
			fileStream?.Close();
		}
	}

	public void loadFencedOffAnimals()
	{
		if (File.Exists(saveSlot() + "/animalDetails.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/animalDetails.dat", FileMode.Open);
				((FencedOffAnimalSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadAnimals();
				fileStream.Close();
				makeABackUp(saveSlot() + "/animalDetails.dat", saveSlot() + "/animalDetails.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading animal details.");
				fileStream?.Close();
				loadFencedOffAnimalsBackup();
				return;
			}
		}
		loadFencedOffAnimalsBackup();
	}

	public void loadFencedOffAnimalsBackup()
	{
		if (EasyLoadFencedOffAnimals() || !File.Exists(saveSlot() + "/animalDetails.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/animalDetails.bak", FileMode.Open);
			((FencedOffAnimalSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadAnimals();
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading animal details backup");
			fileStream?.Close();
		}
	}

	public void LoadInvForMultiplayer()
	{
		if (loadingNewSaveFormat)
		{
			newFileSaver.LoadInvForMultiplayerConnect();
		}
		else
		{
			LoadInv();
		}
	}

	public void LoadInv()
	{
		if (File.Exists(saveSlot() + "/playerInfo.dat"))
		{
			FileStream fileStream = null;
			PlayerInv playerInv;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/playerInfo.dat", FileMode.Open);
				playerInv = (PlayerInv)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				makeABackUp(saveSlot() + "/playerInfo.dat", saveSlot() + "/playerInfo.bak");
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading player details.");
				fileStream?.Close();
				loadInvBackup();
				return;
			}
			Inventory.Instance.changeWalletToLoad(playerInv.money);
			BankMenu.menu.accountBalance = playerInv.bankBalance;
			BankMenu.menu.accountOverflow = playerInv.accountOverflow;
			if (playerInv.hair < 0)
			{
				playerInv.hair = Mathf.Abs(playerInv.hair + 1);
			}
			Inventory.Instance.playerHair = playerInv.hair;
			Inventory.Instance.playerHairColour = playerInv.hairColour;
			Inventory.Instance.playerEyes = playerInv.eyeStyle;
			Inventory.Instance.nose = playerInv.nose;
			Inventory.Instance.mouth = playerInv.mouth;
			Inventory.Instance.playerEyeColor = playerInv.eyeColour;
			Inventory.Instance.skinTone = playerInv.skinTone;
			Inventory.Instance.playerName = playerInv.playerName;
			Inventory.Instance.islandName = playerInv.islandName;
			EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(playerInv.head, 1);
			EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(playerInv.face, 1);
			EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(playerInv.body, 1);
			EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(playerInv.pants, 1);
			EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(playerInv.shoes, 1);
			StatusManager.manage.snagsEaten = playerInv.snagsEaten;
			StartCoroutine(EquipWindow.equip.wearingMinersHelmet());
			if (playerInv.catalogue != null)
			{
				for (int i = 0; i < playerInv.catalogue.Length; i++)
				{
					CatalogueManager.manage.collectedItem[i] = playerInv.catalogue[i];
				}
			}
			StatusManager.manage.loadStatus(playerInv.health, playerInv.healthMax, playerInv.stamina, playerInv.staminaMax);
			for (int j = 0; j < playerInv.itemsInInvSlots.Length; j++)
			{
				Inventory.Instance.invSlots[j].itemNo = playerInv.itemsInInvSlots[j];
				Inventory.Instance.invSlots[j].stack = playerInv.stacksInSlots[j];
				Inventory.Instance.invSlots[j].updateSlotContentsAndRefresh(playerInv.itemsInInvSlots[j], playerInv.stacksInSlots[j]);
			}
			loadNpcRelations();
			loadQuests();
			loadRecipesUnlocked();
			loadLicences();
			loadPedia();
			loadLevels();
		}
		else
		{
			loadInvBackup();
		}
	}

	public void loadInvBackup()
	{
		if (EasyLoadInv())
		{
			loadNpcRelations();
			loadQuests();
			loadRecipesUnlocked();
			loadLicences();
			loadPedia();
			loadLevels();
		}
		else
		{
			if (!File.Exists(saveSlot() + "/playerInfo.bak"))
			{
				return;
			}
			FileStream fileStream = null;
			PlayerInv playerInv;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/playerInfo.bak", FileMode.Open);
				playerInv = (PlayerInv)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading player backup.");
				fileStream?.Close();
				return;
			}
			Inventory.Instance.changeWalletToLoad(playerInv.money);
			BankMenu.menu.accountBalance = playerInv.bankBalance;
			BankMenu.menu.accountOverflow = playerInv.accountOverflow;
			if (playerInv.hair < 0)
			{
				playerInv.hair = Mathf.Abs(playerInv.hair + 1);
			}
			Inventory.Instance.playerHair = playerInv.hair;
			Inventory.Instance.playerHairColour = playerInv.hairColour;
			Inventory.Instance.playerEyes = playerInv.eyeStyle;
			Inventory.Instance.nose = playerInv.nose;
			Inventory.Instance.mouth = playerInv.mouth;
			Inventory.Instance.playerEyeColor = playerInv.eyeColour;
			Inventory.Instance.skinTone = playerInv.skinTone;
			Inventory.Instance.playerName = playerInv.playerName;
			Inventory.Instance.islandName = playerInv.islandName;
			EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(playerInv.head, 1);
			EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(playerInv.face, 1);
			EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(playerInv.body, 1);
			EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(playerInv.pants, 1);
			EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(playerInv.shoes, 1);
			StatusManager.manage.snagsEaten = playerInv.snagsEaten;
			StartCoroutine(EquipWindow.equip.wearingMinersHelmet());
			if (playerInv.catalogue != null)
			{
				for (int i = 0; i < playerInv.catalogue.Length; i++)
				{
					CatalogueManager.manage.collectedItem[i] = playerInv.catalogue[i];
				}
			}
			StatusManager.manage.loadStatus(playerInv.health, playerInv.healthMax, playerInv.stamina, playerInv.staminaMax);
			for (int j = 0; j < playerInv.itemsInInvSlots.Length; j++)
			{
				Inventory.Instance.invSlots[j].itemNo = playerInv.itemsInInvSlots[j];
				Inventory.Instance.invSlots[j].stack = playerInv.stacksInSlots[j];
				Inventory.Instance.invSlots[j].updateSlotContentsAndRefresh(playerInv.itemsInInvSlots[j], playerInv.stacksInSlots[j]);
			}
			loadNpcRelations();
			loadQuests();
			loadRecipesUnlocked();
			loadLicences();
			loadPedia();
			loadLevels();
		}
	}

	private IEnumerator saveOverFrames(bool isEndOfDaySave)
	{
		if (NetworkMapSharer.Instance.nextDayIsReady)
		{
			loadingScreen.loadingBarOnlyAppear();
		}
		string path = saveSlot() + "/savefile.dat";
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
							loadingScreen.showPercentage((float)(y * 10) + (float)ydif / 1000f);
							yield return null;
							num = 50;
						}
					}
				}
				if (num <= 0)
				{
					loadingScreen.showPercentage((float)(y * 10) / 1000f);
					yield return null;
					num = 50;
				}
			}
		}
		if (!isEndOfDaySave)
		{
			yield return StartCoroutine(WorldManager.Instance.fenceCheck());
		}
		for (int j = 0; j < NetworkNavMesh.nav.charsConnected.Count; j++)
		{
			CharPickUp component = NetworkNavMesh.nav.charsConnected[j].GetComponent<CharPickUp>();
			if ((bool)component)
			{
				if (component.myInteract.InsideHouseDetails != null)
				{
					component.myInteract.InsideHouseDetails.houseMapOnTileStatus[component.sittingXpos, component.sittingYPos] = sittingPosOriginals[j];
				}
				else
				{
					WorldManager.Instance.onTileStatusMap[component.sittingXpos, component.sittingYPos] = sittingPosOriginals[j];
				}
			}
		}
		if (quitAfterSave)
		{
			Application.Quit();
		}
	}

	public void saveVersionNumber()
	{
		using BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(saveSlot() + "/versionCheck.dat"));
		binaryWriter.Write((short)WorldManager.Instance.versionNumber);
	}

	public void loadVersionNumber()
	{
		using BinaryReader binaryReader = new BinaryReader(File.OpenRead(saveSlot() + "/versionCheck.dat"));
		Debug.LogWarning("Lasted version saved " + (int)binaryReader.ReadInt16());
		_ = WorldManager.Instance.versionNumber;
	}

	public IEnumerator loadOverFrames()
	{
		loadingScreen.appear("Tip_Loadingg", loadingTipsOn: true);
		yield return new WaitForSeconds(1f);
		if (!new DirectoryInfo(saveSlot()).Exists)
		{
			yield break;
		}
		loadDate();
		loadTown();
		loadItemsOnTop();
		yield return null;
		string path = saveSlot() + "/savefile.dat";
		int frameCounter = 0;
		using (BinaryReader r = new BinaryReader(File.OpenRead(path)))
		{
			GenerateMap.generate.seed = r.ReadInt32();
			yield return StartCoroutine(GenerateMap.generate.generateNewMap(GenerateMap.generate.seed));
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
								if (WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif] > 0 && (bool)WorldManager.Instance.allObjects[WorldManager.Instance.onTileMap[x * 10 + i, y * 10 + ydif]].tileObjectChest)
								{
									managerToLoadOldChestSaves.OpenChestFromMapForSaveConversion(x * 10 + i, y * 10 + ydif);
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
							loadingScreen.showPercentage(0.5f + (float)(y * 10 + ydif) / 1000f / 2f);
							yield return null;
							frameCounter = 3;
						}
					}
				}
				if (frameCounter <= 0)
				{
					loadingScreen.showPercentage(0.5f + (float)y / 100f / 2f);
					yield return null;
					frameCounter = 5;
				}
			}
		}
		managerToLoadOldChestSaves.loadStashes();
		loadHouse();
		MuseumManager.manage.loadMuseum();
		yield return StartCoroutine(WorldManager.Instance.fenceCheck());
		GenerateMap.generate.onFileLoaded();
		if (saveOrLoad.DoesInvSaveExist())
		{
			saveOrLoad.LoadInv();
		}
		loadDeeds();
		loadTownStatus();
		loadFencedOffAnimals();
		loadVersionNumber();
		loadingComplete = true;
		loadChangers();
		EasyLoadBuried();
		EasyLoadSigns();
	}

	public void createPhotoDir()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(saveSlot() + "/Photos");
		if (!directoryInfo.Exists)
		{
			Debug.Log("Creating subdirectory");
			directoryInfo.Create();
		}
	}

	public void DeleteOldSaveFiles()
	{
		DeleteOldFilesAndBackUps("/date");
		DeleteOldFilesAndBackUps("/mail");
		DeleteOldFilesAndBackUps("/townSave");
		DeleteOldFilesAndBackUps("/bboard");
		DeleteOldFilesAndBackUps("/playerInfo");
		DeleteOldFilesAndBackUps("/quests");
		DeleteOldFilesAndBackUps("/savefile");
		DeleteOldFilesAndBackUps("/houseSave");
		DeleteOldFilesAndBackUps("/museumSave");
		DeleteOldFilesAndBackUps("/vehicleInfo");
		DeleteOldFilesAndBackUps("/npc");
		DeleteOldFilesAndBackUps("/versionCheck");
		DeleteOldFilesAndBackUps("/animalHouseSave");
		DeleteOldFilesAndBackUps("/deeds");
		DeleteOldFilesAndBackUps("/licences");
		DeleteOldFilesAndBackUps("/mapIcons");
		DeleteOldFilesAndBackUps("/photoDetails");
		DeleteOldFilesAndBackUps("/animalDetails");
		DeleteOldFilesAndBackUps("/carry");
		DeleteOldFilesAndBackUps("/changers");
		DeleteOldFilesAndBackUps("/drops");
		DeleteOldFilesAndBackUps("/farmAnimalSave");
		DeleteOldFilesAndBackUps("/levels");
		DeleteOldFilesAndBackUps("/onTop");
		DeleteOldFilesAndBackUps("/pedia");
		DeleteOldFilesAndBackUps("/buried");
		DeleteOldFilesAndBackUps("/unlocked");
		DeleteOldFilesAndBackUps("/weather");
		DeleteOldFilesAndBackUps("/signs");
		DeleteOldFilesAndBackUps("/townStatus");
		try
		{
			string path = saveSlot() + "/Chests";
			if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error deleting directory: " + ex.Message);
		}
	}

	private void DeleteOldFilesAndBackUps(string fileName)
	{
		File.Delete(saveSlot() + fileName + ".dat");
		File.Delete(saveSlot() + fileName + ".bak");
		File.Delete(saveSlot() + fileName + ".es3");
	}

	public void DeleteSave(int saveSlotToDelete)
	{
		setSlotToLoad(saveSlotToDelete);
		if (new DirectoryInfo(saveSlot()).Exists)
		{
			Debug.Log("Folder Deleted");
			Directory.Delete(saveSlot(), recursive: true);
		}
	}

	public bool DoesSaveExist()
	{
		if (new DirectoryInfo(saveSlot()).Exists)
		{
			return true;
		}
		Debug.LogWarning("NO slot for folder found");
		return false;
	}

	public void setSlotToLoad(int slotId)
	{
		saveSlotToLoad = slotId;
	}

	public PlayerInv getSaveDetailsForFileButton(int slotToCheck)
	{
		FileStream fileStream = null;
		try
		{
			saveSlotToLoad = slotToCheck;
			if (new DirectoryInfo(saveSlot()).Exists && File.Exists(saveSlot() + "/playerInfo.dat"))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/playerInfo.dat", FileMode.Open);
				PlayerInv result = (PlayerInv)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				return result;
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error getting player details for save button. Trying backup.");
			fileStream?.Close();
		}
		try
		{
			saveSlotToLoad = slotToCheck;
			if (new DirectoryInfo(saveSlot()).Exists && File.Exists(saveSlot() + "/playerInfo.bak"))
			{
				BinaryFormatter binaryFormatter2 = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/playerInfo.bak", FileMode.Open);
				PlayerInv result2 = (PlayerInv)binaryFormatter2.Deserialize(fileStream);
				fileStream.Close();
				return result2;
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error getting player details for save button");
			fileStream?.Close();
		}
		return EasyInvForLoadSlot();
	}

	public DateSave getSaveDateDetailsForButton(int slotToCheck)
	{
		FileStream fileStream = null;
		try
		{
			saveSlotToLoad = slotToCheck;
			if (new DirectoryInfo(saveSlot()).Exists && File.Exists(saveSlot() + "/date.dat"))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/date.dat", FileMode.Open);
				DateSave result = (DateSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				return result;
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error getting date file.");
			fileStream?.Close();
			return null;
		}
		return null;
	}

	public bool DoesHouseSaveExist()
	{
		if (File.Exists(saveSlot() + "/houseSave.dat"))
		{
			return true;
		}
		return false;
	}

	public bool DoesInvSaveExist()
	{
		if (File.Exists(saveSlot() + "/playerInfo.dat"))
		{
			return true;
		}
		if (File.Exists(saveSlot() + "/playerInfo.bak"))
		{
			return true;
		}
		if (File.Exists(saveSlot() + "/playerInfo.es3"))
		{
			return true;
		}
		return false;
	}
}
