using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : MonoBehaviour
{
    //占地长宽
    [SerializeField]
    private int Width;
    [SerializeField]
    private int Height;

    [SerializeField]
    private BundlePrimaryType BundlePrimaryType;

    
    public string BundleName => string.Format("{0}.ab", BundlePrimaryType.ToString());

    public string PfbName => this.gameObject.name;

    public Vector2Int Size => new Vector2Int(Height, Width);

    public bool hasAnima = false;

    public Animation animation;

    public GameObject body;

    public BuildData buildData;

    public bool buildFlag = false;
     
    public virtual void OnConfirmBuild()
    {
        buildFlag = true;
        gameObject.tag = "Building";
        if (hasAnima)
        {
            body.SetActive(false);
            animation.gameObject.SetActive(true);
            float time = animation.clip.length;
            Invoke("ShowBody", time);
            animation.Play();
        }
    }

    public void ShowBody()
    {
        body.SetActive(true);
        animation.gameObject.SetActive(false);
    }

    public virtual string GetIntroduce()
    {
        return string.Empty;
    }
}
