using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUtility : MonoBehaviour
{
    //Sahne yüklenmesini sağlayan fonksiyon.
    public void LoadScene(string scenename) { 
        SceneManager.LoadScene(scenename);
    }
    
}
