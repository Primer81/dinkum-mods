using UnityEngine;

public class KiteAppearence : MonoBehaviour
{
	public MeshRenderer meshRenderer;

	public SkinnedMeshRenderer[] tails;

	public void SetKiteAppearence(InventoryItem item)
	{
		if ((bool)item.equipable.useAltMesh)
		{
			meshRenderer.GetComponent<MeshFilter>().mesh = item.equipable.useAltMesh;
		}
		if ((bool)item.equipable.shirtMesh)
		{
			for (int i = 0; i < tails.Length; i++)
			{
				tails[i].sharedMesh = item.equipable.shirtMesh;
			}
		}
	}
}
