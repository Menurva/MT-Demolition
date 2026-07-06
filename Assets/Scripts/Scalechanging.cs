using UnityEngine;

public class Scalechanging : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private float reducedScaleMultiplier = 0.2f;
    [SerializeField] private float growScaleMultiplier = 4f;

 private Vector3 originalScale;
    private bool isGrown;
    private bool isReduced;

    private void Start()
    {
     originalScale = transform.localScale;

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

        if (Input.GetKeyDown(KeyCode.K))
        {
            isGrown = !isGrown;
            playerObject.transform.localScale = isGrown ? originalScale * growScaleMultiplier : originalScale;

            if (!isGrown)
            {
                Vector3 currentPosition = playerObject.transform.position;
            }
        }
    } //delete this braket to active the previous code line
  
       /*
           if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleGrowScale();
        }
    }
     private void ToggleGrowScale()
    {
        Vector3 currentPosition = playerObject.transform.position;

        isGrown = !isGrown;
        transform.localScale = isGrown ? originalScale * growScaleMultiplier : originalScale;

       // float heightDifference = transform.localScale.y - previousScale.y;
        //transform.position += Vector3.up * (heightDifference * 0.5f);
    } */
}
