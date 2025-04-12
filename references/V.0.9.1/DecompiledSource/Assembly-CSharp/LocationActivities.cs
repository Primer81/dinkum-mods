using UnityEngine;

public class LocationActivities : MonoBehaviour
{
	public InteriorLocationOfInterest[] placesOfInterest;

	public InteriorLocationOfInterest getAPlaceOfInterest()
	{
		return placesOfInterest[Random.Range(0, placesOfInterest.Length)];
	}
}
