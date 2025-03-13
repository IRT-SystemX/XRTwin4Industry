using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextButton : MonoBehaviour
{
    [SerializeField]
    Sprite normalSprite;
    [SerializeField]
    Sprite validateSprite;

    [SerializeField]
    public SpriteRenderer spriteRenderer;

    public void SetNormalSprite()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = normalSprite;
    }

    public void SetValidateSprite()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = validateSprite;
    }
}
