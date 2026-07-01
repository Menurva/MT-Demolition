using UnityEngine;

public class Scalechanging : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private float reducedScaleMultiplier = 0.2f;

    private Vector3 originalScale;

    private bool isReduced;

    private void Start()
    {
        if (playerObject == null)
        {
            playerObject = gameObject;
        }

        originalScale = playerObject.transform.localScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            isReduced = !isReduced;
            playerObject.transform.localScale = isReduced ? originalScale * reducedScaleMultiplier : originalScale;

            if (!isReduced)
            {
                Vector3 currentPosition = playerObject.transform.position;
            }
        }
    }
}
