using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface BulletSkills
{
    public void Reset();

    public void Shoot(Vector3 vector,ForceMode forceMode);

    public void Skill(Collision collision);

    IEnumerator DestroyBullet(float time);
}
