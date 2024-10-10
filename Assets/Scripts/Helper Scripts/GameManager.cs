using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour {

	public static GameManager instance;

	private GameData gameData;

	[HideInInspector]
	public int starScore, score_Count, selected_Index;

	[HideInInspector]
	public bool[] heroes;

	[HideInInspector]
	public bool playSound = true;

	private string data_Path = "GameData.dat";

	void Awake () {
		MakeSingleton ();

		InitializeGameData ();
	}

	void Start() {
//		print (Application.persistentDataPath + data_Path);
	}
	
	void MakeSingleton() {
		if (instance != null) {
			Destroy (gameObject);
		} else if(instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);
		}
	}

	async void  InitializeGameData() {
		// WalletManager.Instance.GenerateAccount();
		// await LoadGameDataFromBlockchain ();

		if (gameData == null) {
			// we are running our game for the first time
			// set up initial values
//			starScore = 0;

			// FOR TESTING ONLY REMOVE FOR PRODUCTION
			starScore = 9000;

			score_Count = 0;
			selected_Index = 0;

			heroes = new bool[9];
			heroes [0] = true;

			for (int i = 1; i < heroes.Length; i++) {
				heroes [i] = false;
			}

			gameData = new GameData ();

			gameData.StarScore = starScore;
			gameData.ScoreCount = score_Count;
			gameData.Heroes = heroes;
			gameData.SelectedIndex = selected_Index;

			SaveGameData ();

		}

	}

	public void SaveGameData() {
		FileStream file = null;

		try {

			BinaryFormatter bf = new BinaryFormatter();

			file = File.Create(Application.persistentDataPath + data_Path);

			if(gameData != null) {

				gameData.Heroes = heroes;
				gameData.StarScore = starScore;
				gameData.ScoreCount = score_Count;
				gameData.SelectedIndex = selected_Index;

				bf.Serialize(file, gameData);
			}

		} catch(Exception e) {
			
		} finally {
			if (file != null) {
				file.Close ();
			}
		}

	}


	 async Task LoadGameDataFromBlockchain() {
        try {
            ulong blockchainScore = await WalletManager.Instance.GetScore();
            ulong blockchainStarScore = await WalletManager.Instance.GetStarScore();
            ulong blockchainBestScore = await WalletManager.Instance.GetBestScore();

            score_Count = (int)blockchainScore;
            starScore = (int)blockchainStarScore;
            // Update other fields as necessary

            gameData = new GameData();
            gameData.StarScore = starScore;
            gameData.ScoreCount = score_Count;
            // Set other fields in gameData

            LoadGameData(); // Load local data as a backup or for additional info not stored on blockchain

            // Merge blockchain and local data if necessary
            // For example, take the higher score between blockchain and local storage
           

        } catch (Exception e) {
            Debug.LogError($"Failed to load game data from blockchain: {e.Message}");
            LoadGameData(); // Fallback to local data if blockchain load fails
        }
    }


	void LoadGameData() {

		FileStream file = null;

		try {

			BinaryFormatter bf = new BinaryFormatter();

			file = File.Open(Application.persistentDataPath + data_Path, FileMode.Open);

			gameData = (GameData)bf.Deserialize(file);

			if(gameData != null) {
				starScore = gameData.StarScore;
				score_Count = gameData.ScoreCount;
				heroes = gameData.Heroes;
				selected_Index = gameData.SelectedIndex;
			}

		} catch(Exception e) {
			
		} finally {
			if (file != null) {
				file.Close ();
			}
		}
	}

} // class




































