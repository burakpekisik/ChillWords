using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySlider : MonoBehaviour
{
    public Slider slider; //Zorluðun seçileceði sliderýn tanýmý.
    public GameData currentGameData; //Oyun durumunun datasýnýn alýnmasý.
    public BoardData GameDataInstance; 

    private void Start() {
        slider.value = PlayerPrefs.GetFloat("SliderValue", 0); //Oyun açýldýðýnda slider deðerinin oyun kapatýldýðý zamanki deðerinden devam etmesinin saðlanmasý.
    }

    public void SelectDifficulty() {
        currentGameData = Resources.Load<GameData>("Data/CurrentGameData"); //currentGameData dosyasýnýn script tarafýndan bulunmasý.
        GameDataInstance = Resources.Load<BoardData>("Data/Puzzles/Puzzle1"); //GameDataInstance dosyasýnýn script tarafýndan bulunmasý.
        currentGameData.selectedBoardData.Rows = Convert.ToInt32(slider.value) * 5;  //Oyundaki satýr sayýsýnýn belirlenmesi.
        currentGameData.selectedBoardData.Columns = Convert.ToInt32(slider.value) * 5; //Oyundaki sütun sayýsýnýn belirlenmesi.
        currentGameData.selectedBoardData.CreateNewBoard(); //Deðiþiklikler sonrasý Inspector menüsünde yeni þablonun oluþturulmasý.

        //Oluþturulan þablona rastgele harflerin yerleþtirilmesi.
        for (int i = 0; i < GameDataInstance.Columns; i++) { 
            for (int j = 0; j < GameDataInstance.Rows; j++) {
                int errorCounter = Regex.Matches(GameDataInstance.Board[i].Row[j], @"[a-zA-Z]").Count;
                string letters = "ABCÇDEFGÐHIÝJKLMNOÖPRSÞTUÜVYZ";
                int index = UnityEngine.Random.Range(0, letters.Length);

                if (errorCounter == 0) {
                    GameDataInstance.Board[i].Row[j] = letters[index].ToString();
                }
            }
        }

        PlayerPrefs.SetFloat("SliderValue", slider.value); //Slider'daki son deðerin kaydedilmesi.
    }
}
