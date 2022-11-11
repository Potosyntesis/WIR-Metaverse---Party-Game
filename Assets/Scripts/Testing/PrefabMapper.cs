using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabMapper : MonoBehaviour
{
    public static PrefabMapper Instance;
    public List<PrefabStruct> prefabs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public GameObject GetPrefabClientWithId(string prefabId)
    {
        int i = 0;
        int size = prefabs.Count;
        for (; i < size; i++)
        {
            if (prefabs[i].id == prefabId)
            {
                return prefabs[i].prefabClient;
            }
        }
        return null;
    }

    public GameObject GetPrefabServerWithId(string prefabId)
    {
        int i = 0;
        int size = prefabs.Count;
        for (; i < size; i++)
        {
            if (prefabs[i].id == prefabId)
            {
                return prefabs[i].prefabServer;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        Debug.Log("Destroy prefabmapper");
    }
}
[Serializable]
public struct PrefabStruct
{
    public string id;
    public GameObject prefabClient;
    public GameObject prefabServer;
}
