using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public string name = "Enemy";
    public float health = 100f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    public void Damage(float damage)
    {
        Damage(damage, Vector3.zero);
    }

    public void Damage(float damage, Vector3 knockback)
    {
        health -= damage;
        if (rb != null) rb.AddForce(knockback);
        if (health <= 0) Kill();
    }

    public void Kill()
    {
        Debug.Log("Oh no, I am dead.\n" + name);
        Destroy(gameObject);
    }
}
