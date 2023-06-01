using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int score = 0;
    public Text scoreBox; //Skor textinin tanımlanması.
    
    void Update()
    {
        //Skorun güncellenmesinin sağlanması.
        ShowScore();
    }

    //Skorun gösterilmesinin sağlanması.
    public void ShowScore() {
        scoreBox.text = "Skor: " + score.ToString();
    }
}
