using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class TreesController : MonoBehaviour
{
    // Singleton
    public static TreesController instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this);
        }
    }


    // Tree prefab
    public GameObject treePrefab;

    // Circular Range of the forest
    public Vector2 circularRangeCenter;
    public float circularRangeRadius;
    public float circularRangeBottom;

    // Num of trees at the beginning
    public int minInitNum;
    public int maxInitNum;

    // Fire
    public Vector2 fireStartPos;
    public float fireSpreadRadius;

    // Trees
    private GameObject treesParent;
    private List<GameObject> treesList = new List<GameObject>();

    // Planting 
    private bool isPlanting;
    private GameObject heldTree;


    // Start planting a tree
    public void Plant() {
        if (!heldTree) 
        {
            isPlanting = true;
            heldTree = Instantiate(treePrefab);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            heldTree.transform.position = mousePos;
        }
    }

    // Burn the nearest tree from start point
    public void StartFire() 
    {
        GameObject chosenTree = treesList[0];
        float minDistance = Vector2.Distance(fireStartPos, treesList[0].transform.position);

        foreach (GameObject tree in treesList) 
        {
            float distance = Vector2.Distance(fireStartPos, tree.transform.position);
            if (distance < minDistance) 
            {
                chosenTree = tree;
                minDistance = distance;
            }
        }

        chosenTree.GetComponent<Tree>().StartBurning();
    }

    // Burn trees in spread range
    public void SpreadFire(Vector2 startPos) 
    {
        foreach (GameObject tree in treesList)
            if (Vector2.Distance(startPos, tree.transform.position) < fireSpreadRadius)
                tree.GetComponent<Tree>().StartBurning();
    }

    public void RemoveTree(GameObject tree)
    {
        treesList.Remove(tree);
    }

    // Start is called before the first frame update
    void Start()
    {
        //Initialize trees (Random number & Random positions)
        treesParent = new GameObject("TreesParent");
        if (minInitNum > 0 && minInitNum <= maxInitNum)
        {
            for (int i = 0; i < Random.Range(minInitNum, maxInitNum + 1); i++)
            {
                Vector2 treePos;
                while (true) {
                    treePos = circularRangeCenter + Random.insideUnitCircle * circularRangeRadius;
                    if (treePos.y > circularRangeBottom)
                        break;
                }
                
                GameObject tree = Instantiate(treePrefab, treesParent.transform);
                tree.transform.position = treePos;
                treesList.Add(tree);
            }
        }

        isPlanting = false;
        heldTree = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Plant the held tree
        if (isPlanting && heldTree)
        {
            // Get mousepos
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            heldTree.transform.position = mousePos;

            if (IsInCircularRange(mousePos))
            {
                heldTree.GetComponent<SpriteRenderer>().color = Color.white;

                // Left mouse click
                if (Input.GetMouseButtonDown(0))
                {
                    heldTree.transform.parent = treesParent.transform;
                    treesList.Add(heldTree);

                    isPlanting = false;
                    heldTree = null;
                }
            }
            else
                heldTree.GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }

    private void OnDrawGizmos()
    {
        // Draw gizmos only in Scene

        // Draw the boundary of forest
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(circularRangeCenter, circularRangeRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector2(circularRangeCenter.x - circularRangeRadius, circularRangeBottom), new Vector2(circularRangeCenter.x + circularRangeRadius, circularRangeBottom));

        // Draw the burning point
        Gizmos.DrawSphere(fireStartPos, 0.2f);

        // Draw spread circle
        foreach (GameObject tree in treesList)
            Gizmos.DrawWireSphere(tree.transform.position, fireSpreadRadius);
    }

    private bool IsInCircularRange(Vector2 _pos)
    {
        if (Vector2.Distance(_pos, circularRangeCenter) < circularRangeRadius && _pos.y > circularRangeBottom)
            return true;
        else
            return false;
    }

}
