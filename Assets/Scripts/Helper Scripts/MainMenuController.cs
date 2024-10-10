using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour {

	public GameObject hero_Menu;
	public Text starScoreText;

	public Image music_Img;
	public Sprite music_Off, music_On;
	public string ModuleAddress = "0xd36ee9d2883da4b1eb018b1f6d7eab57588e5e273c49c919927a8c54b4c647b9";

	
	public async void PlayGame() {
    bool isInitialized = await WalletManager.Instance.IsInitialized();

	if(!isInitialized){
		await WalletManager.Instance.InitializeGame();
	}
		await WalletManager.Instance.StartGame();

		SceneManager.LoadScene ("Gameplay");

			
	}

	public void HeroMenu() {
		hero_Menu.SetActive (true);
		starScoreText.text = "" + GameManager.instance.starScore;
	}

	public void HomeButton() {
		hero_Menu.SetActive (false);
	}

	public void MusicButton() {
		if (GameManager.instance.playSound) {
			music_Img.sprite = music_Off;
			GameManager.instance.playSound = false;
		} else {
			music_Img.sprite = music_On;
			GameManager.instance.playSound = true;
		}
	}

} // class


































