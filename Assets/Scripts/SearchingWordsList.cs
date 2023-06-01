using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchingWordsList : MonoBehaviour
{
    public GameData currentGameData; //currentGameData'n�n tan�mlanmas�.
    public GameObject searchingWordPrefab; //Aranan kelimenin prefab halinin tan�mlanmas�.
    public HintScript hintScript; //�pucu sisteminin �a��r�lmas�.
    public float offset = 0.0f;
    public int maxColumns = 5; //�pu�lar�n�n yerle�tirilebilece�i maksimum s�tun say�s�n�n belirlenmesi.
    public int maxRows = 5; //�pu�lar�n�n yerle�tirilebilece�i maksimum sat�r say�s�n�n belirlenmesi.

    public int _columns = 2;
    public int _rows;
    public int _wordsNumber; //Kelime say�s�n� belirleyen de�i�ken.

    public List<GameObject> _words = new List<GameObject>(); //Yazd�r�lacak kelimelerin listenin belirlenmesi.

    // Start is called before the first frame update
    private void Start()
    {
        hintScript = FindAnyObjectByType<HintScript>(); //�pucu scriptinin bulunmas�.
        currentGameData = Resources.Load<GameData>("Data/CurrentGameData"); //currentGameData'n�n bulunmas�.

        _wordsNumber = currentGameData.selectedBoardData.SearchWords.Count; //Kelime say�s�n�n belirlenmesi.

        if (_wordsNumber < _columns) {
            _rows = 1;
        }
        else {
            CalculateColumnsAndRowsNumber();
        }
        CreateWordObjects(); //�pu�lar�n� olu�turacak olan fonksiyonun �a��r�lmas�.
        SetWordsPosition(); //�pu�lar�n�n konumlar�n� ayarlayacak olan fonksiyonun �a��r�lmas�.
        hintScript.FindChild(); //�pu�lar� olu�tuktan sonra ipucu sistemi i�in olu�turulan kelime kutular�n�n bulunmas�n�n sa�lanmas�.
        _words.Clear();
    }

    public void CalculateColumnsAndRowsNumber() { //�pu�lar�n�n yazd�r�laca�� sat�r ve s�tun say�s�n�n belirleyen fonksiyon.
        do {
            _columns++;
            _rows = _wordsNumber / _columns;
        } while (_rows >= maxRows);

        if (_columns > maxColumns) {
            _columns = maxColumns;
            _rows = _wordsNumber / _columns;
        }
    }

    private bool TryIncreaseColumnNumber() {
        _columns++;
        _rows = _wordsNumber / _columns;

        if (_columns > maxColumns) {
            _columns = maxColumns;
            _rows = _wordsNumber / _columns;

            return false;
        }

        if (_wordsNumber % _columns > 0) {
            _rows++;
        }

        return true;
    }

    public void CreateWordObjects() { //Kelime kutucuklar�n�n(�pucu) olu�turulmas�.
        var squareScale = GetSquareScale(new Vector3(1f, 1f, 0.1f)); 
        for (var index = 0; index < _wordsNumber; index++) {
            _words.Add(Instantiate(searchingWordPrefab) as GameObject);
            _words[index].transform.SetParent(GameObject.Find("Words").transform);
            _words[index].GetComponent<RectTransform>().localScale = squareScale;
            _words[index].GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
            _words[index].GetComponent<SearchingWord>().SetWord(currentGameData.selectedBoardData.SearchWords[index].Word);
        }
    }

    private Vector3 GetSquareScale(Vector3 defaultScale) {
        var finalScale = defaultScale;
        var adjustment = 0.01f;

        while (ShouldScaleDown(finalScale)) {
            finalScale.x -= adjustment;
            finalScale.y -= adjustment;

            if (finalScale.x <= 0 || finalScale.y <= 0) {
                finalScale.x = adjustment;
                finalScale.y = adjustment;

                return finalScale;
            }
        }
        return finalScale;
    }

    private bool ShouldScaleDown(Vector3 targetScale) {
        var squareRect = searchingWordPrefab.GetComponent<RectTransform>();
        var parentRect = this.GetComponent<RectTransform>();

        var squareSize = new Vector3(0f, 0f);

        squareSize.x = squareRect.rect.width * targetScale.x + offset;
        squareSize.y = squareRect.rect.height * targetScale.y + offset;

        var totalSquaresHeight = squareSize.y * _rows;

        if (totalSquaresHeight > parentRect.rect.height) {
            while (totalSquaresHeight > parentRect.rect.height) {
                if (TryIncreaseColumnNumber()) {
                    totalSquaresHeight = squareSize.y * _rows;
                }
                else {
                    return true;
                }
            }
        }
        var totalSquareWidth = squareSize.x * _columns;

        if (totalSquareWidth > parentRect.rect.width) {
            return true;
        }
        return false;
    }

    public void SetWordsPosition() { //Olu�turulan kelime kutucuklar�n�n konumlar�n�n ayarlanmas�n� sa�layan fonksiyon.
        var squareRect = _words[0].GetComponent<RectTransform>();
        var wordOffset = new Vector2 {
            x = squareRect.rect.width * squareRect.transform.localScale.x + offset,
            y = squareRect.rect.height * squareRect.transform.localScale.y + offset
        };

        int columnNumber = 0;
        int rowNumber = 0;
        var startPosition = GetFirstSquarePosition();

        foreach ( var word in _words ) {
            if (columnNumber + 1 > _columns) {
                columnNumber = 0;
                rowNumber++;
            }

            var positionX = startPosition.x + wordOffset.x * columnNumber;
            var positionY = startPosition.y + wordOffset.y * rowNumber;

            word.GetComponent<RectTransform>().localPosition = new Vector2(positionX, positionY);
            columnNumber++;
        }
        squareRect = _words[0].GetComponent<RectTransform>();
        wordOffset = new Vector2 { };
        columnNumber = 0;
        rowNumber = 0;
        startPosition = new Vector2();
    }

    private Vector2 GetFirstSquarePosition() { //Kutucuklar�n konumlar� i�in ilk olu�turulan kutucu�un konumunun al�nmas�n� sa�layan fonksiyon.
        var startPosition = new Vector2(0f, transform.position.y);
        var squareRect = _words[0].GetComponent<RectTransform>();
        var parentRect = this.GetComponent<RectTransform>();
        var squareSize = new Vector2(0f, 0f);

        squareSize.x = squareRect.rect.width * squareRect.transform.localScale.x + offset;
        squareSize.y = squareRect.rect.height * squareRect.transform.localScale.y + offset;

        var shiftBy = (parentRect.rect.width - (squareSize.x * _columns)) / 2;

        startPosition.x = ((parentRect.rect.width - squareSize.x) / 2) * (-1);
        startPosition.x += shiftBy;
        startPosition.y = (parentRect.rect.height - squareSize.y) / 2;

        squareRect = _words[0].GetComponent<RectTransform>();
        parentRect = this.GetComponent<RectTransform>();
        squareSize = new Vector2(0f, 0f);
        return startPosition;
    }

}
