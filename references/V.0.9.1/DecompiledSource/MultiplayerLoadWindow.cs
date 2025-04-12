using UnityEngine;

public class MultiplayerLoadWindow : MonoBehaviour
{
	public static MultiplayerLoadWindow load;

	public Transform joinGameMenu;

	public SaveSlotButton hostGameSaveSlot;

	public bool characterSelectedOnWindow;

	public bool joiningInvite;

	private void OnEnable()
	{
		load = this;
	}

	public void onCharSelectedForMultiplayer(int slotDataToLoad)
	{
		joinGameMenu.gameObject.SetActive(value: true);
		if (SaveLoad.saveOrLoad.newFileSaver.CheckIfNewSaveAvaliableInSlot(slotDataToLoad))
		{
			SaveLoad.saveOrLoad.loadingNewSaveFormat = true;
			hostGameSaveSlot.setSlotNo(slotDataToLoad, SaveLoad.saveOrLoad.newFileSaver.LoadPlayerDetailsForFileButton(slotDataToLoad), SaveLoad.saveOrLoad.newFileSaver.LoadDateForSaveSlot(slotDataToLoad));
		}
		else
		{
			SaveLoad.saveOrLoad.loadingNewSaveFormat = false;
			hostGameSaveSlot.setSlotNo(slotDataToLoad, SaveLoad.saveOrLoad.getSaveDetailsForFileButton(slotDataToLoad), SaveLoad.saveOrLoad.getSaveDateDetailsForButton(slotDataToLoad));
		}
		base.gameObject.SetActive(value: false);
		characterSelectedOnWindow = true;
		if (joiningInvite)
		{
			SteamLobby.instance.joinButton.onButtonPress.Invoke();
			joiningInvite = false;
		}
	}

	public void closeWindow()
	{
		base.gameObject.SetActive(value: false);
	}

	public void cancelCharacterSelected()
	{
		characterSelectedOnWindow = false;
		joiningInvite = false;
	}

	public void openForInvite()
	{
		base.gameObject.SetActive(value: true);
		joiningInvite = true;
	}
}
