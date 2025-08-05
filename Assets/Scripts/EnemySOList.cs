using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySOList", menuName = "Database/Enemy SO List")]
public class EnemySOList : ScriptableObject
{
    public List<EnemySO> enemies = new List<EnemySO>();

    public EnemySO GetEnemyById(string id)
    {
        return enemies.Find(e => e.enemyId == id);
    }
}
