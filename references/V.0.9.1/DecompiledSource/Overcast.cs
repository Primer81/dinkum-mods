public class Overcast : WeatherBase
{
	protected override void Show()
	{
		UpdateOvercast();
	}

	protected override void Hide()
	{
		UpdateOvercast();
	}

	public void UpdateOvercast()
	{
		CameraController.control.updateDepthOfFieldAndFog(NewChunkLoader.loader.getChunkDistance());
	}
}
