using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(AlphabetData))]
[CanEditMultipleObjects]
[System.Serializable]
public class AlphabetDataDrawer : Editor
{
    //Oyun d�zeninde kullan�lacak olan kelimeler i�in harf g�rsellerinin al�nmas�n� sa�layan Edit�r men�s�n�n olu�turulmas�.

    private ReorderableList AlphabetPlainList;
    private ReorderableList AlphabetNormalList;
    private ReorderableList AlphabetHighlitedList;
    private ReorderableList AlphabetWrongList;
    private void OnEnable() {
        InitializeReordableList(ref AlphabetPlainList, "AlphabetPlain", "Alphabet Plain");
        InitializeReordableList(ref AlphabetNormalList, "AlphabetNormal", "Alphabet Normal");
        InitializeReordableList(ref AlphabetHighlitedList, "AlphabetHighlited", "Alphabet Highlited");
        InitializeReordableList(ref AlphabetWrongList, "AlphabetWrong", "Alphabet Wrong");
    }
    public override void OnInspectorGUI() {
        serializedObject.Update();
        AlphabetPlainList.DoLayoutList();
        AlphabetNormalList.DoLayoutList();
        AlphabetHighlitedList.DoLayoutList();
        AlphabetWrongList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
    private void InitializeReordableList(ref ReorderableList list, string propertyName, string listLabel) {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty(propertyName), true, true, true, true);
        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, listLabel);
        };

        var l = list;

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("letter"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width - 60 - 30, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("image"), GUIContent.none);
        };
    }

}
