using SQLite;

[Table("Conditions")]
public class ConditionModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string ConditionId { get; set; }

    public int Value { get; set; }

    public ConditionModel()
    {
        ConditionId = "";
        Value = 0;
    }

    public ConditionModel(string conditionId, int value)
    {
        ConditionId = conditionId;
        Value = value;
    }
}