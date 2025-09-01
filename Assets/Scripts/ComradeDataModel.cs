using SQLite;

[Table("ActiveComrades")]
public class ComradeDataModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // The ID of the active comrade
    public string ComradeId { get; set; }

    public ComradeDataModel()
    {
        // Default constructor for SQLite
    }

    public ComradeDataModel(string comradeId)
    {
        ComradeId = comradeId;
    }
}