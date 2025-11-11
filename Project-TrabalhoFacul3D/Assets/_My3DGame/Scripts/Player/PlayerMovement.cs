using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float laneSwitchSpeed = 15f;
    [SerializeField] private int numLanes = 3;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float laneDistance = 5f;
    private int currentLane = 1;
    private Vector3 targetPosition;
    private Rigidbody rb;
    private bool alive = true;

    private void Start()
    {
        // Player sempre na mesma posição Z
        targetPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        
        // Congela a posição Z para o player não se mover para frente
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (!alive) return;

        HandleInput();
        MovePlayerLaterally();
    }

    void LateUpdate()
    {
        CheckAxeY();
    }

    private void HandleInput()
    {
        // Movimento lateral (esquerda/direita)
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentLane = Mathf.Max(0, currentLane - 1);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            currentLane = Mathf.Min(numLanes - 1, currentLane + 1);
        }

        // Pulo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Calcula a posição X do lane
        float targetX = (currentLane - 1) * laneDistance;
        targetPosition = new Vector3(targetX, targetPosition.y, transform.position.z);
    }

    private void Jump()
    {
        if (transform.position.y <= 0.01f)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }
    }

    private void MovePlayerLaterally()
    {
        // Apenas movimento lateral (X), Z fica congelado
        Vector3 newPosition = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            laneSwitchSpeed * Time.deltaTime
        );
        
        transform.position = newPosition;
    }

    private void CheckAxeY()
    {
        if (transform.position.y < -0.01f)
        {
            transform.position = new(transform.position.x, 0, transform.position.z);
        }
    }

    public void Die()
    {
        GameManager.PlayerDied();
    }
}