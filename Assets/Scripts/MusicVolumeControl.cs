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
        //sliderin deðerini müziðin baþlangýç sesinin deðerine ata
        slider.value = audioSource.volume;
        slider.value = PlayerPrefs.GetFloat("MusicVolume", 0); //Oyun açýldýðýnda slider deðerinin oyun kapatýldýðý zamanki deðerinden devam etmesinin saðlanmasý.
        //sliderin deðiþimine baðlý  olarak sesi güncelle
        slider.onValueChanged.AddListener(ChangeVolume);
    }

    
    private void ChangeVolume(float volume)
    {
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", slider.value); //Slider'daki son deðerin kaydedilmesi.
    }
}
