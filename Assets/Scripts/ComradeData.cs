using UnityEngine;

[CreateAssetMenu(fileName = "NewComrade", menuName = "Combat/Comrade")]
public class ComradeData : ScriptableObject
{
    [Header("Basic Info")]
    public string comradeId;
    public string displayName;
    public Sprite icon;
    
    [Header("Combat Stats")]
    public int maxHP;
    public int maxMP;
    
    [Header("Attributes")]
    public int strength;
    public int defense;
    public int agility;
    public int intelligence;
    
}