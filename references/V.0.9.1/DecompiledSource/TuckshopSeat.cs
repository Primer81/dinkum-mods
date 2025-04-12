using UnityEngine;

public class TuckshopSeat : MonoBehaviour
{
	public int mySeatId;

	public FurnitureStatus theSeatsStatus;

	private void OnEnable()
	{
		TuckshopManager.manage.connectSeatToId(this);
	}

	public void updateTheSeat(bool isSeatFull)
	{
		if (isSeatFull)
		{
			theSeatsStatus.disableSeat(theSeatsStatus.seatPosition1);
		}
		else
		{
			theSeatsStatus.enableSeat(theSeatsStatus.seatPosition1);
		}
	}
}
