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

    private bool FishFoodNearby = false;
    private object lockObject = new object(); // Object to lock access to shared data
    private Vector2 foodFishDirection;
    private float ChangeDirectionProbability = 0f;


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
            nearby = FishFoodNearby;
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
            float rotationAngle = 0f;
            
            UnityThreadHelper.ExecuteOnMainThread(() =>
            {
                lock (lockObject)
                {
                    ChangeDirectionProbability = Random.Range(0f, 1f); // Determine whether the fish should change direction
                }
            });

            // Check for objects nearby on the Unity main thread
            UnityThreadHelper.ExecuteOnMainThread(() =>
            {
                Vector2 foodFishDirection;
                bool isNearby = CheckFoodFishNearby(out foodFishDirection);
                lock (lockObject)
                {
                    FishFoodNearby = isNearby;
                }

                // Store the direction towards the food fish
                lock (lockObject)
                {
                    this.foodFishDirection = foodFishDirection;

                    // Calculate the angle between the fish's top corner (facing up) and the direction towards the food
                    rotationAngle = Mathf.Atan2(foodFishDirection.y, foodFishDirection.x) * Mathf.Rad2Deg;
                }
            });

            // Occasionally change direction randomly
            if (ChangeDirectionProbability < 0.1f && FishFoodNearby == true)
            {
                //Debug.Log("Change direction!! shouldChangeDirection: " + ChangeDirectionProbability); // Log the value of shouldChangeDirection

                // Randomly choose a new direction within the main thread
                Vector2 randomDirection = Vector2.zero;
                UnityThreadHelper.ExecuteOnMainThread(() =>
                {
                    lock (lockObject)
                    {
                        randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                    }
                });

                // Set the velocity of the fish towards the random direction on the main thread
                UnityThreadHelper.ExecuteOnMainThread(() =>
                {
                    FishRigidbody.velocity = randomDirection * moveSpeed;

                    // Adjust rotation to face the new direction
                    float randomRotationAngle = Mathf.Atan2(randomDirection.y, randomDirection.x) * Mathf.Rad2Deg;
                    FishRigidbody.rotation = randomRotationAngle - 90f; // Adjusting by -90 degrees to make the top corner face the direction

                    // Stop rotation 
                    FishRigidbody.angularVelocity = 0f;
                });
            }
            else
            {
                //Move towards the fish food
                if (FishFoodNearby == true)
                {
                    // Get the stored direction towards the food fish
                    Vector2 direction;
                    lock (lockObject)
                    {
                        direction = foodFishDirection;
                    }

                    // Set the velocity of the fish towards the fish food on the main thread
                    UnityThreadHelper.ExecuteOnMainThread(() =>
                    {
                        // Move the fish in that direction with the specified move speed
                        FishRigidbody.velocity = direction.normalized * moveSpeed;

                        if (rotationAngle != 0.0f) // Only set the rotation towards the food if the rotation angle is not 0 or null
                        {
                            // Adjust rotation to make the top corner face the food
                            FishRigidbody.rotation = rotationAngle - 90f; // Adjusting by -90 degrees to make the top corner face the food
                        }
                    });
                }
                else
                {
                    // Stop the fish if there is no food fish detected 
                    UnityThreadHelper.ExecuteOnMainThread(() =>
                    {
                        // Stop the fish by setting its velocity to zero
                        FishRigidbody.velocity = Vector2.zero;

                        // Stop rotation 
                        FishRigidbody.angularVelocity = 0f;
                    });
                }
            }

            Thread.Sleep(400); // Wait before recalculating the detection (in milliseconds)
        }
    }

    private bool CheckFoodFishNearby(out Vector2 foodFishDirection)
    {
        foodFishDirection = Vector2.zero;

        // Get all colliders within the detection radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        // Variables to store the closest distance and direction
        float closestDistance = float.MaxValue;
        Vector2 closestDirection = Vector2.zero;

        // Iterate through all colliders
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                // Check if the detected fish food and the fish both have the same tag
                if (collider.CompareTag(this.tag) && collider.gameObject.GetComponent<FishFood>() != null) 
                {
                    // Calculate direction to the food fish
                    Vector2 directionToFood = (collider.transform.position - transform.position).normalized;

                    // Calculate distance to the food fish
                    float distanceToFood = Vector2.Distance(transform.position, collider.transform.position);

                    // Update closest direction if this food fish is closer
                    if (distanceToFood < closestDistance)
                    {
                        closestDistance = distanceToFood;
                        closestDirection = directionToFood;
                    }
                }
            }
        }

        // If a food fish was found, return its direction
        if (closestDistance < float.MaxValue)
        {
            foodFishDirection = closestDirection;
            return true;
        }

        // Return false if no food fish are found
        return false;
    }


    //Called when the fish detect a collision 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("We hit : " + collision.rigidbody.name);

        // Check if the object that received a collision is not self and that the gameObject implement the FishFood script
        if(collision.gameObject.GetComponent<FishFood>() != null && collision.gameObject != gameObject)
        {
            // Check if the object that received the collision and the current fish both implement the same tag by comparing them
            if (collision.gameObject.CompareTag(this.tag)) 
            {
                lock (lockObject)   // Only one fish at a time can eat the food, prevent errors if two fish try to eat the food at the same time
                {
                    // Destroy the collided FoodFish immediately 
                    Destroy(collision.gameObject);
                }

                // Stop rotation after the collision
                FishRigidbody.angularVelocity = 0f;

                //Debug.Log("FoodFish is being destroyed!"); // food fish destroy
            }
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
