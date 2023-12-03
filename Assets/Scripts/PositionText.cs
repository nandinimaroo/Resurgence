using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
           RectTransform rectTransform = GetComponent<RectTransform>();

        // Set the anchors to the upper-left corner (0, 1, 0, 1)
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);

        // Set the pivot to the upper-left corner (0, 1)
        rectTransform.pivot = new Vector2(0f, 1f);

        // Set the position to the upper-left corner of the Canvas
        rectTransform.anchoredPosition = new Vector2(0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
  RectTransform rectTransform = GetComponent<RectTransform>();

        // Set the anchors to the upper-left corner (0, 1, 0, 1)
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);

        // Set the pivot to the upper-left corner (0, 1)
        rectTransform.pivot = new Vector2(0f, 1f);

        // Set the position to the upper-left corner of the Canvas
        rectTransform.anchoredPosition = new Vector2(0f, 0f);
    }
}
