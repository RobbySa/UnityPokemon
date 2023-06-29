using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogueBox : MonoBehaviour
{
    // Various battle related references
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;

    [SerializeField] Text dialogueText;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    // Sets Text rather than animating it
    public void SetDialogue(string dialogue)
    {
        dialogueText.text = dialogue;
    }

    // Display dialogue
    public IEnumerator TypeDialogue(string dialogue)
    {
        dialogueText.text = "";
        foreach(var letter in dialogue)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    // Enable/Disable dialogue box
    public void EnableDialogueText(bool enable)
    {
        dialogueText.enabled = enable;
    }

    // Enable/Disable Action selector
    public void EnableActionSelector(bool enable)
    {
        actionSelector.SetActive(enable);
    }

    // Enable/Disable Action selector
    public void EnableMoveSelector(bool enable)
    {
        moveSelector.SetActive(enable);
        moveDetails.SetActive(enable);
    }

    // Change the color of the currently selected action
    public void UpdateActionSelector(int selectedAction)
    {
        for(int action = 0; action < actionTexts.Count; action++)
        {
            if (action == selectedAction)
                actionTexts[action].color = highlightedColor;
            else
                actionTexts[action].color = Color.black;
        }
    }

    // Change the color of the currently selected move
    public void UpdateMoveSelector(int selectedMove, Move currentMove)
    {
        for (int move = 0; move < moveTexts.Count; move++)
        {
            if (move == selectedMove)
                moveTexts[move].color = highlightedColor;
            else
                moveTexts[move].color = Color.black;
        }

        ppText.text = $"{currentMove.Pp}/{currentMove.MoveBase.Pp}";
        typeText.text = currentMove.MoveBase.Type.ToString().ToUpper();

        if (currentMove.Pp == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;
    }

    // Set the names that appear in the UI to those of the current Pokemon
    public void SetMoveNames(List<Move> moves)
    {
        for(int move = 0; move < moveTexts.Count; move++)
        {
            if (move < moves.Count)
                moveTexts[move].text = moves[move].MoveBase.Name;
            else
                moveTexts[move].text = "-";
        }


    }
}
