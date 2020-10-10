using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fire : MonoBehaviour
{
    [Header("Fire check")]
    [Range(0, 100)]
    public int checkProbability = 15;    // 0% ~ 100%
    public float checkIntervalTime = 2f;

    [Header("Fire causes")]
    public GameObject causeUI;
    public string[] causeTexts;
    public float displayTime = 3f;

    private float checkTimer;

    private List<GameObject> treesList;

    void Start()
    {
        checkTimer = checkIntervalTime;
        treesList = TreesController.instance.GetTreesList();
        causeUI.SetActive(false);
    }

    void Update()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0)
        {
            checkTimer = checkIntervalTime;
            RandomFire();
        }
    }

    void RandomFire()
    {
        int rnum = Random.Range(0, 100);
        if (rnum < checkProbability)
        {
            GameObject selectedTree = treesList[Random.Range(0, treesList.Count)];
            selectedTree.GetComponent<Tree>().StartBurning();

            // Show cause UI
            if (causeTexts.Length != 0)
            {
                causeUI.SetActive(true);
                causeUI.GetComponentInChildren<Text>().text = causeTexts[Random.Range(0, causeTexts.Length)];

                Vector2 selectedTreePos = Camera.main.WorldToScreenPoint(selectedTree.transform.position);
                causeUI.transform.position = selectedTreePos + new Vector2(0, 140);

                StartCoroutine(CloseCauseUI());
            }
        }
    }

    IEnumerator CloseCauseUI()
    {
        yield return new WaitForSeconds(displayTime);
        causeUI.SetActive(false);
    }
}
