using UnityEngine;

[System.Serializable]
public class FighterData
{
    // References
    public EnemySO enemySO;             // Enemy for EnemySO
    public bool isPlayer;               // Is this fighter the player?
    public bool isComrade;              // Is this fighter a comrade?
    public ComradeData comradeData;     // Comrade for ComradeData
    public CharacterStats characterStats; // Player for stats

    // Dynamic runtime values
    public int currentHP;
    public int currentMP;
    public bool isAlive = true;

    // --- Universal properties (always use these instead of direct fields!) ---

    public string displayName
    {
        get
        {
            if (isPlayer && characterStats != null)
                return characterStats.DisplayName;
            else if (isComrade && comradeData != null)
                return comradeData.displayName;
            else if (enemySO != null)
                return enemySO.displayName;
            else
                return "Unknown";
        }
    }

    public int maxHP
    {
        get
        {
            if (isPlayer && characterStats != null)
                return characterStats.Endurance.Value * 5 + 100;
            else if (isComrade && comradeData != null)
                return comradeData.maxHP;
            else if (enemySO != null)
                return enemySO.maxHP;
            else
                return 100;
        }
    }

    public int maxMP
    {
        get
        {
            if (isPlayer && characterStats != null)
                return 50 + characterStats.Intelligence.Value * 5;
            else if (isComrade && comradeData != null)
                return comradeData.maxMP;
            else if (enemySO != null)
                return enemySO.maxMP;
            else
                return 50;
        }
    }

    public int damage
    {
        get
        {
            if (isPlayer && characterStats != null)
                return 10 + characterStats.Strength.Value;
            else if (isComrade && comradeData != null)
                return 10;
            else if (enemySO != null)
                return 10;
            else
                return 10;
        }
    }

    // --- Constructors ---

    // Enemy constructor
    public FighterData(EnemySO enemy)
    {
        enemySO = enemy;
        isPlayer = false;
        isComrade = false;
        comradeData = null;
        characterStats = null;
        currentHP = enemySO != null ? enemySO.maxHP : 100;
        currentMP = enemySO != null ? enemySO.maxMP : 50;
        isAlive = true;
    }

    // Player constructor
    public FighterData(CharacterStats stats)
    {
        enemySO = null;
        isPlayer = true;
        isComrade = false;
        comradeData = null;
        characterStats = stats;
        currentHP = stats.maxHP;
        currentMP = stats.maxMP;
        isAlive = true;
    }

    // Comrade constructor
    public FighterData(ComradeData comrade)
    {
        enemySO = null;
        isPlayer = false;
        isComrade = true;
        comradeData = comrade;
        characterStats = null;
        currentHP = comrade.maxHP;
        currentMP = comrade.maxMP;
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
