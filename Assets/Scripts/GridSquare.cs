using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEvents;

public class GridSquare : MonoBehaviour
{
    public int SquareIndex { get; set; }

    private AlphabetData.LetterData _normalLetterData; //Normal durumda bulunan harflerin datas�.
    private AlphabetData.LetterData _selectedLetterData; //Se�im durumunda bulunan harflerin datas�.
    private AlphabetData.LetterData _correctLetterData; //Do�ru durumunda bulunan harflerin datas�.

    private SpriteRenderer _displayedImage;

    private bool _selected; //Harfin se�ilip se�ilmedi�ine dair bool de�eri.
    private bool _clicked; //Harfin t�klan�p se�ilmedi�ine dair bool de�eri.
    private int _index = -1;
    private bool _correct; //Se�imin do�ru olup olmad���na dair bool de�eri.

    public void SetIndex(int index) {
        _index = index;
    }

    public int GetIndex() {
        return _index;
    }

    void Start()
    {
        _selected = false;
        _clicked = false;
        _correct = false;
        _displayedImage = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() { //Script devreye girdi�inde ger�ekle�ecek i�lemler.
        GameEvents.OnEnableSquareSelection += OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection += OnDisableSquareSelection;
        GameEvents.OnSelectSquare += SelectSquare;
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable() { //Script devreden ��kt���nda ger�ekle�ecek i�lemler.
        GameEvents.OnEnableSquareSelection -= OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection -= OnDisableSquareSelection;
        GameEvents.OnSelectSquare -= SelectSquare;
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    //Kelime do�ru oldu�unda ger�ekle�ecek fonksiyon.
    private void CorrectWord(string word, List<int> squareIndexes) { 
        if (squareIndexes.Contains(_index) && _selected == true) {
            _correct = true; 
            _displayedImage.sprite = _correctLetterData.image; //Kelime do�ru ise harfin g�r�nt�s�n�n de�i�tirilmesi.
        }

        _selected = false; 
        _clicked = false;
    }

    //Harfin se�iminin sa�lanmas�.
    public void OnEnableSquareSelection() {
        _clicked = true;
        _selected = false;
    }

    //Harfin se�iminin devre d��� b�rak�lmas�.
    public void OnDisableSquareSelection() {
        _selected = false;
        _clicked = false;

        if (_correct == true) {
            _displayedImage.sprite = _correctLetterData.image; //Kelime do�ru ise harfin g�r�nt�s�n�n de�i�tirilmesi.
        }
        else {
            _displayedImage.sprite = _normalLetterData.image; //Kelime do�ru de�il ise harfin g�r�nt�s�n�n de�i�tirilmesi.
        }
    }

    //Harf se�iminin sa�lanmas�.
    private void SelectSquare(Vector3 position) {
        if (this.gameObject.transform.position == position) {
            _displayedImage.sprite = _selectedLetterData.image; //Harfin g�r�nt�s�n�n se�ilmi� duruma getirilmesi.
        }
    }


    public void SetSprite(AlphabetData.LetterData normalLetterData, AlphabetData.LetterData selectedLetterData, AlphabetData.LetterData correctLetterData) { //Harflerin sprite renderer bile�enlerinin al�nmas�.
        _normalLetterData = normalLetterData;
        _selectedLetterData = selectedLetterData;
        _correctLetterData = correctLetterData;

        GetComponent<SpriteRenderer>().sprite = _normalLetterData.image;
    }

    //Ekrana girdi al�nd���nda yap�lacak i�lemler.
    private void OnMouseDown() {
        OnEnableSquareSelection();
        GameEvents.EnableSquareSelectionMethod();
        CheckSquare();
        _displayedImage.sprite = _selectedLetterData.image;
    }

    //Girilen girdi bir harfa isabet etti�inde ger�ekle�ecek i�lemler.
    private void OnMouseEnter() {
        CheckSquare();
    }

    //Girdi i�lemi bitti�inde ger�ekle�ecek i�lemler.
    private void OnMouseUp() {
        GameEvents.ClearSelectionMethod();
        GameEvents.DisableSquareSelectionMethod();
    }

    //Harf kontrol�n�n sa�lanmas�.
    public void CheckSquare() {
        if (_selected == false && _clicked == true) {
            _selected = true;
            GameEvents.CheckSquareMethod(_normalLetterData.letter, gameObject.transform.position, _index);
        }
    }
}
