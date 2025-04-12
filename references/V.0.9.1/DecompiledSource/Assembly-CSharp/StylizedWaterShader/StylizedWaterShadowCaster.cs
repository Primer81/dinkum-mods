using UnityEngine;
using UnityEngine.Rendering;

namespace StylizedWaterShader;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
[AddComponentMenu("")]
public class StylizedWaterShadowCaster : MonoBehaviour
{
	private CommandBuffer cmd;

	public Light dirLight;

	private void OnEnable()
	{
		if (!dirLight)
		{
			dirLight = GetComponent<Light>();
		}
		if ((bool)dirLight && dirLight.GetCommandBuffers(LightEvent.AfterScreenspaceMask).Length < 1)
		{
			cmd = new CommandBuffer();
			cmd.name = "Water Shadow Mask";
			cmd.SetGlobalTexture("_ShadowMask", new RenderTargetIdentifier(BuiltinRenderTextureType.CurrentActive));
			dirLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, cmd);
		}
	}

	private void OnDisable()
	{
		if ((bool)dirLight)
		{
			dirLight.RemoveAllCommandBuffers();
		}
	}
}
