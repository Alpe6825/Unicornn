using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WORDSLOW {ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE, FORWARD, BACKWARD, LEFT, RIGHT, UP, DOWN }

public class SpeechCommandsLow : MonoBehaviour
{
    public WORDSLOW firstWord;
    public WORDSLOW secondWord;
    public List<GameObject> allGameObjects = new List<GameObject>();
    public void SendWords()
    {
        GameObject selectedObject = null;
        switch (firstWord)
        {
            case WORDSLOW.ONE:
                selectedObject = allGameObjects[0];
                break;
            case WORDSLOW.TWO:
                selectedObject = allGameObjects[1];
                break;
            case WORDSLOW.THREE:
                selectedObject = allGameObjects[2];
                break;
            case WORDSLOW.FOUR:
                selectedObject = allGameObjects[3];
                break;
            case WORDSLOW.FIVE:
                selectedObject = allGameObjects[4];
                break;
            case WORDSLOW.SIX:
                selectedObject = allGameObjects[5];
                break;
            case WORDSLOW.SEVEN:
                selectedObject = allGameObjects[6];
                break;
            case WORDSLOW.EIGHT:
                selectedObject = allGameObjects[7];
                break;
            case WORDSLOW.NINE:
                selectedObject = allGameObjects[8];
                break;
            default:
                Debug.LogWarning("Not a Word.");
                return;
        }
        SetMovement(selectedObject);
    }

    private void SetMovement(GameObject selectedObject)
    {
        if (selectedObject == null)
            return;

        Vector3 movement = Vector3.zero;
        switch(secondWord)
        {
            case WORDSLOW.FORWARD:
                movement = Vector3.forward;
                break;
            case WORDSLOW.BACKWARD:
                movement = Vector3.back;
                break;
            case WORDSLOW.RIGHT:
                movement = Vector3.right;
                break;
            case WORDSLOW.LEFT:
                movement = Vector3.left;
                break;
            case WORDSLOW.UP:
                movement = Vector3.up;
                break;
            case WORDSLOW.DOWN:
                movement = Vector3.down;
                break;
        }
        
        selectedObject.transform.position += movement;
    }
}
