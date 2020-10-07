using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { healthy, unhealthy, burning };
public enum Risk { low, middle, high, burning };
public class Tree : MonoBehaviour
{
    
    private State state;
    private State preState;

    [Header("Health Settings")]
    [Range(0, 100)]
    public int mutationProbability = 10;    // 0% ~ 100%
    public float mutationIntervalTime = 2;
    private float mutationTimer;

    [Header("Burning Settings")]
    public float burningTime;
    public float fireSpreadTimeHealthy;
    public float fireSpreadTimeUnhealthy;

    private float burningTimer;
    private float fireSpreadTimer;

    // Start is called before the first frame update
    void Start()
    {
        state = State.healthy;
        preState = State.healthy;
        mutationTimer = mutationIntervalTime;
    }

    // Update is called once per frame
    void Update()
    {
        // If mutationTimer is 0, randomly become unhealthy & reset timer
        if (state == State.healthy) 
        {
            mutationTimer -= Time.deltaTime;
            if (mutationTimer <= 0) 
            {
                mutationTimer = mutationIntervalTime;
                int rnum = Random.Range(0, 100);
                if (rnum < mutationProbability)
                    BecomeUnhealthy();
            }
        }

        else if (state == State.burning)
        {
            // If burningTimer is 0, destroy the tree
            burningTimer -= Time.deltaTime;
            if (burningTimer <= 0)
            {
                Destroy(gameObject);
                TreesController.instance.RemoveTree(gameObject);
            }

            // If fireSpreadTimer is 0, spread the fire
            fireSpreadTimer -= Time.deltaTime;
            if (fireSpreadTimer <= 0)
            {
                fireSpreadTimer = fireSpreadTimeHealthy;
                TreesController.instance.SpreadFire(transform.position);
            }

        }
    }

    private void ChangeStateTo(State s) 
    {
        preState = state;
        state = s;
    }

    public void StartBurning() 
    {
        if (state == State.burning)
            return;
        ChangeStateTo(State.burning);
        burningTimer = burningTime;
        fireSpreadTimer = fireSpreadTimeHealthy;
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    public void StopBurning()
    {
        if (state == State.burning) {
            ChangeStateTo(preState);
            if(state == State.healthy)
                GetComponent<SpriteRenderer>().color = Color.white;
            else
                GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.gray, 0.5f);
        }
    }

    public void BecomeUnhealthy() 
    {
        ChangeStateTo(State.unhealthy);
        GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.gray, 0.5f);
    }

    public void OnMouseDown()
    {
        if (TreesController.instance && TreesController.instance.isCutting)
        {
            Destroy(gameObject);
            TreesController.instance.RemoveTree(gameObject);
        }
    }

    public State GetState() {
        return state;
    }

}
