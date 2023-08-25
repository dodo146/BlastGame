using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using static UnityEditor.Progress;
using Unity.Mathematics;
using System.Data;
using Mono.Data.Sqlite;

public class GameLogic : MonoBehaviour
{
    private Level current_level;
    private Dictionary<Tile, int> tile_count = new Dictionary<Tile, int>();
    public TextMeshProUGUI move_count_text;
    public GameObject prefabBoxImage;
    public GameObject prefabStoneImage;
    public GameObject prefabVaseImage;
    public GameObject prefabTNT;
    public TextMeshProUGUI Box_text;
    public TextMeshProUGUI Stone_text;
    public TextMeshProUGUI Vase_text;
    public GameObject CelebrationText;
    public GameObject Star;
    private List<List<Tile>> tile_groups = new List<List<Tile>>();
    [SerializeField]
    public List<Tile> tiles;
    private bool isCheckingTnt = false; // Flag to control coroutine execution
    [HideInInspector]
    public bool BoardCleaning = false;


    // Start is called before the first frame update
    private void Awake()
    {
        current_level = MainLogic.Levels[MainLogic.current_level_number - 1];

    }
    void Start()
    {
        Board.Instance.GenerateGrid();
        //instentiate the prefab images for obstacles
        GameObject box_container = GameObject.Find("Box Count");
        GameObject stone_container = GameObject.Find("Stone Count");
        GameObject vase_container = GameObject.Find("Vase Count");
        SpawnObstacles(box_container,stone_container,vase_container);
        for (int i = 0; i < tiles.Count; i++)
        {
            tile_count.Add(tiles[i], 0);
        }
        //Write the move count and obstacle count to the scoreboard
        WriteObstacles();
        StartCoroutine(CheckTntRegularly());
    }

    private void CleanBoard()
    {
        for (int y = 0; y < Board.Instance.grid_height; y++)
        {
            for (int x = 0; x < Board.Instance.grid_width; x++)
            {
                if (Board.Instance.board[y,x] != null)
                {
                    Destroy(Board.Instance.board[y, x].tile);
                }
            }
        }
        BoardCleaning = true;
    }
    private void SpawnObstacles(GameObject _box_container,GameObject _stone_container,GameObject _vase_container)
    {
        if(Board.Instance.box_Count != 0) Instantiate(prefabBoxImage, _box_container.transform, false);
        else
        {
            GameObject gameObject = GameObject.Find("Box Count Text");
            gameObject.SetActive(false);
        }
        if(Board.Instance.Stone_Count != 0) Instantiate(prefabStoneImage, _stone_container.transform, false);
        else
        {
            GameObject gameObject = GameObject.Find("Stone Count Text");
            gameObject.SetActive(false);
        }
        if (Board.Instance.Vase_Count != 0) Instantiate(prefabVaseImage, _vase_container.transform, false);
        else
        {
            GameObject gameObject = GameObject.Find("Vase Count Text");
            gameObject.SetActive(false);
        }
    }

    private IEnumerator CheckTntRegularly()
    {
        while (true) // Keep running the loop indefinitely
        {
            //Debug.LogWarning("Inside the coroutine");
            if (!isCheckingTnt)
            {
                //Debug.LogWarning("Checking TNT spawn");
                isCheckingTnt = true;
                CheckTntSpawn();
                isCheckingTnt = false;
                //Debug.LogWarning("Finished Checking waiting for one second");
            }

            // Wait for the next frame before running the loop again
            yield return new WaitForSeconds(0.2f);
            //yield return null;
        }
    }

    private void CheckWinLoseConditions()
    {   
        TextMeshProUGUI end_screen_text = CelebrationText.GetComponent<TextMeshProUGUI>();
        if (Board.Instance.move_count <= 0)
        {
            if (Board.Instance.box_Count > 0 || Board.Instance.Stone_Count > 0 || Board.Instance.Vase_Count > 0)
            {
                // Lose condition: No moves left and there are obstacles remaining
                Debug.LogWarning("You Lose!");
                StopAllCoroutines();
                Board.Instance.StopAllCoroutines();
                CleanBoard();
                CelebrationText.SetActive(true);
                end_screen_text.text = "You lose!Try again next time";
            }
            else
            {
                // Win condition: No moves left and all obstacles cleared
                Debug.LogWarning("You Win!");
                StopAllCoroutines();
                Board.Instance.StopAllCoroutines();
                CleanBoard();
                CelebrationText.SetActive(true);
                end_screen_text.text = "Perfect!";
                Star.SetActive(true);
                current_level.clear_status = true;
                // update the level's information
                using (IDbConnection dbConnection = new SqliteConnection(MainLogic.connectionString))
                {
                    dbConnection.Open();

                    using (IDbCommand dbCmd = dbConnection.CreateCommand())
                    {
                        int updatedLevelNumber = current_level.level_number; // The level number you want to update
                        bool newClearStatus = true; // The new value for clear_status column

                        string updateQuery = $"UPDATE Levels SET clear_status = {newClearStatus} WHERE level_number = {updatedLevelNumber}";
                        dbCmd.CommandText = updateQuery;
                        dbCmd.ExecuteNonQuery();
                    }
                }
                using (IDbConnection dbConnection = new SqliteConnection(MainLogic.connectionString))
                {
                    dbConnection.Open();

                    using (IDbCommand dbCmd = dbConnection.CreateCommand())
                    {
                        int updatedLevelNumber = current_level.level_number; // The level number you want to update
                        string updateQuery = $"UPDATE Current_Level SET current_level = {current_level.level_number + 1}";
                        dbCmd.CommandText = updateQuery;
                        dbCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        else if((Board.Instance.box_Count == 0 && Board.Instance.Stone_Count == 0 && Board.Instance.Vase_Count == 0))
        {
            //daha move var ama engeller yok
            //You win
            Debug.LogWarning("You Win!");
            StopAllCoroutines();
            Board.Instance.StopAllCoroutines();
            CleanBoard();
            CelebrationText.SetActive(true);
            end_screen_text.text = "Perfect!";
            Star.SetActive(true);
            current_level.clear_status = true;
            // update the level's information
            using (IDbConnection dbConnection = new SqliteConnection(MainLogic.connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    int updatedLevelNumber = current_level.level_number; // The level number you want to update
                    bool newClearStatus = true; // The new value for clear_status column

                    string updateQuery = $"UPDATE Levels SET clear_status = {newClearStatus} WHERE level_number = {updatedLevelNumber}";
                    dbCmd.CommandText = updateQuery;
                    dbCmd.ExecuteNonQuery();
                }
            }
            using (IDbConnection dbConnection = new SqliteConnection(MainLogic.connectionString))
            {
                dbConnection.Open();
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    int updatedLevelNumber = current_level.level_number; // The level number you want to update
                    string updateQuery = $"UPDATE Current_Level SET current_level = {current_level.level_number + 1} ";
                    dbCmd.CommandText = updateQuery;
                    dbCmd.ExecuteNonQuery();
                }
            }
        }
    }

    private void WriteObstacles()
    {
        move_count_text.text = Board.Instance.move_count.ToString();
        var keys = tile_count.Keys.ToList();

        foreach (var key in keys)
        {
            switch (key.first_letter)
            {
                case "bo":
                    tile_count[key] = Board.Instance.box_Count;
                    Box_text.text = tile_count[key].ToString();
                    break;
                case "s":
                    tile_count[key] = Board.Instance.Stone_Count;
                    Stone_text.text = tile_count[key].ToString();
                    break;
                case "v":
                    tile_count[key] = Board.Instance.Vase_Count;
                    Vase_text.text = tile_count[key].ToString();
                    break;
            }
        }

    }

    public static  void GetLevelInformation()
    {
        //Burda her level bilgisi için gridin height ve widthini assignlýyacan
        Level level = MainLogic.Levels[MainLogic.current_level_number - 1];
        Board.Instance.grid_height = level.grid_height;
        Board.Instance.grid_width = level.grid_width;
        Board.Instance.move_count = level.move_count;
    }

    public void ClickTile(Tile tile)
    {
        //Debug.LogWarning($"Clicked the tile ({tile.y},{tile.x})");
        List<Tile> tntsCaughInRadius = new List<Tile>();
        if (tile.first_letter == "bo" || tile.first_letter == "s" || tile.first_letter == "v")
        {
            //bu bir engel bi þey olmayacak
        }
        else
        {
            if (tile.status == "TNT") //clicked a tnt
            {
                List<Tile> surr_tiles_to_explode = Board.Instance.GetTilesAroundATNT(tile);
                for (int i = 0; i < surr_tiles_to_explode.Count; i++)
                {
    
                    switch (surr_tiles_to_explode[i].first_letter)
                    {
                        case "bo":
                            Board.Instance.box_Count--;
                            break;
                        case "s":
                            Board.Instance.Stone_Count--;
                            break;
                        case "v":
                            break;
                        case "v2":
                            Board.Instance.Vase_Count--;
                            break;
                    }
                    // explode the surronding tiles minus the original tile
                    if (surr_tiles_to_explode[i].status == "TNT") // we found another tnt in the explosion radius of the current tnt
                    {
                        if (surr_tiles_to_explode[i] != tile) tntsCaughInRadius.Add(surr_tiles_to_explode[i]);
                        else
                        {
                            if (surr_tiles_to_explode[i].first_letter == "v")
                            {
                                //normal vase.Make it broken
                                GameObject broken_vase = tiles[11].tile;
                                Image broken_vase_image = broken_vase.GetComponent<Image>();
                                Board.Instance.board[tile.y, tile.x].tile.GetComponent<Image>().sprite = broken_vase_image.sprite;
                                Board.Instance.board[tile.y, tile.x].first_letter = "v2";
                                Board.Instance.board[tile.y, tile.x].status = "broken vase";
                            }
                            else
                            {
                                Board.Instance.board[surr_tiles_to_explode[i].y, surr_tiles_to_explode[i].x] = null;
                                Destroy(surr_tiles_to_explode[i].tile);
                            }
                        }
                    }
                    else
                    {
                        if (surr_tiles_to_explode[i].first_letter == "v")
                        {
                            //normal vase.Make it broken
                            GameObject broken_vase = tiles[11].tile;
                            Image broken_vase_image = broken_vase.GetComponent<Image>();
                            Board.Instance.board[surr_tiles_to_explode[i].y, surr_tiles_to_explode[i].x].tile.GetComponent<Image>().sprite = broken_vase_image.sprite;
                            Board.Instance.board[surr_tiles_to_explode[i].y, surr_tiles_to_explode[i].x].first_letter = "v2";
                            Board.Instance.board[surr_tiles_to_explode[i].y, surr_tiles_to_explode[i].x].status = "broken vase";
                        }
                        else
                        {
                            Board.Instance.board[surr_tiles_to_explode[i].y, surr_tiles_to_explode[i].x] = null;
                            Destroy(surr_tiles_to_explode[i].tile);
                        }
                    }
                }
                Board.Instance.move_count--;
                Board.Instance.ApplyFallMechanic();
                WriteObstacles();
                ConvertNonTntTiles();
                CheckWinLoseConditions();
            }
            else
            {
                List<Tile> tile_chain = tile.CheckTnt();
                tile_chain.Sort(new TileComparer());
                if (tile_chain.Count >= 5) // a tnt cube chain
                {
                    for (int i = 0; i < tile_chain.Count; i++) //Destroy all tnt tiles except the tile that has been clicked. Change the image to tnt for that tile.
                    {
                        if (tile_chain[i] == tile)
                        {
                            Image tnt_image = prefabTNT.GetComponent<Image>();
                            Board.Instance.board[tile.y, tile.x].tile.GetComponent<Image>().sprite = tnt_image.sprite;
                            Board.Instance.board[tile.y, tile.x].first_letter = "TNT";
                            Board.Instance.board[tile.y, tile.x].status = "TNT";
                            tile_chain[i].tile = Board.Instance.board[tile.y, tile.x].tile;
                        }
                        else
                        {
                            Board.Instance.board[tile_chain[i].y, tile_chain[i].x] = null;
                            Destroy(tile_chain[i].tile);
                        }
                    }
                    tile_groups.Remove(tile_chain);
                    Board.Instance.move_count--;
                    Board.Instance.ApplyFallMechanic();
                    WriteObstacles();
                    ConvertNonTntTiles();
                    CheckWinLoseConditions();
                }
                else if (tile_chain.Count >= 2) // a normal chain
                {
                    bool vase_cracked = false;
                    foreach (var exploding_tile in tile_chain)
                    {
                        List<Tile> tilesToExplode = Board.Instance.GetTilesAroundATile(exploding_tile);

                        for (int i = 0; i < tilesToExplode.Count; i++)
                        {
                            if (tilesToExplode[i].status == "TNT")
                            {
                                tntsCaughInRadius.Add(tilesToExplode[i]);
                                continue;
                            }
                            // explode the surronding tiles minus the original tile
                            if (tile_chain.Contains(tilesToExplode[i]))
                            {
                                continue;
                            }
                            else
                            {
                                switch (tilesToExplode[i].first_letter)
                                {
                                    case "bo":
                                        Board.Instance.box_Count--;
                                        break;
                                    case "v2":
                                        Board.Instance.Vase_Count--;
                                        break;
                                }
                                //Check vase status
                                if (tilesToExplode[i].first_letter == "v")
                                {
                                    //normal vase.Make it broken
                                    GameObject broken_vase = tiles[11].tile;
                                    Image broken_vase_image = broken_vase.GetComponent<Image>();
                                    Board.Instance.board[tilesToExplode[i].y, tilesToExplode[i].x].tile.GetComponent<Image>().sprite = broken_vase_image.sprite;
                                    Board.Instance.board[tilesToExplode[i].y, tilesToExplode[i].x].first_letter = "v2";
                                    Board.Instance.board[tilesToExplode[i].y, tilesToExplode[i].x].status = "broken vase";
                                    vase_cracked = true;
                                }
                                else
                                {
                                    if (tilesToExplode[i].first_letter != "s")
                                    {
                                        if (vase_cracked && tilesToExplode[i].first_letter == "v2")
                                        {
                                            //vase is cracked beforehand no need to destroy it
                                        }
                                        else
                                        {
                                            Board.Instance.board[tilesToExplode[i].y, tilesToExplode[i].x] = null;
                                            Destroy(tilesToExplode[i].tile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //now explode the main four tile in tile chain
                    for (int i = 0; i < tile_chain.Count; i++)
                    {
                        Board.Instance.board[tile_chain[i].y, tile_chain[i].x] = null;
                        Destroy(tile_chain[i].tile);
                    }
                    Board.Instance.move_count--;
                    Board.Instance.ApplyFallMechanic();
                    WriteObstacles();
                    ConvertNonTntTiles();
                    CheckWinLoseConditions();
                }
            }
        }

        // after the fall mechanics activated, detonate the tnts that has been caught in an explosin radius
        if (tntsCaughInRadius.Count != 0)
        {
            StartCoroutine(TntCoroutine(tntsCaughInRadius));
        }

    }
    IEnumerator TntCoroutine(List<Tile> tntsCaughInRadius)
    {
        if (BoardCleaning)
        {
            StopCoroutine(TntCoroutine(tntsCaughInRadius));
        }
        else
        {
            yield return new WaitForSeconds(1f);
            Tile tnt_tile = tntsCaughInRadius[0];
            tntsCaughInRadius.RemoveAt(0);
            List<Tile> surr_tiles_to_explode = Board.Instance.GetTilesAroundATNT(tnt_tile);
            for (int i = 0; i < surr_tiles_to_explode.Count; i++)
            {
                switch (surr_tiles_to_explode[i].first_letter)
                {
                    case "bo":
                        Board.Instance.box_Count--;
                        break;
                    case "s":
                        Board.Instance.Stone_Count--;
                        break;
                    case "v":
                        Board.Instance.Vase_Count--;
                        break;
                }
                // explode the surronding tiles minus the original tile
                Board.Instance.board[surr_tiles_to_explode[i].y, surr_tiles_to_explode[i].x] = null;
                Destroy(surr_tiles_to_explode[i].tile);
            }
            WriteObstacles();
            Board.Instance.ApplyFallMechanic();
            ConvertNonTntTiles();
            CheckWinLoseConditions();
            StopCoroutine(TntCoroutine(tntsCaughInRadius));
        }
    }

    void CheckTntSpawn()
    {
        for (int y = 0; y < Board.Instance.grid_height; y++)
        {
            for (int x = 0; x < Board.Instance.grid_width; x++)
            {
                if (Board.Instance.board[y, x] == null) { continue; }
                if (Board.Instance.board[y, x].first_letter == "bo" || Board.Instance.board[y, x].first_letter == "v" || Board.Instance.board[y, x].first_letter == "s")
                {

                    continue;
                }
                else
                {
                    //burda gerekli taþlarý tntye çevir
                    List<Tile> tnt_group = Board.Instance.board[y, x].CheckTnt();
                    tnt_group.Sort(new TileComparer());
                    bool same_list = false;
                    if (tnt_group.Count >= 5) // tnt grubu olacak küpler
                    {
                        foreach (var tnt_list in tile_groups)
                        {
                            if (TileComparer.AreTilesEqual(tnt_list,tnt_group))
                            {
                                same_list = true;
                                break;
                            }
                        }
                        if (same_list) continue;
                        else
                        {
                            for (int i = 0; i < tnt_group.Count; i++)
                            {
                                int curr_x = tnt_group[i].x;
                                int curr_y = tnt_group[i].y;
                                GameObject tnt_spawn = null;
                                switch (tnt_group[i].first_letter)
                                {
                                    case "r":
                                        tnt_spawn = tiles[7].tile;
                                        break;
                                    case "b":
                                        tnt_spawn = tiles[8].tile;
                                        break;
                                    case "g":
                                        tnt_spawn = tiles[10].tile;
                                        break;
                                    case "y":
                                        tnt_spawn = tiles[9].tile;
                                        break;
                                }
                                Image tnt_image = tnt_spawn.GetComponent<Image>();
                                Board.Instance.board[curr_y, curr_x].tile.GetComponent<Image>().sprite = tnt_image.sprite;
                                Board.Instance.board[curr_y, curr_x].status = "tnt";
                                tnt_group[i].tile = Board.Instance.board[curr_y, curr_x].tile;
                            }
                            tile_groups.Add(tnt_group);
                        }                      
                    }                       
                    
                }
            }
        }
    }

    void ConvertNonTntTiles()
    {
        List<List<Tile>> groups = new List<List<Tile>>();
        for (int y = 0; y < Board.Instance.grid_height; y++)
        {
            for (int x = 0; x < Board.Instance.grid_width; x++)
            {
                if (Board.Instance.board[y, x] == null) { continue; }
                if (Board.Instance.board[y, x].first_letter == "bo" || Board.Instance.board[y, x].first_letter == "v" || Board.Instance.board[y, x].first_letter == "s") continue;
                else
                {
                    if (Board.Instance.board[y,x].status == "tnt")
                    {
                        List<Tile> result = Board.Instance.board[y,x].CheckTnt();
                        result.Sort(new TileComparer());
                        if (result.Count < 5)
                        {
                            //not a tnt cube anymore
                            bool same = false;
                            foreach (var list in groups)
                            {
                                if (TileComparer.AreTilesEqual(list, result))
                                {
                                    same = true;
                                    break;
                                }
                            }
                            if (same) continue;
                            else
                            {
                                //tekrar normal taþa çevir
                                for (int i = 0; i < result.Count; i++)
                                {
                                    int curr_x = result[i].x;
                                    int curr_y = result[i].y;
                                    GameObject normal_spawn = null;
                                    switch (result[i].first_letter)
                                    {
                                        case "r":
                                            normal_spawn = tiles[0].tile;
                                            break;
                                        case "b":
                                            normal_spawn = tiles[1].tile;
                                            break;
                                        case "g":
                                            normal_spawn = tiles[3].tile;
                                            break;
                                        case "y":
                                            normal_spawn = tiles[2].tile;
                                            break;
                                    }
                                    Image tnt_image = normal_spawn.GetComponent<Image>();
                                    Board.Instance.board[curr_y, curr_x].tile.GetComponent<Image>().sprite = tnt_image.sprite;
                                    Board.Instance.board[curr_y, curr_x].status = "normal";
                                    result[i].tile = Board.Instance.board[curr_y, curr_x].tile;
                                }
                                groups.Add(result);
                            }

                        }
                    }               
                }
            }
        }
        groups.Clear();
    }
}

public class TileComparer : IComparer<Tile> //This class is the sort the Tile objects according to their y and x's in a list
{
    public int Compare(Tile tile1, Tile tile2)
    {
        if (tile1.y != tile2.y)
        {
            return tile1.y.CompareTo(tile2.y);
        }
        else
        {
            return tile1.x.CompareTo(tile2.x);
        }
    }

     public static bool AreTilesEqual(List<Tile> tiles1, List<Tile> tiles2)
    {
        if (tiles1.Count != tiles2.Count)
        {
            return false;
        }

        for (int i = 0; i < tiles1.Count; i++)
        {
            if (tiles1[i].x != tiles2[i].x || tiles1[i].x != tiles2[i].x)
            {
                return false;
            }
        }

        return true;
    }
}



