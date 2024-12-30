using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager
{
    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
    Transform _root;
    public Transform root { get { Init(); return _root; } }


    public void Init()
    {
        if(_root == null)
        {
            GameObject go = GameObject.Find("Pool");
            if(go == null)
            {
                go = new GameObject { name = "Pool" };
            }
            GameObject.DontDestroyOnLoad(go);
            _root = go.transform;
        }
    }

    //poolable 여부에 따라 캐싱된 프리팹을 가져오거나 새로 생성함
    public GameObject Instantiate(GameObject prefab,bool poolable = false)
    {
        if(prefab == null) { Debug.Log("prefab's dosen't exist"); return null; }
        if(poolable == false) { return GameObject.Instantiate(prefab); }

        //풀이 있으면 프리팹을 꺼내쓰고 없다면 생성과 동시에 꺼내줌
        if (_pool.ContainsKey(prefab.name)) { return _pool[prefab.name].PopPrefabs(prefab); }
        Pool pool = new Pool();
        pool.Init(prefab, root.transform);
        _pool.Add(prefab.name, pool);

        return _pool[prefab.name].PopPrefabs(prefab);
    }


    public void Destroy(GameObject gameObject,float DestroyTime = 0f)
    {
        if (_pool.ContainsKey(gameObject.name))
        {
            _pool[gameObject.name].PushPrefabs(gameObject);
            return;
        }

        GameObject.Destroy(gameObject,DestroyTime);
    }

    public void ClearPool(GameObject origin)
    {
        _pool[origin.name].ClearPool();
        _pool.Remove(origin.name);
    }
}

public class Pool
{
    public GameObject _origin;
    public Transform _root;
    public Stack<GameObject> prefabs = new Stack<GameObject>();

    //풀생성
    public void Init(GameObject origin, Transform parent)
    {
        _origin = origin;
        _origin.name = origin.name;
        GameObject folder = new GameObject { name = origin.name + "Folder" };
        folder.transform.parent = parent;
        _root = folder.transform;

        CreatePrefabs();
    }

    //프리팹 추가생성
    public void CreatePrefabs()
    {
        for(int i = 0; i < 5; i++)
        {
            GameObject prefab = GameObject.Instantiate(_origin);
            prefab.name = _origin.name;
            prefab.transform.parent = _root;
            prefab.SetActive(false);
            prefabs.Push(prefab);
        }
    }

    //꺼내기
    public GameObject PopPrefabs(GameObject prefab)
    {
        if (prefab.name != _origin.name) { Debug.Log("dosen't mach"); return null; }

        if (prefabs.Count == 0) { Debug.Log("Create new prefabs"); CreatePrefabs(); }

        GameObject stackPrefab = prefabs.Pop();
        stackPrefab.SetActive(true);
        return stackPrefab;
    }

    //넣기
    public void PushPrefabs(GameObject prefab)
    {
        prefab.transform.parent = _root;
        prefab.SetActive(false);
        prefabs.Push(prefab);
    }

    public void ClearPool()
    {
        foreach (GameObject prefab in prefabs)
        {
            Object.Destroy(prefab);
        }
        prefabs.Clear();
        GameObject.Destroy(_origin);
        GameObject.Destroy(_root.gameObject);
    }
}