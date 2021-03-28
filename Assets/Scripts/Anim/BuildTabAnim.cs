using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BuildTabAnim : MonoBehaviour
{
    [SerializeField] Animation animation;

    public void Rise()
    {
        animation["BuildTabAnima"].speed = 1;
        animation.Play("BuildTabAnima");
        GetComponent<Button>().interactable = false;
    }

    public void Hide()
    {
        animation["BuildTabAnima"].speed = -1;
        animation["BuildTabAnima"].time = animation["BuildTabAnima"].length;
        animation.Play("BuildTabAnima"); 
        GetComponent<Button>().interactable = true;
    }

}
