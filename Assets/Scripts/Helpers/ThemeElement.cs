using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeElement : MonoBehaviour
{
    public List<GameObject> coloredElements = new List<GameObject>();

    public void Setup(Color color)
    {
        foreach (GameObject element in coloredElements)
        {
            foreach (Material mat in element.GetComponent<Renderer>().materials)
            {
                mat.color = color;
            }
        }
    }
}
