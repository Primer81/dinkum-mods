using System;

[Serializable]
public class SignDetails
{
	public string signSays;

	public int xPos;

	public int yPos;

	public int houseX;

	public int houseY;

	public SignDetails()
	{
	}

	public SignDetails(int xPosNew, int yPosNew, string newSignSays)
	{
		xPos = xPosNew;
		yPos = yPosNew;
		houseX = -1;
		houseY = -1;
		signSays = newSignSays;
	}

	public SignDetails(int xPosNew, int yPosNew, int xNewHouse, int yNewHouse, string newSignSays)
	{
		xPos = xPosNew;
		yPos = yPosNew;
		houseX = xNewHouse;
		houseY = yNewHouse;
		signSays = newSignSays;
	}

	public bool isAtPosition(int checkX, int checkY, int checkHouseX, int checkHouseY)
	{
		DoubleCheckHousePos();
		if (checkX == xPos && checkY == yPos && checkHouseX == houseX)
		{
			return checkHouseY == houseY;
		}
		return false;
	}

	public void updateSignSays(string newString)
	{
		signSays = newString;
	}

	public bool isInChunk(int chunkX, int chunkY)
	{
		DoubleCheckHousePos();
		if (houseX != -1 && houseY != -1)
		{
			return false;
		}
		if ((int)((float)xPos / 10f) * 10 == chunkX)
		{
			return (int)((float)yPos / 10f) * 10 == chunkY;
		}
		return false;
	}

	public bool IsInHouse(int checkHouseX, int checkHouseY)
	{
		if (houseX == checkHouseX && houseY == checkHouseY)
		{
			return true;
		}
		return false;
	}

	private void DoubleCheckHousePos()
	{
		if (houseX == 0)
		{
			houseX = -1;
		}
		if (houseY == 0)
		{
			houseY = -1;
		}
	}
}
