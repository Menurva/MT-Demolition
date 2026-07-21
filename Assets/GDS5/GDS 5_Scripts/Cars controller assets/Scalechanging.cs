using UnityEngine;

public class Scalechanging : MonoBehaviour
{
    [Header("Scale Targets")]
    [SerializeField] private Transform[] wheelTargets = new Transform[4];
    [SerializeField] private WheelCollider[] colliderTargets = new WheelCollider[4];
    [SerializeField] private float reducedScaleMultiplier = 0.2f;
    [SerializeField] private float growScaleMultiplier = 4f;

    private Vector3[] originalWheelScales;
    private float[] originalColliderRadii;
    private bool isGrown;
    private bool isReduced;

    private void Start()
    {
        originalWheelScales = StoreOriginalScales(wheelTargets);
        originalColliderRadii = StoreOriginalRadii(colliderTargets);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isReduced = !isReduced;
            isGrown = false;

            ApplyScale(isReduced ? reducedScaleMultiplier : 1f);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            isGrown = !isGrown;
            isReduced = false;

            ApplyScale(isGrown ? growScaleMultiplier : 1f);
        }
    }

    private Vector3[] StoreOriginalScales(Transform[] targets)
    {
        if (targets == null)
        {
            return new Vector3[0];
        }

        Vector3[] originalScales = new Vector3[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                originalScales[i] = targets[i].localScale;
            }
        }

        return originalScales;
    }

    private float[] StoreOriginalRadii(WheelCollider[] targets)
    {
        if (targets == null)
        {
            return new float[0];
        }

        float[] originalRadii = new float[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                originalRadii[i] = targets[i].radius;
            }
        }

        return originalRadii;
    }

    private void ApplyScale(float scaleMultiplier)
    {
        ApplyScaleToTargets(wheelTargets, originalWheelScales, scaleMultiplier);
        ApplyRadiusToTargets(colliderTargets, originalColliderRadii, scaleMultiplier);
    }

    private void ApplyScaleToTargets(Transform[] targets, Vector3[] originalScales, float scaleMultiplier)
    {
        if (targets == null || originalScales == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].localScale = originalScales[i] * scaleMultiplier;
            }
        }
    }

    private void ApplyRadiusToTargets(WheelCollider[] targets, float[] originalRadii, float scaleMultiplier)
    {
        if (targets == null || originalRadii == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].radius = originalRadii[i] * scaleMultiplier;
            }
        }
    }

}
