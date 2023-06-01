using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public delegate void EnableSquareSelection();
    public static event EnableSquareSelection OnEnableSquareSelection;
    public static void EnableSquareSelectionMethod() { //Kare se�iminin sa�lanmas�.
        if (OnEnableSquareSelection != null) {
            OnEnableSquareSelection();
        }
    }

    public delegate void DisableSquareSelection();
    public static event DisableSquareSelection OnDisableSquareSelection;
    public static void DisableSquareSelectionMethod() { //Kare se�iminin kapanmas�.
        if (OnDisableSquareSelection != null) {
            OnDisableSquareSelection();
        }
    }

    public delegate void SelectSquare(Vector3 position);
    public static event SelectSquare OnSelectSquare;
    public static void SelectSquareMethod(Vector3 position) {
        if (OnSelectSquare != null) {
            OnSelectSquare(position);
        }
    }

    public delegate void CheckSquare(string letter, Vector3 squarePosition, int squareIndex);
    public static event CheckSquare OnCheckSquare;
    public static void CheckSquareMethod(string letter, Vector3 squarePosition, int squareIndex) { //Girilen harflerin kontrol�.
        if (OnCheckSquare != null) {
            OnCheckSquare(letter, squarePosition, squareIndex);
        }
    }

    public delegate void ClearSelection();
    public static event ClearSelection OnClearSelection;
    public static void ClearSelectionMethod() { //Se�imin temizlenmesi.
        if (OnClearSelection != null) {
            OnClearSelection();
        }
    }

    public delegate void CorrectWord(string word, List<int> squareIndexes);
    public static event CorrectWord OnCorrectWord;
    public static void CorrectWordMethod(string word, List<int> squareIndexes) { //Se�ilen harflerin kombinasyonunun do�ru kelime olmas� halinde ger�ekle�ecek fonksiyon.
        if (OnCorrectWord != null) {
            OnCorrectWord(word, squareIndexes);
        }
    }
}
