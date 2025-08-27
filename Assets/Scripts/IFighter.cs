using UnityEngine;
using System.Collections.Generic;

public enum FactionType
{
    Allied,  // Player + Comrade
    Enemy    // Enemy NPCs
}
public interface IFighter
{
    // Basic identification
    string DisplayName { get; }
    Sprite Icon { get; }
    
    // Combat stats
    int MaxHP { get; }
    int MaxMP { get; }
    
    // Skills available to this fighter
    List<SkillModel> AvailableSkills { get; }

    // Faction system for targeting
    FactionType GetFactionType();
    FactionType GetEnemyFaction();
    FactionType GetAllyFaction();
}