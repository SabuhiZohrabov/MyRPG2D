using SQLite;

public class AdventureProgressModel
{
    [PrimaryKey, AutoIncrement]
    public int AdventureProgressId { get; set; }
    public string CurrentAdventureId { get; set; }
}
