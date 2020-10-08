using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseCondition : MonoBehaviour
{
    private List<GameObject> treesList;

    public GameObject WinPanel;
    public GameObject LosePanel;

    // Start is called before the first frame update
    void Start()
    {
        treesList = TreesController.instance.GetTreesList();
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        WinCondition();
        LoseCondition();

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    void WinCondition() 
    {
        if (treesList.Count >= 40)
        {
            WinPanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    void LoseCondition() 
    {
        if (treesList.Count <= 0) 
        {
            LosePanel.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void Restart() 
    {
        SceneManager.LoadScene("Game");
    }
}
