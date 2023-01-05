using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance = null;
    private float musicVolumn = 0.1f;
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    private string curPlayMusic;
    public static MusicManager Instance
    {
        get
        {
            return instance;
        }
    }
    public float Volumn
    {
        get
        {
            return musicVolumn;
        }
        set
        {
            musicVolumn = Mathf.Clamp(value, 0, 1);
            ValueChangeCheck(musicVolumn);
        }
    }
    private void Awake()
    {
        if (instance != null && instance != this)//检测Instance是否存在且只有一个
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);//加载关卡时不销毁GameManager
        musicVolumn = PlayerPrefs.GetFloat("MusicVolumn", 0.1f);
        ValueChangeCheck(musicVolumn);
    }
    private void Start()
    {
        PlayMusic();
    }
    public void ValueChangeCheck(float vol)
    {
        audioSource.volume = vol;

    }
    public void PlayMusic()
    {
        int rand = Random.Range(0, 8);
        audioSource.PlayOneShot(audioClips[rand]);
        Debug.Log("play"+ audioClips[rand].name);
        float time = audioClips[rand].length;
        Invoke("PlayMusic", time + 30);


    }
    IEnumerator DoSthAfterClipFinished(float time, AudioSource self)
    {
        yield return new WaitForSecondsRealtime(time + 1);
        PlayMusic();
    }
}
