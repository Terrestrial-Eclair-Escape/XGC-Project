using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private WayPointPath _wayPointPath;

    [SerializeField]
    private float _speed;

    private int _targetWayPointIndex;

    private Transform _previousWayPoint;
    private Transform _targetWayPoint;

    private float _timeToWayPoint;
    private float _elapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        TargetNextWayPoint();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _elapsedTime += Time.deltaTime;

        float elapsedPercentage = _elapsedTime / _timeToWayPoint;
        elapsedPercentage = Mathf.SmoothStep(0, 1, elapsedPercentage);
        transform.position = Vector3.Lerp(_previousWayPoint.position, _targetWayPoint.position, elapsedPercentage);
        transform.rotation = Quaternion.Lerp(_previousWayPoint.rotation, _targetWayPoint.rotation, elapsedPercentage);

        if(elapsedPercentage >= 1)
        {
            TargetNextWayPoint();
        }
    }

    private void TargetNextWayPoint()
    {
        _previousWayPoint = _wayPointPath.GetWayPoint(_targetWayPointIndex);
        _targetWayPointIndex = _wayPointPath.GetNextWayPointIndex(_targetWayPointIndex);
        _targetWayPoint = _wayPointPath.GetWayPoint(_targetWayPointIndex);

        _elapsedTime = 0;

        float distanceToWayPoint = Vector3.Distance(_previousWayPoint.position, _targetWayPoint.position);
        _timeToWayPoint = distanceToWayPoint / _speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(transform);
    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);
    }
}
