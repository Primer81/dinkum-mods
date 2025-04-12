using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SignManager : MonoBehaviour
{
	public static SignManager manage;

	public GameObject signWritingWindow;

	public bool signWritingWindowOpen;

	public TMP_InputField signInput;

	public InventoryItem signWritingPen;

	private int currentlyEditingX;

	private int currentlyEditingY;

	private int currentlyEditingHouseX;

	private int currentlyEditingHouseY;

	public List<SignDetails> allSigns = new List<SignDetails>();

	private void Awake()
	{
		manage = this;
	}

	public void openSignWritingWindow(int xPos, int yPos)
	{
		currentlyEditingX = xPos;
		currentlyEditingY = yPos;
		if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails != null)
		{
			currentlyEditingHouseX = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.xPos;
			currentlyEditingHouseY = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.yPos;
		}
		else
		{
			currentlyEditingHouseX = -1;
			currentlyEditingHouseY = -1;
		}
		signInput.text = getSignText(xPos, yPos, currentlyEditingHouseX, currentlyEditingHouseY);
		signWritingWindow.SetActive(value: true);
		signWritingWindowOpen = true;
		signInput.Select();
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closed = false;
	}

	public void spinSignWheel(int xPos, int yPos)
	{
		float num = Random.Range(0, 360);
		if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails != null)
		{
			currentlyEditingHouseX = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.xPos;
			currentlyEditingHouseY = NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.yPos;
		}
		else
		{
			currentlyEditingHouseX = -1;
			currentlyEditingHouseY = -1;
		}
		NetworkMapSharer.Instance.localChar.myPickUp.CmdEditSignDetails(xPos, yPos, currentlyEditingHouseX, currentlyEditingHouseY, num.ToString());
	}

	public void closeSignWritingWindow()
	{
		if (checkIfSignMessageHasChanged(currentlyEditingX, currentlyEditingY, currentlyEditingHouseX, currentlyEditingHouseY, signInput.text))
		{
			NetworkMapSharer.Instance.localChar.myPickUp.CmdEditSignDetails(currentlyEditingX, currentlyEditingY, currentlyEditingHouseX, currentlyEditingHouseY, signInput.text);
		}
		signWritingWindow.SetActive(value: false);
		signWritingWindowOpen = false;
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void changeSignDetails(int xPos, int yPos, int houseX, int houseY, string newDetails)
	{
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].isAtPosition(xPos, yPos, houseX, houseY))
			{
				allSigns[i].updateSignSays(newDetails);
				findTileObjectInUseAndUpdate(xPos, yPos, houseX, houseY);
				return;
			}
		}
		SignDetails item = new SignDetails(xPos, yPos, houseX, houseY, newDetails);
		allSigns.Add(item);
		findTileObjectInUseAndUpdate(xPos, yPos, houseX, houseY);
	}

	public void findTileObjectInUseAndUpdate(int xPos, int yPos, int houseX, int houseY)
	{
		if (houseX == -1 && houseY == -1)
		{
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(xPos, yPos);
			if ((bool)tileObject && (bool)tileObject.tileObjectWritableSign)
			{
				tileObject.tileObjectWritableSign.updateSignText(xPos, yPos, -1, -1);
			}
			return;
		}
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles)
		{
			TileObject tileObject2 = displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos];
			if ((bool)tileObject2 && (bool)tileObject2.tileObjectWritableSign)
			{
				tileObject2.tileObjectWritableSign.updateSignText(xPos, yPos, houseX, houseY);
			}
		}
	}

	public void MoveAllSignsInHouseOnHouseMove(int houseX, int houseY, int newHouseX, int newHouseY)
	{
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].IsInHouse(houseX, houseY))
			{
				allSigns[i].houseX = newHouseX;
				allSigns[i].houseY = newHouseY;
			}
		}
	}

	public string getSignText(int xPos, int yPos, int houseX, int houseY)
	{
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].isAtPosition(xPos, yPos, houseX, houseY))
			{
				return allSigns[i].signSays;
			}
		}
		return "";
	}

	public bool areThereSignsInThisChunk(int chunkX, int chunkY)
	{
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].isInChunk(chunkX, chunkY))
			{
				return true;
			}
		}
		return false;
	}

	public bool areThereSignsInThisHouse(int houseX, int houseY)
	{
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].IsInHouse(houseX, houseY))
			{
				return true;
			}
		}
		return false;
	}

	public bool checkIfSignMessageHasChanged(int xPos, int yPos, int houseX, int houseY, string newMessage)
	{
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].isAtPosition(xPos, yPos, houseX, houseY))
			{
				return allSigns[i].signSays != newMessage;
			}
		}
		return true;
	}

	public void removeSignAtPos(int xPos, int yPos, int houseX, int houseY)
	{
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].isAtPosition(xPos, yPos, houseX, houseY))
			{
				allSigns.Remove(allSigns[i]);
				break;
			}
		}
	}

	public SignDetails[] collectSignsInChunk(int chunkX, int chunkY)
	{
		List<SignDetails> list = new List<SignDetails>();
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].isInChunk(chunkX, chunkY))
			{
				list.Add(allSigns[i]);
			}
		}
		return list.ToArray();
	}

	public SignDetails[] collectSignsInHouse(int houseX, int houseY)
	{
		List<SignDetails> list = new List<SignDetails>();
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].IsInHouse(houseX, houseY))
			{
				list.Add(allSigns[i]);
			}
		}
		return list.ToArray();
	}

	public int GetAmountOfFishInPond(int pondX, int pondY)
	{
		for (int i = 0; i < allSigns.Count; i++)
		{
			if (allSigns[i].isAtPosition(pondX, pondY, -1, -1))
			{
				return CountFishString(allSigns[i].signSays.Split(' '));
			}
		}
		return 0;
	}

	private int CountFishString(string[] toCount)
	{
		int num = 0;
		for (int i = 0; i < toCount.Length && i < 5; i++)
		{
			toCount[i] = toCount[i].Trim('<', '>');
			int result = -1;
			int.TryParse(toCount[i], out result);
			if ((result >= 0 && (bool)Inventory.Instance.allItems[result].fish) || (result >= 0 && (bool)Inventory.Instance.allItems[result].bug))
			{
				num++;
			}
		}
		return num;
	}
}
