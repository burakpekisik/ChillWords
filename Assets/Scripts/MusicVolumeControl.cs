using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeControl : MonoBehaviour
{
    public AudioSource audioSource;
    public Slider slider;
    
    private void Start()
    {
        //sliderin de�erini m�zi�in ba�lang�� sesinin de�erine ata
        slider.value = audioSource.volume;
        slider.value = PlayerPrefs.GetFloat("MusicVolume", 0); //Oyun a��ld���nda slider de�erinin oyun kapat�ld��� zamanki de�erinden devam etmesinin sa�lanmas�.
        //sliderin de�i�imine ba�l�  olarak sesi g�ncelle
        slider.onValueChanged.AddListener(ChangeVolume);
    }

    
    private void ChangeVolume(float volume)
    {
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", slider.value); //Slider'daki son de�erin kaydedilmesi.
    }
}
