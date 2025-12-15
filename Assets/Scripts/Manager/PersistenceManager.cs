using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SaveSlot
{
    Slot1,
    Slot2,
    Slot3
}

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance;

    [Header("Save data settings")]
    [SerializeField] private string[] saveNames;
    [SerializeField] private bool useEncryption;
    [SerializeField] private string encryptionKey;

    private List<ISaveable> saveableObjects;

    // AES Key and IV (Initialization Vector) for encryption
    private byte[] key = new byte[16];
    private byte[] iv;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);

            // register saveable objects list
            saveableObjects = new List<ISaveable>();

            // encryption key and iv
            key = System.Text.Encoding.UTF8.GetBytes(encryptionKey.PadRight(16).Substring(0, 16));
            iv = System.Text.Encoding.UTF8.GetBytes(encryptionKey.PadRight(16).Substring(0, 16));
        }
    }

    private void Start()
    {
        ActionManager.Instance.onSaveRequested += Save;
        ActionManager.Instance.onLoadRequested += Load;
        Debug.Log($"PersistenceManager started. Persistent data path: {Application.persistentDataPath}");
    }

    private string GetPath(SaveSlot saveSlot)
    {
        return Path.Combine(Application.persistentDataPath, saveNames[(int)saveSlot]);
    }

    public void RegisterSaveable(ISaveable saveable)
    {
        if (!saveableObjects.Contains(saveable))
        {
            saveableObjects.Add(saveable);
        }
    }

    public void UnregisterSaveable(ISaveable saveable)
    {
        if (saveableObjects != null && saveableObjects.Contains(saveable))
        {
            saveableObjects.Remove(saveable);
        }
    }

    private void Save(SaveSlot saveSlot)
    {
        SaveData data = new SaveData();

        // Write data class
        foreach (ISaveable saveable in saveableObjects)
        {
            object state = saveable.Save();
            string jsonState = JsonUtility.ToJson(state);

            data.Add(saveable.ID, jsonState);
        }

        // Save to disk
        string jsonPayload = JsonUtility.ToJson(data);
        string path = GetPath(saveSlot);

        if (useEncryption)
        {
            byte[] encryptedData = Encrypt(jsonPayload);
            File.WriteAllBytes(path, encryptedData);
        }
        else
        {
            File.WriteAllText(path, jsonPayload);
        }
        Debug.Log($"Saved file {saveSlot} at path: {path}");

    }

    private void Load(SaveSlot saveSlot)
    {
        string path = GetPath(saveSlot);
        if (!File.Exists(path))
        {
            Debug.Log($"Save file at path: {path} not found!");
            return;
        }

        // load into Data class
        string jsonPayload = "";
        if (useEncryption)
        {
            byte[] encryptedData = File.ReadAllBytes(path);
            jsonPayload = Decrypt(encryptedData);
        }
        else
        {
            jsonPayload = File.ReadAllText(path);
        }
        SaveData data = JsonUtility.FromJson<SaveData>(jsonPayload);

        // Restore each object's state
        foreach (ISaveable saveable in saveableObjects)
        {
            if (data.TryGetValue(saveable.ID, out string jsonState))
            {
                saveable.Load(jsonState);
            }
        }

        Debug.Log($"Loaded file {saveSlot} from path: {path}");
    }

    private byte[] Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return ms.ToArray();
                }
            }
        }
    }

    private string Decrypt(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
