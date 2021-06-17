using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSensor : MonoBehaviour
{
    public delegate void Brake();
    public Brake OnBrake;
    public Brake OnStopBrake;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("car"))
        {
            if (OnBrake != null)
            {
                if (Vector3.Angle(other.transform.forward, transform.forward) > 70)
                {
                    OnBrake();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("car"))
        {
            if (OnStopBrake != null)
            {
                OnStopBrake();
            }
        }
    }
}
