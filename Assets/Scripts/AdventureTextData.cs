using System;
using System.Collections.Generic;
using UnityEngine;
public enum AdventureTextType
{
    Narration=0,
    Dialog=1,
    Battle=2,
    Quest=3,
    Item=4
}

[Serializable]
public class AdventureLink
{
    public string linkID;                   // e.g. "goblin", "npc", "cave"
    public string nextLinkID;                   // e.g. "goblin", "npc", "cave"
    public AdventureTextType type;          // e.g. Battle, Dialog, Narration
    public string objectID;                 // e.g. "item", "enemy", "quest" etc
}
[Serializable]
public class AdventureTextData
{
    public string id;      // unique identifier for this text
    [TextArea(3, 10)]
    public string text;    // the adventure message to show in UI
    public List<AdventureLink> links = new List<AdventureLink>();
    //[TextArea(3, 10)]
    //public string nextLinkOnVictory;
    //map
    public string mapId;
    public Vector2Int mapPosition;
}
