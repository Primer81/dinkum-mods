using UnityEngine;

namespace StylizedWaterShader;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class EnableDepthBuffer : MonoBehaviour
{
	private Camera cam;

	private void Reset()
	{
		cam = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		if (!cam)
		{
			cam = GetComponent<Camera>();
		}
		if ((bool)cam)
		{
			cam.depthTextureMode |= DepthTextureMode.Depth;
		}
	}

	private void OnDisable()
	{
		if ((bool)cam)
		{
			cam.depthTextureMode = DepthTextureMode.None;
		}
	}

	private void OnDestroy()
	{
		if ((bool)cam)
		{
			cam.depthTextureMode = DepthTextureMode.None;
		}
	}
}
