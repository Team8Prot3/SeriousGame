using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // Visualize the circle area
    public GameObject circleArea;

    // Circular Range of the forest
    [Header("Forest Range")]
    public Vector2 circularRangeCenter;
    public float circularRangeRadius;
    public float circularRangeBottom;

    // Num of trees at the beginning
    [Header("Trees Num")]
    public int minInitNum;
    public int maxInitNum;

    // Fire
    [Header("Fire Settings")]
    public Vector2 fireStartPos;
    public float fireSpreadRadius;

    // Trees
    private GameObject treesParent;
    private List<GameObject> treesList = new List<GameObject>();

    // Planting 
    private bool isPlanting;
    private GameObject heldTree;

    // Watering
    [HideInInspector]
    public bool isWatering;
    [Header("Water Settings")]
    public float wateringRadius;

    // Cutting
    [HideInInspector]
    public bool isCutting;

    // Exploring
    [HideInInspector]
    public bool isExploring;
    [Header("Exploration Settings")]
    public float exploringRadius;
    public GameObject explorationInfoPanel;
    public float infoDisplayTime = 10f;


    public List<GameObject> GetTreesList() {
        return treesList;
    }

    // Start planting a tree
    public void Plant() {
        if (!heldTree) 
        {
            isPlanting = true;
            isWatering = false;
            isCutting = false;
            isExploring = false;
            CloseCircleArea();
            heldTree = Instantiate(treePrefab);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            heldTree.transform.position = mousePos;
        }
    }

    // Burn the nearest tree from start point
    public void StartFire() 
    {
        if (treesList.Count == 0)
            return;

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

    public void Water()
    {
        isWatering = true;
        isCutting = false;
        isPlanting = false;
        isExploring = false;
        DisplayCircleArea(wateringRadius);
    }

    public void Cut()
    {
        isCutting = true;
        isWatering = false;
        isPlanting = false;
        isExploring = false;
        CloseCircleArea();
    }

    public void Explore() 
    {
        isExploring = true;
        isPlanting = false;
        isWatering = false;
        isCutting = false;
        DisplayCircleArea(exploringRadius);
    }

    public void DisplayCircleArea(float radius) 
    {
        circleArea.SetActive(true);
        circleArea.transform.localScale = Vector2.one * radius;
    }

    public void CloseCircleArea()
    {
        circleArea.SetActive(false);
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

        explorationInfoPanel.SetActive(false);
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

        //Water chosen tree
        if(isWatering)
        {
            // Get mousepos
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (IsInCircularRange(mousePos))
            {
                // Left mouse click
                if (Input.GetMouseButtonDown(0))
                {

                    foreach (GameObject tree in treesList)
                        if (Vector2.Distance(mousePos, tree.transform.position) < wateringRadius)
                            tree.GetComponent<Tree>().StopBurning();

                    isWatering = false;
                    CloseCircleArea();
                }
            }

        }

        //Cut chosen tree
        if (isCutting)
        {
            // Get mousepos
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (IsInCircularRange(mousePos))
            {
                // Left mouse click
                if (Input.GetMouseButtonDown(0))
                {
                    isCutting = false;
                }
            }

        }

        if (isExploring) 
        {
            // Get mousepos
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsInCircularRange(mousePos))
            {
                // Left mouse click
                if (Input.GetMouseButtonDown(0))
                {
                    ShowExplorationInfo(mousePos);
                    isExploring = false;
                    CloseCircleArea();
                }
            }
        }

        if (circleArea.activeSelf) 
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            circleArea.transform.position = mousePos;
        }
    }

    private void ShowExplorationInfo(Vector2 pos) 
    {
        explorationInfoPanel.SetActive(true);

        int healthyNum = 0;
        int unhealthyNum = 0;
        int burningNum = 0;

        foreach (GameObject tree in treesList) 
        {
            if (Vector2.Distance(pos, tree.transform.position) < exploringRadius) 
            {
                State _s = tree.GetComponent<Tree>().GetState();
                if (_s == State.healthy)
                    healthyNum++;
                else if (_s == State.unhealthy)
                    unhealthyNum++;
                else
                    burningNum++;
            }
        }

        // Info text
        string info = "Survey\n"
        + "\nHealthy:\t" + healthyNum
        + "\nUnhealthy:\t" + unhealthyNum
        + "\nBurning:\t" + burningNum
        + "\nFire risk level:\t" + "High"
        + "\nDensity:\t"
        + "\nHint:\tCut unhealthy trees";
        explorationInfoPanel.GetComponentInChildren<Text>().text = info;

        //Close panel
        StartCoroutine(CloseExplorationInfo());
    }

    IEnumerator CloseExplorationInfo()
    {
        yield return new WaitForSeconds(infoDisplayTime);
        explorationInfoPanel.SetActive(false);
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
