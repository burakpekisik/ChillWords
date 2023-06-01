using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HintScript : MonoBehaviour
{
    public Button button; //�pucu butonunun tan�mlanmas�.
    public GameObject wordsBoxes; //Kelimelerin yaz�l� oldu�u kutucuklar�n ana objesinin tan�mlanmas�.
    public WordDatabase wordDatabase; //Database'e ba�lant�n�n tan�mlanmas�.
    public List<GameObject> wordsBoxesChilds; //Kelimelerin yaz�l� oldu�u kutucuklar�n liste �eklinde yaz�m�.
    public List<GameObject> wordsBoxesChildsTexts; //Kelimelerin yaz�l� oldu�u kutucuklar�n "Text" bile�enlerinin liste halinde tan�m�.
    public int childCount; //Kutucuk say�s�n�n belirtilmesi
    public int scoreDecrease = 0; //Skor azaltma seviyesinin belirlenmesi.

    //�pucu fonksiyonunun tan�mlanmas�.
    public void ShowHint() {
        int randomBox; //Rastgele kelimenin se�ilmesi index'inin belirlenmesi.
        bool allActive = false; //B�t�n ipu�lar�n�n a��k olup olmad���na dair bool de�eri.

        if (scoreDecrease >= wordDatabase.wordCountAlone + wordDatabase.wordCountCollide ) { //B�t�n kelimelerin a��l�p a��lmad���na dair kontrol.
            foreach (GameObject item in wordsBoxesChilds) { //Kelime kutular�n�n her birinin incelenemesinin sa�lanmas�.
                if (item != null) { //Kutunun silinip silinmedi�inin kontrol�.
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

            while (wordsBoxesChilds[randomBox] == null || wordsBoxesChildsTexts[randomBox] == null) { //Text veya kutucuk de�eri yoksa tekrar se�im yap�l�r.
                randomBox = Random.Range(0, childCount);
            }

            wordsBoxesChilds[randomBox].GetComponent<Image>().enabled = true; //Random kutucu�un devreye sokulmas�.
            wordsBoxesChildsTexts[randomBox].GetComponent<Text>().enabled = true; //Random kutucu�un text eleman�n�n devreye sokulmas�.

            wordsBoxesChilds[randomBox] = null; //Random kutucu�unun listeden silinmesi.
            wordsBoxesChildsTexts[randomBox] = null; //Random kutucu�un text child'�n�n listeden silinmesi.

            scoreDecrease++; //Skor d���rme de�i�keninin artt�r�lmas�.
        }
    }

    //Kutucuklar�n bulunmas�.
    public void FindChild() {
        wordsBoxes = GameObject.Find("Words"); //Kutucuklar�n bulunmas�.

        childCount = wordsBoxes.transform.childCount; //Kutucuk say�s�n�n belirlenmesi.

        //Kutucuklar�n listeye eklenmesi.
        for (int i = 0; i < childCount; i++) {
            Transform child = wordsBoxes.transform.GetChild(i);

            wordsBoxesChilds.Add(child.gameObject);
            wordsBoxesChilds[i].GetComponent<Image>().enabled = false;

            //Kutucuklar�n "Text" elemanlar�n�n al�nmas�.
            if (child.childCount > 0) {
                Transform childText = wordsBoxesChilds[i].transform.GetChild(0);
                wordsBoxesChildsTexts.Add(childText.gameObject);
                wordsBoxesChildsTexts[i].GetComponent<Text>().enabled = false;
            }
        }
    }
}
