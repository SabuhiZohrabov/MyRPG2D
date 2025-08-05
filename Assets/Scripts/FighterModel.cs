using UnityEngine;

[System.Serializable]
public class FighterModel : MonoBehaviour
{
    public FighterData data;

    public FighterModel(string name, bool isPlayer)
    {
        this.data.enemySO.name = name;
        this.data.isPlayer = isPlayer;
        this.data.currentHP = data.enemySO.maxHP;
        this.data.isAlive = true;
    }

    public int GetDamage()
    {
        return data != null ? data.damage : 0;
    }

    public int GetMaxHP()
    {
        return data != null ? data.maxHP : 0;
    }
}
