using UnityEngine;

public class MatchInteriorWalls : MonoBehaviour
{
	public MeshRenderer[] myRens;

	public Material defaultMat;

	private void OnEnable()
	{
		ChangeMaterialInside(defaultMat);
	}

	public void ChangeMaterialInside(Material newMat)
	{
		for (int i = 0; i < myRens.Length; i++)
		{
			myRens[i].sharedMaterial = newMat;
		}
	}
}
