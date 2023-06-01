using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.IO;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    //Database konumunun bulunmasý.
    private string GetDatabasePath(string databaseName) {
        string destinationPath = "";
#if UNITY_EDITOR || UNITY_STANDALONE
        destinationPath = Path.Combine(Application.streamingAssetsPath, databaseName);
#elif UNITY_ANDROID
            destinationPath = Path.Combine(Application.persistentDataPath, databaseName);
            if (!File.Exists(destinationPath))
            {
                using (WWW www = new WWW("jar:file://" + Application.dataPath + "!/assets/" + databaseName))
                {
                    while (!www.isDone) { }
                    File.WriteAllBytes(destinationPath, www.bytes);
                }
            }
#endif
        return destinationPath;
    }

    //Oyun sonunda alýnan skorlarýn sýralamasýnýn yapýlmasý.(Insertion Sort kullanýlarak.)
    private void InsertionSort(List<int> all_score) {
        int n = all_score.Count;

        //Alýnan skorlarýn kýyaslama yaparak kaydediilmesi.
        for (int i = 1; i < n; i++) {
            int key = all_score[i];
            int j = i - 1;

            while (j >= 0 && all_score[j] < key) {
                all_score[j + 1] = all_score[j];
                j--;
            }

            all_score[j + 1] = key;
        }
    }
    //Skorun databasaye kaydedilmesi.
    public void AddingScores(int cur_score) {
        string connectionString = "URI=file:" + GetDatabasePath("Scoreboard.sqlite"); //Database ile baðlantý kurulmasý.
        int len;

        List<int> all_score = new List<int>(); //Bütün skorlarýn bulunduðu listenin oluþturulmasý.
        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();
            using (SqliteCommand cmd = new SqliteCommand("SELECT score FROM Scores WHERE id BETWEEN 2 AND 11", connection)) { //Bütün skorlar listesine databasede bulunan skorlarýn eklenmesi.
                using (SqliteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        all_score.Add(Convert.ToInt16(reader[0]));
                    }
                }
            }
        }
        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();
            using (SqliteCommand kmt = new SqliteCommand("UPDATE Scores Set score = @p1 where id = 1", connection)) { //En son oyunda elde edilen skorun databasede 1. sýraya eklenmesi.
                kmt.Parameters.AddWithValue("@p1", cur_score);
                kmt.ExecuteNonQuery();
            }
        }

        all_score.Add((int)cur_score);
        InsertionSort(all_score);
        len = all_score.Count;
        all_score.Remove(all_score[len - 1]);
        for (int i = 2; i <= 11; i++) {
            using (SqliteConnection connection = new SqliteConnection(connectionString)) {
                connection.Open();
                using (SqliteCommand kmt = new SqliteCommand("UPDATE Scores Set score = @p1 where id = @p2", connection)) {
                    kmt.Parameters.AddWithValue("@p1", all_score[i - 2]);
                    kmt.Parameters.AddWithValue("@p2", i);
                    kmt.ExecuteNonQuery();
                }
            }
        }
    }
}
