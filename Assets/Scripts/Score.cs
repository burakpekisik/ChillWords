using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int score = 0;
    public Text scoreBox; //Skor textinin tan�mlanmas�.
    
    void Update()
    {
        //Skorun g�ncellenmesinin sa�lanmas�.
        ShowScore();
    }

    //Skorun g�sterilmesinin sa�lanmas�.
    public void ShowScore() {
        scoreBox.text = "Skor: " + score.ToString();
    }
}
