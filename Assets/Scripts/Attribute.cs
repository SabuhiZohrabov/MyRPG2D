[System.Serializable]
public class Attribute
{
    public int Value;

    public Attribute(int value)
    {
        Value = value;
    }

    // for future
    // public int BaseValue;
    // public int Bonus;
    // public int MaxValue;
    // public int CurrentValue => BaseValue + Bonus;
}
