using UnityEngine;

public class BiomeObjectVariation : MonoBehaviour
{
	public MeshRenderer[] objectsToChange;

	public Material normalTexture;

	public Material beachTexture;

	private bool showingNormal = true;

	private void OnEnable()
	{
		if (!WorldManager.Instance.isPositionOnMap((int)base.transform.position.x / 2, (int)base.transform.position.z / 2))
		{
			return;
		}
		if (WorldManager.Instance.tileTypeMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2] == 3)
		{
			if (showingNormal)
			{
				for (int i = 0; i < objectsToChange.Length; i++)
				{
					objectsToChange[i].material = beachTexture;
				}
				showingNormal = false;
			}
		}
		else if (!showingNormal)
		{
			for (int j = 0; j < objectsToChange.Length; j++)
			{
				objectsToChange[j].material = normalTexture;
			}
			showingNormal = true;
		}
	}
}
