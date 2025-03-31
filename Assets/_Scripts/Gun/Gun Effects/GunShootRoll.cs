using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GenericGun))]
public class GunShootRoll : MonoBehaviour
{
    [SerializeField] private GunEventVariable gunEventVariable;
    [SerializeField] private FloatVariable gunShootRoll;

    [SerializeField] private float targetAngle = 5f;
    [SerializeField, Min(0)] private float inDuration = 0.125f;
    [SerializeField] private AnimationCurve inCurve;

    private GenericGun _attachedGun;
    private bool _isBound;

    private Coroutine _rollCoroutine;
    private float _modifier;

    private void Awake()
    {
        _modifier = 0;

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
        if (_rollCoroutine != null)
        {
            StopCoroutine(_rollCoroutine);
            _rollCoroutine = null;
        }

        _rollCoroutine = StartCoroutine(RollCoroutine());
    }

    private IEnumerator RollCoroutine()
    {
        var startTime = Time.time;
        
        var cTarget = UnityEngine.Random.Range(-targetAngle, targetAngle);

        // Zoom in based on the curve
        while (Time.time - startTime < inDuration)
        {
            _modifier = inCurve.Evaluate((Time.time - startTime) / inDuration) * cTarget;
            yield return null;
        }

        _modifier = cTarget;

        // Unroll
        while (!Mathf.Approximately(_modifier, 1))
        {
            _modifier = Mathf.Lerp(_modifier, 1,
                CustomFunctions.FrameAmount(.1f, false, false)
            );

            yield return null;
        }

        _modifier = 0;
    }

    private void Update()
    {
        // if (_isBound)
        //     gunShootRoll.value = Mathf.Clamp(_modifier, targetAngle, 1);
        
        gunShootRoll.value = _modifier;
    }
}