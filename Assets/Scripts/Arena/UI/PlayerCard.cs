using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    public Image colorImg;
    public TextMeshProUGUI nameTxt;

    public void Set(string name, Color color){
        nameTxt.text = name;
        colorImg.color = color;
        gameObject.SetActive(true);
    }
}
