using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class character : MonoBehaviour
{
    private grid ourgrid;
    private GameObject otherCharacter;

    //swipe angle variables
    //angleTouch stores the angle between firstTouch and finalTouch
    public float angleTouch = 0;
    public float angleResist = 1f;
    private Vector2 firstTouch;
    private Vector2 finalTouch;
    private Vector2 tempPosition;

    //row and column of current character
    public int column;
    public int row;

    //swapping variables
    public int targetX;
    public int targetY;

    //match checking variables
    public bool isMatched = false;
    public int prevCol;
    public int prevRow;
    


    // Start is called before the first frame update
    void Start()
    {
        ourgrid = FindObjectOfType<grid>();
        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        row = targetY;
        column = targetX;
    }

    // Update is called once per frame
    void Update()
    {
        findMatch();
        targetX = column;
        targetY = row;

        //Left-Right Movement
        if(Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Change position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if(ourgrid.allCharacters[column, row] != this.gameObject)
            {
                ourgrid.allCharacters[column, row] = this.gameObject;
            }
        }
        else
        {
            //Directly set the poistion
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }

        //Up-Down Movement
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Change position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (ourgrid.allCharacters[column, row] != this.gameObject)
            {
                ourgrid.allCharacters[column, row] = this.gameObject;
            }
        }
        else
        {
            //Directly set the poistion
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }

        //Update when in match
        if (isMatched)
        {
            //Change sprite color to grey
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1, 1, 1, .2f);
        }
    }


    private void OnMouseDown()
    {
        if(ourgrid.currentState == GameState.move)
        {
            firstTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }


    private void OnMouseUp()
    {
        if (ourgrid.currentState == GameState.move)
        {
            finalTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            FindAngle();
        }

    }


    void FindAngle()
    {
        //Make sure there is some sort of swipe distance
        if(Mathf.Abs(finalTouch.y - firstTouch.y) > angleResist || Mathf.Abs(finalTouch.x - firstTouch.x) > angleResist)
        {
            //The angle between the two touches is Arctan( yDifference / xDifference ). 
            //The additional math outside of the Atan2 function converts the radian output of Atan2 into degrees.
            angleTouch = Mathf.Atan2(finalTouch.y - firstTouch.y, finalTouch.x - firstTouch.x) * 180 / Mathf.PI;
            SwapCharacters();
            ourgrid.currentState = GameState.wait;
        }
        else
        {
            ourgrid.currentState = GameState.move;
        }
    }


    void SwapCharacters()
    {
        //Right Swap
        if(angleTouch > -45 && angleTouch <= 45 && column < ourgrid.width - 1)
        {
            //grab other character and change it's column/row as needed
            otherCharacter = ourgrid.allCharacters[column + 1, row];
            prevCol = column;
            prevRow = row;
            otherCharacter.GetComponent<character>().column -= 1;
            //change current character's column/row as needed
            column += 1;
        }
        //Up Swap
        else if(angleTouch > 45 && angleTouch <= 135 && row < ourgrid.height - 1)
        {
            otherCharacter = ourgrid.allCharacters[column, row + 1];
            prevCol = column;
            prevRow = row;
            otherCharacter.GetComponent<character>().row -= 1;
            row += 1;
        }
        //Left Swap
        else if ((angleTouch > 135 || angleTouch <= -135) && column > 0)
        {
            otherCharacter = ourgrid.allCharacters[column - 1, row];
            prevCol = column;
            prevRow = row;
            otherCharacter.GetComponent<character>().column += 1;
            column -= 1;
        }
        //Down Swap
        else if (angleTouch < -45 && angleTouch >= -135 && row > 0)
        {
            otherCharacter = ourgrid.allCharacters[column, row - 1];
            prevCol = column;
            prevRow = row;
            otherCharacter.GetComponent<character>().row += 1;
            row -= 1;
        }
        StartCoroutine(fairMove());
    }


    void findMatch()
    {
        //Horizontal matches
        if(column > 0 && column < ourgrid.width - 1){
            GameObject leftChar = ourgrid.allCharacters[column - 1, row];
            GameObject rightChar = ourgrid.allCharacters[column + 1, row];
            if (leftChar != null && rightChar != null) 
            { 
                if (leftChar.tag == this.gameObject.tag && rightChar.tag == this.gameObject.tag)
                {
                    leftChar.GetComponent<character>().isMatched = true;
                    rightChar.GetComponent<character>().isMatched = true;
                    isMatched = true;
                }
            }
        }

        //Vertical matches
        if (row > 0 && row < ourgrid.height - 1)
        {
            GameObject upChar = ourgrid.allCharacters[column, row + 1];
            GameObject downChar = ourgrid.allCharacters[column, row - 1];
            if (upChar != null && downChar != null)
            {
                if (upChar.tag == this.gameObject.tag && downChar.tag == this.gameObject.tag)
                {
                    upChar.GetComponent<character>().isMatched = true;
                    downChar.GetComponent<character>().isMatched = true;
                    isMatched = true;
                }
            }
        }

    }

    public IEnumerator fairMove()
    {
        yield return new WaitForSeconds(.5f);
        if(otherCharacter != null)
        {
            //If swap did not yield a match, swap characters back
            if (!isMatched && !otherCharacter.GetComponent<character>().isMatched)
            {
                otherCharacter.GetComponent<character>().row = row;
                otherCharacter.GetComponent<character>().column = column;
                row = prevRow;
                column = prevCol;

                yield return new WaitForSeconds(.5f);
                ourgrid.currentState = GameState.move;
            }
            //Else the match was successfull and must be DESTROYED
            else
            {
                ourgrid.DestroyMatches();
            }
            otherCharacter = null;
        }
        else
        {
            ourgrid.currentState = GameState.move;

        }

    }
}
