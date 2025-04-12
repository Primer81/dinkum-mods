using TMPro;
using UnityEngine;

public class MultiPlayerButton : MonoBehaviour
{
	public TextMeshProUGUI playerNameText;

	public GameObject fullSlot;

	public GameObject emptySlot;

	public GameObject inviteButton;

	private bool slotFull;

	private int myId;

	public void FillSlot(string playerName, int newId)
	{
		myId = newId;
		playerNameText.text = playerName;
		slotFull = true;
		fullSlot.SetActive(value: true);
		emptySlot.SetActive(value: false);
	}

	public void EmptySlot()
	{
		slotFull = false;
		fullSlot.SetActive(value: false);
		emptySlot.SetActive(value: true);
		inviteButton.SetActive(!CustomNetworkManager.manage.checkIfLanGame());
	}

	public void PressKick()
	{
		NetworkPlayersManager.manage.KickPlayer(myId);
	}

	public void PressInvite()
	{
		MonoBehaviour.print("Pressing Invite");
		CustomNetworkManager.manage.lobby.pressInviteButton();
	}
}
