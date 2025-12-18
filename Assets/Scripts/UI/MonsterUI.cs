using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterUI : CharacterUI
{
    public int Id { get; private set; }

    [SerializeField] private Transform healthBarTrans;
    private ValueBar healthBar;

    public void MonsterUIInit(
        ICharacter character, MonsterData monsterData, int Id)
    {
        base.CharacterUIInit(character, monsterData);

        this.Id = Id;
    }

    public void TakeDamageRegist(MonsterData monsterData)
    {
        monsterData.OnTakeDamageEvent += ShowHealthBar;
    }

    private void ShowHealthBar(int damage, int currentHealth, int maxHealth)
    {
        if (healthBar != null)
            PoolManager.Instance.PushObj(DataManager.HEALTHBAR, healthBar.gameObject);

        healthBar = PoolManager.Instance.PullObj(DataManager.HEALTHBAR).
            GetComponent<ValueBar>();
        healthBar.transform.SetParent(UIManager.Instance.WorldCanvas);
        healthBar.ValueBarInit(currentHealth + damage, maxHealth);
        healthBar.UpdateValueBar(currentHealth, maxHealth, (obj) =>
        {
            PoolManager.Instance.PushObj(DataManager.HEALTHBAR, obj);
            this.healthBar = null;
        });
    }

    private void Update()
    {
        if (healthBar == null) return;

        healthBar.UpdatePos(healthBarTrans.position);
        healthBar.transform.LookAt(Camera.main.transform.position);
    }
}
