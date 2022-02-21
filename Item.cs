using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes items scriptable objects so they can easily be created within the editor.

[CreateAssetMenu(fileName = "New Item", menuName = "Item")] 
public class Item : ScriptableObject
{
    [SerializeField] string itemName; 
    public int score; 
    public int health; 

    public Sprite art;

}
