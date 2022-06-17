using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class WallFollow : MonoBehaviour
{
    public GameObject path;

    public float startDelay = 3.0f;
    public float chaseSpeed = 1.0f;
    public TextMeshProUGUI startDelayText;
    private BezierCurveGenerator _bezierCurveGenerator;

    private float _startDelayTimer = 0.0f;
    
    
    private int routeNumber = 0;

    private float _tValue;

    private bool _begin = false;
    // Start is called before the first frame update
    void Start()
    {
        _bezierCurveGenerator = path.GetComponent<BezierCurveGenerator>();
        routeNumber = 0;
        _tValue = 0.0f;
        _startDelayTimer = startDelay;
        _begin = false;
    }

    public float GetStartDelayTimer()
    {
        return _startDelayTimer;
    }
    
    // Update is called once per frame
    void Update()
    {
     
        if (_begin)
        {
            _tValue += Time.deltaTime * chaseSpeed;

            Vector3 oldPosition = transform.position;

            var newPosition = Mathf.Pow(1 - _tValue, 3) * _bezierCurveGenerator.controlPoints[routeNumber].position +
                              3 * Mathf.Pow(1 - _tValue, 2) * _tValue * _bezierCurveGenerator.controlPoints[routeNumber + 1].position +
                              3 * (1 - _tValue) * Mathf.Pow(_tValue, 2) * _bezierCurveGenerator.controlPoints[routeNumber + 2].position +
                              Mathf.Pow(_tValue, 3) * _bezierCurveGenerator.controlPoints[routeNumber + 3].position;

            transform.position = newPosition;

            Vector3 difference = newPosition - oldPosition;

            float rotationAngle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, rotationAngle, 0);

            if (_tValue > 1)
            {
                //Debug.Log("_tValue is > 1");
                _tValue = 0.0f;
                routeNumber += 3;
                if (routeNumber >= _bezierCurveGenerator.controlPoints.Count - 1)
                {
                    routeNumber = 0;
                }
            }
        }
        else
        {
            _startDelayTimer = _startDelayTimer <= 0 ? 0 : _startDelayTimer - Time.deltaTime;
            startDelayText.text = ((int)_startDelayTimer).ToString();
            if (_startDelayTimer < 1)
            {
                StartCoroutine(RemoveDelayText());
                _startDelayTimer = startDelay;
                _begin = true;
            }
        }

    }

    private IEnumerator RemoveDelayText()
    {
        startDelayText.text = "GO";
        yield return new WaitForSeconds(2);
        startDelayText.text = "";
    }

}
