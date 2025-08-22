using UnityEngine;

[CreateAssetMenu(fileName = "NewComrade", menuName = "Combat/Comrade")]
public class ComradeData : ScriptableObject, IFighter
{
    [Header("Basic Info")]
    public string comradeId;
    public string displayName;
    public Sprite icon;
    
    [Header("Combat Stats")]
    public int maxHP;
    public int maxMP;
    
    // IFighter interface implementation
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public int MaxHP => maxHP;
    public int MaxMP => maxMP;
}