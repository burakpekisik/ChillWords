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
    public GameData currentGameData; //Oyunumuzda kelimelerimizi yerle�tirecek olan �ablonumuza eri�memiz i�in arac� olan dataya eri�im.
    public GameObject gridSquarePrefab; //Harflerin prefablerinin tan�mlanmas�.
    public AlphabetData alphabetData;
    public float squareOffset = 0.0f;
    public float topPosition;
    public List<GameObject> _squareList = new List<GameObject>();
    public BoardData GameDataInstance;

    private List<int> otherWordSamePositionIndex;
    private int positionIndex; //Olu�turulan dikd�rtgen �zerindeki konumlardan yer se�meyi sa�layan de�i�ken.
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
                        Debug.Log("Tablonuzdaki b�t�n bo�luklarda bir harf olmas� gerekiyor. L�tfen harf doldurma butonunu kullanarak tabloyu tamamlay�n�z.");

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

    public void CreateLayout() { //Oyun �ablonunun olu�turulmas�n� sa�layan fonksiyon.
        int direction; //Kelimeler yerle�tirirken kelimenin harflerinin hangi y�nde yerle�tirilece�ini g�steren de�i�ken.
        string kelime1, kelime2; //�ak��acak olan kelimelerin de�i�kenleri.
        bool ortakHarfBulundu = false; //�ak��an kelimeler aras�nda ortak harf bulunup bulunmad���n�n kontrol�n� sa�layan bool de�i�keni.
        otherWordsSamePosition = new List<Tuple<int, int>>(); //Ortak harflerin konumu liste halinde tutan de�i�ken
        otherWordSamePositionIndex = new List<int>(); 
        boardList = new List<Tuple<int, int>>(); //Dikd�rtgen �zerinde konumlar�n numaras�n� matris �eklinde tutan liste.
        char letter = ' ';
        int equalIndex1 = 0, equalIndex2 = 0;
        int tryCount = 0;

        ClearBoard(); //Fonksiyon �a��r�ld���nda oyun alan�n�n temizlenmesi.
        FillUpWithRandomLetters(); //Fonksiyon �a��r�ld���nda rastgele harflerin yerle�tirilmesi.

        //Ortak harf bulunmas�n� sa�layan while d�ng�s�.
        do {
            wordDatabase.RandomWords(); //Rastgele kelimelerin database �zerinden �a��r�lmas�.
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

        //�ak��malar�n �nlenmesi amac�yla listelerin temizlenmesi.
        otherWordSamePositionIndex.Clear();
        otherWordsSamePosition.Clear();
        boardList.Clear();

        //Dikd�rtgendeki konumlar�n listeye matris �eklinde aktar�lmas�.
        for (int i = 0; i < GameDataInstance.Rows; i++) {
            for (int j = 0; j < GameDataInstance.Columns; j++) {
                boardList.Add(new Tuple<int, int>(i, j));
            }
        }

        positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Rastgele �ekilde konumlar� se�meyi sa�layan de�i�kenin doldurulmas�.

        while (boardList[positionIndex] == null) { //E�er rastgele se�ilen de�erin y�nlendirdi�i konum bo�sa tekrardan konum adresi se�imi yap�l�r.
            positionIndex = UnityEngine.Random.Range(0, boardList.Count);
        }

        Tuple<int, int> position = boardList[positionIndex]; //Yaz�m� basitle�tirmek amac�yla konumlar bir de�i�kene atan�r.

        otherWordsSamePosition = new List<Tuple<int, int>>();
        while ((position.Item2 + kelime1.Length) > GameDataInstance.Columns) {
            position = rePosition();
        }
        for (int i = 0; i < kelime1.Length; i++) {
            GameDataInstance.Board[position.Item2 + i].Row[position.Item1] = kelime1[i].ToString(); //Kelimenin harflerinin s�tun numaras� sabit tutularak ve sat�r say�s� artt�r�larak yazd�r�lmas�.
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

        //Herhangi bir ta�ma durumu ya�anmas� halinde tekrar pozisyonlanman�n sa�lanmas� veya �ablon olu�turma i�leminin tekrardan yap�lmas�n�n sa�lanmas�.
        while (otherWordsSamePosition[0].Item1 + (kelime2.Length - equalIndex2 - 1) > GameDataInstance.Rows || otherWordsSamePosition[0].Item1 - (kelime2.Length + equalIndex2 - 1) < 0 || otherWordsSamePosition[0].Item1 + equalIndex2 > GameDataInstance.Rows - 1) {
            rePositionCollide(kelime1, letter);
            tryCount++;

            if (tryCount >= 5) {
                CreateLayout();
                return;
            }
        }
        if (kelime2[equalIndex2] == kelime2[kelime2.Length - 1] && equalIndex2 == kelime2.Length - 1) { //Son harfler ortak ise yazd�r�lma durumunun ger�ekle�tirilmesi.
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
        else if (kelime2[equalIndex2] == kelime2[0] && equalIndex2 == 0) { //�lk harfler ortak ise yerle�tirilme i�leminin ger�ekle�tirilmesi.
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
        else { //Harfler bu iki durumun d���nda ise yerle�tirilmenin yap�lmas�.
            int j = 1;
            for (int i = (equalIndex2 - 1); 0 <= i; --i) {
                tryCount = 0;
                if (boardList[otherWordSamePositionIndex[0] + (10 * j)] != null) {
                    //Ortak indeks �ncesi
                    GameDataInstance.Board[otherWordsSamePosition[0].Item2].Row[otherWordsSamePosition[0].Item1 + j] = kelime2[i].ToString(); //Kelimenin harflerinin s�tun numaras� sabit tutularak ve sat�r say�s� azalt�larak yazd�r�lmas�.
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
                    //Ortak indeks sonras�
                    GameDataInstance.Board[otherWordsSamePosition[0].Item2].Row[otherWordsSamePosition[0].Item1 - j] = kelime2[i].ToString(); //Kelimenin harflerinin s�tun numaras� sabit tutularak ve sat�r say�s� azalt�larak yazd�r�lmas�.
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
            //Oyun i�i kontrollerin yap�lmas� amac�yla debug ��kt�lar�n�n al�nmas�.
            Debug.Log("Position: " + position);
            Debug.Log("OtherWordsSamePosition: " + otherWordsSamePosition[0]);
        }
        for (int i = 0; i < wordDatabase.wordCountAlone; i++) {
            if (wordDatabase.aloneWords[i] != kelime1 && wordDatabase.aloneWords[i] != kelime2) {
                direction = UnityEngine.Random.Range(1, 5);
                positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Rastgele �ekilde konumlar� se�meyi sa�layan de�i�kenin doldurulmas�.
                while (boardList[positionIndex] == null) { //E�er rastgele se�ilen de�erin y�nlendirdi�i konum bo�sa tekrardan konum adresi se�imi yap�l�r.
                    positionIndex = UnityEngine.Random.Range(0, boardList.Count);
                }
                position = boardList[positionIndex];
                switch (direction) {
                    case 1: //Harflerin yukar� y�nde yazd�r�lma durumu.
                        Debug.Log("Before - Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Se�ilen kelimenin her harf konumu i�in kontrol i�leminin yap�lmas�.
                            while (position.Item1 - (wordDatabase.aloneWords[i].Length) < 0 || boardList[positionIndex - (10 * j)] == null) { //��lem sonunda kelimenin dikd�rtgen ta�mamas�n�n sa�lanmas� ve konumun bo� olmamas�n�n kontrol�.
                                position = rePosition();
                                j = 0;
                            }
                        }
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Kelimenin harf uzunlu�u kadar yazd�rma i�leminin yap�lmas�.
                            GameDataInstance.Board[position.Item2].Row[position.Item1 - j] = (wordDatabase.aloneWords[i][j]).ToString(); //Kelimenin harflerinin s�tun numaras� sabit tutularak ve sat�r say�s� azalt�larak yazd�r�lmas�.
                            boardList[positionIndex - (10 * j)] = null; //Kelimenin yazd�r�lan harflerinin konumlar�n�n listeden kald�r�lmas�.
                        }
                        Debug.Log("Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        break;
                    case 2: //Harflerin a�a�� y�nde yazd�r�lma durumu.
                        Debug.Log("Before - Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Se�ilen kelimenin her harf konumu i�in kontrol i�leminin yap�lmas�.
                            while (position.Item1 + (wordDatabase.aloneWords[i].Length) > GameDataInstance.Rows || boardList[positionIndex + (10 * j)] == null) { //��lem sonunda kelimenin dikd�rtgen ta�mamas�n�n sa�lanmas� ve konumun bo� olmamas�n�n kontrol�.
                                position = rePosition();
                                j = 0;
                            }
                        }
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Kelimenin harf uzunlu�u kadar yazd�rma i�leminin yap�lmas�.
                            GameDataInstance.Board[position.Item2].Row[position.Item1 + j] = (wordDatabase.aloneWords[i][j]).ToString(); //Kelimenin harflerinin s�tun numaras� sabit tutularak ve sat�r say�s� artt�r�larak yazd�r�lmas�.
                            boardList[positionIndex + (10 * j)] = null; //Kelimenin yazd�r�lan harflerinin konumlar�n�n listeden kald�r�lmas�.
                        }
                        Debug.Log("Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        break;
                    case 3: //Harflerin sola do�ru yazd�r�lmas�.
                        Debug.Log("Before - Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Se�ilen kelimenin her harf konumu i�in kontrol i�leminin yap�lmas�.
                            while (position.Item2 - (wordDatabase.aloneWords[i].Length) < 0 || boardList[positionIndex - j] == null) { //��lem sonunda kelimenin dikd�rtgen ta�mamas�n�n sa�lanmas� ve konumun bo� olmamas�n�n kontrol�.
                                position = rePosition();
                                j = 0;
                            }
                        }
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Kelimenin harf uzunlu�u kadar yazd�rma i�leminin yap�lmas�.
                            GameDataInstance.Board[position.Item2 - j].Row[position.Item1] = (wordDatabase.aloneWords[i][j]).ToString(); //Kelimenin harflerinin s�tun numaras� sabit tutularak ve sat�r say�s� artt�r�larak yazd�r�lmas�.
                            boardList[positionIndex - j] = null; //Kelimenin yazd�r�lan harflerinin konumlar�n�n listeden kald�r�lmas�.
                        }
                        Debug.Log("Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        break;
                    case 4: //Harflerin sa�a do�ru yazd�r�lmas�.
                        Debug.Log("Before - Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Se�ilen kelimenin her harf konumu i�in kontrol i�leminin yap�lmas�.
                            while (position.Item2 + (wordDatabase.aloneWords[i].Length) > GameDataInstance.Columns || boardList[positionIndex + j] == null) { //��lem sonunda kelimenin dikd�rtgen ta�mamas�n�n sa�lanmas� ve konumun bo� olmamas�n�n kontrol�.
                                position = rePosition();
                                j = 0;
                            }
                        }
                        for (int j = 0; j < wordDatabase.aloneWords[i].Length; j++) { //Kelimenin harf uzunlu�u kadar yazd�rma i�leminin yap�lmas�.
                            GameDataInstance.Board[position.Item2 + j].Row[position.Item1] = (wordDatabase.aloneWords[i][j]).ToString(); //Kelimenin harflerinin s�tun numaras� sabit tutularak ve sat�r say�s� artt�r�larak yazd�r�lmas�.
                            boardList[positionIndex + j] = null; //Yazd�r�lacak di�er kelimeye ge�i�in yap�lmas�.
                        }
                        Debug.Log("Word: " + wordDatabase.aloneWords[i] + " Start Row: " + position.Item1 + " Start Col: " + position.Item2 + " Direction: " + direction + " boardList Count: " + boardList.Count + " positionIndex: " + positionIndex);
                        break;
                }
            }
        }
        int k = 0;
        //Bulunacak kelimelerin kutucuklar�na kelimelerin yerle�tirilmesi.
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

        boardList = null; //Tekrardan ba�lat�l�rken s�k�nt� ya�anmamas� i�in matris listesi s�f�rlan�r.

        foreach (Transform child in gameObject.transform) { //Yeni tablo olu�tururken b�t�n elemanlar�n silinmesi.
            GameObject.Destroy(child.gameObject);
            _squareList = null;
            _squareList = new List<GameObject>();
        }
        SpawnGridSquares(); //Karelerin olu�turulmas�.
        SetSquaresPosition(); //Karelerin pozisyonlar�n�n ayarlanmas�.
    }

    //�ak��mayan kelimelerin harflerinin uygun konuma yerle�tirilememesi durumunda �a��r�lacak olan fonksiyon.
    private Tuple<int, int> rePosition() {
        positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Tekrardan indeks se�imi yap�lmas�.
        while (boardList[positionIndex] == null) { //Tekrardan indeks kontrol� yap�lmas�.
            positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Tekrardan sorun bulunursa tekrardan indeks se�imi yap�lmas�.
        }
        return boardList[positionIndex]; //Konumun de�i�kene atanmas�.
    }

    //�ak��an kelimelerin harflerinin uygun konuma yerle�tirilememesi durumunda �a��r�lacak olan fonksiyon.
    private void rePositionCollide(string kelime1, char letter) {
        boardList = new List<Tuple<int, int>>();
        otherWordSamePositionIndex = new List<int>();
        otherWordSamePositionIndex.Clear();
        boardList.Clear();
        otherWordsSamePosition.Clear();

        ClearBoard();
        FillUpWithRandomLetters();

        //Dikd�rtgendeki konumlar�n listeye matris �eklinde aktar�lmas�.
        for (int i = 0; i < GameDataInstance.Rows; i++) {
            for (int j = 0; j < GameDataInstance.Columns; j++) {
                boardList.Add(new Tuple<int, int>(i, j));
            }
        }

        positionIndex = UnityEngine.Random.Range(0, boardList.Count); //Rastgele �ekilde konumlar� se�meyi sa�layan de�i�kenin doldurulmas�.

        while (boardList[positionIndex] == null) { //E�er rastgele se�ilen de�erin y�nlendirdi�i konum bo�sa tekrardan konum adresi se�imi yap�l�r.
            positionIndex = UnityEngine.Random.Range(0, boardList.Count);
        }

        Tuple<int, int> position = boardList[positionIndex]; //Yaz�m� basitle�tirmek amac�yla konumlar bir de�i�kene atan�r.

        otherWordsSamePosition = new List<Tuple<int, int>>();
        while ((position.Item2 + kelime1.Length) > GameDataInstance.Columns) {
            position = rePosition();
        }
        for (int i = 0; i < kelime1.Length; i++) {
            GameDataInstance.Board[position.Item2 + i].Row[position.Item1] = kelime1[i].ToString(); //Kelimenin harflerinin s�tun numaras� sabit tutularak ve sat�r say�s� artt�r�larak yazd�r�lmas�.
            if (letter == kelime1[i]) {
                otherWordsSamePosition.Add(new Tuple<int, int>(position.Item1, position.Item2 + i));
                otherWordSamePositionIndex.Add(positionIndex + i);
            }
            boardList[positionIndex + i] = null;
        }
    }

    private void ClearBoard() {
        //Dikd�rtgen i�indeki harflerin temizlenmesi.
        for (int i = 0; i < GameDataInstance.Columns; i++) {
            for (int j = 0; j < GameDataInstance.Rows; j++) {
                GameDataInstance.Board[i].Row[j] = " ";
            }
        }
    }

    private void FillUpWithRandomLetters() {
        //Temizlenmi� olan dikd�rtgenler i�erisine random harfler yerle�tirilmesi.
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
    }
}
