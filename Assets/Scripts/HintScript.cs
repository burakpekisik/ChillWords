using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HintScript : MonoBehaviour
{
    public Button button; //Ýpucu butonunun tanýmlanmasý.
    public GameObject wordsBoxes; //Kelimelerin yazýlý olduðu kutucuklarýn ana objesinin tanýmlanmasý.
    public WordDatabase wordDatabase; //Database'e baðlantýnýn tanýmlanmasý.
    public List<GameObject> wordsBoxesChilds; //Kelimelerin yazýlý olduðu kutucuklarýn liste þeklinde yazýmý.
    public List<GameObject> wordsBoxesChildsTexts; //Kelimelerin yazýlý olduðu kutucuklarýn "Text" bileþenlerinin liste halinde tanýmý.
    public int childCount; //Kutucuk sayýsýnýn belirtilmesi
    public int scoreDecrease = 0; //Skor azaltma seviyesinin belirlenmesi.

    //Ýpucu fonksiyonunun tanýmlanmasý.
    public void ShowHint() {
        int randomBox; //Rastgele kelimenin seçilmesi index'inin belirlenmesi.
        bool allActive = false; //Bütün ipuçlarýnýn açýk olup olmadýðýna dair bool deðeri.

        if (scoreDecrease >= wordDatabase.wordCountAlone + wordDatabase.wordCountCollide ) { //Bütün kelimelerin açýlýp açýlmadýðýna dair kontrol.
            foreach (GameObject item in wordsBoxesChilds) { //Kelime kutularýnýn her birinin incelenemesinin saðlanmasý.
                if (item != null) { //Kutunun silinip silinmediðinin kontrolü.
                    if (item.GetComponent<Image>().IsActive()) {
                        allActive = true;
                    }
                    else {
                        allActive = false;
                        break;
                    }
                }
            }
            if (allActive == true) {
                button.gameObject.SetActive(false);
            }
        }
        else {
            randomBox = Random.Range(0, childCount); //Random kelimenin belirlenmesi.

            while (wordsBoxesChilds[randomBox] == null || wordsBoxesChildsTexts[randomBox] == null) { //Text veya kutucuk deðeri yoksa tekrar seçim yapýlýr.
                randomBox = Random.Range(0, childCount);
            }

            wordsBoxesChilds[randomBox].GetComponent<Image>().enabled = true; //Random kutucuðun devreye sokulmasý.
            wordsBoxesChildsTexts[randomBox].GetComponent<Text>().enabled = true; //Random kutucuðun text elemanýnýn devreye sokulmasý.

            wordsBoxesChilds[randomBox] = null; //Random kutucuðunun listeden silinmesi.
            wordsBoxesChildsTexts[randomBox] = null; //Random kutucuðun text child'ýnýn listeden silinmesi.

            scoreDecrease++; //Skor düþürme deðiþkeninin arttýrýlmasý.
        }
    }

    //Kutucuklarýn bulunmasý.
    public void FindChild() {
        wordsBoxes = GameObject.Find("Words"); //Kutucuklarýn bulunmasý.

        childCount = wordsBoxes.transform.childCount; //Kutucuk sayýsýnýn belirlenmesi.

        //Kutucuklarýn listeye eklenmesi.
        for (int i = 0; i < childCount; i++) {
            Transform child = wordsBoxes.transform.GetChild(i);

            wordsBoxesChilds.Add(child.gameObject);
            wordsBoxesChilds[i].GetComponent<Image>().enabled = false;

            //Kutucuklarýn "Text" elemanlarýnýn alýnmasý.
            if (child.childCount > 0) {
                Transform childText = wordsBoxesChilds[i].transform.GetChild(0);
                wordsBoxesChildsTexts.Add(childText.gameObject);
                wordsBoxesChildsTexts[i].GetComponent<Text>().enabled = false;
            }
        }
    }
}
