using UnityEngine;

public class SetLightToWishColour : MonoBehaviour
{
	public Light lightToChangeColour;

	private void OnEnable()
	{
		WorldManager.Instance.changeDayEvent.AddListener(ChangeLightToWish);
		ChangeLightToWish();
	}

	private void OnDisable()
	{
		WorldManager.Instance.changeDayEvent.RemoveListener(ChangeLightToWish);
	}

	private void ChangeLightToWish()
	{
		lightToChangeColour.color = NetworkMapSharer.Instance.wishManager.GetCurrentWishColour();
	}
}
