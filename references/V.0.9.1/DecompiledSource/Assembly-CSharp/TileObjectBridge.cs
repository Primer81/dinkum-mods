using UnityEngine;

public class TileObjectBridge : MonoBehaviour
{
	public GameObject bridgeStart;

	public GameObject bridgeEnd;

	public GameObject bridge2nd;

	public GameObject bridge2ndEnd;

	public GameObject[] bridgeMiddle;

	public Color bridgeColour;

	public float xDif = 1f;

	public void setUpBridge(int length = 3)
	{
		bridgeEnd.transform.localPosition = new Vector3(xDif, 0f, -length * 2 + 2);
		bridge2ndEnd.transform.localPosition = new Vector3(xDif, 0f, -length * 2 + 4);
		for (int i = 0; i < bridgeMiddle.Length; i++)
		{
			if (i < length - 4)
			{
				bridgeMiddle[i].SetActive(value: true);
				bridgeMiddle[i].transform.localPosition = new Vector3(xDif, 0f, -i * 2 - 4);
			}
			else
			{
				bridgeMiddle[i].SetActive(value: false);
			}
		}
	}
}
