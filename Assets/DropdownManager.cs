using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DropdownManager : MonoBehaviour
{
    public Dropdown dropdown;

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
        DropdownItems();   
    }

    public void DropdownItems() {
        List<string> dropdownOptions = new List<string>();
        string connectionString = "URI=file:" + GetDatabasePath("WordDatabase.sqlite"); //Veritabaný ile baðlantý kurulmasý.
        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();
            using (SqliteCommand command = new SqliteCommand("SELECT type FROM Type", connection)) { //Dropdown menüsünde veritabanýndaki türler tarafýndan ekleme yapýlmasý.
                using (SqliteDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        dropdownOptions.Add(reader[0].ToString());
                    }
                    dropdown.AddOptions(dropdownOptions); //Dropdown menüsüne veritabanýndan alýnan seçeneklerin eklenmesi.
                }
            }
        }
    }
}
