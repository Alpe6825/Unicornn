using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Words { create, delete, select, color, move, cube, sphere, plane, red, green, blue, white, one, two, three, four, five, six, seven, eight, nine, forward, backward, up, down, left, right}

public class SpeechCommands : MonoBehaviour
{
    public GameObject selectedObject;

    public Words firstWord;
    public Words secondWord;
    public Words thirdWord;

    public List<GameObject> ObjectsActiveInScene = new List<GameObject>();
    
    public void SendWords()
    {
        switch (firstWord)
        {
            case Words.create:
                CreateObject(secondWord);
                break;
            case Words.delete:
                if(selectedObject != null)
                    DeleteObject();
                break;
            case Words.select:
                SelectObject(secondWord);
                break;
            case Words.color:
                if(selectedObject != null)
                    ColorObject(secondWord);
                break;
            case Words.move:
                if(selectedObject != null)
                    MoveObject(secondWord, thirdWord);
                break;
            default:
                Debug.Log("The first Word is Invalid");
                return;
        }
    }

    private void CreateObject(Words geometry)
    {
        if (ObjectsActiveInScene.Count > 8)
        {
            Debug.Log("Cant instantiate more than 9 objects.");
            return;
        }
        GameObject newObject = null;
        switch (geometry)
        {
            case Words.cube:
                newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case Words.sphere:
                newObject =  GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            default:
                Debug.Log("Second word is not a geometry.");
                return;
        }
        ObjectsActiveInScene.Add(newObject);
        selectedObject = newObject;
    }
    private void DeleteObject()
    {
        if(selectedObject == null)
        {
            Debug.Log("There is no selected object.");
            return;
        }
        ObjectsActiveInScene.Remove(selectedObject);
        Destroy(selectedObject);
        if (ObjectsActiveInScene.Count == 0)
        {
            Debug.Log("There are no objects to select.");
            return;
        }
        selectedObject = ObjectsActiveInScene[ObjectsActiveInScene.Count - 1];
    }
    private void SelectObject(Words number)
    {
        int index = GetListObject(number);
        if(index == 0)
        {
            Debug.Log("Invalid number returned.");
            return;
        }

        if (index <= ObjectsActiveInScene.Count)
            selectedObject = ObjectsActiveInScene[index - 1];
        else
            Debug.Log("Exceeding List Range");
    }
    private void ColorObject(Words color)
    {
        Color colorForObject;
        switch (color)
        {
            case Words.blue:
                colorForObject = Color.blue;
                break;
            case Words.green:
                colorForObject = Color.green;
                break;
            case Words.red:
                colorForObject = Color.red;
                break;
            case Words.white:
                colorForObject = Color.white;
                break;
            default:
                Debug.Log("Second word is not a color.");
                return;
        }

        selectedObject.GetComponent<MeshRenderer>().material.color = colorForObject;
    }
    private void MoveObject(Words number, Words direction)
    {
        int lenght = GetListObject(number);
        if(lenght == 0)
        {
            Debug.Log("Invalid number");
            return;
        }

        switch (direction)
        {
            case Words.right:
                ActivateMovement(lenght, Vector3.right);
                break;
            case Words.left:
                ActivateMovement(lenght, Vector3.left);
                break;
            case Words.up:
                ActivateMovement(lenght, Vector3.up);
                break;
            case Words.down:
                ActivateMovement(lenght, Vector3.down);
                break;
            case Words.forward:
                ActivateMovement(lenght, Vector3.forward);
                break;
            case Words.backward:
                ActivateMovement(lenght, Vector3.back);
                break;
            default:
                Debug.Log("Third word is not a direction");
                return;
        }
    }
    private void ActivateMovement(int length, Vector3 direction)
    {
        if(selectedObject == null)
        {
            Debug.Log("There is no selected object to move.");
            return;
        }
        selectedObject.transform.position += direction * length;
    }
    private int GetListObject(Words number)
    {
        int index;
        switch (number)
        {
            case Words.one:
                index = 1;
                break;
            case Words.two:
                index = 2;
                break;
            case Words.three:
                index = 3;
                break;
            case Words.four:
                index = 4;
                break;
            case Words.five:
                index = 5;
                break;
            case Words.six:
                index = 6;
                break;
            case Words.seven:
                index = 7;
                break;
            case Words.eight:
                index = 8;
                break;
            case Words.nine:
                index = 9;
                break;
            default:
                Debug.Log("Second word is not a number.");
                return 0;
        }
        return index;
    }
}
