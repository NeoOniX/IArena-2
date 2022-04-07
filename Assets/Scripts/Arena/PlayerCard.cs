using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    public Image colorImg;
    public TextMeshProUGUI nameTxt;
    public TextMeshProUGUI teamTxt;

    public void Set(string name, Color color, int team){
        nameTxt.text = name;
        colorImg.color = color;
        teamTxt.text = team.ToString();
        gameObject.SetActive(true);
    }
}
