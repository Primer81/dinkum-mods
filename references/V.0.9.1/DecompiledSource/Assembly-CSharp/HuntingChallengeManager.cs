using UnityEngine;

public class HuntingChallengeManager : MonoBehaviour
{
	public static HuntingChallengeManager manage;

	private void Awake()
	{
		manage = this;
	}

	public HuntingChallenge createNewChallengeAndAttachToPost()
	{
		return Random.Range(0, 3) switch
		{
			0 => new HuntingChallenge(2, 25), 
			1 => new HuntingChallenge(7, 24), 
			_ => new HuntingChallenge(6, 26), 
		};
	}
}
