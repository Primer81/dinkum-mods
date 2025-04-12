public class CompPlayerScore
{
	public int id;

	public string playerName;

	public float score;

	public uint playerNetId;

	public int currentPlace;

	public CompPlayerScore(int npcId)
	{
		id = npcId;
		score = 0f;
	}

	public CompPlayerScore(EquipItemToChar charDetails)
	{
		id = -1;
		playerName = charDetails.playerName;
		score = charDetails.compScore;
		playerNetId = charDetails.netId;
	}

	public void addToScore(float addAmount)
	{
		score += addAmount;
	}
}
