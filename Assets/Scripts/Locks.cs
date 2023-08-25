using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locks: MonoBehaviour 
{
    [System.Serializable]
    public struct Lock
    {
        public int lock_number;
        public GameObject lock_obj;
    }

    public Lock[] locks = new Lock[10]; 

}
