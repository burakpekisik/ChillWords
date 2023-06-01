using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.IO;
using TMPro;

public class MainMenuScores : MonoBehaviour
{
    public TextMeshProUGUI lastText; //En son oynanan oyunun skorunun text bile�eninin tan�mlanmas�.
    public TextMeshProUGUI bestText; //En iyi skorun text bile�eninin tan�mlanmas�.
    private string maxScore; //Maksimum skor stringi.
    private string lastGameScore; //Son oyun skorunun stringi.

    //Database dosyas�n�n konumunun belirlenmesi.
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

    // Start is called before the first frame update
    void Start()
    {
        GetScores();
    }

    //Skorlar�n elde edilmesi ve yazd�r�lmas�.
    private void GetScores() {
        string connectionString = "URI=file:" + GetDatabasePath("Scoreboard.sqlite"); //Scoreboard dosyas�na eri�im
        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();
            using (SqliteCommand cmd = new SqliteCommand("SELECT score FROM Scores WHERE id = 1", connection)) { //En son oynanan oyunun skorunun al�nmas�.
                SqliteDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    lastGameScore = reader[0].ToString();
                }
            }
            using (SqliteCommand cmd = new SqliteCommand("SELECT score FROM Scores WHERE id = 2", connection)) { //Maksimum skorun database �zerinden al�nmas�.
                SqliteDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    maxScore = reader[0].ToString();
                }
            }
        }
        lastText.text = $"En Son: {lastGameScore}"; //En son oynanan skorunun yazd�r�lmas�.
        bestText.text = $"En iyi: {maxScore}"; //Maksimum oyunun skorunun yazd�r�lmas�.
    }
}
