using UnityEngine;

public class Playermovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;


   

    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);


    }

    private void LogMovementKeyPresses()
    {
        if (Input.GetKeyDown(KeyCode.W)) Debug.Log("W key has been pressed.");
        if (Input.GetKeyDown(KeyCode.A)) Debug.Log("A key has been pressed.");
        if (Input.GetKeyDown(KeyCode.S)) Debug.Log("S key has been pressed.");
        if (Input.GetKeyDown(KeyCode.D)) Debug.Log("D key has been pressed.");
    }
}
