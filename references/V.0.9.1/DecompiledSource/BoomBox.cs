using System.Collections;
using UnityEngine;

public class BoomBox : MonoBehaviour
{
	public ConversationObject swapConvo;

	public AudioSource boomBoxAudio;

	public AudioClip defaultSong;

	public TileObject myTileObject;

	private int showingStatus;

	public ASound insertSound;

	public void InteractWithBoomBox()
	{
		if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails == null)
		{
			if (WorldManager.Instance.onTileMap[myTileObject.xPos, myTileObject.yPos] != myTileObject.tileObjectId)
			{
				ItemOnTop myOnTopPos = GetMyOnTopPos(myTileObject.xPos, myTileObject.yPos, null);
				GiveNPC.give.OpenBoomBox(myTileObject.xPos, myTileObject.yPos, -1, -1, myOnTopPos.onTopPosition);
			}
			else
			{
				GiveNPC.give.OpenBoomBox(myTileObject.xPos, myTileObject.yPos);
				ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, swapConvo);
			}
		}
		else if (NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.houseMapOnTile[myTileObject.xPos, myTileObject.yPos] != myTileObject.tileObjectId)
		{
			ItemOnTop myOnTopPos2 = GetMyOnTopPos(myTileObject.xPos, myTileObject.yPos, NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails);
			if (myOnTopPos2 == null)
			{
				MonoBehaviour.print("No on top item found");
			}
			GiveNPC.give.OpenBoomBox(myTileObject.xPos, myTileObject.yPos, NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.xPos, NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.yPos, myOnTopPos2.onTopPosition);
		}
		else
		{
			GiveNPC.give.OpenBoomBox(myTileObject.xPos, myTileObject.yPos, NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.xPos, NetworkMapSharer.Instance.localChar.myInteract.InsideHouseDetails.yPos);
			ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, swapConvo);
		}
		ConversationManager.manage.TalkToNPC(NPCManager.manage.sign, swapConvo);
	}

	public void SetSongFromStatus(int currentStatus)
	{
		if (currentStatus != showingStatus)
		{
			if (currentStatus <= 0)
			{
				boomBoxAudio.clip = defaultSong;
				boomBoxAudio.Play();
			}
			else
			{
				SoundManager.Instance.playASoundAtPoint(insertSound, base.transform.position);
				MusicCassette component = Inventory.Instance.allItems[currentStatus].GetComponent<MusicCassette>();
				if ((bool)component)
				{
					if (component.useIndoorMusic)
					{
						boomBoxAudio.clip = MusicManager.manage.shopMusic[(int)component.playsIndooorMusic];
						StartCoroutine(PlayOnDelay());
					}
					else
					{
						boomBoxAudio.clip = MusicManager.manage.boomBoxMusic[(int)component.playsMusic];
						StartCoroutine(PlayOnDelay());
					}
				}
			}
		}
		showingStatus = currentStatus;
	}

	private IEnumerator PlayOnDelay()
	{
		yield return new WaitForSeconds(2f);
		boomBoxAudio.Play();
	}

	public ItemOnTop GetMyOnTopPos(int onTopOfX, int onTopOfY, HouseDetails inside)
	{
		int onTopPos = -1;
		if (inside == null)
		{
			TileObject tileObject = WorldManager.Instance.findTileObjectInUse(onTopOfX, onTopOfY);
			if ((bool)tileObject)
			{
				onTopPos = tileObject.returnClosestPositionWithItemOnTop(base.transform.position, onTopOfX, onTopOfY, null);
			}
		}
		else
		{
			DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(inside.xPos, inside.yPos);
			if ((bool)displayPlayerHouseTiles)
			{
				TileObject tileObject2 = displayPlayerHouseTiles.tileObjectsInHouse[onTopOfX, onTopOfY];
				if ((bool)tileObject2)
				{
					onTopPos = tileObject2.returnClosestPositionWithItemOnTop(base.transform.position, onTopOfX, onTopOfY, inside);
				}
			}
		}
		return ItemOnTopManager.manage.getItemOnTopInPosition(onTopPos, onTopOfX, onTopOfY, inside);
	}
}
