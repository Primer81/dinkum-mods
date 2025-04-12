using UnityEngine;
using XNode;

namespace Dinkum.DialogueGraph;

[NodeWidth(600)]
public class DialogueNode : Node
{
	[HideInInspector]
	public string title;

	public CONVERSATION_TARGET conversationTarget;

	public string[] targetOpenings;

	[Input(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None, false)]
	public bool branchEntry;

	public string[] playerOptions;

	public TargetResponsesGraphVisualiser[] targetResponses;

	[Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.None, false)]
	public string branchOut;

	public ConversationObject Data { get; private set; }

	public void Init(ConversationObject data)
	{
		Data = data;
		title = Data.name;
		targetOpenings = Data.targetOpenings.sequence;
		playerOptions = Data.playerOptions;
		targetResponses = new TargetResponsesGraphVisualiser[data.targetResponses.Count];
		for (int i = 0; i < data.targetResponses.Count; i++)
		{
			targetResponses[i] = new TargetResponsesGraphVisualiser
			{
				sequence = data.targetResponses[i].sequence,
				branchToConversation = data.targetResponses[i].branchToConversation,
				previousBranchToConversationValue = data.targetResponses[i].branchToConversation
			};
		}
	}
}
