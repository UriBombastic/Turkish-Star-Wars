using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour, IDamageable
{
    public string name = "Enemy";
    public float initialHealth = 100f;
    public float health;
    public Image healthBar;
    public TextMeshProUGUI nameText;
    private Rigidbody rb;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        health = initialHealth;
        if (nameText != null) nameText.text = name;
    }


    public void Damage(float damage)
    {
        Damage(damage, Vector3.zero);
    }

    public void Damage(float damage, Vector3 knockback)
    {
        health -= damage;
        UpdateHealthBar();
        if (rb != null) rb.AddForce(knockback);
        if (health <= 0) Kill();
    }

    public void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float ratio = health / initialHealth;
        healthBar.fillAmount = ratio;
    }

    public void Kill()
    {
        Debug.Log("Oh no, I am dead.\n" + name);
        Destroy(gameObject);
    }
}
