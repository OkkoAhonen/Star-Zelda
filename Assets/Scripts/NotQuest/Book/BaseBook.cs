using UnityEngine;
using System.Collections.Generic;

public abstract class BaseBook : ScriptableObject
{
    public string title;
    public bool playerCanEdit;
    public List<BookPage> pages = new List<BookPage>();

    // Method to add text to the current page
    public virtual void AddTextToCurrentPage(string newText, int currentPage)
    {
        if (playerCanEdit && currentPage < pages.Count)
        {
            pages[currentPage].text += newText; // Append text to the current page
        }
    }

    // Virtual method that can be overridden by specific book types
    public virtual void OnPageTurnComplete() { }
    
    // Virtual method for handling text overflow
    public virtual bool HandleTextOverflow(string text, int currentPage)
    {
        if (currentPage >= pages.Count - 1)
        {
            if (playerCanEdit)
            {
                // Create a new page if the book is editable
                pages.Add(new BookPage { text = text });
                return true;
            }
            return false;
        }
        return false;
    }
}

[CreateAssetMenu(fileName = "NewJournal", menuName = "Book System/Journal")]
public class JournalBook : BaseBook
{
    public List<string> questEntries = new List<string>();
    
    public override void OnPageTurnComplete()
    {
        // Special handling for journal updates
        UpdateQuestEntries();
    }

    private void UpdateQuestEntries()
    {
        // Update journal specific content
    }
}

[CreateAssetMenu(fileName = "NewSpellBook", menuName = "Book System/Spell Book")]
public class SpellBook : BaseBook
{
    public List<SpellEntry> spells = new List<SpellEntry>();
    
    [System.Serializable]
    public class SpellEntry
    {
        public string spellName;
        public Sprite spellIcon;
        public string description;
    }
} 