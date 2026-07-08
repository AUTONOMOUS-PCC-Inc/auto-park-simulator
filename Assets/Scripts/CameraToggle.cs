using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraToggle : MonoBehaviour
{
    public Camera followCam;
    public Camera topCam;
    public TMP_Text buttonText;

    bool isTop;

    void Start()
    {
        SetView(false);
    }

    void Update()
    {
        if (Keyboard.current.vKey.wasPressedThisFrame)
            SetView(!isTop);
    }

    public void ToggleView()
    {
        SetView(!isTop);
    }

    void SetView(bool top)
    {
        isTop = top;

        followCam.gameObject.SetActive(!isTop);
        topCam.gameObject.SetActive(isTop);

        if (buttonText)
            buttonText.text = isTop ? "Follow View [V]" : "Top View [V]";
    }

}