using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchingWord : MonoBehaviour
{
    public Text displayedText; //Gösterimde olan kelimenin tanımlanması.
    public Image crossLine; //Karalama çizgisinin tanımlanması.

    private string _word;

    private void OnEnable() {
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable() {
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    //Gösterimde olan kelimenin yazdırılmasını sağlayan fonksiyon.
    public void SetWord(string word) {
        _word = word;
        displayedText.text = _word;
    }

    //Seçilen harf kombinasyonu doğru ise kelimenin üstünün karalanması.
    private void CorrectWord(string word, List<int> squareIndexes) {
        if (word == _word) {
            crossLine.gameObject.SetActive(true);
        }
    }
}
