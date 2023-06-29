using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalised)
    {
        health.transform.localScale = new Vector3(hpNormalised, 1f);
        //health.GetComponent<Image>().color = Color.green;
        ChangeHpBarColor(health.transform.localScale.x);
    }

    public IEnumerator SetHpSmoothly(float newHp)
    {
        float currentHp = health.transform.localScale.x;
        float changeToAnimate = currentHp - newHp;

        while (currentHp - newHp > Mathf.Epsilon)
        {
            currentHp -= changeToAnimate * Time.deltaTime;
            health.transform.localScale = new Vector3(currentHp, 1f);
            ChangeHpBarColor(currentHp);

            yield return null;
        }

        health.transform.localScale = new Vector3(newHp, 1f);
        ChangeHpBarColor(currentHp);
    }

    // Does what the name says
    void ChangeHpBarColor(float currentHp)
    {
        if (currentHp > 0.5)
            health.GetComponent<Image>().color = Color.green;
        else if (currentHp <= 0.5 && currentHp >= 0.25)
            health.GetComponent<Image>().color = Color.yellow;
        else if (currentHp < 0.25)
            health.GetComponent<Image>().color = Color.red;
    }
}
