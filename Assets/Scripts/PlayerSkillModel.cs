using SQLite;

[Table("PlayerSkills")]
public class PlayerSkillModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Foreign key reference to PlayerStats
    public int PlayerId { get; set; }

    // The skill name
    public string SkillName { get; set; }

    public PlayerSkillModel()
    {
        // Default constructor
    }

    public PlayerSkillModel(int playerId, string skillName)
    {
        PlayerId = playerId;
        SkillName = skillName;
    }
}