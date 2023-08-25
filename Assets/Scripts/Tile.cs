using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Tile
{
    public string first_letter;
    public GameObject tile;
    [HideInInspector]
    public string status;
    [HideInInspector]
    public int x;
    [HideInInspector]
    public int y;
    [HideInInspector]
    public Button button = null;

    //Burayý düzenle
    [HideInInspector]
    public Tile Left => x > 0 ? Board.Instance.board[y, x-1] : null;
    [HideInInspector]
    public Tile Up => y < Board.Instance.grid_height - 1 ? Board.Instance.board[y+1,x] : null;
    [HideInInspector]
    public Tile Right => x < Board.Instance.grid_width -1 ? Board.Instance.board[y,x+1] : null;
    [HideInInspector]
    public Tile Down => y > 0 ? Board.Instance.board[y-1,x]: null;

    public Coroutine fallCoroutine;

    public bool Equal(Tile other)
    {
        return ((this.y == other.y) && (this.x == other.x));
    }
    public Tile()
    {

    }
    public Tile(string _first_letter,string _status,int _y, int _x)
    {
        first_letter = _first_letter;
        status = _status;
        x = _x;
        y = _y;
    }

    public List<Tile> CheckTnt(List<Tile> exclude = null)
    {
        List<Tile> result = new List<Tile> { this, };
        Tile[] Neighbours = {Left, Right,Up,Down}; 
        if (exclude == null)
        {
            exclude = new List<Tile> { this, };
        }
        else
        {
            exclude.Add(this);
        }

        foreach (var neigbour in Neighbours)
        {
            if (neigbour == null || exclude.Contains(neigbour) || neigbour.first_letter != this.first_letter)
            {
                continue;
            }
            result.AddRange(neigbour.CheckTnt(exclude));
        }

        return result;
    }

}
