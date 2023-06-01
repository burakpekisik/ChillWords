using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEvents;

public class GridSquare : MonoBehaviour
{
    public int SquareIndex { get; set; }

    private AlphabetData.LetterData _normalLetterData; //Normal durumda bulunan harflerin datasý.
    private AlphabetData.LetterData _selectedLetterData; //Seçim durumunda bulunan harflerin datasý.
    private AlphabetData.LetterData _correctLetterData; //Doðru durumunda bulunan harflerin datasý.

    private SpriteRenderer _displayedImage;

    private bool _selected; //Harfin seçilip seçilmediðine dair bool deðeri.
    private bool _clicked; //Harfin týklanýp seçilmediðine dair bool deðeri.
    private int _index = -1;
    private bool _correct; //Seçimin doðru olup olmadýðýna dair bool deðeri.

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

    private void OnEnable() { //Script devreye girdiðinde gerçekleþecek iþlemler.
        GameEvents.OnEnableSquareSelection += OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection += OnDisableSquareSelection;
        GameEvents.OnSelectSquare += SelectSquare;
        GameEvents.OnCorrectWord += CorrectWord;
    }

    private void OnDisable() { //Script devreden çýktýðýnda gerçekleþecek iþlemler.
        GameEvents.OnEnableSquareSelection -= OnEnableSquareSelection;
        GameEvents.OnDisableSquareSelection -= OnDisableSquareSelection;
        GameEvents.OnSelectSquare -= SelectSquare;
        GameEvents.OnCorrectWord -= CorrectWord;
    }

    //Kelime doðru olduðunda gerçekleþecek fonksiyon.
    private void CorrectWord(string word, List<int> squareIndexes) { 
        if (squareIndexes.Contains(_index) && _selected == true) {
            _correct = true; 
            _displayedImage.sprite = _correctLetterData.image; //Kelime doðru ise harfin görüntüsünün deðiþtirilmesi.
        }

        _selected = false; 
        _clicked = false;
    }

    //Harfin seçiminin saðlanmasý.
    public void OnEnableSquareSelection() {
        _clicked = true;
        _selected = false;
    }

    //Harfin seçiminin devre dýþý býrakýlmasý.
    public void OnDisableSquareSelection() {
        _selected = false;
        _clicked = false;

        if (_correct == true) {
            _displayedImage.sprite = _correctLetterData.image; //Kelime doðru ise harfin görüntüsünün deðiþtirilmesi.
        }
        else {
            _displayedImage.sprite = _normalLetterData.image; //Kelime doðru deðil ise harfin görüntüsünün deðiþtirilmesi.
        }
    }

    //Harf seçiminin saðlanmasý.
    private void SelectSquare(Vector3 position) {
        if (this.gameObject.transform.position == position) {
            _displayedImage.sprite = _selectedLetterData.image; //Harfin görüntüsünün seçilmiþ duruma getirilmesi.
        }
    }


    public void SetSprite(AlphabetData.LetterData normalLetterData, AlphabetData.LetterData selectedLetterData, AlphabetData.LetterData correctLetterData) { //Harflerin sprite renderer bileþenlerinin alýnmasý.
        _normalLetterData = normalLetterData;
        _selectedLetterData = selectedLetterData;
        _correctLetterData = correctLetterData;

        GetComponent<SpriteRenderer>().sprite = _normalLetterData.image;
    }

    //Ekrana girdi alýndýðýnda yapýlacak iþlemler.
    private void OnMouseDown() {
        OnEnableSquareSelection();
        GameEvents.EnableSquareSelectionMethod();
        CheckSquare();
        _displayedImage.sprite = _selectedLetterData.image;
    }

    //Girilen girdi bir harfa isabet ettiðinde gerçekleþecek iþlemler.
    private void OnMouseEnter() {
        CheckSquare();
    }

    //Girdi iþlemi bittiðinde gerçekleþecek iþlemler.
    private void OnMouseUp() {
        GameEvents.ClearSelectionMethod();
        GameEvents.DisableSquareSelectionMethod();
    }

    //Harf kontrolünün saðlanmasý.
    public void CheckSquare() {
        if (_selected == false && _clicked == true) {
            _selected = true;
            GameEvents.CheckSquareMethod(_normalLetterData.letter, gameObject.transform.position, _index);
        }
    }
}
