using System;
using UnityEngine;

namespace Dinkum.DialogueGraph;

[Serializable]
public class TargetResponsesGraphVisualiser
{
	public string[] sequence;

	public ConversationObject branchToConversation;

	[HideInInspector]
	public ConversationObject previousBranchToConversationValue;
}
