using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowExplode : MonoBehaviour, BulletSkills
{
    public float mass = 0.5f;
    public float drag = 0f;
    public float crashDestroyTime = 15.0f;
    public float destroyTime = 3.0f;

    public float explodeRadius = 5f;
    public float explodeForce = 500f;

    //ȿ���� �ѹ��� �߻�
    private bool exploded = false;

    Rigidbody rg;
    SoundController soundController;

    private void Awake()
    {
        rg = gameObject.GetComponent<Rigidbody>();
        soundController = gameObject.GetComponent<SoundController>();
    }
    private void OnEnable()
    {
        Reset();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //�浹�� ������ ��Ʈ����
        //��ų���� (�⺻ ,������ , ���� , 2�� �߻�, Ȯ��ź, ����ź,ö��ź)

        int layer = collision.gameObject.layer;
        if (layer == 6) return;

        if (!exploded)
        {
            exploded = true;
            Skill(collision);
        }
        StartCoroutine(DestroyBullet(destroyTime));
    }


    public void Reset()
    {
        rg.mass = mass;
        rg.drag = drag;
        rg.isKinematic = true;
    }

    public void Shoot(Vector3 vector, ForceMode forceMode)
    {
        rg.useGravity = true;
        rg.isKinematic = false;
        rg.AddForce(vector, forceMode);
    }

    public void Skill(Collision collision)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explodeRadius);
        foreach (Collider go in colliders)
        {
            //�ֺ���ü
            Rigidbody goRg = go.gameObject.GetComponent<Rigidbody>();
            if (goRg)
            {
                goRg.AddExplosionForce(explodeForce, transform.position, explodeRadius);

            }
        }
        soundController.PlayAudio("Boom");
    }

    public IEnumerator DestroyBullet(float time)
    {

        yield return new WaitForSeconds(crashDestroyTime);
        Managers.Pool.Destroy(gameObject);

    }

}
