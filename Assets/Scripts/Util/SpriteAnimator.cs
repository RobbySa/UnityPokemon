using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator
{
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    float framerate;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float framerate = 0.16f)
    {
        this.frames = frames;
    }
}
