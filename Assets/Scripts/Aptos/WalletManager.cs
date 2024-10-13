using System.Collections.Generic;
using Aptos;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Aptos.Exceptions;
using System.Collections;
using UnityEngine.Networking;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance;

    private AptosClient client;
    public Ed25519Account account;
    
    private const string MODULE_ADDRESS = "0xd36ee9d2883da4b1eb018b1f6d7eab57588e5e273c49c919927a8c54b4c647b9"; // Replace with your actual module address
    private const string MODULE_NAME = "gameplaymanager";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        client = new AptosClient(Networks.Devnet);
    }

    void Start()
    {
        // GenerateAccount();
    }

    public void GenerateAccount()
    {
        var serializedPrivateKey = PlayerPrefs.GetString("serialized-ed25519-private-key");
        if (serializedPrivateKey != "")
        {
            account = new Ed25519Account(Ed25519PrivateKey.Deserialize(new(serializedPrivateKey)));
        }
        else
        {
            account = Ed25519Account.Generate();
            PlayerPrefs.SetString(
                "serialized-ed25519-private-key",
                account.PrivateKey.BcsToHex().ToString()
            );
        }
    }

    public string GetAddress()
    {
        return account.Address.ToString();
    }

    public async Task UpdateBalance()
    {
        var balance = await client.Account.GetCoinBalance(account.Address);
        Debug.Log($"Balance: {balance.Amount / 100000000m} APT");
    }

    public async Task FundAccount()
    {
        StartCoroutine(RequestAptosTokens(account.Address.ToString(), 10000000));  
    }

   private const string DEVNET_FAUCET_URL = "https://faucet.devnet.aptoslabs.com/mint";

    [Obsolete]
    public IEnumerator RequestAptosTokens(string address, uint amount)
    {
        string url = $"{DEVNET_FAUCET_URL}?amount={amount}&address={address}";

        using (UnityWebRequest www = UnityWebRequest.Post(url, ""))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log("Faucet request successful!");
                Debug.Log($"Response: {www.downloadHandler.text}");

                // The response is expected to be a JSON array of transaction hashes
                // You may want to parse this depending on your needs
            }
        }
    }


public async Task<bool> IsInitialized()
{
    try
    {
        var resource = await client.Account.GetResource(account.Address, $"{MODULE_ADDRESS}::{MODULE_NAME}::GameState");
        return resource != null;
    }
    catch (ApiException ex)
    {
        // If the resource doesn't exist, an ApiException will be thrown
        if (ex.Message.Contains("Resource not found"))
        {
            return false;
        }
        // If it's a different error, rethrow it
        throw;
    }
}

    public async Task InitializeGame()
    {
        await ExecuteTransaction("initialize_game", new List<object>());
    }

    public async Task StartGame()
    {
        await ExecuteTransaction("start_game", new List<object>());
    }

    public async Task UpdateScore(ulong newScore)
    {
        await ExecuteTransaction("update_score", new List<object> { newScore });
    }

    public async Task CollectStar()
    {
        await ExecuteTransaction("collect_star", new List<object>());
    }

    public async Task EndGame()
    {
        await ExecuteTransaction("end_game", new List<object>());
    }

    public async Task MakePurchase(ulong itemCost)
    {
        await ExecuteTransaction("make_purchase", new List<object> { itemCost });
    }

   private async Task ExecuteTransaction(string function, List<object> args)
{
    var transaction = await client.Transaction.Build(
        sender: account.Address,
        data: new GenerateEntryFunctionPayloadData(
            function: $"{MODULE_ADDRESS}::{MODULE_NAME}::{function}",
            typeArguments: new List<object>(),
            functionArguments: args
        )
    );

    var pendingTransaction = await client.Transaction.SignAndSubmitTransaction(account, transaction);
    await client.Transaction.WaitForTransaction(pendingTransaction.Hash);
    Debug.Log($"Transaction {function} completed. Hash: {pendingTransaction.Hash}");
}

public async Task<ulong> GetLatestSequenceNumber()
{
    try
    {
        var accountResource = await client.Account.GetResource(account.Address, "0x1::account::Account");
        
        if (accountResource.Data is Dictionary<string, object> data)
        {
            if (data.TryGetValue("sequence_number", out object sequenceObj))
            {
                if (sequenceObj is string sequenceStr)
                {
                    if (ulong.TryParse(sequenceStr, out ulong sequenceNumber))
                    {
                        return sequenceNumber;
                    }
                }
            }
        }
        
        throw new Exception("Unable to parse sequence number from account resource.");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error getting latest sequence number: {ex.Message}");
        throw;
    }
}

    public async Task<ulong> GetScore()
    {
        return await ViewFunction("get_score");
    }

    public async Task<ulong> GetStarScore()
    {
        return await ViewFunction("get_star_score");
    }

    public async Task<ulong> GetBestScore()
    {
        return await ViewFunction("get_best_score");
    }

    private async Task<ulong> ViewFunction(string function)
    {
        var result = await client.View(
            data: new GenerateViewFunctionPayloadData(
            $"{MODULE_ADDRESS}::{MODULE_NAME}::{function}",
            new List<object>(),
            new List<object> { account.Address })

        );

        if (result.Count > 0 && result[0] is ulong value)
        {
            return value;
        }

        Debug.LogError($"Failed to get {function}");
        return 0;
    }
}