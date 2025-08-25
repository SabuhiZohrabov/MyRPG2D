using UnityEngine;
using System.Collections.Generic;

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
}