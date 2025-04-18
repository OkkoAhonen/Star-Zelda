using UnityEngine;
using System.Collections.Generic;
using TMPro;

[CreateAssetMenu(fileName = "NewBook", menuName = "Book System/Book")]
public class BookData : ScriptableObject
{
    [Header("Book Info")]
    public string title;                     // Title of the book
    public bool playerCanEdit;               // Can the player edit this book?
    public string creator;
    public string creationDate;
    
    [Header("Prefab")]
    public GameObject bookPrefab; // The prefab containing the pages
}
