using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class TimeScript : MonoBehaviour
{
    public float time; //Geri say�mda kullan�lacak olan s�renin tan�mlanmas�.
    
    //Zaman bitince devreye al�nacak veya devre d��� b�rak�lacak GameObjectlerin tan�mlanmas�.
    public Text textBox;
    private GameObject wordsGrid;
    private GameObject words;
    private GameObject timeText;
    private GameObject scoreText;
    public GameObject endScreen;
    public GameObject hintButton;
    public GameObject categoryText;
    public TextMeshProUGUI endScoreText;
    public Score scoreScript;
    public Scoreboard scoreboard;
    public HintScript hintScript;


    private void Start() {
        //Scriptte kullan�lacak olan �gelerin bulunmas�.
        endScreen = FindInActiveObjectByName("EndScreen");
        wordsGrid = GameObject.Find("WordsGrid");
        scoreText = GameObject.Find("Score");
        words = GameObject.Find("Words");
        timeText = GameObject.Find("Time");
        hintButton = GameObject.Find("Hint");
        scoreScript = GameObject.Find("Score").GetComponent<Score>();
        categoryText = GameObject.Find("CategoryText");
        scoreboard = FindAnyObjectByType<Scoreboard>();


        endScreen.SetActive(false);
        wordsGrid.SetActive(true);
        gameObject.SetActive(true);
        timeText.SetActive(true);

    }

    //Kalan zaman�n g�ncellenmesi.
    private void Update() {
        RemainingTime();
    }

    //Kalan zaman� hesaplayan fonksiyonun tan�mlanmas�.
    private void RemainingTime() {
        time -= Time.deltaTime;

        //Kalan zaman�n dakika ve saniye cinsinden yazd�r�lmas�.
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        //Zaman�n 10 saniyeden fazla olmas�nda ve az olmas�nda kalan zaman�n yazd�r�lmas�.
        if (seconds >= 10) {
            textBox.text = "Zaman: " + minutes + ":" + seconds;
        }
        else {
            textBox.text = "Zaman: " + minutes + ":0" + seconds;
        }

        //Belirlenen zaman bitince ger�ekle�tirilecek i�lemler.
        if (time <= 0) {
            wordsGrid.SetActive(false);
            words.SetActive(false);
            gameObject.SetActive(false);
            timeText.SetActive(false);
            endScreen.SetActive(true);
            scoreText.SetActive(false);
            hintButton.SetActive(false);
            categoryText.SetActive(false);
            endScoreText = GameObject.Find("EndScore").GetComponent<TextMeshProUGUI>();
            endScoreText.gameObject.SetActive(true);
            endScoreText.text = "Oyun Sonu Skorunuz: " + (scoreScript.score - (scoreScript.score * (hintScript.scoreDecrease * 10) / 100)).ToString(); //Oyun sonu men�s�nde skorun yazd�r�lmas�.
            scoreboard.AddingScores(scoreScript.score - (scoreScript.score * (hintScript.scoreDecrease * 10) / 100)); //Scoreboard veritaban�na en son elde edilen skorun eklenmesi.
        }
    }

    //Devre d��� b�rak�lm�� GameObjectlerin bulunmas� i�in kullan�lan fonksiyon.
    GameObject FindInActiveObjectByName(string name) {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++) {
            if (objs[i].hideFlags == HideFlags.None) {
                if (objs[i].name == name) {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }
}
