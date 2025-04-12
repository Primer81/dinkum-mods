using UnityEngine;
using UnityEngine.Rendering;

namespace StylizedWaterShader;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("")]
public class StylizedWaterBlur : MonoBehaviour
{
	public Camera cam;

	public float length = 6f;

	public int passes = 4;

	private static Shader m_BlurRenderShader;

	private static Material m_BlurRenderMat;

	private CommandBuffer cmd;

	private readonly int blurredID = Shader.PropertyToID("_BlurBuffer1");

	private readonly int blurredID2 = Shader.PropertyToID("_BlurBuffer2");

	private readonly int BlurLengthID = Shader.PropertyToID("BlurLength");

	private static Shader BlurRenderShader
	{
		get
		{
			if (m_BlurRenderShader == null)
			{
				m_BlurRenderShader = Shader.Find("Hidden/SWS/Blur");
				return m_BlurRenderShader;
			}
			return m_BlurRenderShader;
		}
	}

	private static Material BlurRenderMat
	{
		get
		{
			if (m_BlurRenderMat == null)
			{
				m_BlurRenderMat = new Material(BlurRenderShader);
				m_BlurRenderMat.hideFlags = HideFlags.HideAndDontSave;
				return m_BlurRenderMat;
			}
			return m_BlurRenderMat;
		}
	}

	private void OnEnable()
	{
		if (!cam)
		{
			cam = GetComponent<Camera>();
		}
		Render();
	}

	private void OnDisable()
	{
		Object.DestroyImmediate(BlurRenderMat);
		if (cmd != null)
		{
			cmd.Clear();
			cam.RemoveCommandBuffer(CameraEvent.AfterSkybox, cmd);
		}
	}

	public void Render()
	{
		if ((bool)cam)
		{
			if (cmd != null)
			{
				cam.RemoveCommandBuffer(CameraEvent.AfterSkybox, cmd);
			}
			cmd = new CommandBuffer();
			cmd.name = "Grab screen and blur";
			passes = Mathf.Min(4, passes);
			cmd.GetTemporaryRT(blurredID, 0, 0, 0, FilterMode.Bilinear);
			cmd.GetTemporaryRT(blurredID2, 0, 0, 0, FilterMode.Bilinear);
			cmd.Blit(BuiltinRenderTextureType.CurrentActive, blurredID);
			for (int i = 0; i < passes; i++)
			{
				cmd.SetGlobalFloat(BlurLengthID, length / (float)Screen.height);
				cmd.Blit(blurredID, blurredID2, BlurRenderMat);
				cmd.Blit(blurredID2, blurredID, BlurRenderMat);
			}
			cmd.SetGlobalTexture("_ReflectionTex", blurredID);
			cam.AddCommandBuffer(CameraEvent.AfterSkybox, cmd);
			cmd.ReleaseTemporaryRT(blurredID);
			cmd.ReleaseTemporaryRT(blurredID2);
		}
	}
}
