using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class WalletUI : MonoBehaviour {
    public Button GenerateButton;
    public Button CloseModalButton;
    public Button OpenModalButton;

    public Button GetAirdrop;
    public GameObject WalletModal;


    public TMP_Text AddressText;
    public TMP_Text privateKey; 
    

    public TMP_Text BalanceApt;
    void Start() {
        GenerateButton.onClick.AddListener(() => {
            WalletManager.Instance.GenerateAccount();
            privateKey.text = PlayerPrefs.GetString("serialized-ed25519-private-key");
                        AddressText.text = WalletManager.Instance.GetAddress();

        });

        CloseModalButton.onClick.AddListener(() => {
            WalletModal.SetActive(false);
        });

        OpenModalButton.onClick.AddListener(() => {
            WalletModal.SetActive(true);
        });

        GetAirdrop.onClick.AddListener(() => {
            WalletManager.Instance.FundAccount();
            WalletManager.Instance.UpdateBalance();
        });


        if(!PlayerPrefs.HasKey("serialized-ed25519-private-key")){
            WalletModal.SetActive(true);
            AddressText.text = WalletManager.Instance.GetAddress();
        }
        else {
            WalletModal.SetActive(false);
        }

    }
}