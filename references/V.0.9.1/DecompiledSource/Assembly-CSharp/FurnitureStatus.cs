using UnityEngine;

public class FurnitureStatus : MonoBehaviour
{
	public GameObject seatPosition1;

	public GameObject seatPosition2;

	public GameObject cover;

	public GameObject noCover;

	public bool isSeat = true;

	public bool isToilet;

	public int showingX;

	public int showingY;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isFurniture = this;
	}

	public void updateOnTileStatus(int newX, int newY, HouseDetails inside = null)
	{
		showingX = newX;
		showingY = newY;
		int num = WorldManager.Instance.onTileStatusMap[showingX, showingY];
		if (inside != null)
		{
			num = inside.houseMapOnTileStatus[showingX, showingY];
		}
		if ((bool)cover)
		{
			cover.SetActive(num != 0);
		}
		if ((bool)noCover)
		{
			noCover.SetActive(num == 0);
		}
		switch (num)
		{
		case 0:
			enableSeat(seatPosition1);
			enableSeat(seatPosition2);
			break;
		case 1:
			disableSeat(seatPosition1);
			enableSeat(seatPosition2);
			break;
		case 2:
			enableSeat(seatPosition1);
			disableSeat(seatPosition2);
			break;
		case 3:
			disableSeat(seatPosition1);
			disableSeat(seatPosition2);
			break;
		}
		if (inside == null && WorldManager.Instance.waterMap[showingX, showingY] && WorldManager.Instance.heightMap[showingX, showingY] <= -1)
		{
			disableSeat(seatPosition1);
			disableSeat(seatPosition2);
		}
	}

	public void disableSeat(GameObject disable)
	{
		if ((bool)disable)
		{
			disable.SetActive(value: false);
		}
	}

	public void enableSeat(GameObject enable)
	{
		if ((bool)enable)
		{
			enable.SetActive(value: true);
		}
	}
}
