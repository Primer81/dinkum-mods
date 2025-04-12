using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapeMeasureManager : MonoBehaviour
{
	public GameObject tapeMessureSquarePrefab;

	private bool currentlyMessuring;

	private Vector3 startPos;

	private Vector3 endPos;

	private List<TapeMeasureSquare> squares = new List<TapeMeasureSquare>();

	public static TapeMeasureManager manage;

	private bool measurementSaved;

	private int lastMeasurement;

	public ASound measureBiggerSound;

	public ASound measureSmallerSound;

	public AudioSource takesizeChange;

	private float soundPlayTimer;

	private void Awake()
	{
		manage = this;
	}

	public void useTapeMeasure()
	{
		if (!currentlyMessuring && !measurementSaved && IsOnMesurableTileObject())
		{
			MesureTileObjectRange();
		}
		else if (measurementSaved)
		{
			clearTapeMeasure();
		}
		else if (!currentlyMessuring)
		{
			startMeasurement();
		}
		else
		{
			stopMeasurement();
		}
	}

	public bool IsOnMesurableTileObject()
	{
		int num = WorldManager.Instance.onTileMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y];
		if (num == 16 || num == 703 || num == 302 || num == 125 || num == 36 || num == 773 || num == 852)
		{
			return true;
		}
		return false;
	}

	public void MesureTileObjectRange()
	{
		int num = WorldManager.Instance.onTileMap[(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y];
		Vector3 tileObjectPosition = new Vector3(NetworkMapSharer.Instance.localChar.myInteract.selectedTile.x * 2f, 0f, (float)(int)NetworkMapSharer.Instance.localChar.myInteract.selectedTile.y * 2f);
		switch (num)
		{
		case 16:
			ShowTileObjectDistance(tileObjectPosition, 14, num);
			measurementSaved = true;
			currentlyMessuring = false;
			break;
		case 703:
			ShowTileObjectDistance(tileObjectPosition, 8, num);
			measurementSaved = true;
			currentlyMessuring = false;
			break;
		case 302:
			ShowTileObjectDistance(tileObjectPosition, 8, num);
			measurementSaved = true;
			currentlyMessuring = false;
			break;
		case 125:
			ShowTileObjectDistance(tileObjectPosition, 10, num);
			measurementSaved = true;
			currentlyMessuring = false;
			break;
		case 36:
		case 773:
			ShowTileObjectDistance(tileObjectPosition, 5, num);
			measurementSaved = true;
			currentlyMessuring = false;
			break;
		case 852:
			ShowTileObjectDistance(tileObjectPosition, 10, num);
			measurementSaved = true;
			currentlyMessuring = false;
			break;
		}
	}

	public void ShowTileObjectDistance(Vector3 tileObjectPosition, int tileDistance, int tileObjectId)
	{
		int xSize = WorldManager.Instance.allObjectSettings[tileObjectId].xSize;
		int ySize = WorldManager.Instance.allObjectSettings[tileObjectId].ySize;
		startPos = new Vector3(tileObjectPosition.x - (float)(tileDistance * 2), tileObjectPosition.y, tileObjectPosition.z - (float)(tileDistance * 2));
		endPos = new Vector3(Mathf.Clamp(tileObjectPosition.x + ((float)tileDistance + (float)xSize / 2f) * 2f, startPos.x - 60f, startPos.x + 60f), 0f, Mathf.Clamp(tileObjectPosition.z + ((float)tileDistance + (float)ySize / 2f) * 2f, startPos.z - 60f, startPos.z + 60f));
		drawMessureSquares(startPos, endPos);
	}

	public bool isCurrentlyMeasuring()
	{
		return currentlyMessuring;
	}

	public void clearTapeMeasure()
	{
		measurementSaved = false;
		currentlyMessuring = false;
		for (int i = 0; i < squares.Count; i++)
		{
			Object.Destroy(squares[i].gameObject);
		}
		lastMeasurement = 0;
		squares.Clear();
	}

	public void startMeasurement()
	{
		startPos = NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position;
		currentlyMessuring = true;
		measurementSaved = false;
		StartCoroutine(doMeasurement());
	}

	public void stopMeasurement()
	{
		measurementSaved = true;
		currentlyMessuring = false;
	}

	private IEnumerator doMeasurement()
	{
		while (currentlyMessuring || measurementSaved)
		{
			yield return null;
			if (currentlyMessuring)
			{
				endPos = new Vector3(Mathf.Clamp(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.x, startPos.x - 60f, startPos.x + 60f), 0f, Mathf.Clamp(NetworkMapSharer.Instance.localChar.myInteract.tileHighlighter.transform.position.z, startPos.z - 60f, startPos.z + 60f));
			}
			if (currentlyMessuring || measurementSaved)
			{
				drawMessureSquares(startPos, endPos);
			}
		}
	}

	public void drawMessureSquares(Vector3 startingPos, Vector3 endingPos)
	{
		int num = Mathf.RoundToInt(startingPos.x / 2f);
		int num2 = Mathf.RoundToInt(endingPos.x / 2f);
		int num3 = Mathf.RoundToInt(startingPos.z / 2f);
		int num4 = Mathf.RoundToInt(endingPos.z / 2f);
		int num5 = num;
		int num6 = num3;
		num5 = ((num >= num2) ? num2 : num);
		num6 = ((num3 >= num4) ? num4 : num3);
		if (startingPos.z < endingPos.z)
		{
			num3 = Mathf.RoundToInt(endingPos.z / 2f);
			num4 = Mathf.RoundToInt(startingPos.z / 2f);
		}
		int num7 = 0;
		for (int i = 0; i <= Mathf.Abs(num3 - num4); i++)
		{
			for (int j = 0; j <= Mathf.Abs(num - num2); j++)
			{
				getFreeSquare(num7).setPosition(num5 + j, num6 + i, Mathf.Abs(num - num2) + 1 + " x " + (Mathf.Abs(num3 - num4) + 1));
				num7++;
			}
		}
		for (int k = num7; k < squares.Count; k++)
		{
			squares[k].gameObject.SetActive(value: false);
		}
	}

	public TapeMeasureSquare getFreeSquare(int index)
	{
		if (index >= squares.Count)
		{
			squares.Add(Object.Instantiate(tapeMessureSquarePrefab).GetComponent<TapeMeasureSquare>());
		}
		return squares[index];
	}
}
