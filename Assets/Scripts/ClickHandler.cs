using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MainLogic;
using Mono.Data.Sqlite;
using System.Data;

public class ClickHandler : MonoBehaviour
{
    private int prefabIdentifier = 0;

    public int Prefab 
    {
        get
        { return prefabIdentifier; }
    }

    public void SetPrefabIdentifier(string identifier)
    {
        prefabIdentifier = int.Parse(identifier);
    }

    public void ButtonClick()
    {
        //Debug.Log("Button in " + prefabIdentifier + " prefab was pressed.");
        // Add your button press logic here
        Transform parentTransform = transform.parent;
        Canvas parentComponent = parentTransform.GetComponent<Canvas>();
        MainLogic mainLogic = parentComponent.GetComponent<MainLogic>();
        if (mainLogic != null)
        {
            Tasks.Task specificTask = mainLogic.ALL_TASKS[prefabIdentifier - 1];
            MainLogic.BuildingBlock specificBuildingBlock = mainLogic.BUILDING_BLOCKS[prefabIdentifier - 1];
            specificBuildingBlock.block.SetActive(false);
            specificTask.task_obj.transform.position = specificBuildingBlock.block.transform.position;
            specificTask.task_obj.SetActive(true);
            MainLogic.Levels[prefabIdentifier - 1].build_status = true;
            using (IDbConnection dbConnection = new SqliteConnection(MainLogic.connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    int updatedLevelNumber = MainLogic.Levels[prefabIdentifier - 1].level_number; // The level number you want to update
                    bool newBuildStatus = true; // The new value for clear_status column

                    string updateQuery = $"UPDATE Levels SET build_status = {newBuildStatus} WHERE level_number = {updatedLevelNumber}";
                    dbCmd.CommandText = updateQuery;
                    dbCmd.ExecuteNonQuery();
                }
            }

        }
        else
        {
            Debug.Log("There was an error regarding getting the Mainlogic script component in the canvas");
        }
    }
}
