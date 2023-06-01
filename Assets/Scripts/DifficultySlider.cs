using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySlider : MonoBehaviour
{
    public Slider slider; //Zorlu�un se�ilece�i slider�n tan�m�.
    public GameData currentGameData; //Oyun durumunun datas�n�n al�nmas�.
    public BoardData GameDataInstance; 

    private void Start() {
        slider.value = PlayerPrefs.GetFloat("SliderValue", 0); //Oyun a��ld���nda slider de�erinin oyun kapat�ld��� zamanki de�erinden devam etmesinin sa�lanmas�.
    }

    public void SelectDifficulty() {
        currentGameData = Resources.Load<GameData>("Data/CurrentGameData"); //currentGameData dosyas�n�n script taraf�ndan bulunmas�.
        GameDataInstance = Resources.Load<BoardData>("Data/Puzzles/Puzzle1"); //GameDataInstance dosyas�n�n script taraf�ndan bulunmas�.
        currentGameData.selectedBoardData.Rows = Convert.ToInt32(slider.value) * 5;  //Oyundaki sat�r say�s�n�n belirlenmesi.
        currentGameData.selectedBoardData.Columns = Convert.ToInt32(slider.value) * 5; //Oyundaki s�tun say�s�n�n belirlenmesi.
        currentGameData.selectedBoardData.CreateNewBoard(); //De�i�iklikler sonras� Inspector men�s�nde yeni �ablonun olu�turulmas�.

        //Olu�turulan �ablona rastgele harflerin yerle�tirilmesi.
        for (int i = 0; i < GameDataInstance.Columns; i++) { 
            for (int j = 0; j < GameDataInstance.Rows; j++) {
                int errorCounter = Regex.Matches(GameDataInstance.Board[i].Row[j], @"[a-zA-Z]").Count;
                string letters = "ABC�DEFG�HI�JKLMNO�PRS�TU�VYZ";
                int index = UnityEngine.Random.Range(0, letters.Length);

                if (errorCounter == 0) {
                    GameDataInstance.Board[i].Row[j] = letters[index].ToString();
                }
            }
        }

        PlayerPrefs.SetFloat("SliderValue", slider.value); //Slider'daki son de�erin kaydedilmesi.
    }
}
