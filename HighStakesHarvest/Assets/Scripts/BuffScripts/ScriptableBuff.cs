using UnityEngine;

//[CreateAssetMenu(fileName = "ScriptableBuff", menuName = "Buffs/ScriptableBuff")]
public abstract class ScriptableBuff : ScriptableObject
{
    public string BuffName;
    public Sprite Icon;

    public abstract void Apply(GameObject target);
    public abstract void Remove(GameObject target);
}
