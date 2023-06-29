using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] BattleHud hud;
    [SerializeField] bool isEnemy;

    public Pokemon Pokemon { get; set; }
    public BattleHud Hud { get { return hud; } }

    Image image;
    Vector3 originalPosition;
    Color originalColor;

    // Collect important variables at the beginning of the 
    public void Awake()
    {
        image = GetComponent<Image>();
        originalPosition = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isEnemy)
            image.sprite = Pokemon.Template.FrontSprite;
        else
            image.sprite = Pokemon.Template.BackSprite;

        image.color = originalColor;
        PlayEnterAnimation();

        hud.SetData(pokemon);
    }

    // Animate the pokemons entering the scene
    public void PlayEnterAnimation()
    {
        if (!isEnemy)
            image.transform.localPosition = new Vector3(-500, originalPosition.y);
        else
            image.transform.localPosition = new Vector3(500, originalPosition.y);

        image.transform.DOLocalMoveX(originalPosition.x, 1f);
    }

    // Anime the pokemon attacking
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (!isEnemy)
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x + 50f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x - 50f, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x, 0.25f));
    }

    // Animate the pokemon getting hit
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    // Animate the pokemon fainting
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMoveY(originalPosition.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
}
