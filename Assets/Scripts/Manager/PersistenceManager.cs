using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using System.Linq;

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

    //private List<ISaveable> saveableObjects;

    // AES Key and IV (Initialization Vector) for encryption
    private byte[] key;
    private byte[] iv;

    // Dynamic object list
    [System.Serializable]
    public struct PrefabEntry
    {
        public string key;
        public GameObject prefab;
    }

    [SerializeField] private List<PrefabEntry> prefabRegistryList; // Fill in inspector
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

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
            //saveableObjects = new List<ISaveable>();

            // encryption key and iv
            key = System.Text.Encoding.UTF8.GetBytes(encryptionKey.PadRight(16).Substring(0, 16));
            iv = System.Text.Encoding.UTF8.GetBytes(encryptionKey.PadRight(16).Substring(0, 16));

            // Initialize dynamic prefabs dictionary
            foreach (PrefabEntry entry in prefabRegistryList)
            {
                if (!prefabDictionary.ContainsKey(entry.key))
                {
                    prefabDictionary.Add(entry.key, entry.prefab);
                }
            }
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

    //public void RegisterSaveable(ISaveable saveable)
    //{
    //    if (!saveableObjects.Contains(saveable))
    //    {
    //        saveableObjects.Add(saveable);
    //    }
    //}

    //public void UnregisterSaveable(ISaveable saveable)
    //{
    //    if (saveableObjects != null && saveableObjects.Contains(saveable))
    //    {
    //        saveableObjects.Remove(saveable);
    //    }
    //}

    public void SaveRequest(SaveSlot saveSlot = SaveSlot.Slot1)
    {
        Save(saveSlot);
    }

    private void Save(SaveSlot saveSlot)
    {
        SaveData data = new SaveData();

        SaveableEntity[] entities = FindObjectsByType<SaveableEntity>(FindObjectsSortMode.InstanceID);

        foreach (SaveableEntity entity in entities)
        {
            ISaveable saveable = entity.GetComponent<ISaveable>();
            string jsonState = JsonUtility.ToJson(saveable.Save());

            // Write data class
            if (entity.IsDynamic)
            {
                DynamicObjectData dynamicData = new DynamicObjectData
                {
                    id = entity.ID,
                    prefabKey = entity.PrefabKey,
                    jsonState = jsonState,
                    position = entity.transform.position,
                    rotation = entity.transform.rotation
                };
                data.dynamicObjects.Add(dynamicData);
            }
            else
            {
                // Save as Scene Object
                data.Add(entity.ID, jsonState);
            }
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

    public void LoadRequest(SaveSlot saveSlot = SaveSlot.Slot1)
    {
        Load(saveSlot);
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

        Dictionary<string, SaveableEntity> currentEntities = FindObjectsByType<SaveableEntity>(FindObjectsSortMode.InstanceID).ToDictionary(e => e.ID);

        for (int i = 0; i < data.keys.Count; i++)
        {
            string id = data.keys[i];
            if (currentEntities.TryGetValue(id, out SaveableEntity entity))
            {
                entity.GetComponent<ISaveable>().Load(data.values[i]);
            }
        }

        // Destroy all dynamic objects before loading
        foreach (SaveableEntity entity in currentEntities.Values)
        {
            if (entity.IsDynamic) Destroy(entity.gameObject);
        }

        foreach (DynamicObjectData dData in data.dynamicObjects)
        {
            if (prefabDictionary.TryGetValue(dData.prefabKey, out GameObject prefab))
            {
                // Instantiate
                GameObject newObj = Instantiate(prefab, dData.position, dData.rotation);

                // Restore ID so future saves track it correctly
                SaveableEntity entityScript = newObj.GetComponent<SaveableEntity>();
                entityScript.SetId(dData.id); 

                // Load State
                newObj.GetComponent<ISaveable>().Load(dData.jsonState);
            }
            else
            {
                Debug.LogWarning($"Could not find prefab for key: {dData.prefabKey}");
            }
        }

        Debug.Log($"Loaded file {saveSlot} from path: {path}");
    }

    public void Delete(SaveSlot saveSlot)
    {
        string path = GetPath(saveSlot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save file at path: {path}");
        }
        else
        {
            Debug.Log($"No save file found at path: {path} to delete.");
        }
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
