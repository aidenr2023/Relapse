using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GenericGun))]
public class GunFovZoom : MonoBehaviour
{
    [SerializeField] private GunEventVariable gunEventVariable;
    [SerializeField] private FloatVariable gunFovZoomAmount;

    [SerializeField, Range(0, 1)] private float targetZoom = .8f;
    [SerializeField, Min(0)] private float inDuration = 0.125f;
    [SerializeField] private AnimationCurve inCurve;

    private GenericGun _attachedGun;
    private bool _isBound;

    private Coroutine _zoomCoroutine;
    private float _modifier;

    private void Awake()
    {
        _modifier = 1;

        // Get the attached gun
        _attachedGun = GetComponent<GenericGun>();

        _attachedGun.OnEquip += BindToGun;
        _attachedGun.OnDequip += RemoveFromGun;
    }

    private void BindToGun(IGun gun)
    {
        _isBound = true;

        gunEventVariable += StartZoom;
    }

    private void RemoveFromGun(IGun gun)
    {
        _isBound = false;

        gunEventVariable -= StartZoom;
    }

    private void StartZoom(IGun gun)
    {
        // If the zoom coroutine is already running, stop it
        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
            _zoomCoroutine = null;
        }

        _zoomCoroutine = StartCoroutine(ZoomCoroutine());
    }

    private IEnumerator ZoomCoroutine()
    {
        var startTime = Time.time;
        
        // Zoom in based on the curve
        while (Time.time - startTime < inDuration)
        {
            _modifier = inCurve.Evaluate((Time.time - startTime) / inDuration) * targetZoom;
            yield return null;
        }
        
        _modifier = targetZoom;

        // Unzoom 
        while (!Mathf.Approximately(_modifier, 1))
        {
            _modifier = Mathf.Lerp(_modifier, 1,
                CustomFunctions.FrameAmount(.1f, false, false)
            );

            yield return null;
        }

        _modifier = 1;
    }

    private void Update()
    {
        if (_isBound)
            gunFovZoomAmount.value = Mathf.Clamp(_modifier, targetZoom, 1);
    }
}