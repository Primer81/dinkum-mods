using UnityEngine;

public class PhotoChallengeManager : MonoBehaviour
{
	public enum photoSubject
	{
		Npc,
		Animal,
		Carryable,
		Location,
		Biome
	}

	public enum subjectActions
	{
		None,
		Sleeping,
		Drinking,
		Running,
		Hunting,
		Attacking,
		Howling
	}

	public static PhotoChallengeManager manage;

	public bool checkPhotos;

	public SellByWeight carryablesToPhotograph;

	public void Awake()
	{
		manage = this;
	}

	public PhotoChallenge createRandomPhotoChallengeAndAttachToPost()
	{
		return new PhotoChallenge((photoSubject)Random.Range(0, 4));
	}
}
