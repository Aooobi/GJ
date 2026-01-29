using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("怪物预制体")]
    public GameObject monsterPrefab;

    [Header("每次生成数量")]
    public int spawnCount = 2;

    [Header("生成区域")]
    public Transform spawnLeft;
    public Transform spawnRight;


  

    public void RefreshMonsters()
    {
        ClearExistingMonsters();
        SpawnMonsters();
   
    }

    private void ClearExistingMonsters()
    { 
        for(int i=transform.childCount -1; i>=0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);  

        }
    
    }
        
    private void SpawnMonsters()
    {
        if(monsterPrefab == null)
        {
            Debug.LogError("【MonsterSpawner】未指定怪物预制体！", this);
            return;

        }

        if (spawnLeft == null || spawnRight == null)
        {
            Debug.LogError("生成区域未关联", this);
            return;
        }

        float minX = Mathf.Min(spawnLeft.position.x, spawnRight.position.x);
        float maxX = Mathf.Max(spawnLeft.position.x, spawnRight.position.x);
        float fixedY = transform.position.y;

        for (int i=0; i<spawnCount; i++)
        {
            float randomX = Random.Range(minX,maxX);
            Vector3 spawnPos = new Vector3(randomX, fixedY, transform.position.z);
            Instantiate(monsterPrefab, spawnPos, Quaternion.identity, transform);

        }

        Debug.Log($"【怪物刷新】生成 {spawnCount} 只怪物", this);
    }

    private void Start()
    {
        SpawnMonsters();
    }
}
