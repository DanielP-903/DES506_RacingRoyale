using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private MeshRenderer _meshRendererClip;
    private MeshCollider _meshCollider;
    private static readonly int FadeOutSlider = Shader.PropertyToID("FadeOutSlider");
    private float dissolveTimer;
    private bool _canDetect = true;

    //private GameObject _checkpoints;
    private CheckpointSystem _cs;
    
    public bool dissolve = false;
    public bool isCheckpoint = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (isCheckpoint)
        {
            _cs = transform.parent.Find("CheckpointSystem").GetComponent<CheckpointSystem>();
            //_checkpoints = transform.parent.Find("CheckpointSystem").gameObject;
            _canDetect = true;
            dissolve = false;
        }
        else
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRendererClip = transform.parent.GetChild(0).GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            dissolveTimer = 0.0f;
            _canDetect = true;
            dissolve = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dissolve)
        {
            dissolveTimer += Time.deltaTime;
            dissolveTimer = Mathf.Clamp(dissolveTimer, 0, 3);
            float dissolveFloat = Mathf.Lerp(1, 0, (3 - dissolveTimer) / 3);
            foreach (var material in _meshRenderer.materials)
            {
                material.SetFloat(FadeOutSlider, dissolveFloat);
            }
            if (_meshRendererClip != null)
            {
                _meshRendererClip.materials[0].SetFloat(FadeOutSlider, dissolveFloat);
            }
            if (dissolveFloat >= 0.5f)
            {
                _meshCollider.enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EliminationZone") && _canDetect)
        {
            if (isCheckpoint)
            {
                Debug.Log("Eliminated Checkpoint");
                _cs.EliminateCheckpoint(this.gameObject);
            }
            else
            {
                StartCoroutine(DelayDissolve());
            }
        }
    }

    private IEnumerator DelayDissolve()
    {
        _canDetect = false;
        yield return new WaitForSeconds(3);
        dissolve = true;
        dissolveTimer = 0.0f;
    }
}
