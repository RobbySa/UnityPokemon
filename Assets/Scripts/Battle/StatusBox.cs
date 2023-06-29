using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBox : MonoBehaviour
{
    [SerializeField] Text statusText;
    [SerializeField] Image backgroundImage;

    public void SetUp(Condition condition)
    {
        backgroundImage.color = condition.conditionColor;
        statusText.text = condition.id.ToString().ToUpper();
    }

    public void SetVisible(bool active)
    {
        gameObject.SetActive(active);
    }
}
