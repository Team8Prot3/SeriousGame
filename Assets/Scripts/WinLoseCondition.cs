using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseCondition : MonoBehaviour
{
    private List<GameObject> treesList;

    // Start is called before the first frame update
    void Start()
    {
        treesList = TreesController.instance.GetTreesList();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(treesList.Count);
    }

    void LoseCondition() 
    { 
        
    }
}
