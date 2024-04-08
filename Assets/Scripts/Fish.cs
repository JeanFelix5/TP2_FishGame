using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Fish : MonoBehaviour
{
    public Rigidbody2D FishRigidbody;

    private Thread fishThread;
    private bool isRunning = true;

    private float moveSpeed = 5f;


    // Start is called before the first frame update
    void Start()
    {
        fishThread = new Thread(FishThreadFunction);
        fishThread.Start();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    private void FishThreadFunction()
    {
        while (isRunning)
        {
            // Implémentation de la logique de déplacement du poisson
            // et de la détection de nourriture
            // ...

          //  Debug.Log("Hello: ");

            Thread.Sleep(300); // Attendre avant de recalculer le déplacement (miliseconds)
        }
    }


    //Called when the fish detect a collision 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        Debug.Log("We hit : " + collision.rigidbody.name);
    }
    
    

    private void OnDestroy()
    {
        isRunning = false;
        fishThread.Join(); // Attendre que le thread du poisson se termine
    }

   
}
