using System.Collections;
using UnityEngine;

public class HairDresserMenu : MonoBehaviour
{
	public static HairDresserMenu menu;

	public GameObject hairDresserMenu;

	public bool hairMenuOpen;

	private int startingHair;

	private int showingHair;

	private int startingColour;

	private int showingColour;

	private bool selectingColor;

	public GameObject charEditor;

	public bool mirrorOpen;

	private WaitForSeconds wait = new WaitForSeconds(0.15f);

	private void Awake()
	{
		menu = this;
	}

	public void openHairCutMenu(bool colorSelector)
	{
		hairMenuOpen = true;
		hairDresserMenu.SetActive(value: true);
		startingHair = NetworkMapSharer.Instance.localChar.myEquip.hairId;
		showingHair = startingHair;
		startingColour = NetworkMapSharer.Instance.localChar.myEquip.hairColor;
		showingColour = startingColour;
		selectingColor = colorSelector;
		Inventory.Instance.checkIfWindowIsNeeded();
		NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHeadId(-1);
		NetworkMapSharer.Instance.localChar.myEquip.CmdChangeFaceId(-1);
		CameraController.control.zoomInOnCharForChair();
	}

	public void closeHairCutMenu()
	{
		hairDresserMenu.SetActive(value: false);
		hairMenuOpen = false;
		NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHeadId(EquipWindow.equip.hatSlot.itemNo);
		NetworkMapSharer.Instance.localChar.myEquip.CmdChangeFaceId(EquipWindow.equip.faceSlot.itemNo);
		NetworkMapSharer.Instance.localChar.myPickUp.pickUp();
		ConversationManager.manage.CheckIfLocalPlayerWasTalkingToNPCAndSetNetworkStopTalkingAfterConversationEnds();
		Inventory.Instance.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void openMirror()
	{
		charEditor.SetActive(value: true);
		mirrorOpen = true;
		CharacterCreatorScript.create.setCharForPicture();
		CharacterCreatorScript.create.clearForHairDresserMirror();
		RealWorldTimeLight.time.insideLight.enabled = false;
	}

	public void closeMirror()
	{
		charEditor.SetActive(value: false);
		CharacterCreatorScript.create.closeCharCreator();
		RealWorldTimeLight.time.insideLight.enabled = true;
		mirrorOpen = false;
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void ApplyChangesAndCloseMirror()
	{
		CharacterCreatorScript.create.saveCharacterEdited();
		NetworkMapSharer.Instance.localChar.myEquip.CmdUpdateCharAppearence(Inventory.Instance.playerEyeColor, Inventory.Instance.playerEyes, Inventory.Instance.skinTone, Inventory.Instance.nose, Inventory.Instance.mouth);
		closeMirror();
	}

	public void lastSelection()
	{
		if (!selectingColor)
		{
			showingHair--;
			clampShowingHair();
		}
		else
		{
			showingColour--;
			clampShowingHair();
		}
		NetworkMapSharer.Instance.localChar.myEquip.CmdMakeHairDresserSpin();
		StartCoroutine("delayHairChange");
	}

	public void nextSelection()
	{
		if (!selectingColor)
		{
			showingHair++;
			clampShowingHair();
		}
		else
		{
			showingColour++;
			clampShowingHair();
		}
		NetworkMapSharer.Instance.localChar.myEquip.CmdMakeHairDresserSpin();
		StartCoroutine("delayHairChange");
	}

	public void selectAndChargeForHairCut()
	{
		int num = 5000;
		if (selectingColor)
		{
			num = (int)((float)num * 1.5f);
		}
		if (Inventory.Instance.wallet >= num)
		{
			Inventory.Instance.changeWallet(-num);
			Inventory.Instance.playerHair = showingHair;
			Inventory.Instance.playerHairColour = showingColour;
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.GetAHairCut);
		}
		else
		{
			NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHairId(showingHair);
			NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHairColour(showingColour);
		}
		NPCManager.manage.npcStatus[8].moneySpentAtStore += 5000;
		closeHairCutMenu();
	}

	public void cancleHairCut()
	{
		NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHairId(startingHair);
		NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHairColour(startingColour);
		closeHairCutMenu();
	}

	private void clampShowingHair()
	{
		if (showingHair < 0)
		{
			showingHair = CharacterCreatorScript.create.allHairStyles.Length - 1;
		}
		if (showingHair >= CharacterCreatorScript.create.allHairStyles.Length)
		{
			showingHair = 0;
		}
		if (showingColour < 0)
		{
			showingColour = CharacterCreatorScript.create.allHairColours.Length - 1;
		}
		if (showingColour >= CharacterCreatorScript.create.allHairColours.Length)
		{
			showingColour = 0;
		}
	}

	private IEnumerator delayHairChange()
	{
		yield return wait;
		if (!selectingColor)
		{
			NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHairId(showingHair);
		}
		else
		{
			NetworkMapSharer.Instance.localChar.myEquip.CmdChangeHairColour(showingColour);
		}
	}
}
