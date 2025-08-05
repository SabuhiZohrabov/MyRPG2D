using System.Collections.Generic;
using UnityEngine;

public class AdventureTextJsonLoader
{
    public static List<AdventureTextData> LoadFromJson(string jsonText)
    {
        AdventureTextDataListWrapper wrapper = JsonUtility.FromJson<AdventureTextDataListWrapper>(jsonText);
        return wrapper.texts;
    }

    [System.Serializable]
    private class AdventureTextDataListWrapper
    {
        public List<AdventureTextData> texts;
    }
}
