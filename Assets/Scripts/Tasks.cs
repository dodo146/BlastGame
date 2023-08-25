using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tasks: MonoBehaviour
{
    [System.Serializable]
    public struct Task
    {
        public int task_number;
        public GameObject task_obj;
    }

    public Task[] tasks = new Task[10];
}
