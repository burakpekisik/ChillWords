using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class WordsGrid : MonoBehaviour {
    public GameData currentGameData; //Oyunumuzda kelimelerimizi yerleþtirecek olan þablonumuza eriþmemiz için aracý olan dataya eriþim.
    public GameObject gridSquarePrefab; //Harflerin prefablerinin tanýmlanmasý.
    public AlphabetData alphabetData;
    public float squareOffset = 0.0f;
    public float topPosition;
    public List<GameObject> _squareList = new List<GameObject>();
    public BoardData GameDataInstance;

    private List<int> otherWordSamePositionIndex;
    private int positionIndex; //Oluþturulan dikdörtgen üzerindeki konumlardan yer seçmeyi saðlayan deðiþken.
    private List<Tuple<int, int>> boardList;
    private List<Tuple<int, int>> otherWordsSamePosition;
    public WordDatabase wordDatabase;
    void Start() {
        wordDatabase = FindAnyObjectByType<WordDatabase>();
        currentGameData = Resources.Load<GameData>("Data/CurrentGameData");
        alphabetData = Resources.Load<AlphabetData>("Data/GameAlphabetTurkish");
        GameDataInstance = Resources.Load<BoardData>("Data/Puzzles/Puzzle1");
        CreateLayout();
    }
    public void SetSquaresPosition() {
        var squareRect = _squareList[0].GetComponent<SpriteRenderer>().sprite.rect;
        var squareTransform = _squareList[0].GetComponent<Transform>();

        var offset = new Vector2 {
            x = (squareRect.width * squareTransform.localScale.x + squareOffset) * 0.01f,
            y = (squareRect.height * squareTransform.localScale.y + squareOffset) * 0.01f
        };

        var startPosition = GetFirstSquarePosition();
        int columnNumber = 0;
        int rowNumber = 0;

        foreach (var square in _squareList) {
            if (rowNumber + 1 > currentGameData.selectedBoardData.Rows) {
                columnNumber++;
                rowNumber = 0;
            }

            var positionX = startPosition.x + offset.x * columnNumber;
            var positionY = startPosition.y - offset.y * rowNumber;

            square.GetComponent<Transform>().position = new Vector2(positionX, positionY);
            rowNumber++;
        }
    }

    public Vector2 GetFirstSquarePosition() {
        var startPosition = new Vector2(0f, transform.position.y);
        var squareRect = _squareList[0].GetComponent<SpriteRenderer>().sprite.rect;
        var squareTransform = _squareList[0].GetComponent<Transform>();
        var squareSize = new Vector2(0f, 0f);

        squareSize.x = squareRect.width * squareTransform.localScale.x;
        squareSize.y = squareRect.height * squareTransform.localScale.y;

        var midWidthPosition = (((currentGameData.selectedBoardData.Columns - 1) * squareSize.x) / 2) * 0.01f;
        var midWidthHeight = (((currentGameData.selectedBoardData.Rows - 1) * squareSize.y) / 2) * 0.01f;

        startPosition.x = (midWidthHeight != 0) ? midWidthPosition * -1 : midWidthPosition;
        startPosition.y += midWidthHeight;

        return startPosition;
    }

    public void SpawnGridSquares() {
        if (currentGameData != null) {
            var squareScale = GetSquareScale(new Vector3(1.5f, 1.5f, 0.1f));
            foreach (var squares in currentGameData.selectedBoardData.Board) {
                foreach (var squareLetter in squares.Row) {
                    var normalLetterData = alphabetData.AlphabetNormal.Find(data => data.letter == squareLetter);
                    var selectedLetterData = alphabetData.AlphabetHighlited.Find(data => data.letter == squareLetter);
                    var correctLetterData = alphabetData.AlphabetWrong.Find(data => data.letter == squareLetter);

                    if (normalLetterData.image == null || selectedLetterData.image == null) {
                        Debug.Log("Tablonuzdaki bütün boþluklarda bir harf olmasý gerekiyor. Lütfen harf doldurma butonunu kullanarak tabloyu tamamlayýnýz.");

#if UNITY_EDITOR

                        if (UnityEditor.EditorApplication.isPlaying) {
                            UnityEditor.EditorApplication.isPlaying = false;
                        }
#endif
                    }
                    else {
                        _squareList.Add(Instantiate(gridSquarePrefab));
                        _squareList[_squareList.Count - 1].GetComponent<GridSquare>().SetSprite(normalLetterData, correctLetterData, selectedLetterData);
                        _squareList[_squareList.Count - 1].transform.SetParent(this.transform);
                        _squareList[_squareList.Count - 1].GetComponent<Transform>().position = new Vector3(0f, 0f, 0f);
                        _squareList[_squareList.Count - 1].transform.localScale = squareScale;
                        _squareList[_squareList.Count - 1].GetComponent<GridSquare>().SetIndex(_squareList.Count - 1);
                    }
                }
            }

        }
    }

    public Vector3 GetSquareScale(Vector3 defaultScale) {
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

    public bool ShouldScaleDown(Vector3 targetScale) {
        var squareRect = gridSquarePrefab.GetComponent<SpriteRenderer>().sprite.rect;
        var squareSize = new Vector2(0f, 0f);
        var startPosition = new Vector2(0f, 0f);

        squareSize.y = (squareRect.width * targetScale.x) + squareOffset;

        squareSize.y = (squareRect.height * targetScale.y) + squareOffset;

        var midWidthPosition = ((currentGameData.selectedBoardData.Columns * squareSize.x) / 2) * 0.01f;
        var midWidthHeight = ((currentGameData.selectedBoardData.Rows * squareSize.y) / 2) * 0.01f;

        startPosition.x = (midWidthPosition != 0) ? midWidthPosition * -1 : midWidthPosition;
        startPosition.y = midWidthHeight;

        return startPosition.x < GetHalfScreenWidth() * -1 || startPosition.y > topPosition;
    }

    public float GetHalfScreenWidth() {
        float height = Camera.main.orthographicSize * 2;
        float width = (1.7f * height) * Screen.width / Screen.height;
        return width / 2;
    }

    public void CreateLayout() { //Oyun þablonunun oluþturulmasýný saðlayan fonksiyon.
        int direction; //Kelimeler yerleþtirirken kelimenin harflerinin hangi yönde yerleþtirileceðini gösteren deðiþken.
        string kelime1, kelime2; //Çakýþacak olan kelimelerin deðiþkenleri.
        bool ortakHarfBulundu = false; //Çakýþan kelimeler arasýnda ortak harf bulunup bulunmadýðýnýn kontrolünü saðlayan bool deðiþkeni.
        otherWordsSamePosition = new List<Tuple<int, int>>(); //Ortak harflerin konumu liste halinde tutan deðiþken
        otherWordSamePositionIndex = new List<int>(); 
        boardList = new List<Tuple<int, int>>(); //Dikdörtgen üzerinde konumlarýn numarasýný matris þeklinde tutan liste.
        char letter = ' ';
        int equalIndex1 = 0, equalIndex2 = 0;
        int tryCount = 0;

        ClearBoard(); //Fonksiyon çaðýrýldýðýnda oyun alanýnýn temizlenmesi.
        FillUpWithRandomLetters(); //Fonksiyon çaðýrýldýðýnda rastgele harflerin yerleþtirilmesi.

        //Ortak harf bulunmasýný saðlayan while döngüsü.
        do {
            wordDatabase.RandomWords(); //Rastgele kelimelerin database üzerinden çaðýrýlmasý.
            kelime1 = wordDatabase.commonWordsString[0];
            kelime2 = wordDatabase.commonWordsString[1];

            for (int i = 0; i < kelime1.Length; i++) {
                for (int j = 0; j < kelime2.Length; j++) {
                    if (kelime1[i] == kelime2[j]) {
                        ortakHarfBulundu = true;
                        letter = kelime1[i];
                        equalIndex1 = i;
                        equalIndex2 = j;
                        break;
                    }
                }
                if (ortakHarfBulundu == true) {
                    break;
                }
            }
        } while (ortakHarfBulundu == false);

        //Çakýþmalarýn önlenmesi amacýyla listelerin temizlenmesi.
        otherWordSamePositionIndex.Clear();
        otherWordsSamePosition.Clear();
        boardList.Clear();

        //Dikdörtgendeki konumlarýn listeye matris þeklinde aktarýlmasý.
        for (int i = 0; i < GameDataInstance.Rows; i++) {
            for (int j = 0; j < GameDataInstance.Columns; j++) {
                boardList.Add(new Tuple<int, int>(i, j));
            }
        }

        positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Rastgele þekilde konumlarý seçmeyi saðlayan deðiþkenin doldurulmasý.

        while (boardList[positionIndex] == null) { //Eðer rastgele seçilen deðerin yönlendirdiði konum boþsa tekrardan konum adresi seçimi yapýlýr.
            positionIndex = UnityEngine.Random.Range(0, boardList.Count);
        }

        Tuple<int, int> position = boardList[positionIndex]; //Yazýmý basitleþtirmek amacýyla konumlar bir deðiþkene atanýr.

        otherWordsSamePosition = new List<Tuple<int, int>>();
        while ((position.Item2 + kelime1.Length) > GameDataInstance.Columns) {
            position = rePosition();
        }
        for (int i = 0; i < kelime1.Length; i++) {
            GameDataInstance.Board[position.Item2 + i].Row[position.Item1] = kelime1[i].ToString(); //Kelimenin harflerinin sütun numarasý sabit tutularak ve satýr sayýsý arttýrýlarak yazdýrýlmasý.
            if (letter == kelime1[i]) {
                otherWordsSamePosition.Add(new Tuple<int, int>(position.Item1, position.Item2 + i));
                otherWordSamePositionIndex.Add(positionIndex + i);
            }
            boardList[positionIndex + i] = null;
        }

        int baslangicRow = otherWordsSamePosition[0].Item1;
        int oncesiYazdirilacakHarf = otherWordsSamePosition[0].Item1 + (kelime2.Length - equalIndex2 - 1);
        int sonrasiYazdirilacakHarf = otherWordsSamePosition[0].Item1 - (kelime2.Length + equalIndex2 - 1);
        Debug.Log("Kelime 1: " + kelime1 + " Kelime 2: " + kelime2);

        //Herhangi bir taþma durumu yaþanmasý halinde tekrar pozisyonlanmanýn saðlanmasý veya þablon oluþturma iþleminin tekrardan yapýlmasýnýn saðlanmasý.
        while (otherWordsSamePosition[0].Item1 + (kelime2.Length - equalIndex2 - 1) > GameDataInstance.Rows || otherWordsSamePosition[0].Item1 - (kelime2.Length + equalIndex2 - 1) < 0 || otherWordsSamePosition[0].Item1 + equalIndex2 > GameDataInstance.Rows - 1) {
            rePositionCollide(kelime1, letter);
            tryCount++;

            if (tryCount >= 5) {
                CreateLayout();
                return;
            }
        }
        if (kelime2[equalIndex2] == kelime2[kelime2.Length - 1] && equalIndex2 == kelime2.Length - 1) { //Son harfler ortak ise yazdýrýlma durumunun gerçekleþtirilmesi.
            Debug.Log("OtherWordsSamePosition: " + otherWordsSamePosition[0]);
            int j = 1;
            for (int i = (equalIndex2 - 1); 0 <= i; --i) {
                tryCount = 0;
                if (boardList[otherWordSamePositionIndex[0] + (10 * j)] != null) {
                    GameDataInstance.Board[otherWordsSamePosition[0].Item2].Row[otherWordsSamePosition[0].Item1 + j] = kelime2[i].ToString();
                    boardList[otherWordSamePositionIndex[0] + (10 * j)] = null;
                    j++;
                }
                else {
                    do {
                        rePositionCollide(kelime1, letter);
                        tryCount++;

                        if (tryCount >= 5) {
                            CreateLayout();
                            return;
                        }
                    } while (boardList[otherWordSamePositionIndex[0] + (10 * j)] == null);
                }
            }
        }
        else if (kelime2[equalIndex2] == kelime2[0] && equalIndex2 == 0) { //Ýlk harfler ortak ise yerleþtirilme iþleminin gerçekleþtirilmesi.
            Debug.Log("OtherWordsSamePosition: " + otherWordsSamePosition[0]);
            int j = 1;
            for (int i = equalIndex2 + 1; i < kelime2.Length; i++) {
                tryCount = 0;
                if (boardList[otherWordSamePositionIndex[0] - (10 * j)] != null) {
                    GameDataInstance.Board[otherWordsSamePosition[0].Item2].Row[otherWordsSamePosition[0].Item1 - j] = kelime2[i].ToString();
                    boardList[otherWordSamePositionIndex[0] - (10 * j)] = null;
                    j++;
                }
                else {
                    do {
                        rePositionCollide(kelime1, letter);
                        tryCount++;

                        if (tryCount >= 5) {
                            CreateLayout();
                            return;
                        }
                    } while (boardList[otherWordSamePositionIndex[0] - (10 * j)] == null);
                }
            }
        }
        else { //Harfler bu iki durumun dýþýnda ise yerleþtirilmenin yapýlmasý.
            int j = 1;
            for (int i = (equalIndex2 - 1); 0 <= i; --i) {
                tryCount = 0;
                if (boardList[otherWordSamePositionIndex[0] + (10 * j)] != null) {
                    //Ortak indeks öncesi
                    GameDataInstance.Board[otherWordsSamePosition[0].Item2].Row[otherWordsSamePosition[0].Item1 + j] = kelime2[i].ToString(); //Kelimenin harflerinin sütun numarasý sabit tutularak ve satýr sayýsý azaltýlarak yazdýrýlmasý.
                    boardList[otherWordSamePositionIndex[0] + (10 * j)] = null;
                    j++;
                }
                else {
                    do {
                        rePositionCollide(kelime1, letter);
                        tryCount++;

                        if (tryCount >= 5) {
                            CreateLayout();
                            return;
                        }
                    } while (boardList[otherWordSamePositionIndex[0] + (10 * j)] == null);
                }
            }

            j = 1;

            for (int i = (equalIndex2 + 1); i < kelime2.Length; i++) {
                tryCount = 0;
                if (boardList[otherWordSamePositionIndex[0] - (10 * j)] != null) {
                    //Ortak indeks sonrasý
                    GameDataInstance.Board[otherWordsSamePosition[0].Item2].Row[otherWordsSamePosition[0].Item1 - j] = kelime2[i].ToString(); //Kelimenin harflerinin sütun numarasý sabit tutularak ve satýr sayýsý azaltýlarak yazdýrýlmasý.
                    boardList[otherWordSamePositionIndex[0] - (10 * j)] = null;
                    j++;
                }
                else {
                    do {
                        rePositionCollide(kelime1, letter);
                        tryCount++;

                        if (tryCount >= 5) {
                            CreateLayout();
                            return;
                        }
                    } while (boardList[otherWordSamePositionIndex[0] - (10 * j)] == null);
                }
            }
            //Oyun içi kontrollerin yapýlmasý amacýyla debug çýktýlarýnýn alýnmasý.
            Debug.Log("Position: " + position);
            Debug.Log("OtherWordsSamePosition: " + otherWordsSamePosition[0]);
        }
        for (int i = 0; i < wordDatabase.wordCountAlone; i++) {
            if (wordDatabase.aloneWords[i] != kelime1 && wordDatabase.aloneWords[i] != kelime2) {
                direction = UnityEngine.Random.Range(1, 5);
                positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Rastgele þekilde konumlarý seçmeyi saðlayan deðiþkenin doldurulmasý.
                while (boardList[positionIndex] == null) { //Eðer rastgele seçilen deðerin yönlendirdiði konum boþsa tekrardan konum adresi seçimi yapýlýr.
                    positionIndex = UnityEngine.Random.Range(0, boardList.Count);
                }
                position = boardList[positionIndex];
                switch (direction) {
                    case 1: //Harflerin yukarý yönde yazdýrýlma durumu.
                        Debug.Log("Before - Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Seçilen kelimenin her harf konumu için kontrol iþleminin yapýlmasý.
                            while (position.Item1 - (wordDatabase.aloneWords[i].Length) < 0 || boardList[positionIndex - (10 * j)] == null) { //Ýþlem sonunda kelimenin dikdörtgen taþmamasýnýn saðlanmasý ve konumun boþ olmamasýnýn kontrolü.
                                position = rePosition();
                                j = 0;
                            }
                        }
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Kelimenin harf uzunluðu kadar yazdýrma iþleminin yapýlmasý.
                            GameDataInstance.Board[position.Item2].Row[position.Item1 - j] = (wordDatabase.aloneWords[i][j]).ToString(); //Kelimenin harflerinin sütun numarasý sabit tutularak ve satýr sayýsý azaltýlarak yazdýrýlmasý.
                            boardList[positionIndex - (10 * j)] = null; //Kelimenin yazdýrýlan harflerinin konumlarýnýn listeden kaldýrýlmasý.
                        }
                        Debug.Log("Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        break;
                    case 2: //Harflerin aþaðý yönde yazdýrýlma durumu.
                        Debug.Log("Before - Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Seçilen kelimenin her harf konumu için kontrol iþleminin yapýlmasý.
                            while (position.Item1 + (wordDatabase.aloneWords[i].Length) > GameDataInstance.Rows || boardList[positionIndex + (10 * j)] == null) { //Ýþlem sonunda kelimenin dikdörtgen taþmamasýnýn saðlanmasý ve konumun boþ olmamasýnýn kontrolü.
                                position = rePosition();
                                j = 0;
                            }
                        }
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Kelimenin harf uzunluðu kadar yazdýrma iþleminin yapýlmasý.
                            GameDataInstance.Board[position.Item2].Row[position.Item1 + j] = (wordDatabase.aloneWords[i][j]).ToString(); //Kelimenin harflerinin sütun numarasý sabit tutularak ve satýr sayýsý arttýrýlarak yazdýrýlmasý.
                            boardList[positionIndex + (10 * j)] = null; //Kelimenin yazdýrýlan harflerinin konumlarýnýn listeden kaldýrýlmasý.
                        }
                        Debug.Log("Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        break;
                    case 3: //Harflerin sola doðru yazdýrýlmasý.
                        Debug.Log("Before - Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Seçilen kelimenin her harf konumu için kontrol iþleminin yapýlmasý.
                            while (position.Item2 - (wordDatabase.aloneWords[i].Length) < 0 || boardList[positionIndex - j] == null) { //Ýþlem sonunda kelimenin dikdörtgen taþmamasýnýn saðlanmasý ve konumun boþ olmamasýnýn kontrolü.
                                position = rePosition();
                                j = 0;
                            }
                        }
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Kelimenin harf uzunluðu kadar yazdýrma iþleminin yapýlmasý.
                            GameDataInstance.Board[position.Item2 - j].Row[position.Item1] = (wordDatabase.aloneWords[i][j]).ToString(); //Kelimenin harflerinin sütun numarasý sabit tutularak ve satýr sayýsý arttýrýlarak yazdýrýlmasý.
                            boardList[positionIndex - j] = null; //Kelimenin yazdýrýlan harflerinin konumlarýnýn listeden kaldýrýlmasý.
                        }
                        Debug.Log("Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        break;
                    case 4: //Harflerin saða doðru yazdýrýlmasý.
                        Debug.Log("Before - Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Seçilen kelimenin her harf konumu için kontrol iþleminin yapýlmasý.
                            while (position.Item2 + (wordDatabase.aloneWords[i].Length) > GameDataInstance.Columns || boardList[positionIndex + j] == null) { //Ýþlem sonunda kelimenin dikdörtgen taþmamasýnýn saðlanmasý ve konumun boþ olmamasýnýn kontrolü.
                                position = rePosition();
                                j = 0;
                            }
                        }
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Kelimenin harf uzunluðu kadar yazdýrma iþleminin yapýlmasý.
                            GameDataInstance.Board[position.Item2 + j].Row[position.Item1] = (wordDatabase.aloneWords[i][j]).ToString(); //Kelimenin harflerinin sütun numarasý sabit tutularak ve satýr sayýsý arttýrýlarak yazdýrýlmasý.
                            boardList[positionIndex + j] = null; //Yazdýrýlacak diðer kelimeye geçiþin yapýlmasý.
                        }
                        Debug.Log("Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        break;
                }
            }
        }
        int k = 0;
        //Bulunacak kelimelerin kutucuklarýna kelimelerin yerleþtirilmesi.
        for (k = 0; k < wordDatabase.wordCountAlone + wordDatabase.wordCountCollide; k++) {
            if (k < wordDatabase.wordCountCollide) {
                currentGameData.selectedBoardData.SearchWords[k].Word = wordDatabase.commonWordsString[k];
            }
            else {
                int f = 0;
                currentGameData.selectedBoardData.SearchWords[k].Word = wordDatabase.aloneWords[f];
                f++;
            }
        }

        boardList = null; //Tekrardan baþlatýlýrken sýkýntý yaþanmamasý için matris listesi sýfýrlanýr.

        foreach (Transform child in gameObject.transform) { //Yeni tablo oluþtururken bütün elemanlarýn silinmesi.
            GameObject.Destroy(child.gameObject);
            _squareList = null;
            _squareList = new List<GameObject>();
        }
        SpawnGridSquares(); //Karelerin oluþturulmasý.
        SetSquaresPosition(); //Karelerin pozisyonlarýnýn ayarlanmasý.
    }

    //Çakýþmayan kelimelerin harflerinin uygun konuma yerleþtirilememesi durumunda çaðýrýlacak olan fonksiyon.
    private Tuple<int, int> rePosition() {
        positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Tekrardan indeks seçimi yapýlmasý.
        while (boardList[positionIndex] == null) { //Tekrardan indeks kontrolü yapýlmasý.
            positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Tekrardan sorun bulunursa tekrardan indeks seçimi yapýlmasý.
        }
        return boardList[positionIndex]; //Konumun deðiþkene atanmasý.
    }

    //Çakýþan kelimelerin harflerinin uygun konuma yerleþtirilememesi durumunda çaðýrýlacak olan fonksiyon.
    private void rePositionCollide(string kelime1, char letter) {
        boardList = new List<Tuple<int, int>>();
        otherWordSamePositionIndex = new List<int>();
        otherWordSamePositionIndex.Clear();
        boardList.Clear();
        otherWordsSamePosition.Clear();

        ClearBoard();
        FillUpWithRandomLetters();

        //Dikdörtgendeki konumlarýn listeye matris þeklinde aktarýlmasý.
        for (int i = 0; i < GameDataInstance.Rows; i++) {
            for (int j = 0; j < GameDataInstance.Columns; j++) {
                boardList.Add(new Tuple<int, int>(i, j));
            }
        }

        positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Rastgele þekilde konumlarý seçmeyi saðlayan deðiþkenin doldurulmasý.

        while (boardList[positionIndex] == null) { //Eðer rastgele seçilen deðerin yönlendirdiði konum boþsa tekrardan konum adresi seçimi yapýlýr.
            positionIndex = UnityEngine.Random.Range(0, boardList.Count);
        }

        Tuple<int, int> position = boardList[positionIndex]; //Yazýmý basitleþtirmek amacýyla konumlar bir deðiþkene atanýr.

        otherWordsSamePosition = new List<Tuple<int, int>>();
        while ((position.Item2 + kelime1.Length) > GameDataInstance.Columns) {
            position = rePosition();
        }
        for (int i = 0; i < kelime1.Length; i++) {
            GameDataInstance.Board[position.Item2 + i].Row[position.Item1] = kelime1[i].ToString(); //Kelimenin harflerinin sütun numarasý sabit tutularak ve satýr sayýsý arttýrýlarak yazdýrýlmasý.
            if (letter == kelime1[i]) {
                otherWordsSamePosition.Add(new Tuple<int, int>(position.Item1, position.Item2 + i));
                otherWordSamePositionIndex.Add(positionIndex + i);
            }
            boardList[positionIndex + i] = null;
        }
    }

    private void ClearBoard() {
        //Dikdörtgen içindeki harflerin temizlenmesi.
        for (int i = 0; i < GameDataInstance.Columns; i++) {
            for (int j = 0; j < GameDataInstance.Rows; j++) {
                GameDataInstance.Board[i].Row[j] = " ";
            }
        }
    }

    private void FillUpWithRandomLetters() {
        //Temizlenmiþ olan dikdörtgenler içerisine random harfler yerleþtirilmesi.
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
    }
}
