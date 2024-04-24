using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnFishFoodOnClick : MonoBehaviour
{
    public GameObject LeftClickBlackFishFoodToSpawn;
    public GameObject RightClickWhiteFishFoodToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Get the position of the mouse click in the world
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition.z = 0; // Ensure the object is spawned at z=0 (or adjust as needed)

            // Spawn the prefab at the mouse click position
            Instantiate(LeftClickBlackFishFoodToSpawn, clickPosition, Quaternion.identity);
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                // Get the position of the mouse click in the world
                Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                clickPosition.z = 0; // Ensure the object is spawned at z=0 (or adjust as needed)

                // Spawn the prefab at the mouse click position
                Instantiate(RightClickWhiteFishFoodToSpawn, clickPosition, Quaternion.identity);
            }
        }
    }
}
