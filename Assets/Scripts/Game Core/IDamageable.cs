using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void Kill();

    void Damage(float damage, Vector3 knockback);
}
