using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITest : MonoBehaviour
{
    [SerializeField] private ValueBar healthBar;
    [SerializeField] private int maxHealth;
    [SerializeField] private int health;
    private int preHealth;

    private void Start()
    {
        //bar.ValueBarInit();
        preHealth = health;
    }

    private void Update()
    {
        if (preHealth != health)
        {
            healthBar.UpdateValueBar(health, maxHealth);
            preHealth = health;
        }
    }
}