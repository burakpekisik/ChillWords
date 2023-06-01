using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]

public class AlphabetData : ScriptableObject
{
    //Harf görselleri için harf ve spritelerini içerecek objectin oluþturulmasý.
    [System.Serializable]
    public class LetterData {
        public string letter;
        public Sprite image;
    }

    public List<LetterData> AlphabetPlain = new List<LetterData>();
    public List<LetterData> AlphabetNormal = new List<LetterData>();
    public List<LetterData> AlphabetHighlited = new List<LetterData>();
    public List<LetterData> AlphabetWrong = new List<LetterData>();

}
