using UnityEngine;
using System;
using System.Collections.Generic;

public class UnityThreadHelper : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    private void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public static void ExecuteOnMainThread(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}