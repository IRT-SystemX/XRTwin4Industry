using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusIcon : MonoBehaviour
{

    public Sprite normalState;
    public List<Sprite> downloadingAnimation;
    public SpriteRenderer spriteRenderer;

    public TMPro.TextMeshPro statusText;
    public Color normalStateColor;
    public Color downloadingStateColor;
    public Color errorStateColor;

    public enum State
    {
        normal,
        downloading,
        connection_lost
    }
    public State currentState;
    public float AnimationCooldown;

    float animationElapsedTime = 0;
    int currentFrame = 0;

    // Update is called once per frame
    void Update()
    {
        if (currentState == State.downloading)
            DownloadingAnimation();
        Debug.Log("current sprite: " + spriteRenderer.sprite.name);
    }

    public void SetState(State state)
    {
        currentState = state;
        switch (state)
        {
            case State.normal:
                Debug.Log("setting normal state");
                spriteRenderer.sprite = normalState;
                statusText.text = "AAS status: synchronised";
                statusText.color = normalStateColor;
                break;
            case State.downloading:
                if (currentState != State.downloading)
                {
                    spriteRenderer.sprite = normalState;
                    currentFrame = 0;
                    animationElapsedTime = 0;
                }
                currentState = State.downloading;
                statusText.text = "AAS status: downloading";
                statusText.color  = downloadingStateColor;
                break;
            case State.connection_lost:
                statusText.text = "AAS status: error";
                statusText.color = errorStateColor;
                break;
        }
    }

    void DownloadingAnimation()
    {
        animationElapsedTime += Time.deltaTime;
        if (animationElapsedTime > AnimationCooldown)
        {
            Debug.Log("updating frame");
            currentFrame += 1;
            if (currentFrame >= downloadingAnimation.Count)
            {
                currentFrame = 0;
            }
            spriteRenderer.sprite = downloadingAnimation[currentFrame];
            animationElapsedTime = 0;
            Debug.Log("Frame is now : " + downloadingAnimation[currentFrame].name);
        } else
        {
            Debug.Log("elapsed time : " + animationElapsedTime);
        }
    }
}
