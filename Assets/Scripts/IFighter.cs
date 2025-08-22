using UnityEngine;

public interface IFighter
{
    // Basic identification
    string DisplayName { get; }
    Sprite Icon { get; }
    
    // Combat stats
    int MaxHP { get; }
    int MaxMP { get; }
}