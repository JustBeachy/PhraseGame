using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetterButton : MonoBehaviour
{
    public GameObject wordObject;
    public bool locked = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLetterClick()
    {
        if (!locked)
        {
            gameObject.GetComponent<Button>().interactable = false;
            wordObject.GetComponent<PhraseList>().LetterClicked(char.Parse(GetComponentInChildren<Text>().text),gameObject);
        }
    }

    public void LockLetter()
    {
        locked = true;
        GetComponent<Image>().color = Color.red;
    }

    public void UnlockLetter()
    {
        locked = false;
        GetComponent<Image>().color = Color.yellow;
    }
}
