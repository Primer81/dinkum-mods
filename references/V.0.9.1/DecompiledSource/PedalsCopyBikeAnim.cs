using UnityEngine;

public class PedalsCopyBikeAnim : MonoBehaviour
{
	public Animator myAnim;

	public Animator copyAnim;

	public AudioSource playWhenAbove0;

	public float baseVolume = 0.2f;

	private void Update()
	{
		float @float = copyAnim.GetFloat("MoveSpeed");
		myAnim.SetFloat("MoveSpeed", @float);
		if (@float > 0.35f)
		{
			playWhenAbove0.volume = Mathf.Lerp(playWhenAbove0.volume, baseVolume * SoundManager.Instance.GetGlobalSoundVolume(), Time.deltaTime * 10f);
		}
		else
		{
			playWhenAbove0.volume = Mathf.Lerp(playWhenAbove0.volume, 0f, Time.deltaTime * 5f);
		}
		playWhenAbove0.pitch = Mathf.Lerp(playWhenAbove0.pitch, Mathf.Clamp(0.6f + @float, 0f, 1f), Time.deltaTime * 10f);
	}
}
