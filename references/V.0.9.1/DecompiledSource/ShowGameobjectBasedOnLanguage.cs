using I2.Loc;
using UnityEngine;

public class ShowGameobjectBasedOnLanguage : MonoBehaviour
{
	public GameObject showForKorean;

	private bool hasPlayed;

	private void OnEnable()
	{
		CheckKorean();
		hasPlayed = true;
	}

	private void OnDisable()
	{
	}

	private bool IsCurrentLanguageKorean()
	{
		if (LocalizationManager.CurrentLanguage == "Korean")
		{
			return true;
		}
		return false;
	}

	private void CheckKorean()
	{
		if (!hasPlayed && (bool)showForKorean)
		{
			showForKorean.SetActive(IsCurrentLanguageKorean());
		}
	}
}
