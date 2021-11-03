using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSensor : MonoBehaviour
{
    public delegate void Brake();
    public Brake OnBrake;
    public Brake OnStopBrake;
    public List<Transform> otherCar = new List<Transform>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("car") && other.transform != transform.parent)
        {
            //Debug.Log(Vector3.Dot(transform.forward, other.transform.position - transform.parent.position));
            if (Vector3.Dot(transform.forward, other.transform.position - transform.parent.position) <= 0)
            {
                //Debug.Log(transform.name);
                return;
            }
            otherCar.Add(other.transform);
            if (OnBrake != null)
            {
                //Debug.Log(Vector3.Angle(other.transform.forward, transform.forward));
                if (Vector3.Angle(other.transform.forward, transform.forward) < 20)
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
            //Debug.Log("exit" + other.transform);
            if (otherCar.Contains(other.transform))
            {
                otherCar.Remove(other.transform);
            }
            if (OnStopBrake != null&&otherCar.Count==0)
            {
                OnStopBrake();
            }
        }
    }

    public void CleanUpSensor()
    {
        otherCar = new List<Transform>();
    }
}
