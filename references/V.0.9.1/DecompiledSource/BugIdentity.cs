using UnityEngine;

public class BugIdentity : MonoBehaviour
{
	public GameObject insectType;

	public string buyAnimType;

	public float bugBaseSpeed;

	public bool isFlyingBug;

	public ASound bugSounds;

	public ASound bugMovementSound;

	public bool glows;

	public bool attacksWhenClose;

	public bool poisonOnAttack;

	public Mesh altBody;

	public Mesh altWingsOrLegs;

	public AnimatorOverrideController control;

	public SeasonAndTime mySeason;

	public Sprite bugPediaPhoto;

	public bool sitsOnTileObject;
}
