using System;
using UnityEngine;

[Serializable]
public class ConversationSequence
{
	[Multiline(3)]
	public string[] sequence;

	public CONVERSATION_SPECIAL_ACTION specialAction;

	public TileObject talkingAboutTileObject;

	public InventoryItem talkingAboutItem;

	public NPCDetails talkingAboutNPC;

	public AnimalAI talkingAboutAnimal;

	public ConversationObject branchToConversation;
}
