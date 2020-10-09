using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    public Queue<Vector2> burnedTreesLocation = new Queue<Vector2>();
    public float burnedAreaRadius;

    // Trees
    private GameObject treesParent;
    private List<GameObject> treesList = new List<GameObject>();

    // Planting 
    private bool isPlanting;
    private List<GameObject> heldTrees = new List<GameObject>();
    public float fertilGroundTime;

    // Watering
    [HideInInspector]
    public bool isWatering;
    [Header("Water Settings")]
    public float wateringRadius;
    public GameObject waterPrefab;
    private Color waterCircleColor = new Color(0, 0.5f, 1, 0.5f);

    // Cutting
    [HideInInspector]
    public bool isCutting;
    public GameObject cutPrefab;

    // Exploring
    [HideInInspector]
    public bool isExploring;
    [Header("Exploration Settings")]
    public float exploringRadius;
    public GameObject explorationInfoPanel;
    public float infoDisplayTime = 10f;
    private Color exploreCircleColor = new Color(0.7924f, 0.4182f, 0.3924f, 0.5f);

    //audio
    AudioSource audioSource;
    public AudioClip waterAudio;
    public AudioClip cutAudio;
    public AudioClip surveyAudio;
    public AudioClip plantAudio;


    public List<GameObject> GetTreesList()
    {
        return treesList;
    }

    // Start planting a tree
    public void Plant()
    {
        if (!heldTrees.Any())
        {
            isPlanting = true;
            isWatering = false;
            isCutting = false;
            isExploring = false;
            CloseCircleArea();
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject heldTree = Instantiate(treePrefab);
            heldTree.transform.position = mousePos;
            heldTrees.Add(heldTree);
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
    public void SpreadFire(Vector2 startPos, float radius)
    {
        foreach (GameObject tree in treesList)
            if (Vector2.Distance(startPos, tree.transform.position) < radius)
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
        DisplayCircleArea(wateringRadius, waterCircleColor);
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
        DisplayCircleArea(exploringRadius, exploreCircleColor);
    }

    public void DisplayCircleArea(float radius, Color color)
    {
        circleArea.SetActive(true);
        circleArea.GetComponent<SpriteRenderer>().color = color;
        circleArea.transform.localScale = Vector2.one * radius;
    }

    public void CloseCircleArea()
    {
        circleArea.SetActive(false);
    }

    public void AddBurnedTreeLocation(Vector2 burnedTreePos)
    {
        burnedTreesLocation.Enqueue(burnedTreePos);
        StartCoroutine(DequeBurnedTreeLocation());
    }

    IEnumerator DequeBurnedTreeLocation()
    {
        yield return new WaitForSeconds(fertilGroundTime);
        burnedTreesLocation.Dequeue();
    }




    // Start is called before the first frame update
    void Start()
    {
        //Initialize trees (Random number & Random positions)
        treesParent = new GameObject("TreesParent");
        if (minInitNum > 0 && minInitNum <= maxInitNum)
        {
            for (int i = 0; i < UnityEngine.Random.Range(minInitNum, maxInitNum + 1); i++)
            {
                Vector2 treePos;
                while (true)
                {
                    treePos = circularRangeCenter + UnityEngine.Random.insideUnitCircle * circularRangeRadius;
                    if (treePos.y > circularRangeBottom)
                        break;
                }

                GameObject tree = Instantiate(treePrefab, treesParent.transform);
                tree.transform.position = treePos;
                treesList.Add(tree);
            }
        }

        isPlanting = false;
        heldTrees.Clear();

        explorationInfoPanel.SetActive(false);

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Plant the held tree
        if (isPlanting && heldTrees.Any())
        {
            // Get mousepos
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (IsInFertilArea(mousePos))
            {         
                if(heldTrees.Count < 3)
                {
                    heldTrees.Add(Instantiate(treePrefab));
                }

            }
            else
            {
                if (heldTrees.Count > 1)
                {
                    for(int i = 1; i < 2; i++)
                    {
                        GameObject o = heldTrees[i];
                        heldTrees.RemoveAt(i);
                        Destroy(o);
                    }
                }
            }

            float posVariation = 0;
            foreach(GameObject heldTree in  heldTrees)
            {
                heldTree.transform.position = mousePos;
                heldTree.transform.position += Vector3.right * posVariation;

                if (IsInCircularRange(heldTree.transform.position))
                {
                    heldTree.GetComponent<SpriteRenderer>().color = Color.white;
                }
                else
                    heldTree.GetComponent<SpriteRenderer>().color = Color.gray;

                posVariation += 1;

            }

            if (IsInCircularRange(mousePos))
            {
                // Left mouse click
                if (Input.GetMouseButtonDown(0))
                {
                    foreach (GameObject heldTree in heldTrees)
                    {
                        heldTree.transform.parent = treesParent.transform;
                        treesList.Add(heldTree);
                    }
                    audioSource.PlayOneShot(plantAudio, 0.7F);
                    isPlanting = false;
                    heldTrees.Clear();
                }
            }
         
        }

        //Water chosen tree
        if (isWatering)
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
    
                    GameObject splash = Instantiate(waterPrefab, mousePos, Quaternion.identity);
                    Destroy(splash, splash.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + 2);
                    audioSource.PlayOneShot(waterAudio, 0.7F);
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
                    GameObject cutObj = Instantiate(cutPrefab, mousePos, Quaternion.identity);
                    Destroy(cutObj, cutObj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + 1);
                    audioSource.PlayOneShot(cutAudio, 0.7F);
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
                    audioSource.PlayOneShot(surveyAudio, 0.7F);
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

        int prNum;
        if (burningNum > 0)
            prNum = 100;
        else
            prNum = 5 * healthyNum + 10 * unhealthyNum;

        // Info text
        string prtext;
        if (prNum >= 20)
            prtext = "<color=red>" + prNum + "%</color>";
        else
            prtext = prNum + "%";

        string tips;
        if (burningNum > 0)
            tips = "\nTips : Please stop the fire!";
        else if (unhealthyNum > 0)
            tips = "\nTips : Cut unhealthy trees.";
        else if (healthyNum >= 4)
            tips = "\nTips : The forest is much too dense.";
        else
            tips = "";

        string info = "You're using 'Survey'.\nSelected area has a " + prtext + " chance of inflammation." + tips;
        /*"Survey\n"
        + "\nHealthy(" + healthyNum + ") "
        + "Unhealthy(" + unhealthyNum + ") "
        + "Burning(" + burningNum + ") "
        + "\nFire danger rating:\t" + "High"
        + "\nDensity:\t"
        + "\nHint:\tCut unhealthy trees";*/
        explorationInfoPanel.GetComponentInChildren<Text>().text = info;

        //Close panel after few secondss
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
    }

    private bool IsInCircularRange(Vector2 _pos)
    {
        if (Vector2.Distance(_pos, circularRangeCenter) < circularRangeRadius && _pos.y > circularRangeBottom)
            return true;
        else
            return false;
    }

    private bool IsInFertilArea(Vector2 _pos)
    {

        foreach (Vector2 burnedArea in burnedTreesLocation)
        {
            if (Vector2.Distance(_pos, burnedArea) < burnedAreaRadius)
                return true;
        }

        return false;
    }

}
