using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestImageRect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Assuming `image` is your Image component.
        RectTransform rectTransform = GetComponent<RectTransform>();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        float ratio = width / height;
        Debug.Log("Width: " + width);
        Debug.Log("Height: " + height);
        Debug.Log("The width/height ratio is: " + ratio);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
