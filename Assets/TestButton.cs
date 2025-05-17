using UnityEngine;

public class TestButton : MonoBehaviour
{

    public Item item;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void katso()
    {
        InventoryManager.Instance.FindAndConsumeItem(item, true);

        Debug.Log("katsottu");
    }
}
