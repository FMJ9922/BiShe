using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseParticle : MonoBehaviour
{
    ParticleSystem sys;
    private void Start()
    {
        sys = transform.GetComponent<ParticleSystem>();
        EventManager.StartListening(ConstEvent.OnPauseGame, Pause);
        EventManager.StartListening(ConstEvent.OnResumeGame, ResumeParticle);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(ConstEvent.OnPauseGame, Pause);
        EventManager.StopListening(ConstEvent.OnResumeGame, ResumeParticle);
    }
    public void Pause()
    {
        sys.Pause();
    }

    public void ResumeParticle()
    {
        sys.Play();
    }
}
