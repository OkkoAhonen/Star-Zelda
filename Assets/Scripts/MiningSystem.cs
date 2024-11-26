using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MiningSystem : MonoBehaviour
{
    private bool mined = false; //Tracks has the mining finished
    private bool isMining = false; //Tracks is the mining going on
    private float mineSpeed = 10.0f; //The amount of time used for mining
    private float miningProgress = 0.0f; //Tracks progress of mining

    public GameObject mineral; //Mineral or stone to be mined
    // public GameObject pickaxe; //Player's pickaxe used to mine

    // Start is called before the first frame update
    void Start()
    {
        GameObject mineral = GetComponent<GameObject>();

        //For debugging
        if (mineral == null)
        {
            Debug.LogError("Mineral/Stone could not be found");
        }

        /* GameObject pickaxe = GameObject.Find("Pickaxe");

        //For debugging
        if (pickaxe == null)
        {
            Debug.LogError("Player can't break minerals without a pickaxe");
        } */
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) == true)
        {
            isMining = true;
        }

        if (isMining == true)
        {
            Mining(mineSpeed);
        }
    }

    public void Mining(float mineSpeed)
    {
        //For temporaly showing the mining progress
        Debug.Log($"Mining progress: {miningProgress}");

        miningProgress += mineSpeed * Time.deltaTime;

        if (miningProgress >= 100.0f)
        {
            mined = true;
            isMining = false;

            DeleteObject(mineral);

            Debug.Log("Mining has been completed!");

            // For the future plan is to put here a code about collecting minerals from the object

        }
    }

    public void DeleteObject(GameObject mineral)
    {

        if (mineral != null)
        {
            Destroy(mineral);
            Debug.Log("Mineral has been destroyed");
        }
        else
        {
            Debug.LogWarning("Mineral cannot be found");
        }
    }
}
