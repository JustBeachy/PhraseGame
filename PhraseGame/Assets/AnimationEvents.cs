using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PauseAnimation()
    {
        GetComponent<Animator>().speed = 0;
    }

    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
