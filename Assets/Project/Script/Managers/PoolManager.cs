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

    //poolable ���ο� ���� ĳ�̵� �������� �������ų� ���� ������
    public GameObject Instantiate(GameObject prefab,bool poolable = false)
    {
        if(prefab == null) { Debug.Log("prefab's dosen't exist"); return null; }
        if(poolable == false) { return GameObject.Instantiate(prefab); }

        //Ǯ�� ������ �������� �������� ���ٸ� ������ ���ÿ� ������
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

    //Ǯ����
    public void Init(GameObject origin, Transform parent)
    {
        _origin = origin;
        _origin.name = origin.name;
        GameObject folder = new GameObject { name = origin.name + "Folder" };
        folder.transform.parent = parent;
        _root = folder.transform;

        CreatePrefabs();
    }

    //������ �߰�����
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

    //������
    public GameObject PopPrefabs(GameObject prefab)
    {
        if (prefab.name != _origin.name) { Debug.Log("dosen't mach"); return null; }

        if (prefabs.Count == 0) { Debug.Log("Create new prefabs"); CreatePrefabs(); }

        GameObject stackPrefab = prefabs.Pop();
        stackPrefab.SetActive(true);
        return stackPrefab;
    }

    //�ֱ�
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