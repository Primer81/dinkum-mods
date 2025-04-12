using UnityEngine;

public class PlaceOnAnimal : MonoBehaviour
{
	public AnimalAI toPlaceOn;

	public string defaultName = "Doggo";

	[Header("Pet Info-------")]
	public AnimalAI becomePet;

	public int specialVariation = -1;

	[Header("Vehicle Info-------")]
	public Vehicle becomeVehicle;
}
