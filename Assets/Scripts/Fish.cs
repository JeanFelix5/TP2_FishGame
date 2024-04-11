using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Fish : MonoBehaviour
{
    public Rigidbody2D FishRigidbody; // Reference to the fish rigidbody
    public float detectionRadius = 2f; // Radius of the detection area
    private float moveSpeed = 5f;

    private Thread fishThread;
    private bool isRunning = true;

    private bool foodFishNearby = false;
    private object lockObject = new object(); // Object to lock access to shared data



    // Start is called before the first frame update
    void Start()
    {
        fishThread = new Thread(FishThreadFunction);
        fishThread.Start();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool nearby;
        lock (lockObject)
        {
            nearby = foodFishNearby;
        }
        if (nearby)
        {
            Debug.Log("FoodFish is nearby!");
            // Implement logic to move towards the foodFish or perform some action
        }
    }

    private void FishThreadFunction()
    {
        while (isRunning)
        {
            // Check for objects nearby on the Unity main thread
            UnityThreadHelper.ExecuteOnMainThread(() =>
            {
                bool isNearby = CheckFoodFishNearby();
                lock (lockObject)
                {
                    foodFishNearby = isNearby;
                }
            });


            Thread.Sleep(300); // Wait before recalculating the detection
        }
    }

    private bool CheckFoodFishNearby()
    {
        // Get all colliders within the detection radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        // Check if any colliders are found (excluding self)
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject) // Exclude self
            {
                // Perform logic based on the detected object
                return true; // Return true as soon as one object is found
            }
        }

        return false; // Return false if no objects are found
    }


    //Called when the fish detect a collision 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        Debug.Log("We hit : " + collision.rigidbody.name);
    }

    // Draw gizmos in the scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void OnDestroy()
    {
        isRunning = false;
        fishThread.Join(); // Attendre que le thread du poisson se termine
    }
}
