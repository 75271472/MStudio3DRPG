using UnityEngine;

public class CharacterStateInfo
{
    public int maxHealth;
    public int defence;
}

public class CharacterState
{
    public bool isDie;
    public int health;

    public CharacterState(CharacterStateInfo characterStateInfo)
    {
        this.isDie = false;
        this.health = characterStateInfo.maxHealth;
    }

    public void SetDie()
    {
        health = 0;
        isDie = true;
    }
}
