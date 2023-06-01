using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchingWord : MonoBehaviour
{
    public Text displayedText; //G�sterimde olan kelimenin tan�mlanmas�.
    public Image crossLine; //Karalama �izgisinin tan�mlanmas�.

    private string _word;

    private void OnEnable() {
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable() {
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    //G�sterimde olan kelimenin yazd�r�lmas�n� sa�layan fonksiyon.
    public void SetWord(string word) {
        _word = word;
        displayedText.text = _word;
    }

    //Se�ilen harf kombinasyonu do�ru ise kelimenin �st�n�n karalanmas�.
    private void CorrectWord(string word, List<int> squareIndexes) {
        if (word == _word) {
            crossLine.gameObject.SetActive(true);
        }
    }
}
