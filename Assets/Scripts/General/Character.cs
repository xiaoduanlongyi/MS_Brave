using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    [Header("Event Monitor")]
    public VoidEventSO newGameEvent;

    [Header("Basic Attributes")]
    public float maxHealth;
    public float currentHealth;
    public float maxPower;
    public float currentPower;
    public float powerRecoverSpeed;

    [Header("Hit Counter")]
    public float invulnerableDuration; 
    [HideInInspector]public float invulnerableCounter;
    public bool isInvulnerable;
     
    public UnityEvent<Character> OnHealthChange;
    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;

    private void NewGame()
    {
        currentHealth = maxHealth;
        currentPower = maxPower;
        OnHealthChange?.Invoke(this);
    }

    private void OnEnable()
    {
        newGameEvent.OnEventRaised += NewGame;
    }

    private void OnDisable()
    {
        newGameEvent.OnEventRaised -= NewGame;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if(isInvulnerable)
        {
            invulnerableCounter -= Time.deltaTime;
            if (invulnerableCounter <= 0)
            {
                isInvulnerable = false;
            }
        }

        if(currentPower < maxPower)
        {
            currentPower += Time.deltaTime * powerRecoverSpeed;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Water"))
        {
            currentHealth = 0;
            OnHealthChange?.Invoke(this);
            OnDie?.Invoke();
        }
    } 

    public void TakeDamage(Attack attacker)
    {
        if (isInvulnerable)
            return;

        if(currentHealth - attacker.damage > 0)
        {
            currentHealth -= attacker.damage;
            //trigger take damage
            OnTakeDamage?.Invoke(attacker.transform);
            TriggerInvulnerable();
        }
        else
        {
            currentHealth = 0;
            //trigger death
            OnDie?.Invoke();
        }

        OnHealthChange?.Invoke(this);
    }

    private void TriggerInvulnerable()
    {
        if(!isInvulnerable)
        {
            isInvulnerable = true;
            invulnerableCounter = invulnerableDuration;
        }
    }

    public void OnSlide(int cost)
    {
        currentPower -= cost;
        OnHealthChange?.Invoke(this);
    }
}
