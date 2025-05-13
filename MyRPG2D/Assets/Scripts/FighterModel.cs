using UnityEngine;

[System.Serializable]
public class FighterModel
{
    public string name;
    public bool isPlayer;
    public int maxHP = 100;
    public int currentHP = 100;
    public bool isAlive = true;

    // Optional: for later
    public int speed = 5;
    public int defense = 0;

    public FighterModel(string name, bool isPlayer)
    {
        this.name = name;
        this.isPlayer = isPlayer;
        this.currentHP = maxHP;
        this.isAlive = true;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            isAlive = false;
        }
    }

    public void Heal(int amount)
    {
        if (!isAlive) return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }
}
