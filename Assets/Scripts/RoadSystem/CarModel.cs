using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarModel : MonoBehaviour
{
    public TransportationType CarType;
    public CarSensor CarSensor;
    private CarDriver _carDriver;
    private Transform _thisTrans;
    private Vector3 _leftOriginPos;
    private Vector3 _rightOriginPos;
    [SerializeField] private Transform _leftWheel;
    [SerializeField] private Transform _rightWheel;
    

    public Transform GetTransform()
    {
        if (!_thisTrans)
        {
            _thisTrans = this.transform;
        }
        return _thisTrans;
    }

    public void SetDriver(CarDriver carDriver)
    {
        _carDriver = carDriver;
    }

    public CarDriver GetDriver()
    {
        return _carDriver;
    }

    public void ChangeWheelDirection()
    {
        if (IsCantTurnWheelCar())
        {
            return;
        }
        Vector3 leftNowPos = _leftWheel.position;
        Vector3 rightNowPos = _rightWheel.position;
        if (leftNowPos != _leftOriginPos)
        {
            _leftWheel.LookAt(leftNowPos * 2 - _leftOriginPos, transform.up);
        }

        if (rightNowPos != _rightOriginPos)
        {
            _rightWheel.LookAt(rightNowPos * 2 - _rightOriginPos, transform.up);
        }

        _leftOriginPos = _leftWheel.position;
        _rightOriginPos = _rightWheel.position;
    }

    public bool IsCantTurnWheelCar()
    {
        return CarType == TransportationType.harvester;
    }
}
