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

    [Header("怪物巡逻目标点【新增！】")]
    public Transform monsterPatrol1; // 怪物巡逻起点（对应Monsters脚本的place1）
    public Transform monsterPatrol2; // 怪物巡逻终点（对应Monsters脚本的place2）

    [Header("通关神像配置【清完怪生成】")]
    public GameObject godStatuePrefab;
    public Transform statueSpawnPos;
    private bool isLevelCleared = false;


    public void RefreshMonsters()
    {
        ClearExistingMonsters();
        SpawnMonsters();
    }

    private void ClearExistingMonsters()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void SpawnMonsters()
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("【MonsterSpawner】未指定怪物预制体！", this);
            return;
        }
        if (spawnLeft == null || spawnRight == null)
        {
            Debug.LogError("生成区域未关联", this);
            return;
        }
        // 新增：检测巡逻目标点是否配置
        if (monsterPatrol1 == null || monsterPatrol2 == null)
        {
            Debug.LogError("【MonsterSpawner】未配置怪物巡逻目标点（monsterPatrol1/2）！", this);
            return;
        }

        float minX = Mathf.Min(spawnLeft.position.x, spawnRight.position.x);
        float maxX = Mathf.Max(spawnLeft.position.x, spawnRight.position.x);
        float fixedY = transform.position.y;

        for (int i = 0; i < spawnCount; i++)
        {
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPos = new Vector3(randomX, fixedY, transform.position.z);
            // 实例化怪物，并获取其Monsters脚本
            GameObject monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity, transform);
            Monsters monsterScript = monster.GetComponent<Monsters>();
            if (monsterScript != null)
            {
                // 核心：给怪物动态赋值巡逻目标点
                monsterScript.place1 = monsterPatrol1;
                monsterScript.place2 = monsterPatrol2;
            }
            else
            {
                Debug.LogError($"【MonsterSpawner】怪物预制体未挂载Monsters脚本！", this);
            }
        }

        Debug.Log($"【怪物刷新】生成 {spawnCount} 只怪物", this);
    }

    private void Start()
    {
        SpawnMonsters();
    }

    private void Update()
    {
        if (!isLevelCleared && transform.childCount == 0)
        {
            LevelClearSpawnStatue();
        }
    }

    private void LevelClearSpawnStatue()
    {
        isLevelCleared = false;
        Debug.Log($"【层通关】{gameObject.name} 所有怪物已清除！生成通关神像", this);

        if (godStatuePrefab == null)
        {
            Debug.LogError("【MonsterSpawner】未指定通关神像预制体！", this);
            return;
        }
        if (statueSpawnPos == null)
        {
            Debug.LogWarning("【MonsterSpawner】未指定神像生成位置，将在生成器位置生成", this);
            Instantiate(godStatuePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(godStatuePrefab, statueSpawnPos.position, Quaternion.identity);
            Debug.LogWarning("生成神像", this);
            isLevelCleared = true;
        }
    }
}