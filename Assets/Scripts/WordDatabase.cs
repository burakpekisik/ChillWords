using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using System.Security.Cryptography;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;
using static Unity.Burst.Intrinsics.X86.Avx;

public class WordDatabase : MonoBehaviour {
    public InputField inputWord;
    public InputField inputCategoryName;
    public Dropdown dropdown;
    public Text categoryText;
    public List<string> words;
    private int groupCount;
    public int wordCountCollide = 2;
    public int wordCountAlone = 1;
    public List<string> commonWords;
    public List<string> commonWordsString;
    public List<string> aloneWords;
    public List<int> commonWordsScores;
    public List<int> aloneWordsScores;
    

    //Veritabaný konumunun bulunmasý.
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

    //Ortak harf bulmayý saðlayan fonksiyonun tanýmlanmasý.
    private bool HasCommonLetter(List<string> stringList) {
        if (stringList.Count == 0)
            return false;

        HashSet<char> commonLetters = new HashSet<char>(stringList[0]);

        for (int i = 1; i < stringList.Count; i++) {
            commonLetters.IntersectWith(stringList[i]);
        }

        return commonLetters.Count > 0;
    }

    //Admin menüsünde yükleme yapýlacak kelime için kategori seçiminin saðlanmasý.
    //private void Start() {
    //    if (dropdown != null) {
    //        DropdownItems();
    //    }
    //}

    //Veritabanýna kelime yüklenmesinin saðlanmasý.
    private int inserting_into() {
        string connectionString = "URI=file:" + GetDatabasePath("WordDatabase.sqlite"); //Veritabaný ile baðlantý kurulmasý.
        string req = (inputWord.text).ToUpper(); //Kelimenin input tablosundan alýnmasý.
        string type = dropdown.options[dropdown.value].text; //Türün dropdown menüsünden alýnmasý.
        int req_len = req.Length;

        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();
            using (SqliteCommand cmd = new SqliteCommand("SELECT id FROM Type WHERE Type = @type", connection)) { //Seçilen türe göre type_id belirlenmesi.
                cmd.Parameters.AddWithValue("@type", type);
                using (SqliteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        type = reader[0].ToString();
                    }
                }
            }

            using (SqliteCommand kmt = new SqliteCommand("INSERT INTO Names (name, score, type_id) VALUES (@p1, @p2, @p3)", connection)) { //Ýsim, kelimenin skoru ve türünün numarasýnýn veritabanýna aktarýlmasý.
                kmt.Parameters.AddWithValue("@p1", req);
                kmt.Parameters.AddWithValue("@p2", req.Length * 2);
                kmt.Parameters.AddWithValue("@p3", type);
                kmt.ExecuteNonQuery();
            }
        }
        return req_len;
    }

    //Kategori eklenmesinin saðlanmasý.
    public void InsertCategory() {
        string connectionString = "URI=file:" + GetDatabasePath("WordDatabase.sqlite"); //Veritabaný ile baðlantý kurulmasý.
        string categoryName = (inputCategoryName.text).ToUpper(); //Kategori adýnýn input tablosundan alýnmasý.
        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();
            using (SqliteCommand cmd = new SqliteCommand("INSERT INTO Type (type) VALUES (@categoryName)", connection)) { //Kategorinin, Type tablosuna eklenmesi.
                cmd.Parameters.AddWithValue("@categoryName", categoryName);
                cmd.ExecuteNonQuery();
            }
        }
        dropdown.ClearOptions();
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

    private string reading_id() {
        string req = (inputWord.text).ToUpper(); //Ýnput tablosundan kelimenin alýnmasý.
        string count = "";
        string connectionString = "URI=file:" + GetDatabasePath("WordDatabase.sqlite"); //Veritabaný ile baðlantýnýn kurulmasý.

        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();
            using (SqliteCommand komut = new SqliteCommand("SELECT name_id FROM Names WHERE name IN (@req)", connection)) { //Elde bulunan kelime kullanýlarak kelimenin id'sinin alýnmasý.
                komut.Parameters.AddWithValue("@req", req);
                using (SqliteDataReader r = komut.ExecuteReader()) {
                    r.Read();
                    count = r[0].ToString();
                }
            }
        }
        return count;
    }

    private string is_it_exist(int sayac) { //Kelimenin ortak harflerine göre gruplandýrýlmasýnda var olup olmadýðýnýn kontrolü.
        string deg = "";
        string connectionString = "URI=file:" + GetDatabasePath("WordDatabase.sqlite");

        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();
            using (SqliteCommand deneme = new SqliteCommand("SELECT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='Grup_" + sayac + "')", connection)) {
                using (SqliteDataReader m = deneme.ExecuteReader()) {
                    while (m.Read()) {
                        deg = m[0].ToString();
                    }
                }
            }
        }
        return deg;
    }

    public void OtherGroups() { //Kelimelerin ortak harflerine göre gruplara yerleþtirilmesi.
        words = new List<string>();
        string connectionString = "URI=file:" + GetDatabasePath("WordDatabase.sqlite");
        SqliteConnection bag = new SqliteConnection(connectionString);
        string req = (inputWord.text).ToUpper();
        string quote;
        int sayac = 1;
        bool letter;
        int req_len;
        string deg = "";
        string count = "";

        while (3 > 2) {
            req_len = inserting_into();
            count = reading_id();

            while (3 > 2) {
                deg = is_it_exist(sayac);
                if (deg == "1") {
                    bag.Open();
                    quote = "Select name from Names inner join Grup_" + sayac + " on Names.name_id = Grup_" + sayac + ".name_id";
                    SqliteCommand cmd = new SqliteCommand(quote, bag);
                    SqliteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read()) {
                        words.Add(reader["name"].ToString());
                    }
                    bag.Close();
                    words.Add(req);
                    letter = HasCommonLetter(words);
                    if (letter) {
                        bag.Open();
                        SqliteCommand kmt2 = new SqliteCommand("insert into Grup_" + sayac + "(name_id) values (@p2)", bag);
                        kmt2.Parameters.AddWithValue("@p2", count);
                        kmt2.ExecuteNonQuery();
                        bag.Close();
                        words.Clear();
                        sayac = 1;
                        break;
                    }

                    else {
                        sayac++;
                        words.Clear();
                    }

                }

                else {
                    bag.Open();
                    string tableName = "Grup_" + sayac;
                    string isTableCreated = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
                    using (SqliteCommand command = new SqliteCommand(isTableCreated, bag)) {
                        using (SqliteDataReader reader = command.ExecuteReader()) {
                            if (reader.Read() == false) {
                                SqliteCommand comm = new SqliteCommand("CREATE TABLE Grup_" + sayac + " (name_id INTEGER,FOREIGN KEY(name_id) REFERENCES Names(name_id));", bag);
                                comm.ExecuteNonQuery();
                                //groupCount++;
                            }
                        }
                    }
                    SqliteCommand kmt = new SqliteCommand("insert into Grup_" + sayac + " (name_id) values (@p1)", bag);
                    kmt.Parameters.AddWithValue("@p1", count);
                    kmt.ExecuteNonQuery();
                    bag.Close();
                    sayac = 1;
                    break;
                }
            }
            break;
        }
    }

    public void RandomWords() {
        commonWords.Clear();
        aloneWords.Clear();
        commonWords = new List<string>();
        commonWordsString = new List<string>();
        commonWordsScores = new List<int>();
        aloneWords = new List<string>();
        aloneWordsScores = new List<int>();

        List<string> categories = new List<string>();
        List<string> aloneWordsCategories = new List<string>();
        SqliteCommand cmd;

        //string filePath = "Data Source=" + Path.Combine(Application.streamingAssetsPath, "WordDatabase.sqlite") + ";Version=3;";
        string connectionString = "URI=file:" + GetDatabasePath("WordDatabase.sqlite");
        using (SqliteConnection connection = new SqliteConnection(connectionString)) {
            connection.Open();

            // Seçim yapýlabilecek Grup_ larýn belirlenmesi..
            string rootTableName = "Grup_";
            string rootNames = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name LIKE @prefix";

            using (SqliteCommand command = new SqliteCommand(rootNames, connection)) {
                command.Parameters.AddWithValue("@prefix", rootTableName + "%");
                groupCount = Convert.ToInt32(command.ExecuteScalar());
            }

            // Rastgele grubun belirlenmesi.
            int randomGroup = UnityEngine.Random.Range(1, groupCount + 1);
            string tableName = "Grup_" + randomGroup;
            string columnName = "name_id";
            string elemanSayisiString = $"SELECT COUNT({columnName}) FROM {tableName}";
            using (cmd = new SqliteCommand(elemanSayisiString, connection)) {
                int elemanSayisi = Convert.ToInt32(cmd.ExecuteScalar());
                // Grup içerisindeki eleman sayýsý 2'den az ise yeniden seçim yapýlmasý.
                if (elemanSayisi <= 2) {
                    RandomWords();
                    return;
                }
            }

            // Kelimelerin name id'lerinin belirlenmesi.
            string selectedWords = $"SELECT {columnName} FROM {tableName} ORDER BY RANDOM() LIMIT " + wordCountCollide;
            using (cmd = new SqliteCommand(selectedWords, connection)) {
                using (SqliteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        commonWords.Add(reader[0].ToString());
                    }
                }
            }

            // Kelimelerin ayný kategoriden olup olmadýklarýnýn kontrolü.
            foreach (string rowNumber in commonWords) {
                int rowNumberInt = Convert.ToInt32(rowNumber);
                string checkCategory = $"SELECT * FROM Names LIMIT 1 OFFSET {rowNumberInt - 1}";
                using (cmd = new SqliteCommand(checkCategory, connection)) {
                    using (SqliteDataReader reader = cmd.ExecuteReader()) {
                        if (reader.Read()) {
                            categories.Add(reader[3].ToString());
                        }
                    }
                }
            }
            // Kelimeler ayný kategoriden deðilse fonksiyon tekrardan baþlatýlýr.
            for (int i = 1; i < categories.Count; i++) {
                if (categories[i - 1] != categories[i]) {
                    RandomWords();
                    return;
                }
            }

            // Oluþan kelimelerin kategori adlarýnýn ekrana yazdýrýlmasý.
            string writeCategory = $"SELECT type FROM Type WHERE ID = {categories[0]}";
            using (cmd = new SqliteCommand(writeCategory, connection)) {
                using (SqliteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        categoryText.text = "Kategori Adý: " + reader[0].ToString();
                    }
                }
            }

            // Name ID'lerin stringe dönüþtürülmesi.
            foreach (string rowNumber in commonWords) {
                int rowNumberInt = Convert.ToInt32(rowNumber);
                string idToName = $"SELECT * FROM Names LIMIT 1 OFFSET {rowNumberInt - 1}";
                using (cmd = new SqliteCommand(idToName, connection)) {
                    using (SqliteDataReader reader = cmd.ExecuteReader()) {
                        if (reader.Read()) {
                            commonWordsString.Add(reader[1].ToString());
                        }
                    }
                }
            }

            // Ortak harfi olan kelimelerin scorelerinin alýnmasý.
            foreach (string rowNumber in commonWords) {
                int rowNumberInt = Convert.ToInt32(rowNumber);
                string commonScores = $"SELECT * FROM Names LIMIT 1 OFFSET {rowNumberInt - 1}";
                using (cmd = new SqliteCommand(commonScores, connection)) {
                    using (SqliteDataReader reader = cmd.ExecuteReader()) {
                        if (reader.Read()) {
                            commonWordsScores.Add(Convert.ToInt32(reader[2]));
                        }
                    }
                }
            }
            
            // Kendi baþýna takýlacak kelimelerin belirlenmesi.
            string straightLine = $"SELECT * FROM Names ORDER BY RANDOM() LIMIT " + wordCountAlone;
            using (cmd = new SqliteCommand(straightLine, connection)) {
                using (SqliteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        aloneWords.Add(reader[1].ToString());
                        aloneWordsCategories.Add(reader[3].ToString());
                        aloneWordsScores.Add(Convert.ToInt32(reader[2]));
                    }
                }
                for (int i = 0; i < commonWordsString.Count; i++) {
                    for (int j = 0; j < aloneWords.Count; j++) {
                        if (commonWordsString[i] == aloneWords[j] || categories[i] != aloneWordsCategories[j]) {
                            RandomWords();
                            return;
                        }
                    }
                }
                
            }
        }
        foreach (string word in aloneWords) {
            Debug.Log("Selected Word Alone: " + word);
        }

        foreach (string word in commonWordsString) {
            Debug.Log("Common Words String: " + word);
        }
    }
}
