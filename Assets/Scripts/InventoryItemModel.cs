using SQLite;

[Table("InventoryItems")]
public class InventoryItemModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string ItemId { get; set; }

    public int Amount { get; set; }
}
