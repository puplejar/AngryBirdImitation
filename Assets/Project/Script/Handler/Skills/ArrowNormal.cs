using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowNormal : MonoBehaviour , BulletSkills
{
    public float mass = 0.3f;
    public float drag = 0f;
    public float crashDestroyTime = 15.0f;
    public float destroyTime = 3.0f;

    Rigidbody rg;

    private void Awake()
    {
        rg = gameObject.GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        Reset();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //충돌이 됐을때 디스트로이
        //스킬구현 (기본 ,터지가 , 관통 , 2차 발사, 확산탄, 유도탄,철갑탄)
        int layer = collision.gameObject.layer;
        if (layer == 6)
        {
            //몬스터나 충돌류가 아니라면
            return;
        }

        Skill(collision);

        StartCoroutine(DestroyBullet(destroyTime));
    }

    public void Reset()
    {
        rg.mass = mass;
        rg.drag = drag;
        rg.isKinematic = true;
    }

    public void Shoot(Vector3 vector,ForceMode forceMode)
    {
        rg.useGravity = true;
        rg.isKinematic = false;
        rg.AddForce(vector,forceMode);
    }


    public void Skill(Collision collision)
    {

    }        
    

    public IEnumerator DestroyBullet(float time)
    {

        yield return new WaitForSeconds(crashDestroyTime);
        Managers.Pool.Destroy(gameObject);

    }
}
