using SQLite;
using System.Collections.Generic;

[Table("PlayerStats")]
public class PlayerStatsModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int Level { get; set; }
    public int CurrentXP { get; set; }
    public int XPToNextLevel { get; set; }
    public int AvailableAttributePoints { get; set; }
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Intelligence { get; set; }
    public int Endurance { get; set; }
    public string CurrentAdventureId { get; set; }
    public int Gold { get; set; }

    public PlayerStatsModel()
    {
        // Default starting values
        Level = 1;
        CurrentXP = 0;
        XPToNextLevel = 10;
        AvailableAttributePoints = 0;
        Strength = 5;
        Dexterity = 5;
        Intelligence = 5;
        Endurance = 5;
        CurrentAdventureId = "start_adventure";
        Gold = 100;
    }
}
