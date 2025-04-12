using TMPro;
using UnityEngine;

public class TapeMeasureSquare : MonoBehaviour
{
	public TextMeshPro sizeText;

	public TextMeshPro secondSizeText;

	public void fillSize(string fillstring)
	{
		sizeText.text = fillstring;
		secondSizeText.text = fillstring;
	}

	public void setPosition(int xPos, int yPos, string fillString)
	{
		base.transform.position = new Vector3(xPos * 2, (float)WorldManager.Instance.heightMap[xPos, yPos] + 0.05f, yPos * 2);
		base.gameObject.SetActive(value: true);
		fillSize(fillString);
	}
}
