using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // Variables
    [SerializeField] GameObject dialogueBox;
    [SerializeField] Text dialogueText;
    [SerializeField] int lettersPerSecond;

    Dialogue dialogue;
    int currentLine = 0;
    bool isTyping;

    // Events
    public event Action OnStartDialogue;
    public event Action OnEndDialogue;

    // Instance of the class
    public static DialogueManager Instance {get; private set;}

    public void Awake()
    {
        Instance = this;
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A) && !isTyping)
        {
            currentLine++;
            if (currentLine < dialogue.Lines.Count)
                StartCoroutine(TypeDialogue(dialogue.Lines[currentLine]));
            else
            {
                currentLine = 0;
                dialogueBox.SetActive(false);
                OnEndDialogue?.Invoke();
            }
        }
    }

    public IEnumerator ShowDialogue(Dialogue dialogue)
    {
        // Helps to avoid user pressing key to skip the dialogue
        yield return new WaitForEndOfFrame();

        OnStartDialogue?.Invoke();

        this.dialogue = dialogue;
        dialogueBox.SetActive(true);
        StartCoroutine(TypeDialogue(dialogue.Lines[0]));
    }

    // Display dialogue
    public IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (var letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }
}
