using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

public class LevelOnClick : MonoBehaviour
{
    public void LevelClicked()
    {
        GameObject canvas = GameObject.Find("Canvas");
        MainLogic mainScript = canvas.GetComponent<MainLogic>();
        if (!mainScript.all_finished)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
