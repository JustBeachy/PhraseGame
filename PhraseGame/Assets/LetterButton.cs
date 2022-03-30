using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetterButton : MonoBehaviour
{
    public GameObject wordObject;
    public bool locked = false;
    public GameObject lockIcon;
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
        if (!locked&&PhraseList.StartGame)
        {
            gameObject.GetComponent<Button>().interactable = false;
            wordObject.GetComponent<PhraseList>().LetterClicked(char.Parse(GetComponentInChildren<Text>().text),gameObject);
        }
    }

    public void LockLetter()
    {
        Instantiate(lockIcon, transform);
        locked = true;
        GetComponent<Image>().color = Color.red;
    }

    public void UnlockLetter()
    {
        GameObject[] allLocks = GameObject.FindGameObjectsWithTag("Lock");
        foreach(GameObject L in allLocks)
        {
            L.GetComponent<Animator>().speed = 1;
            L.GetComponent<Animator>().SetBool("Unlock", true);
        }
        locked = false;
        GetComponent<Image>().color = Color.yellow;
    }
}
