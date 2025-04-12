using System.Collections.Generic;
using UnityEngine;

public class LoadGameWindow : MonoBehaviour
{
	public GameObject slotButtonPrefab;

	public Transform slotsParent;

	private List<SaveSlotButton> buttonsShown = new List<SaveSlotButton>();

	[SerializeField]
	private bool loadingWindowForMultiplayer;

	private void OnEnable()
	{
		NetworkPlayersManager.manage.singlePlayerOptions.SetActive(!loadingWindowForMultiplayer);
		NetworkPlayersManager.manage.IsPlayingSinglePlayer = !loadingWindowForMultiplayer;
		refreshList();
	}

	private void OnDisable()
	{
		destroyList();
	}

	private void destroyList()
	{
		for (int i = 0; i < buttonsShown.Count; i++)
		{
			Object.Destroy(buttonsShown[i].gameObject);
		}
		buttonsShown.Clear();
	}

	private void refreshList()
	{
		destroyList();
		for (int i = 0; i < 100; i++)
		{
			if (SaveLoad.saveOrLoad.newFileSaver.CheckIfNewSaveAvaliableInSlot(i))
			{
				PlayerInv playerInv = SaveLoad.saveOrLoad.newFileSaver.LoadPlayerDetailsForFileButton(i);
				DateSave saveDate = SaveLoad.saveOrLoad.newFileSaver.LoadDateForSaveSlot(i);
				if (playerInv != null)
				{
					buttonsShown.Add(Object.Instantiate(slotButtonPrefab, slotsParent).GetComponent<SaveSlotButton>());
					buttonsShown[buttonsShown.Count - 1].setSlotNo(i, playerInv, saveDate, loadingWindowForMultiplayer);
				}
			}
			else
			{
				PlayerInv playerInv = SaveLoad.saveOrLoad.getSaveDetailsForFileButton(i);
				DateSave saveDate = SaveLoad.saveOrLoad.getSaveDateDetailsForButton(i);
				if (playerInv != null)
				{
					buttonsShown.Add(Object.Instantiate(slotButtonPrefab, slotsParent).GetComponent<SaveSlotButton>());
					buttonsShown[buttonsShown.Count - 1].setSlotNo(i, playerInv, saveDate, loadingWindowForMultiplayer);
				}
			}
		}
		sortByLastSaved();
	}

	public void sortByLastSaved()
	{
		if (buttonsShown.Count > 0)
		{
			buttonsShown.Sort(sortButtons);
			for (int i = 0; i < buttonsShown.Count; i++)
			{
				buttonsShown[i].transform.SetSiblingIndex(i);
			}
			Inventory.Instance.setCurrentlySelectedAndMoveCursor(buttonsShown[0].GetComponent<RectTransform>());
		}
	}

	public int sortButtons(SaveSlotButton a, SaveSlotButton b)
	{
		if (a.savedTime < b.savedTime)
		{
			return 1;
		}
		if (a.savedTime > b.savedTime)
		{
			return -1;
		}
		return 0;
	}
}
