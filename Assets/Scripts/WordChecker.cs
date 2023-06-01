using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordChecker : MonoBehaviour {
    //Kelime kontrolünde kullanýlacak olan scriptlerin tanýmlanmasý.
    public GameData currentGameData;
    private WordDatabase wordDatabase;
    public Score scoreScript;
    public TimeScript timeScript;

    //Kelime tanýmýnda kullanýlacak olan kelime tanýmýnýn yapýlmasý.
    private string _word;

    //Doðru bulunan kelimelerde müdahale edilecek scriptlerin tanýmlanmasý.
    private WordsGrid wordsGrid;
    private SearchingWordsList searchingWordsList;
    private int foundCorrectWords;

    private int _assignedPoints = 0;
    private Ray _rayUp, _rayDown;
    private Ray _rayLeft, _rayRight;
    private Ray _currentRay = new Ray();
    private Vector3 _rayStartPosition;
    private List<int> _correctSquareList = new List<int>();
    public List<string> foundWords;
    public HintScript hintScript;

    private void OnEnable() {
        GameEvents.OnCheckSquare += SquareSelected;
        GameEvents.OnClearSelection += ClearSelection;
    }

    private void OnDisable() {
        GameEvents.OnCheckSquare -= SquareSelected;
        GameEvents.OnClearSelection -= ClearSelection;
    }
    // Start is called before the first frame update
    void Start() {
        currentGameData = Resources.Load<GameData>("Data/CurrentGameData");
        _assignedPoints = 0;
        wordsGrid = FindAnyObjectByType<WordsGrid>();
        searchingWordsList = FindAnyObjectByType<SearchingWordsList>();
        wordDatabase = GameObject.Find("WordDatabase").GetComponent<WordDatabase>();
        scoreScript = GameObject.Find("Score").GetComponent<Score>();
        foundWords = new List<string>(wordDatabase.wordCountAlone + wordDatabase.wordCountCollide);
        hintScript = FindAnyObjectByType<HintScript>();
        timeScript = FindAnyObjectByType<TimeScript>();
    }

    //Ýþaretleme yapýlan harflerin tek yönde ilerleyecek þekilde alýnmasýnýn saðlanmasý.
    void Update() {
        if (_assignedPoints > 0 && Application.isEditor) {
            Debug.DrawRay(_rayUp.origin, _rayUp.direction * 4);
            Debug.DrawRay(_rayDown.origin, _rayDown.direction * 4);
            Debug.DrawRay(_rayLeft.origin, _rayLeft.direction * 4);
            Debug.DrawRay(_rayRight.origin, _rayRight.direction * 4);
        }
    }

    //Harf iþaretlemesinin yapýlmasýnýn saðlanmasý.
    private void SquareSelected(string letter, Vector3 squarePosition, int squareIndex) {
        if (_assignedPoints == 0) {
            _rayStartPosition = squarePosition;
            _correctSquareList.Add(squareIndex);
            _word += letter;

            _rayUp = new Ray(new Vector2(squarePosition.x, squarePosition.y), new Vector2(0f, 1));
            _rayDown = new Ray(new Vector2(squarePosition.x, squarePosition.y), new Vector2(0f, -1));
            _rayLeft = new Ray(new Vector2(squarePosition.x, squarePosition.y), new Vector2(-1, 0f));
            _rayRight = new Ray(new Vector2(squarePosition.x, squarePosition.y), new Vector2(1, 0f));
        }
        else if (_assignedPoints == 1) {
            _correctSquareList.Add(squareIndex);
            _currentRay = SelectRay(_rayStartPosition, squarePosition);
            GameEvents.SelectSquareMethod(squarePosition);
            _word += letter;
            CheckWord();
        }
        else {
            if (IsPointOnTheRay(_currentRay, squarePosition)) {
                _correctSquareList.Add(squareIndex);
                GameEvents.SelectSquareMethod(squarePosition);
                _word += letter;
                CheckWord();
            }
        }
        _assignedPoints++;
    }

    //Seçilen harflerin kombinasyonun kontrolünün saðlanmasý.
    private void CheckWord() {
        bool isWordFound = false;
        
        foreach (var searchingWord in currentGameData.selectedBoardData.SearchWords) {
            if (_word == searchingWord.Word) {
                for (int k = 0; k <= foundWords.Count - 1; k++) {
                    if (foundWords[k] != _word) {
                        isWordFound = false;
                    }
                    else {
                        isWordFound = true;
                        break;
                    }
                }
                if (isWordFound == false) {
                    foundWords.Add(_word);
                    for (int i = 0; i < hintScript.wordsBoxesChildsTexts.Count; i++) {
                        if (hintScript.wordsBoxesChildsTexts[i] != null) {
                            if (_word == hintScript.wordsBoxesChildsTexts[i].GetComponent<Text>().text) {
                                hintScript.wordsBoxesChilds[i].GetComponent<Image>().enabled = true;
                                hintScript.wordsBoxesChildsTexts[i].GetComponent<Text>().enabled = true;

                                hintScript.wordsBoxesChildsTexts[i] = null;
                                hintScript.wordsBoxesChilds[i] = null;
                            }
                        }
                        
                    }

                    for (int i = 0; i < wordDatabase.wordCountCollide; i++) {
                        if (_word == wordDatabase.commonWordsString[i]) {
                            scoreScript.score += wordDatabase.commonWordsScores[i];
                        }
                    }
                    for (int i = 0; i < wordDatabase.wordCountAlone; i++) {
                        if (_word == wordDatabase.aloneWords[i]) {
                            scoreScript.score += wordDatabase.aloneWordsScores[i];
                        }
                    }
                    GameEvents.CorrectWordMethod(_word, _correctSquareList);
                    _word = string.Empty;
                    _correctSquareList.Clear();
                    foundCorrectWords++;
                    Debug.Log(foundCorrectWords);
                    if (foundCorrectWords == wordDatabase.wordCountAlone + wordDatabase.wordCountCollide) {
                        foreach (Transform child in GameObject.Find("Words").transform) { //Yeni tablo oluþtururken bütün elemanlarýn silinmesi.
                            GameObject.Destroy(child.gameObject);
                        }
                        searchingWordsList._words.Clear();
                        searchingWordsList._columns = 2;
                        searchingWordsList._rows = 3;
                        searchingWordsList._wordsNumber = currentGameData.selectedBoardData.SearchWords.Count;

                        if (searchingWordsList._wordsNumber < searchingWordsList._columns) {
                            searchingWordsList._rows = 1;
                        }
                        else {
                            searchingWordsList.CalculateColumnsAndRowsNumber();
                        }
                        wordsGrid.CreateLayout();
                        searchingWordsList.CreateWordObjects();
                        searchingWordsList.SetWordsPosition();
                        foundWords.Clear();
                        foundCorrectWords = 0;
                        hintScript.wordsBoxesChilds.Clear();
                        hintScript.wordsBoxesChildsTexts.Clear();
                        hintScript.FindChild();
                        timeScript.time += 20;
                    }
                }
                return;
            }
        }
    }

    private bool IsPointOnTheRay(Ray currentRay, Vector3 point) {
        var hits = Physics.RaycastAll(currentRay, 100.0f);

        for (int i = 0; i < hits.Length; i++) {
            if (hits[i].transform.position == point) {
                return true;
            }
        }

        return false;
    }

    private Ray SelectRay(Vector2 firstPosition, Vector2 secondPosition) {
        var direction = (secondPosition - firstPosition).normalized;
        float tolerance = 0.01f;

        if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - 1f) < tolerance) {
            return _rayUp;
        }
        if (Math.Abs(direction.x) < tolerance && Math.Abs(direction.y - (-1f)) < tolerance) {
            return _rayDown;
        }
        if (Math.Abs(direction.x - (-1f)) < tolerance && Math.Abs(direction.y) < tolerance) {
            return _rayLeft;
        }
        if (Math.Abs(direction.x - 1f) < tolerance && Math.Abs(direction.y) < tolerance) {
            return _rayRight;
        }
        return _rayDown;
    }

    private void ClearSelection() {
        _assignedPoints = 0;
        _correctSquareList.Clear();
        _word = string.Empty;
    }
}
