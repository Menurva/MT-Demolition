using UnityEngine;

public class Playermovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float growScaleMultiplier = 4f;

    private Vector3 originalScale;
    private bool isGrown;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleGrowScale();
        }

        LogMovementKeyPresses();
    }

    private void ToggleGrowScale()
    {
        Vector3 previousScale = transform.localScale;

        isGrown = !isGrown;
        transform.localScale = isGrown ? originalScale * growScaleMultiplier : originalScale;

        float heightDifference = transform.localScale.y - previousScale.y;
        transform.position += Vector3.up * (heightDifference * 0.5f);
    }

    private void LogMovementKeyPresses()
    {
        if (Input.GetKeyDown(KeyCode.W)) Debug.Log("W key has been pressed.");
        if (Input.GetKeyDown(KeyCode.A)) Debug.Log("A key has been pressed.");
        if (Input.GetKeyDown(KeyCode.S)) Debug.Log("S key has been pressed.");
        if (Input.GetKeyDown(KeyCode.D)) Debug.Log("D key has been pressed.");
    }
}
