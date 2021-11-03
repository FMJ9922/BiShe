using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;
    private float soundVolumn;
    public AudioClip[] audioClips;


    public AudioSource[] efxSources;
    private Coroutine[] coroutines; 
    private List<AudioClip> clipList;

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
        clipList = new List<AudioClip>();
        float soundVolume = PlayerPrefs.GetFloat("SoundVolume", 0.1f);
        ValueChangeCheck(soundVolume);
    }

    public static SoundManager Instance
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
            return soundVolumn;
        }
        set
        {
            soundVolumn = Mathf.Clamp(value, 0, 1);
            ValueChangeCheck(soundVolumn);
        }
    }

    
    public void ValueChangeCheck(float vol)
    {
        PlayerPrefs.SetFloat("SoundVolume", vol);

        foreach (AudioSource audioSource in efxSources)
        {
            audioSource.volume = vol;
        }

    }
    public void AddToSoundList(AudioClip clip)
    {
        clipList.Add(clip);
    }
    public void RemoveFromSoundList(AudioClip clip)
    {
        if (clipList.Contains(clip))
        {
            clipList.Remove(clip);
        }
    }
    public void ClearSoundList()
    {
        clipList = new List<AudioClip>();
    }
    public void PauseAllTheSounds()
    {
        foreach (AudioSource audioSource in efxSources)
        {
            audioSource.Pause();
            StopAllCoroutines();
        }
    }
    public void PlaySoundEffect(SoundResource soundResource)
    {
        AudioClip audioClip = audioClips[(int)soundResource];
        PlaySingle(audioClip);
    }
    public void UnPauseAllTheSounds()
    {
        foreach (AudioSource audioSource in efxSources)
        {
            audioSource.UnPause();

        }
    }

    
    private void PlaySingle(AudioClip audioClip)
    {
        for(int i = 0; i < efxSources.Length; i++)
        {
            AudioSource audioSource = efxSources[i];
            if (!audioSource.isPlaying)
            {
                audioSource.clip = audioClip;
                audioSource.PlayOneShot(audioClip);
                return;
            }

            
        }
    }
    IEnumerator DoSthAfterClipFinished(float time,AudioSource self)
    {
        yield return new WaitForSecondsRealtime(time);
        self.clip = null;
        CheckWaitClip();
    }
    public void CheckWaitClip()
    {
        if (clipList.Count > 0)
        {

            PlaySingle(clipList[0]);
            RemoveFromSoundList(clipList[0]);
        }
    }

}
