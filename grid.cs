using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class grid : MonoBehaviour{
    public GameState currentState = GameState.move;

    //global variables
    public int width;
    public int height;
    public GameObject tilePrefab;
    public GameObject[] characters;
    public GameObject[,] allCharacters;

    private Scores Scores;

    //2d list
    private tilebackground[,] cells;

    //initialize grid
    void Start(){
        cells = new tilebackground[width, height];
        allCharacters = new GameObject[width, height];
        Scores = FindObjectOfType<Scores>();

        setup();
    }

    private void setup() {
        for(int i = 0; i < width; i++){
            for(int j = 0; j < height; j++){
                Vector2 temp = new Vector2(i, j);
                GameObject backgroundTile = Instantiate(tilePrefab,temp,Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "( " + i + ", " + j + " )";

                //generate random character
                int characterToRender = UnityEngine.Random.Range(0, characters.Length);
                //if the generated character makes a match, generate a new one until it doesn't
                int maxIterations = 0;
                while(MatchesAt(i, j, characters[characterToRender]) && maxIterations < 50)
                {
                    characterToRender = UnityEngine.Random.Range(0, characters.Length);
                    maxIterations++;
                }
                maxIterations = 0;

                GameObject character = Instantiate(characters[characterToRender], temp, Quaternion.identity);
                character.transform.parent = this.transform;
                character.name = "( " + i + ", " + j + " )";
                allCharacters[i, j] = character;
            }
        }
    }
    
    //Match checker to check during generation
    private bool MatchesAt(int column, int row, GameObject piece)
    {
        if(column > 1 && row > 1)
        {
            if(allCharacters[column - 1, row].tag == piece.tag && allCharacters[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            if (allCharacters[column, row - 1].tag == piece.tag && allCharacters[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        }
        //edge cases
        else if(column <= 1 || row <= 1)
        {
            if(row > 1)
            {
                if (allCharacters[column, row - 1].tag == piece.tag && allCharacters[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
            if ( column > 1)
            {
                if (allCharacters[column - 1, row].tag == piece.tag && allCharacters[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }


    //Destroy matches loop
    public void DestroyMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allCharacters[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(rowFall());
    }


    //Destroyer
    private void DestroyMatchesAt(int column, int row)
    {
        if (allCharacters[column, row].GetComponent<character>().isMatched)
        {
            Destroy(allCharacters[column, row]);
            allCharacters[column, row] = null;

            //Score update
            Scores.IncreaseScore();
        }
    }


    //Co-routine to enable filling of space created by match destruction
    private IEnumerator rowFall()
    {
        //space count represents the amount of space below
        int spaceCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allCharacters[i, j] == null)
                {
                    spaceCount++;
                }
                else if (spaceCount > 0)
                {
                    allCharacters[i, j].GetComponent<character>().row -= spaceCount;
                    allCharacters[i, j] = null;
                }
            }
            spaceCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(refillGrid());
    }


    //Refill after destruction
    private IEnumerator refillGrid()
    {
        refill();
        yield return new WaitForSeconds(.5f);

        while (gridHasMatches())
        {
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(.5f);
        currentState = GameState.move;
    }



    private void refill()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allCharacters[i, j] == null)
                {
                    Vector2 temp = new Vector2(i, j);
                    int characterToRender = UnityEngine.Random.Range(0, characters.Length);
                    GameObject character = Instantiate(characters[characterToRender], temp, Quaternion.identity);
                    allCharacters[i, j] = character;
                }
            }
        }
    }


    private bool gridHasMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(allCharacters[i, j] != null)
                {
                    if (allCharacters[i, j].GetComponent<character>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
    return false;
    }

}
