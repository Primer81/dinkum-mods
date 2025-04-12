using UnityEngine;

public class TileObjectInstancer : MonoBehaviour
{
	public string groupName;

	public Mesh[] meshes;

	public Material[] materials;

	private Matrix4x4[] matrices;

	private void Awake()
	{
		if (meshes.Length != materials.Length)
		{
			Debug.LogError("Meshes and materials arrays must have the same length");
		}
		else
		{
			matrices = new Matrix4x4[meshes.Length];
		}
	}

	private void OnEnable()
	{
		RegisterInstances();
	}

	private void OnDisable()
	{
		UnregisterInstances();
	}

	private void RegisterInstances()
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			matrices[i] = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.localScale);
			InstancerManager.instance.RegisterInstance(groupName, meshes[i], materials[i], matrices[i]);
		}
	}

	private void UnregisterInstances()
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			InstancerManager.instance.UnregisterInstance(groupName, matrices[i]);
		}
	}
}
