using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueBox : MonoBehaviour
{

	/// <summary>The transform that is used for the dialogue box's location</summary>
	private Transform owner;

	/// <summary>The text mesh pro text component. This is where the text is rendered</summary>
	[SerializeField]
	private TextMeshProUGUI textRenderer;

	/// <summary>The UI Panel that contains the tail and body objects of the dialogue box. This gets scaled during overflow conditions.</summary>
	[SerializeField]
	private RectTransform dialoguePanel;

	/// <summary>This is the UI component for the dialogue body, which will render the body of the box.</summary>
	[SerializeField]
	private Image dialogueBody;

	/// <summary>This is the UI component for the dialogue tail, which will render the tail of the box</summary>
	[SerializeField]
	private Image dialogueTail;

	/// <summary>How fast the text types.</summary>
	[SerializeField]
	private float textSpeed;

	///<summary>Used for flagging if text is still being written or not.</summary>
	private bool isPrinting;

	// Choice boxes will always have the same body and tail style.
	[SerializeField]
	private Sprite choiceBodyStyle;

	[SerializeField]
	private Sprite choiceTailStyle;

	// Used for restoring the graphics and text to original dimension values.
	private Vector2 originalDimensions, originalTextDimensions;

	private void Awake()
	{
		originalDimensions = dialoguePanel.sizeDelta;
		originalTextDimensions = textRenderer.rectTransform.sizeDelta;
	}

	void Start()
	{
		StartCoroutine(WriteText());
	}

	void Update()
	{
		CorrectOverflow();
		MoveDialogueBox();
	}


	/// <summary>
	/// Prints dialogue to this character's dialogue box.
	/// </summary>
	/// <param name="newDialogue">The dialogue to print to the dialogue box.</param>
	public void PrintText(string newDialogue)
	{
		// Reset dimensions of the box to start.
		dialoguePanel.sizeDelta = originalDimensions;
		textRenderer.rectTransform.sizeDelta = originalTextDimensions;

		// Assign the new dialogue to the text renderer
		textRenderer.text = newDialogue;

		// Set layering of the box
		SetLayerPosition();

		// Check for text overflow and adjust the height of the panel and the renderer to accomodate the length of the dialogue.
		CorrectOverflow();

		// Print the text to the dialogue box.
		StartCoroutine(WriteText());
	}


	/// <summary>
	/// Moves this dialogue box to the top of the UI heirarchy so it layers on top of other dialogue boxes.
	/// </summary>
	private void SetLayerPosition()
	{
		transform.SetAsLastSibling();
	}


	/// <summary>
	/// Resizes dialogue box to correct for overflow.
	/// </summary>
	private void CorrectOverflow()
	{
		if (textRenderer.isTextOverflowing)
		{
			dialoguePanel.sizeDelta += new Vector2(0, textRenderer.fontSize + 5.0f);
			textRenderer.rectTransform.sizeDelta += new Vector2(0, textRenderer.fontSize);
		}
	}


	/// <summary>
	/// Prints a list of choices in the dialogue boxes.
	/// </summary>
	/// <param name="transitions">Hashtable of transitions</param>
	public void PrintChoices(Dictionary<int, DialogueState> transitions)
	{
		SetLayerPosition();

		SetStyle(choiceBodyStyle, choiceTailStyle); // set the style of the box to the default choice style.

		string newText = null;

		foreach (KeyValuePair<int, DialogueState> destination in transitions)
			// print input # and description. (E.G: 1. Friendly)
			newText += destination.Key + ". " + destination.Value.description + "\n";

		textRenderer.text = newText;

		CorrectOverflow();
		// Print the text to the dialogue box.
		StartCoroutine(WriteText());
	}


	/// <summary>
	/// Writes the text out onto the box.
	/// </summary>
	private IEnumerator WriteText()
	{
		isPrinting = true;

		for (int i = 0; i < textRenderer.text.Length; i++)
		{
			textRenderer.maxVisibleCharacters = i + 1;

			yield return new WaitForSeconds(textSpeed);
		}

		isPrinting = false;
	}


	/// <summary>
	/// Sets the body and tail styles for the dialogue box.
	/// </summary>
	/// <param name="newBodyStyle">The style of the dialogue box's body. The body is where the text is written.</param>
	/// <param name="newTailStyle">The style of the dialogue box's tail. The tail connects a character's mouth to the dialogue body.</param>
	public void SetStyle(Sprite newBodyStyle, Sprite newTailStyle)
	{
		dialogueBody.sprite = newBodyStyle;
		dialogueTail.sprite = newTailStyle;
	}


	/// <summary>
	/// Tells the Dialogue Box which character it will appear above.
	/// </summary>
	/// <param name="newOwner">The transform of the character this dialogue box will hover over.</param>
	public void SetOwner(Transform newOwner)
	{
		owner = newOwner;
	}


	/// <summary>Moves the dialogue box with the character it belongs to.</summary>
	private void MoveDialogueBox()
	{
		transform.position = owner.position;
	}


	/// <summary>returns true if the box has finished printing.</summary>
	public bool IsPrintingFinished()
	{
		return isPrinting == false;
	}
}
