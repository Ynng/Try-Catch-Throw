using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VRButton : MonoBehaviour
{
    [SerializeField]
    private UnityEvent events = new UnityEvent();

    public Color hoverColor = Color.gray;
    Color originalColor;

    bool hover = false;
    bool press = false;

    BoxCollider boxCollider;
    RectTransform rectTransform;
    Image image;
    public AudioClip buttonDownSound;
    public AudioClip buttonUpSound;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        boxCollider = GetComponent<BoxCollider>();

        originalColor = image.color;
        boxCollider.size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hover()
    {
        if (hover) return;
        image.color = hoverColor;
        hover = true;
    }

    public void ButtonDown()
    {
        press = true;
        events.Invoke();
        if (buttonDownSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonDownSound, transform.position);
        }
    }

    public void ButtonUp()
    {
        if (!press) return;
        press = false;
        if(buttonUpSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonUpSound, transform.position);
        }
    }

    public void LeaveHover()
    {
        if (!hover) return;
        image.color = originalColor;
        hover = false;
        press = false;
    }
}
