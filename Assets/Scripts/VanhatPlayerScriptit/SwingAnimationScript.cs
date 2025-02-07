using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingAnimationScript : MonoBehaviour
{

    [SerializeField] private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    public void StartSwordAnimation()
    {
        animator.SetBool("IsSwingingTheSword", true);
    }

    public void StopSwordAnimation()
    {
        animator.SetBool("IsSwingingTheSword", false);
    }


}
