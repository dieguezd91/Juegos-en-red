using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemList", menuName = "ItemList")]
public class GameItemsList : ScriptableObject
{
    [SerializeField] private ItemBase[] gameItems;
    public ItemBase[] GameItems { get { return gameItems; } }
}
