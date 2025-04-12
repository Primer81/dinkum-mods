using UnityEngine;

public class LineConnectToTransforms : MonoBehaviour
{
	public LineRenderer ren;

	public Transform[] connections;

	private void Start()
	{
		ren.positionCount = connections.Length;
		for (int i = 0; i < connections.Length; i++)
		{
			ren.SetPosition(i, connections[i].position);
		}
	}

	private void Update()
	{
		for (int i = 0; i < connections.Length; i++)
		{
			ren.SetPosition(i, connections[i].position);
		}
	}
}
