using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightbulb : MonoBehaviour
{
    public bool state = false;
    SpriteRenderer spriteRenderer;
    Color onColor = Color.yellow;
    Color offColor = Color.gray;

    Color roomLightColor = Color.white;
    Color roomDarkColor = Color.black;
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    
        Off();
    }

    public void On() {
        state = true;
        spriteRenderer.material.color = onColor;
        mainCamera.backgroundColor = roomLightColor;
    }

    public void Off() {
        state = false;
        spriteRenderer.material.color = offColor;
        mainCamera.backgroundColor = roomDarkColor;
    }

    public void Flick() {
        if (state) {
            Off();
        } else {
            On();
        }
    }
}
