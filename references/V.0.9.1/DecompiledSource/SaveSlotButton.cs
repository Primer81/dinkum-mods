using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
	public int slotNoToLoad;

	public RawImage charSlotPic;

	public TextMeshProUGUI slotName;

	public TextMeshProUGUI islandName;

	public TextMeshProUGUI slotMoney;

	public TextMeshProUGUI slotDate;

	public GameObject deleteButton;

	private bool playerOnly;

	public bool createMultiPlayerGame;

	private PlayerInv showingInvSave;

	public long savedTime;

	public GameObject updateSaveSlotNotification;

	public void setSlotNo(int slotNo, PlayerInv invSave, DateSave saveDate, bool loadPlayerOnly = false)
	{
		slotNoToLoad = slotNo;
		fillFromSaveSlot(invSave, saveDate);
		playerOnly = loadPlayerOnly;
		if (loadPlayerOnly)
		{
			if (!SaveLoad.saveOrLoad.newFileSaver.CheckIfNewSaveAvaliableWithId(slotNoToLoad))
			{
				updateSaveSlotNotification.SetActive(value: true);
			}
			deleteButton.gameObject.SetActive(value: false);
		}
	}

	private void fillFromSaveSlot(PlayerInv invSave, DateSave saveDate)
	{
		if (invSave != null)
		{
			slotName.text = invSave.playerName;
			islandName.text = invSave.islandName;
			slotMoney.text = "<sprite=11> " + invSave.money.ToString("n0");
			showingInvSave = invSave;
			savedTime = invSave.savedTime;
			getPhotoDelay();
		}
		if (saveDate != null)
		{
			if (saveDate.year <= 1)
			{
				slotDate.text = ConversationGenerator.generate.GetTimeNameByTag("Age_Year").Replace("{0}", saveDate.year.ToString()) + ", " + RealWorldTimeLight.time.getDayName(saveDate.day - 1) + " " + (saveDate.day + (saveDate.week - 1) * 7) + " " + RealWorldTimeLight.time.getSeasonName(saveDate.month - 1);
			}
			else
			{
				slotDate.text = ConversationGenerator.generate.GetTimeNameByTag("Age_Years").Replace("{0}", saveDate.year.ToString()) + ", " + RealWorldTimeLight.time.getDayName(saveDate.day - 1) + " " + (saveDate.day + (saveDate.week - 1) * 7) + " " + RealWorldTimeLight.time.getSeasonName(saveDate.month - 1);
			}
		}
		else
		{
			slotDate.text = "";
		}
	}

	public void getPhotoDelay()
	{
		charSlotPic.texture = CharacterCreatorScript.create.loadSlotPhotoById(slotNoToLoad);
	}

	public void onPress()
	{
		if (playerOnly && !SaveLoad.saveOrLoad.newFileSaver.CheckIfNewSaveAvaliableWithId(slotNoToLoad))
		{
			SoundManager.Instance.play2DSound(SoundManager.Instance.buttonCantPressSound);
			return;
		}
		TownManager.manage.firstConnect = false;
		CharacterCreatorScript.create.gameObject.SetActive(value: false);
		SaveLoad.saveOrLoad.setSlotToLoad(slotNoToLoad);
		if (!playerOnly)
		{
			MultiplayerLoadWindow.load.characterSelectedOnWindow = true;
			SaveLoad.saveOrLoad.StartCoroutine(startDelay());
			MultiplayerLoadWindow.load.closeWindow();
		}
		else
		{
			MultiplayerLoadWindow.load.onCharSelectedForMultiplayer(slotNoToLoad);
		}
	}

	private IEnumerator startDelay()
	{
		NewChunkLoader.loader.inside = true;
		if (SaveLoad.saveOrLoad.newFileSaver.CheckIfNewSaveAvaliable())
		{
			SaveLoad.saveOrLoad.loadingNewSaveFormat = true;
			yield return SaveLoad.saveOrLoad.newFileSaver.StartCoroutine(SaveLoad.saveOrLoad.newFileSaver.LoadOverFrames());
		}
		else
		{
			yield return SaveLoad.saveOrLoad.StartCoroutine(SaveLoad.saveOrLoad.loadOverFrames());
		}
		if (createMultiPlayerGame)
		{
			CustomNetworkManager.manage.createLobbyBeforeConnection();
		}
		cameraWonderOnMenu.wonder.enabled = false;
		CustomNetworkManager.manage.StartUpHost();
		CharacterCreatorScript.create.myCamera.gameObject.SetActive(value: false);
		SaveLoad.saveOrLoad.loadingScreen.completed();
		yield return new WaitForSeconds(2f);
		SaveLoad.saveOrLoad.loadingScreen.disappear();
	}

	public void deleteSave()
	{
		SaveLoad.saveOrLoad.DeleteSave(slotNoToLoad);
		base.gameObject.SetActive(value: false);
	}
}
