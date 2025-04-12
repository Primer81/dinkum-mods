using System.Collections.Generic;
using UnityEngine;

public class FishPondManager : MonoBehaviour
{
	public InventoryItem fishRoe;

	public InventoryItem silkItem;

	public InventoryItem honeyItem;

	private List<int[]> fishPondPositions = new List<int[]>();

	private List<int[]> bugTerrariumPositions = new List<int[]>();

	public bool IsTileIdFishPond(int tileId)
	{
		return true;
	}

	public bool IsTileIdBugTerrarium(int tileId)
	{
		return true;
	}

	public void AddToPondToEndOfDayList(int xPos, int yPos)
	{
		if (NetworkMapSharer.Instance.isServer)
		{
			fishPondPositions.Add(new int[2] { xPos, yPos });
		}
	}

	public void AddBugTerrariumToEndOfDayList(int xPos, int yPos)
	{
		if (NetworkMapSharer.Instance.isServer)
		{
			bugTerrariumPositions.Add(new int[2] { xPos, yPos });
		}
	}

	public void DoFishPondNextDay()
	{
		if (NetworkMapSharer.Instance.isServer)
		{
			for (int i = 0; i < fishPondPositions.Count; i++)
			{
				int xPos = fishPondPositions[i][0];
				int yPos = fishPondPositions[i][1];
				ContainerManager.manage.DoFishBreeding(fishRoe.getItemId(), xPos, yPos);
				ContainerManager.manage.DoFishPondRoe(fishRoe.getItemId(), xPos, yPos);
			}
			fishPondPositions.Clear();
		}
	}

	public void DoBugTerrariumNextDay()
	{
		if (NetworkMapSharer.Instance.isServer)
		{
			for (int i = 0; i < bugTerrariumPositions.Count; i++)
			{
				int xPos = bugTerrariumPositions[i][0];
				int yPos = bugTerrariumPositions[i][1];
				ContainerManager.manage.DoFishBreeding(silkItem.getItemId(), xPos, yPos);
				ContainerManager.manage.DoFishPondRoe(silkItem.getItemId(), xPos, yPos);
			}
			bugTerrariumPositions.Clear();
		}
	}
}
