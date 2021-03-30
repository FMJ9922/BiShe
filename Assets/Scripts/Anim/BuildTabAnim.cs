using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BuildTabAnim : MonoBehaviour
{
    [SerializeField] Animation animation;
    public Image image;
    public string iconName;
    private const string _iconBundle = "icon.ab";
    public void Rise()
    {
        animation["BuildTabAnima"].speed = 2;
        animation.Play("BuildTabAnima");
        GetComponent<Button>().interactable = false;
        string name = string.Format("Click{0}Icon", iconName);
        image.sprite = LoadAB.LoadSprite(_iconBundle, name);
        image.SetNativeSize();
    }

    public void InitSprite()
    {
        string name = string.Format("Unclick{0}Icon", iconName);
        image.sprite = LoadAB.LoadSprite(_iconBundle, name);
        //Debug.Log(name);
    }
    public void Hide()
    {
        animation["BuildTabAnima"].speed = -2;
        animation["BuildTabAnima"].time = animation["BuildTabAnima"].length;
        animation.Play("BuildTabAnima"); 
        GetComponent<Button>().interactable = true;
        string name = string.Format("Unclick{0}Icon", iconName);
        image.sprite = LoadAB.LoadSprite(_iconBundle, name);
        image.SetNativeSize();
    }

}
