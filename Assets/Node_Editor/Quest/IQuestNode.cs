
/// <summary>
/// Interface that is specific to quest nodes. Primarily used for polymorphism reasons in NodeEditorQuest.
/// </summary>
interface IQuestNode
{
	/// <summary>
	/// Returns true if this node is part of a turn in quest conversation.
	/// </summary>
	bool IsTurnInDialogue();

	/// <summary>
	/// Returns true if this node is part of an in-progress quest conversation.
	/// </summary>
	bool IsInProgressDialogue();
}
