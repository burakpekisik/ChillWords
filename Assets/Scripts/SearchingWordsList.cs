using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchingWordsList : MonoBehaviour
{
    public GameData currentGameData; //currentGameData'nýn tanýmlanmasý.
    public GameObject searchingWordPrefab; //Aranan kelimenin prefab halinin tanýmlanmasý.
    public HintScript hintScript; //Ýpucu sisteminin çaðýrýlmasý.
    public float offset = 0.0f;
    public int maxColumns = 5; //Ýpuçlarýnýn yerleþtirilebileceði maksimum sütun sayýsýnýn belirlenmesi.
    public int maxRows = 5; //Ýpuçlarýnýn yerleþtirilebileceði maksimum satýr sayýsýnýn belirlenmesi.

    public int _columns = 2;
    public int _rows;
    public int _wordsNumber; //Kelime sayýsýný belirleyen deðiþken.

    public List<GameObject> _words = new List<GameObject>(); //Yazdýrýlacak kelimelerin listenin belirlenmesi.

    // Start is called before the first frame update
    private void Start()
    {
        hintScript = FindAnyObjectByType<HintScript>(); //Ýpucu scriptinin bulunmasý.
        currentGameData = Resources.Load<GameData>("Data/CurrentGameData"); //currentGameData'nýn bulunmasý.

        _wordsNumber = currentGameData.selectedBoardData.SearchWords.Count; //Kelime sayýsýnýn belirlenmesi.

        if (_wordsNumber < _columns) {
            _rows = 1;
        }
        else {
            CalculateColumnsAndRowsNumber();
        }
        CreateWordObjects(); //Ýpuçlarýný oluþturacak olan fonksiyonun çaðýrýlmasý.
        SetWordsPosition(); //Ýpuçlarýnýn konumlarýný ayarlayacak olan fonksiyonun çaðýrýlmasý.
        hintScript.FindChild(); //Ýpuçlarý oluþtuktan sonra ipucu sistemi için oluþturulan kelime kutularýnýn bulunmasýnýn saðlanmasý.
        _words.Clear();
    }

    public void CalculateColumnsAndRowsNumber() { //Ýpuçlarýnýn yazdýrýlacaðý satýr ve sütun sayýsýnýn belirleyen fonksiyon.
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

    public void CreateWordObjects() { //Kelime kutucuklarýnýn(Ýpucu) oluþturulmasý.
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

    public void SetWordsPosition() { //Oluþturulan kelime kutucuklarýnýn konumlarýnýn ayarlanmasýný saðlayan fonksiyon.
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

    private Vector2 GetFirstSquarePosition() { //Kutucuklarýn konumlarý için ilk oluþturulan kutucuðun konumunun alýnmasýný saðlayan fonksiyon.
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
