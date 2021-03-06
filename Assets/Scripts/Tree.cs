﻿using System.Collections;
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
    public Sprite unhealthySprite;

    [Header("Burning Settings")]
    public float burningTime;
    public float fireSpreadTimeHealthy;
    public float fireSpreadTimeUnhealthy;
    public float fireSpreadRadiusHealthy;
    public float fireSpreadRadiusUnhealthy;
    public AudioClip fireAudio;

    private float burningTimer;
    private float fireSpreadTimer;

    AudioSource audioSource;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        state = State.healthy;
        preState = State.healthy;
        mutationTimer = mutationIntervalTime;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
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
                TreesController.instance.AddBurnedTreeLocation(gameObject.transform.position);
                Destroy(gameObject);
                TreesController.instance.RemoveTree(gameObject);
            }

            // If fireSpreadTimer is 0, spread the fire
            fireSpreadTimer -= Time.deltaTime;
            if (fireSpreadTimer <= 0)
            {
                if (preState == State.healthy)
                {
                    fireSpreadTimer = fireSpreadTimeHealthy;
                    TreesController.instance.SpreadFire(transform.position, fireSpreadRadiusHealthy);
                }
                else
                {
                    fireSpreadTimer = fireSpreadTimeUnhealthy;
                    TreesController.instance.SpreadFire(transform.position, fireSpreadRadiusUnhealthy);
                }
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
        animator.SetBool("isBurning", true);
        audioSource.PlayOneShot(fireAudio, 0.7F);
        GetComponent<SpriteRenderer>().sortingOrder += 1;

        burningTimer = burningTime;
        if(preState == State.healthy)
            fireSpreadTimer = fireSpreadTimeHealthy;
        else
            fireSpreadTimer = fireSpreadTimeUnhealthy;
    }

    public void StopBurning()
    {
        if (state == State.burning) {
            ChangeStateTo(preState);
            animator.SetBool("isBurning", false);
            GetComponent<SpriteRenderer>().sortingOrder -= 1;
        }
    }

    public void BecomeUnhealthy() 
    {
        ChangeStateTo(State.unhealthy);
        animator.SetBool("isUnhealthy", true);
        GetComponent<SpriteRenderer>().sprite = unhealthySprite;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (state == State.healthy || (state == State.burning && preState == State.healthy))
            Gizmos.DrawWireSphere(transform.position, fireSpreadRadiusHealthy);
        else
            Gizmos.DrawWireSphere(transform.position, fireSpreadRadiusUnhealthy);
    }
}
