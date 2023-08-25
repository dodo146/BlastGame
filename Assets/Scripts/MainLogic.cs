using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;
using System;

public class MainLogic : MonoBehaviour
{
    public static int current_level_number;
    public GameObject BuildingBlockPrefab;
    private Locks.Lock[] All_locks;
    private Tasks.Task[] All_tasks;
    private List<BuildingBlock> BuildingBlocks = new List<BuildingBlock>();
    public static Level[] Levels = new Level[10];
    [HideInInspector]
    public bool all_finished = false;
    private string dbName = "Level.db";
    private string tableName = "Levels";
    public static string connectionString;

    public struct BuildingBlock
    {
        public GameObject block;
        public int ID;
    }

    public Tasks.Task[] ALL_TASKS { get { return All_tasks; } }
    public List<BuildingBlock> BUILDING_BLOCKS { get { return BuildingBlocks; } }

    void GetLocks()
    {
        GameObject locksObject = GameObject.Find("Locks"); // Find the GameObject with the Locks script
        if (locksObject != null)
        {
            Locks locksComponent = locksObject.GetComponent<Locks>(); // Get the Locks component from the GameObject
            if (locksComponent != null)
            {
                // Now you can access the locks array through the Locks component
                All_locks = locksComponent.locks;
            }
        }
    }

    void GetTasks()
    {
        GameObject taskObject = GameObject.Find("Tasks");
        if (taskObject != null)
        {
            Tasks tasksComponent = taskObject.GetComponent<Tasks>(); // Get the Tasks component from the GameObject
            if (tasksComponent != null)
            {
                // Now you can access the tasks array through the Tasks component
                All_tasks = tasksComponent.tasks;
            }
        }
    }


    private void AllFinished()
    {
        int clear_count = 0;
        foreach (var level in Levels)
        {
            if (level.clear_status)
            {
                clear_count++;
            }
        }
        if (clear_count == Levels.Length)
        {
            all_finished = true;
        }
        else
        {
            all_finished = false;
        }
    }
    void Start()
    {
        connectionString = "URI=file:" + Application.dataPath + "/" + dbName;
        CreateTable();
        GetAllData(); //this gets all the data from the database and create the levels and assign the current level
        GetLocks();
        GetTasks();
        AllFinished();
        SetUp();
        CheckBuildStatus();

        //This part is to write the correct game level
            
        TextMeshProUGUI textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            // You've got access to the TextMeshPro component here
            if (all_finished)
            {
                textMeshPro.text = "Finished";
            }
            else
            {
                textMeshPro.text = $"Level {current_level_number}";
            }
        }
        else
        {
            Debug.LogWarning("TextMeshPro component not found on the button.");
        }
    }

    private void CreateTable()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} (level_number INT, grid_height INT, grid_width INT, move_count INT, clear_status BOOL, build_status BOOL)";
                dbCmd.CommandText = createTableQuery;
                dbCmd.ExecuteNonQuery();
            }
        }
    }
    private void GetAllData()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string selectQuery = $"SELECT * FROM {tableName}";
                dbCmd.CommandText = selectQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    int iter = 0 ;
                    while (reader.Read())
                    {
                        int level_number = reader.GetInt32(0);
                        int grid_h = reader.GetInt32(1);
                        int grid_w = reader.GetInt32(2);
                        int move_count = reader.GetInt32(3);
                        bool clear_state = reader.GetBoolean(4);
                        bool build_state = reader.GetBoolean(5);
                        Level level = new Level();
                        level.level_number = level_number;
                        level.grid_height = grid_h;
                        level.grid_width = grid_w;
                        level.move_count = move_count;
                        level.clear_status = clear_state;
                        level.build_status = build_state;
                        Levels[iter] = level;
                        iter++;
                    }
                }
            }
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string selectQuery = $"SELECT * FROM Current_Level";
                dbCmd.CommandText = selectQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    int iter = 0;
                    while (reader.Read())
                    {
                        current_level_number = reader.GetInt32(0);
                        iter++;
                    }
                }
            }
        }
    }

    private void CheckBuildStatus()
    {
        //Burda da build information txt den al bilgileri çünkü scene deðiþinde dictionaryler de sýfýrlanýyo

        foreach (Level level in Levels)
        {
            if (level.build_status)
            {
                int id = level.level_number;
                BuildingBlock buildBlock = BuildingBlocks[id-1];
                Tasks.Task specificTask = All_tasks[id - 1];
                buildBlock.block.SetActive(false);
                specificTask.task_obj.transform.position = buildBlock.block.transform.position;
                specificTask.task_obj.SetActive(true);
            }
        }
    }
    private void SetUp()
    {
        try
        {
            
            foreach (Level level in Levels)
            {
                if (level.clear_status)
                {
                    //deactivate that specific lock and instentiate a building block instead
                    All_locks[level.level_number-1].lock_obj.SetActive(false);
                    GameObject instantiatedPrefab = Instantiate(BuildingBlockPrefab, transform,false);

                    Transform parentTransform = All_locks[level.level_number - 1].lock_obj.transform;
                    Vector3 parentPosition = parentTransform.position;
                    Quaternion parentRotation = parentTransform.rotation;
                    Vector3 parentScale = parentTransform.localScale;

                    instantiatedPrefab.transform.position = parentPosition;
                    instantiatedPrefab.transform.rotation = parentRotation;
                    instantiatedPrefab.transform.localScale = parentScale;
                    instantiatedPrefab.name = "Building block";
                    BuildingBlock newBlock;
                    newBlock.block = instantiatedPrefab;
                    newBlock.ID = level.level_number;
                    BuildingBlocks.Add(newBlock);
                    instantiatedPrefab.GetComponent<ClickHandler>().SetPrefabIdentifier($"{level.level_number}");
                }
            }
            

        }
        catch (IOException e)
        {

            Debug.Log($"An error occured: {e.Message}");
        }
    }
}



[System.Serializable]
public class Level
{
    //Non gameplay
    public int level_number;
    public bool clear_status;
    public bool build_status;

    //gameplay
    public int grid_width;
    public int grid_height;
    public int move_count;

}
