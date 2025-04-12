using UnityEngine;

public class MeshRenderersInChildren : MonoBehaviour
{
	public MeshRenderer[] renderers;

	private void OnEnable()
	{
		EnableRenderers();
	}

	public void OnDisable()
	{
		DisableRenderers();
	}

	public void EnableRenderers()
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = true;
			renderers[i].gameObject.layer = 0;
		}
	}

	public void DisableRenderers()
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = false;
			renderers[i].gameObject.layer = 31;
		}
	}
}
