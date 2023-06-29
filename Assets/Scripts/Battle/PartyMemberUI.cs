using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Image frontSprite;
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HpBar hpBar;

    [SerializeField] Color highlightedColor;

    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        frontSprite.sprite = pokemon.Template.FrontSprite;
        nameText.text = pokemon.Template.Name;
        levelText.text = "Lv" + pokemon.Level;

        hpBar.SetHP(((float)pokemon.CurrentHp) / pokemon.Hp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }
}
