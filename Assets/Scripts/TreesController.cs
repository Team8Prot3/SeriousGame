﻿using System.Collections;
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
    public GameObject burnedAreaPrefab;
    private GameObject burnedAreaParent;

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
    public GameObject educationalPanel1;
    public GameObject educationalPanel2;
    public string[] educationalText;

    private Color exploreCircleColor = new Color(0.6424f, 0.9482f, 0.1224f, 0.5f);
    private bool isPaused = false;

    //audio
    [Header("Audio")]
    AudioSource audioSource;
    public AudioClip waterAudio;
    public AudioClip cutAudio;
    public AudioClip surveyAudio;
    public AudioClip plantAudio;

    [Header("Count Trees")]
    public Text treesCountText;

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
        DisplayBurnedArea(burnedTreePos);
        StartCoroutine(DequeBurnedTreeLocation());
    }

    IEnumerator DequeBurnedTreeLocation()
    {
        yield return new WaitForSeconds(fertilGroundTime);
        burnedTreesLocation.Dequeue();
    }

    public void DisplayBurnedArea(Vector2 pos)
    {
        GameObject burnedArea = Instantiate(burnedAreaPrefab, burnedAreaParent.transform);
        burnedArea.transform.position = pos;
        burnedArea.transform.localScale = Vector2.one * burnedAreaRadius;
        StartCoroutine(CloseBurnedArea(burnedArea));
    }

    IEnumerator CloseBurnedArea(GameObject burnedAreaObject)
    {
        yield return new WaitForSeconds(fertilGroundTime);
        Destroy(burnedAreaObject);

    }


    // Start is called before the first frame update
    void Start()
    {
        //Initialize trees (Random number & Random positions)
        treesParent = new GameObject("TreesParent");
        burnedAreaParent = new GameObject("BurnedAreaParent");
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
        educationalPanel1.SetActive(false);
        educationalPanel2.SetActive(false);
        isPaused = false;

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        treesCountText.text = treesList.Count + "/40";

        // Click to resume (from survey pause)
        if (isPaused && Input.GetMouseButtonDown(0))
            CloseSurveyInfo();

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
                if (heldTrees.Count > 1) {
                    if (posVariation == 0)
                        heldTree.transform.position += Vector3.up;
                    else if (posVariation == 1)
                        heldTree.transform.position += Vector3.left + Vector3.down;
                    else if (posVariation == 2)
                        heldTree.transform.position += Vector3.right + Vector3.down;
                }

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
                    ShowSurveyInfo(mousePos);
                    if (GetComponent<Fire>())
                        GetComponent<Fire>().causeUI.SetActive(false);
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

    private void ShowSurveyInfo(Vector2 pos)
    {
        // 1 Show exploration info
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

        // Survey text
        string prtext;
        if (prNum >= 20)
            prtext = "<color=red>" + prNum + "%</color>";
        else
            prtext = prNum + "%";

        string info = "Selected area has a " + prtext + " chance of inflammation";
        explorationInfoPanel.GetComponentInChildren<Text>().text = info;


        // 2 Show Educational text
        string chosen_text = "";
        if (educationalText.Length != 0)
            chosen_text = educationalText[Random.Range(0, educationalText.Length)];

        if (Random.Range(0, 2) == 0)
        {
            educationalPanel1.SetActive(true);
            educationalPanel1.GetComponentInChildren<Text>().text = chosen_text;
        }
        else
        {
            educationalPanel2.SetActive(true);
            educationalPanel2.GetComponentInChildren<Text>().text = chosen_text;
        }
        
        // Pause the game
        Time.timeScale = 0;
        isPaused = true;
    }

    private void CloseSurveyInfo()
    {
        isPaused = false;
        Time.timeScale = 1;

        explorationInfoPanel.SetActive(false);
        educationalPanel1.SetActive(false);
        educationalPanel2.SetActive(false);
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
