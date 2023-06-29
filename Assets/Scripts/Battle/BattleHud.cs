using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HpBar hpBar;
    [SerializeField] StatusBox statusBox;

    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Template.Name;
        levelText.text = "Lv" + pokemon.Level;

        hpBar.SetHP(((float) pokemon.CurrentHp) / pokemon.Hp);

        SetStatusBox();
        _pokemon.OnStatusChanged += SetStatusBox;
    }

    void SetStatusBox()
    {
        if (_pokemon.Status == null)
        {
            statusBox.SetVisible(false);
        }
        else
        {
            statusBox.SetUp(_pokemon.Status);
            statusBox.SetVisible(true);
        }
    }

    public IEnumerator UpdateHp()
    {
        if (_pokemon.HpChanged)
        {
            yield return hpBar.SetHpSmoothly(((float)_pokemon.CurrentHp) / _pokemon.Hp);
            _pokemon.HpChanged = false;
        }
    }
}
