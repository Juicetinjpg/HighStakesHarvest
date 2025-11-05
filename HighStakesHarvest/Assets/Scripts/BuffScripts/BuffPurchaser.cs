using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BuffPurchaser : MonoBehaviour
{
    [SerializeField] public List<ScriptableBuff> allBuffs = new List<ScriptableBuff>();
    private BuffManager buffManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buffManager = GetComponent<BuffManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyRandomBuff()
    {
        int randomNum = Random.Range(0, allBuffs.Count);
        buffManager.AddBuff(allBuffs[randomNum]);
    }
}
