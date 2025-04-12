using UnityEngine;

public class FogColorSync : MonoBehaviour
{
	public Camera skyboxCamera;

	private RenderTexture renderTexture;

	private Texture2D texture;

	private int lastSecondGot = -1;

	private Color lastColor;

	private void Start()
	{
		renderTexture = new RenderTexture(1, 1, 24);
		texture = new Texture2D(1, 1, TextureFormat.RGB24, mipChain: false);
	}

	public Color GetSkyBoxColour(int currentSecond)
	{
		if (currentSecond != lastSecondGot)
		{
			lastColor = SampleSkyboxColor();
			lastSecondGot = currentSecond;
		}
		return lastColor;
	}

	private Color SampleSkyboxColor()
	{
		skyboxCamera.targetTexture = renderTexture;
		skyboxCamera.Render();
		RenderTexture.active = renderTexture;
		texture.ReadPixels(new Rect(0f, 0f, 1f, 1f), 0, 0);
		texture.Apply();
		Color pixel = texture.GetPixel(0, 0);
		RenderTexture.active = null;
		skyboxCamera.targetTexture = null;
		return pixel;
	}

	private void OnDestroy()
	{
		if (renderTexture != null)
		{
			renderTexture.Release();
			Object.DestroyImmediate(renderTexture);
		}
		if (texture != null)
		{
			Object.DestroyImmediate(texture);
		}
	}
}
