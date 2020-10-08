using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [Range(0, 100)]
    public int fireProbability = 10;    // 0% ~ 100%
    public float fireCheckIntervalTime = 2.5f;
    private float fireCheckTimer;
    private List<GameObject> treesList;

    void Start()
    {
        fireCheckTimer = fireCheckIntervalTime;
        treesList = TreesController.instance.GetTreesList();
    }

    void Update()
    {
        fireCheckTimer -= Time.deltaTime;
        if (fireCheckTimer <= 0)
        {
            fireCheckTimer = fireCheckIntervalTime;
            RandomFire();
        }
    }

    void RandomFire()
    {
        int rnum = Random.Range(0, 100);
        if (rnum < fireProbability)
            treesList[Random.Range(0, treesList.Count)].GetComponent<Tree>().StartBurning();
    }
}
