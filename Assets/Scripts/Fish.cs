using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Fish : MonoBehaviour
{
    public Rigidbody2D FishRigidbody; // Reference to the fish rigidbody
    public float detectionRadius = 2f; // Radius of the detection area
    public float moveSpeed = 2f; 

    private Thread fishThread;
    private bool isRunning = true;

    private bool foodFishNearby = false;
    private object lockObject = new object(); // Object to lock access to shared data
    private Vector2 foodFishDirection;



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
            //Debug.Log("FoodFish is nearby!"); //food fish detected
        }
    }

    private void FishThreadFunction()
    {
        while (isRunning)
        {
            // Check for objects nearby on the Unity main thread
            UnityThreadHelper.ExecuteOnMainThread(() =>
            {
                Vector2 foodFishDirection;
                bool isNearby = CheckFoodFishNearby(out foodFishDirection);
                lock (lockObject)
                {
                    foodFishNearby = isNearby;
                }

                // Store the direction towards the food fish
                lock (lockObject)
                {
                    this.foodFishDirection = foodFishDirection;
                }
            });

            //Move towards the food fish
            if(foodFishNearby == true)
            {
                // Get the stored direction towards the food fish
                Vector2 direction;
                lock (lockObject)
                {
                    direction = foodFishDirection;
                }

                // Set the velocity of the fish towards the food fish on the main thread because I cannot set the velocity otherwise
                UnityThreadHelper.ExecuteOnMainThread(() =>
                {
                    // Move the fish in that direction with the specified move speed
                    FishRigidbody.velocity = direction.normalized * moveSpeed;
                });
            }
            else
            {
                //Stop the fish if there is no food fish detected 

            }

            Thread.Sleep(500); // Wait before recalculating the detection (in milliseconds)
        }
    }

    private bool CheckFoodFishNearby(out Vector2 foodFishDirection)
    {
        foodFishDirection = Vector2.zero;

        // Get all colliders within the detection radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        // Check if any colliders are found (excluding self)
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.gameObject.tag == "FishFood") // Exclude self and check tag
            {
                //Debug.Log("FoodFish is nearby!"); //food fish detected
                foodFishDirection = (collider.transform.position - transform.position).normalized;
                return true; // Return true as soon as one object is found
            }
        }

        return false; // Return false if no objects are found
    }


    //Called when the fish detect a collision 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("We hit : " + collision.rigidbody.name);

        if(collision.gameObject.tag == "FishFood")
        {
            Debug.Log("FoodFish is being destroyed!"); //food fish destroy
            Destroy(collision.gameObject);
        }

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
