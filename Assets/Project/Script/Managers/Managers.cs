using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{

    static Managers _instance;
    PoolManager _pool = new PoolManager();

    public static Managers Instance { get { Init(); return _instance; } }
    public static PoolManager Pool { get { return Instance._pool; } }
   
    static void Init()
    {
        if (_instance == null) {
            GameObject go = GameObject.Find("Managers");
            if (go == null)
            {
                go = new GameObject { name = "Managers" };
                go.AddComponent<Managers>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
        }
    }
}
