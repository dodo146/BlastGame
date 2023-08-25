using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [HideInInspector]
    public int grid_height;
    [HideInInspector]
    public int grid_width;
    [HideInInspector]
    public int move_count;
    [HideInInspector]
    public int box_Count;
    [HideInInspector]
    public int Stone_Count;
    [HideInInspector]
    public int Vase_Count;

    public Tile[,] board;

    private GameLogic gameLogic_class;

    public float fallAnimationDuration = 0.2f;


    string[] grid_values;
    private readonly System.Random random = new System.Random();

    [SerializeField]
    public float cellSize = 35.0f;

    private void Awake()
    {
        Instance = this;
        box_Count = 0;
        Stone_Count = 0;
        Vase_Count = 0;
        gameLogic_class = GameObject.Find("Canvas").GetComponent<GameLogic>();
    }

    private GameObject InstantiateTile(GameObject tilePrefab,int x, int y)
    {
        // Instantiate a new tile GameObject and set its position
        GameObject newTile = Instantiate(tilePrefab, new Vector2(x * cellSize, y * cellSize), Quaternion.identity);
        return newTile;
    }

    public void GenerateGrid()
    {
        int iter = 0;
        string[] box_choices = { "r", "g", "b", "y" };
        GameLogic.GetLevelInformation();
        GetBoardValues();
        CalculateBoardSize();
        board = new Tile[grid_height, grid_width];
        List<Tile> tiles = gameLogic_class.tiles; 
        //int tile_index = -1;
        for (int y = 0; y < grid_height; y++)
        {
            for (int x = 0; x < grid_width; x++)
            {
                int localY = y;
                int localX = x;
                if (grid_values[iter] == "rand")
                {
                    //random bi tane ata
                    int random_index = random.Next(0, box_choices.Length);
                    string randColor = box_choices[random_index];
                    board[y, x] = new Tile(randColor,"normal",y,x);
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        if (tiles[i].first_letter == board[y, x].first_letter)
                        {
                            board[y, x].tile = tiles[i].tile;
                            break;
                        }
                    }

                    GameObject cell = InstantiateTile(board[y, x].tile, board[y, x].x, board[y, x].y);
                    board[y, x].tile = cell;
                    board[y, x].button = board[y, x].tile.GetComponent<Button>();
                    board[y, x].button.onClick.AddListener(() => gameLogic_class.ClickTile(board[localY, localX]));
                    cell.transform.SetParent(transform, false);
                    cell.name = $"Tile ({y},{x})";
                }
                else
                {
                    board[y, x] = new Tile(grid_values[iter],"normal",y,x);
                    if (board[y,x].first_letter == "t")
                    {
                        board[y, x].status = "TNT";
                    }
                    CountObstacle(iter);
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        if (tiles[i].first_letter == board[y, x].first_letter)
                        {
                            board[y, x].tile = tiles[i].tile;
                            break;
                        }
                    }

                    GameObject cell = InstantiateTile(board[y, x].tile, board[y, x].x, board[y, x].y);
                    board[y, x].tile = cell;
                    board[y, x].button = board[y, x].tile.GetComponent<Button>();
                    board[y, x].button.onClick.AddListener(() => gameLogic_class.ClickTile(board[localY,localX]));
                    cell.transform.SetParent(transform, false);
                    cell.name = $"Tile ({y},{x})";
                }
                iter++;
            }
        }

    }

    private void CountObstacle(int it)
    {
        string obstacle_name = grid_values[it];
        switch (obstacle_name)
        {
            case "bo":
                box_Count++; break;
            case "s":
                Stone_Count++; break;
            case "v":
                Vase_Count++; break;
        }
    }

    private void GetBoardValues()
    {
        Level current_level = MainLogic.Levels[MainLogic.current_level_number - 1];
        string jsonPath = Path.Combine(Application.dataPath, "Levels", $"level_{current_level.level_number}_with_grid.json");
        string jsonContent = File.ReadAllText(jsonPath);
        JObject json = JObject.Parse(jsonContent);
        // get the grid array as a JArray
        JArray gridArray = (JArray)json["grid"];
        //then convert it to the normal C# Array
        string[] grid = gridArray.ToObject<string[]>();
        grid_values = grid;
    }

    private void CalculateBoardSize()
    {
        float new_height = grid_height * cellSize;
        float new_width = grid_width * cellSize;

        var rectTransform = this.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(new_width, new_height);
        }
    }

    public List<Tile> GetTilesAroundATile(Tile tile)
    {
        List<Tile> surroundingTiles = new List<Tile>();
        for (int yOffset = -1; yOffset <= 1; yOffset++)
        {
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                int neighborX = tile.x + xOffset;
                int neighborY = tile.y + yOffset;

                if (neighborX >= 0 && neighborX < Board.Instance.grid_width &&
                    neighborY >= 0 && neighborY < Board.Instance.grid_height)
                {
                    if (Board.Instance.board[neighborY,neighborX] == null)
                    {
                        continue;
                    }
                    surroundingTiles.Add(Board.Instance.board[neighborY, neighborX]);
                }
            }
        }
        return surroundingTiles;
    }

    public List<Tile> GetTilesAroundATNT(Tile tile)
    {
        List<Tile> surroundingTiles = new List<Tile>();
        for (int yOffset = -2; yOffset <= 2; yOffset++)
        {
            for (int xOffset = -2; xOffset <= 2; xOffset++)
            {
                int neighborX = tile.x + xOffset;
                int neighborY = tile.y + yOffset;

                if (neighborX >= 0 && neighborX < Board.Instance.grid_width &&
                    neighborY >= 0 && neighborY < Board.Instance.grid_height)
                {
                    if (Board.Instance.board[neighborY, neighborX] == null)
                    {
                        continue;
                    }
                    surroundingTiles.Add(Board.Instance.board[neighborY, neighborX]);
                }
            }
        }
        return surroundingTiles;
    }

    public void ApplyFallMechanic()
    {
        bool isMoved; // this will tell if the current tile doesn't have any tile above that can be dropped down in the grid
        bool isBoxOrStone; //this will tell if the current tile's above tile is a stone or a box or not
        int aboveY; // this will be the iterator for the tiles above the current tile
        for (int x = 0; x < grid_width; x++)
        {
            for (int y = 0; y < grid_height; y++)
            {
                isMoved = false;
                isBoxOrStone = false;
                int localY = y;
                int localX = x;
                if (board[y, x] == null)
                {
                    for (aboveY = y + 1; aboveY < grid_height; aboveY++)
                    {
                        if (board[aboveY, x] != null)
                        {
                            if (board[aboveY,x].first_letter == "bo" || board[aboveY,x].first_letter == "s")
                            {
                                isBoxOrStone = true;
                                break;
                            }
                            // Move the tile down
                            isMoved = true;
                            board[y, x] = board[aboveY, x];
                            board[aboveY, x] = null;
                            board[y, x].y = y;
                            board[y, x].tile.name = $"Tile ({y},{x})";
                            Vector2 targetPosition = new Vector2(x * cellSize, y * cellSize);

                            //coroutinele düþme animasyonu.Açýkçasý tam olarak nasýl çalýþýyor bilmiyorum ama ok
                            Coroutine fallCoroutine = StartCoroutine(FallTileCoroutine(board[y, x], targetPosition));
                            board[y, x].fallCoroutine = fallCoroutine; // Store reference to coroutine
                            // hareket eden tilelarýn buttonlarýnýn listenerlarýný updatele
                            board[y, x].button.onClick.RemoveAllListeners();
                            board[y, x].button.onClick.AddListener(() => gameLogic_class.ClickTile(board[localY, localX]));
                            break;
                        }
                    }
                    if (!isMoved) // this means the tile at (y,x) doesn't have any tile that can be dropped down to its position.
                    {
                        if (!isBoxOrStone) // this means that we didn't break the loop just for there being a box or a stone above current tile
                        {
                            string[] box_choices = { "r", "g", "b", "y" };
                            List<Tile> tiles = gameLogic_class.tiles;
                            int random_index = random.Next(0, box_choices.Length);
                            string randColor = box_choices[random_index];
                            Tile outside_tile = new Tile(randColor, "normal", aboveY, x);
                            for (int i = 0; i < tiles.Count; i++)
                            {
                                if (tiles[i].first_letter == outside_tile.first_letter)
                                {
                                    outside_tile.tile = tiles[i].tile;
                                    break;
                                }
                            }
                            Vector2 targetPosition = new Vector2(x * cellSize, y * cellSize);
                            GameObject outside_cell = InstantiateTile(outside_tile.tile, x, aboveY);
                            outside_cell.transform.SetParent(transform, false);
                            board[y, x] = outside_tile;
                            board[y, x].tile = outside_cell;
                            board[y, x].button = board[y, x].tile.GetComponent<Button>();
                            board[y, x].button.onClick.AddListener(() => gameLogic_class.ClickTile(board[localY, localX]));
                            outside_tile = null;
                            board[y, x].y = y;
                            board[y, x].tile.name = $"Tile ({y},{x})";

                            //coroutinele düþme animasyonu.Açýkçasý tam olarak nasýl çalýþýyor bilmiyorum ama ok
                            Coroutine fallCoroutine = StartCoroutine(FallTileCoroutine(board[y, x], targetPosition));
                            board[y, x].fallCoroutine = fallCoroutine; // Store reference to coroutine
                            
                        }
                    }
                }
            }
            //We need to spawn tiles here for the top row just before we change y

        }
    }

    IEnumerator FallTileCoroutine(Tile tile, Vector2 targetPosition)
    {

        float elapsedTime = 0;
        Vector2 startingPosition = tile.tile.GetComponent<RectTransform>().anchoredPosition;

        while (elapsedTime < fallAnimationDuration)
        {
            float t = elapsedTime / fallAnimationDuration;
            tile.tile.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startingPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.04f);
        }

        tile.tile.GetComponent<RectTransform>().anchoredPosition = targetPosition;
        if (tile.fallCoroutine != null)
        {
            StopCoroutine(tile.fallCoroutine); // Stop the coroutine
            tile.fallCoroutine = null;
        }
        
    }

}


