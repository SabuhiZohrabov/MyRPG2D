using UnityEngine;

[System.Serializable]
public class FighterData
{
    // Interface reference
    public IFighter fighter;
    
    // Type identification
    public bool isPlayer;
    public bool isComrade;
    public bool isEnemy;

    // Dynamic runtime values
    public int currentHP;
    public int currentMP;
    public bool isAlive = true;

    // --- Universal properties (always use these instead of direct fields!) ---

    public string displayName
    {
        get
        {
            return fighter?.DisplayName ?? "Unknown";
        }
    }

    public int maxHP
    {
        get
        {
            return fighter?.MaxHP ?? 100;
        }
    }

    public int maxMP
    {
        get
        {
            return fighter?.MaxMP ?? 50;
        }
    }

    // --- Constructors ---

    // Enemy constructor
    public FighterData(EnemySO enemy)
    {
        fighter = enemy;
        isPlayer = false;
        isComrade = false;
        isEnemy = true;
        currentHP = enemy?.MaxHP ?? 100;
        currentMP = enemy?.MaxMP ?? 50;
        isAlive = true;
    }

    // Player constructor
    public FighterData(CharacterStats stats)
    {
        fighter = stats;
        isPlayer = true;
        isComrade = false;
        isEnemy = false;
        currentHP = stats?.MaxHP ?? 100;
        currentMP = stats?.MaxMP ?? 50;
        isAlive = true;
    }

    // Comrade constructor
    public FighterData(ComradeData comrade)
    {
        fighter = comrade;
        isPlayer = false;
        isComrade = true;
        isEnemy = false;
        currentHP = comrade?.MaxHP ?? 100;
        currentMP = comrade?.MaxMP ?? 50;
        isAlive = true;
    }

    // --- Universal methods ---

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0)
            currentHP = 0;
        if (currentHP == 0)
            isAlive = false;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    public bool HasEnoughMana(int amount)
    {
        return currentMP >= amount;
    }

    public void UseMana(int amount)
    {
        currentMP -= amount;
        if (currentMP < 0)
            currentMP = 0;
    }
}
