using UnityEngine;

public class WheelDisplacementOnTrigger : MonoBehaviour
{
    [Header("Car Speed")]
    [SerializeField] private PrometeoCarController carController;
    [SerializeField] private int displacedMaxSpeed = 120;
    [SerializeField] private int displacedMaxReverseSpeed = 60;
    [SerializeField] private int displacedAccelerationMultiplier = 5;

    [Header("Wheels")]
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;

    [Header("Effects")]
    [SerializeField] private Transform frontLeftEffect;
    [SerializeField] private Transform frontRightEffect;
    [SerializeField] private Transform rearLeftEffect;
    [SerializeField] private Transform rearRightEffect;

    [Header("Target Local Positions")]
    [SerializeField] private Vector3 frontLeftTargetLocalPosition;
    [SerializeField] private Vector3 frontRightTargetLocalPosition;
    [SerializeField] private Vector3 rearLeftTargetLocalPosition;
    [SerializeField] private Vector3 rearRightTargetLocalPosition;

    private Vector3 frontLeftOriginalLocalPosition;
    private Vector3 frontRightOriginalLocalPosition;
    private Vector3 rearLeftOriginalLocalPosition;
    private Vector3 rearRightOriginalLocalPosition;
    private Vector3 frontLeftEffectOriginalLocalPosition;
    private Vector3 frontRightEffectOriginalLocalPosition;
    private Vector3 rearLeftEffectOriginalLocalPosition;
    private Vector3 rearRightEffectOriginalLocalPosition;
    private int originalMaxSpeed;
    private int originalMaxReverseSpeed;
    private int originalAccelerationMultiplier;
    private bool isDisplaced;

    private void Start()
    {
        StoreOriginalCarSpeed();
        StoreOriginalPositions();
        WarnForMissingAssignments();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleWheelDisplacement();
        }
    }

    private void StoreOriginalPositions()
    {
        if (frontLeftWheel != null)
        {
            frontLeftOriginalLocalPosition = frontLeftWheel.localPosition;
        }

        if (frontRightWheel != null)
        {
            frontRightOriginalLocalPosition = frontRightWheel.localPosition;
        }

        if (rearLeftWheel != null)
        {
            rearLeftOriginalLocalPosition = rearLeftWheel.localPosition;
        }

        if (rearRightWheel != null)
        {
            rearRightOriginalLocalPosition = rearRightWheel.localPosition;
        }

        if (frontLeftEffect != null)
        {
            frontLeftEffectOriginalLocalPosition = frontLeftEffect.localPosition;
        }

        if (frontRightEffect != null)
        {
            frontRightEffectOriginalLocalPosition = frontRightEffect.localPosition;
        }

        if (rearLeftEffect != null)
        {
            rearLeftEffectOriginalLocalPosition = rearLeftEffect.localPosition;
        }

        if (rearRightEffect != null)
        {
            rearRightEffectOriginalLocalPosition = rearRightEffect.localPosition;
        }
    }

    private void StoreOriginalCarSpeed()
    {
        if (carController == null)
        {
            return;
        }

        originalMaxSpeed = carController.maxSpeed;
        originalMaxReverseSpeed = carController.maxReverseSpeed;
        originalAccelerationMultiplier = carController.accelerationMultiplier;
    }

    private void WarnForMissingAssignments()
    {
        if (carController == null)
        {
            Debug.LogWarning("Car Controller is not assigned on WheelDisplacementOnTrigger. Wheel movement will work, but car speed will not change.", this);
        }

        if (frontLeftWheel == null)
        {
            Debug.LogWarning("Front Left Wheel is not assigned on WheelDisplacementOnTrigger.", this);
        }

        if (frontRightWheel == null)
        {
            Debug.LogWarning("Front Right Wheel is not assigned on WheelDisplacementOnTrigger.", this);
        }

        if (rearLeftWheel == null)
        {
            Debug.LogWarning("Rear Left Wheel is not assigned on WheelDisplacementOnTrigger.", this);
        }

        if (rearRightWheel == null)
        {
            Debug.LogWarning("Rear Right Wheel is not assigned on WheelDisplacementOnTrigger.", this);
        }
    }

    private void ToggleWheelDisplacement()
    {
        isDisplaced = !isDisplaced;

        SetWheelLocalPosition(frontLeftWheel, frontLeftOriginalLocalPosition, frontLeftTargetLocalPosition);
        SetWheelLocalPosition(frontRightWheel, frontRightOriginalLocalPosition, frontRightTargetLocalPosition);
        SetWheelLocalPosition(rearLeftWheel, rearLeftOriginalLocalPosition, rearLeftTargetLocalPosition);
        SetWheelLocalPosition(rearRightWheel, rearRightOriginalLocalPosition, rearRightTargetLocalPosition);

        SetEffectLocalPosition(frontLeftEffect, frontLeftEffectOriginalLocalPosition, frontLeftOriginalLocalPosition, frontLeftTargetLocalPosition);
        SetEffectLocalPosition(frontRightEffect, frontRightEffectOriginalLocalPosition, frontRightOriginalLocalPosition, frontRightTargetLocalPosition);
        SetEffectLocalPosition(rearLeftEffect, rearLeftEffectOriginalLocalPosition, rearLeftOriginalLocalPosition, rearLeftTargetLocalPosition);
        SetEffectLocalPosition(rearRightEffect, rearRightEffectOriginalLocalPosition, rearRightOriginalLocalPosition, rearRightTargetLocalPosition);

        ApplyCarSpeed();
    }

    private void SetWheelLocalPosition(Transform wheel, Vector3 originalLocalPosition, Vector3 targetLocalPosition)
    {
        if (wheel == null)
        {
            return;
        }

        wheel.localPosition = isDisplaced ? targetLocalPosition : originalLocalPosition;
    }

    private void SetEffectLocalPosition(Transform effect, Vector3 originalEffectLocalPosition, Vector3 originalWheelLocalPosition, Vector3 targetWheelLocalPosition)
    {
        if (effect == null)
        {
            return;
        }

        var wheelDisplacementOffset = targetWheelLocalPosition - originalWheelLocalPosition;
        effect.localPosition = isDisplaced ? originalEffectLocalPosition + wheelDisplacementOffset : originalEffectLocalPosition;
    }

    private void ApplyCarSpeed()
    {
        if (carController == null)
        {
            return;
        }

        carController.maxSpeed = isDisplaced ? displacedMaxSpeed : originalMaxSpeed;
        carController.maxReverseSpeed = isDisplaced ? displacedMaxReverseSpeed : originalMaxReverseSpeed;
        carController.accelerationMultiplier = isDisplaced ? displacedAccelerationMultiplier : originalAccelerationMultiplier;
    }
}
