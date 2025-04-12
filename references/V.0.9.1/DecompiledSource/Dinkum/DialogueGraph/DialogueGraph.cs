using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Dinkum.DialogueGraph;

public class DialogueGraph : NodeGraph
{
	public ConversationObject mainConvoData;

	private readonly List<ConversationObject> convosVisited = new List<ConversationObject>();

	private int nodeIndex;

	public void SetupGraph(ConversationObject data)
	{
		mainConvoData = data;
		CreateNode(mainConvoData);
	}

	public void RecreateGraph()
	{
		nodes.Clear();
		convosVisited.Clear();
		nodeIndex = 0;
		CreateNode(mainConvoData);
	}

	public DialogueNode CreateNode(ConversationObject newConvo, DialogueNode parentNode = null, int responseIndex = -1)
	{
		DialogueNode dialogueNode = ScriptableObject.CreateInstance<DialogueNode>();
		dialogueNode.Init(newConvo);
		dialogueNode.graph = this;
		if (parentNode != null)
		{
			dialogueNode.position = new Vector2(parentNode.position.x + 850f, parentNode.position.y - 150f + (float)(nodeIndex * 700));
		}
		nodes.Add(dialogueNode);
		convosVisited.Add(newConvo);
		AddBranchingNodes(newConvo, dialogueNode);
		return dialogueNode;
	}

	private void AddBranchingNodes(ConversationObject convo, DialogueNode parentNode)
	{
		nodeIndex = 0;
		for (int i = 0; i < convo.targetResponses.Count; i++)
		{
			ConversationObject branchToConversation = convo.targetResponses[i].branchToConversation;
			if (branchToConversation != null && !convosVisited.Contains(branchToConversation))
			{
				CreateNode(branchToConversation, parentNode, i).GetInputPort("branchEntry").Connect(parentNode.GetPort("branchOut"));
				nodeIndex++;
			}
		}
	}
}
