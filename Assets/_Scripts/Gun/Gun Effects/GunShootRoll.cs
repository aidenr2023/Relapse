using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GenericGun))]
public class GunShootRoll : MonoBehaviour
{
    [SerializeField] private GunEventVariable gunEventVariable;
    [SerializeField] private FloatVariable gunShootRoll;

    [SerializeField] private float targetAngle = 5f;
    [SerializeField, Min(0)] private float inDuration = 0.125f;
    [SerializeField, Min(0.0001f)] private float lerpAmount = .1f;
    [SerializeField] private AnimationCurve inCurve;

    private GenericGun _attachedGun;

    private Coroutine _rollCoroutine;
    private float _modifier;

    private void Awake()
    {
        SetModifier(0);

        // Get the attached gun
        _attachedGun = GetComponent<GenericGun>();

        _attachedGun.OnEquip += BindToGun;
        _attachedGun.OnDequip += RemoveFromGun;
    }

    private void BindToGun(IGun gun)
    {
        gunEventVariable += StartZoom;
    }

    private void RemoveFromGun(IGun gun)
    {
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
            var newValue = inCurve.Evaluate((Time.time - startTime) / inDuration) * cTarget;
            SetModifier(newValue);

            yield return null;
        }

        SetModifier(cTarget);

        // Unroll
        while (!Mathf.Approximately(_modifier, 0))
        {
            var newValue = Mathf.Lerp(_modifier, 0, CustomFunctions.FrameAmount(lerpAmount, false, false));
            SetModifier(newValue);

            yield return null;
        }

        SetModifier(0);
    }

    private void SetModifier(float value)
    {
        _modifier = value;
        gunShootRoll.value = _modifier;
    }
}