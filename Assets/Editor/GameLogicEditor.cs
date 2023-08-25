using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(GameLogic))]
public class GameLogicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw the default inspector for GameLogic

        GameLogic gameLogic = (GameLogic)target;

        if (GUILayout.Button("Add Tile"))
        {
            gameLogic.tiles.Add(new Tile());
        }

        if (gameLogic.tiles != null)
        {
            EditorGUILayout.Space();

            for (int i = 0; i < gameLogic.tiles.Count; i++)
            {
                EditorGUILayout.LabelField("Tile " + i);
                EditorGUI.indentLevel++;

                gameLogic.tiles[i].first_letter = EditorGUILayout.TextField("First Letter", gameLogic.tiles[i].first_letter);
                gameLogic.tiles[i].tile = (GameObject)EditorGUILayout.ObjectField("Tile Object", gameLogic.tiles[i].tile, typeof(GameObject), false);
                gameLogic.tiles[i].status = EditorGUILayout.TextField("Status", gameLogic.tiles[i].status);
                gameLogic.tiles[i].x = EditorGUILayout.IntField("X", gameLogic.tiles[i].x);
                gameLogic.tiles[i].y = EditorGUILayout.IntField("Y", gameLogic.tiles[i].y);

                EditorGUI.indentLevel--;
            }
        }
    }
}