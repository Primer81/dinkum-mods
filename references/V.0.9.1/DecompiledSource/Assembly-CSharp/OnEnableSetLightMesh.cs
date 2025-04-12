using UnityEngine;

public class OnEnableSetLightMesh : MonoBehaviour
{
	public Transform parentObject;

	public LightTurnsOnAtNight lightScript;

	private void OnEnable()
	{
		for (int i = 0; i < parentObject.childCount; i++)
		{
			_ = parentObject.GetChild(i).gameObject.activeSelf;
		}
	}
}
