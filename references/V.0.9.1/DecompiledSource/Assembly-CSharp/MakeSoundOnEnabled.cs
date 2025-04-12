using UnityEngine;

public class MakeSoundOnEnabled : MonoBehaviour
{
	public ASound soundToMake;

	private void OnEnable()
	{
		SoundManager.Instance.play2DSound(soundToMake);
	}
}
