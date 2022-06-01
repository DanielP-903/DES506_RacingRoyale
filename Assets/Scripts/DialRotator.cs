using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialRotator : MonoBehaviour
{
    private float _rotationalValue = 0;
    public PlayerHUDController hudRef;
    private RectTransform _rectTransform;
    public Vector3 endAngles = new Vector3(0, 0, 90);

    private Quaternion _toAngle;
    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _toAngle = Quaternion.Euler(endAngles);
    }

    private void Update()
    {
        _rotationalValue = Mathf.Clamp(hudRef.currentSpeed, 0, 120);
        _toAngle = new Quaternion(0, 0, (-_rotationalValue) * Mathf.Deg2Rad + (endAngles.z * Mathf.Deg2Rad), 1);
        _rectTransform.rotation = Quaternion.Lerp(_rectTransform.rotation, _toAngle, 10*Time.deltaTime);
    }
}