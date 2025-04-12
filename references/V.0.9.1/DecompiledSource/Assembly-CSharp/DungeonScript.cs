using UnityEngine;

public class DungeonScript : MonoBehaviour
{
	public DungeonMap thisMap;

	public bool northConnect;

	public bool eastConnect;

	public bool southConnect;

	public bool westConnect;

	public int[,] convertTo2dArray()
	{
		int[,] array = new int[16, 16];
		for (int i = 0; i < thisMap.rows2.Length; i++)
		{
			for (int j = 0; j < thisMap.rows2[i].row.Length; j++)
			{
				array[j, thisMap.rows2.Length - 1 - i] = thisMap.rows2[i].row[j];
			}
		}
		return array;
	}
}
