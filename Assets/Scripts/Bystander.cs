using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bystander : MonoBehaviour, IDamageable
{
    private Rigidbody rb;
    private AudioSource aud;

    public float maxHealth;
    public float health;

    public GameObject healthBarCanvas;
    public Image healthBar;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
        health = maxHealth;
    }

    void OnEnable()
    {
        healthBarCanvas.SetActive(true);
    }

    public void Damage(float damage, Vector3 knockback)
    {
        health -= damage;
        rb.AddForce(knockback);
        UpdateHealthBar();
        if (health <= 0) Kill();
    }

    public void Kill()
    {
        rb.constraints = RigidbodyConstraints.None;
        aud.Play();
        Destroy(this);
    }

    public void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float ratio = health / maxHealth;
        healthBar.fillAmount = ratio;
    }
}
