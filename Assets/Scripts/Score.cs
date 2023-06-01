using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int score = 0;
    public Text scoreBox; //Skor textinin tanýmlanmasý.
    
    void Update()
    {
        //Skorun güncellenmesinin saðlanmasý.
        ShowScore();
    }

    //Skorun gösterilmesinin saðlanmasý.
    public void ShowScore() {
        scoreBox.text = "Skor: " + score.ToString();
    }
}
