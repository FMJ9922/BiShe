using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CarSensor : MonoBehaviour
{
    public List<Transform> _otherCar = new List<Transform>();
    private Transform _avoidTrans;

    private void OnTriggerEnter(Collider other)
    {
        var trans = other.transform;
        if (other.CompareTag("Building")||other.CompareTag("car") && trans != transform.parent)
        {
            _otherCar.Add(trans);
            if (GetSqrDis(trans) < GetSqrDis(_avoidTrans))
            {
                _avoidTrans = trans;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var trans = other.transform;
        if (other.CompareTag("Building")||other.CompareTag("car"))
        {
            //Debug.Log("exit" + other.transform);
            if (_otherCar.Contains(trans))
            {
                _otherCar.Remove(trans);
            }

            if (_avoidTrans == trans)
            {
                _avoidTrans = GetNearestTranform();
            }
        }
    }

    public void CleanUpSensor()
    {
        _otherCar.Clear();
    }

    public bool IsNeedBrake(out Vector3 brakeDir)
    {
        if (_avoidTrans != null)
        {
            brakeDir =  _avoidTrans.position -transform.position;
            if (Vector3.Dot(brakeDir, transform.forward) > 0)
            {
                return true;
            }
        }

        brakeDir = Vector3.zero;
        return false;
    }

    private Transform GetNearestTranform()
    {
        Transform ret = null;
        float near = float.MaxValue;
        for (int i = 0; i < _otherCar.Count; i++)
        {
            float dis = GetSqrDis(_otherCar[i]);
            if (dis < near)
            {
                ret = _otherCar[i];
                near = dis;
            }
        }

        return ret;
    }

    private float GetSqrDis(Transform a)
    {
        if (a)
        {
            return (a.position - transform.position).sqrMagnitude;
        }
        else
        {
            return 999f;
        }
    }
}
