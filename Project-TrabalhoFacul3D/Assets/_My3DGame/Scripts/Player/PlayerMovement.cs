using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float laneSwitchSpeed = 15f;
    [SerializeField] private int numLanes = 3;
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
        
        // Desliga a gravidade para este Rigidbody (não afeta outros objetos)
        rb.useGravity = false;
        // Congela Z (frente) e Y (altura) para evitar qualquer movimento vertical/penetrações
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        // melhorar física para reduzir jitter interpenetration
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        if (!alive) return;

        HandleInput();
        MovePlayerLaterally();
    }

    private void HandleInput()
    {
        // Movimento lateral (esquerda/direita)
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentLane = Mathf.Max(0, currentLane - 1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentLane = Mathf.Min(numLanes - 1, currentLane + 1);
        }

        // Calcula a posição X do lane
        float targetX = (currentLane - 1) * laneDistance;
        // mantém a altura Y atual (não mover em Y)
        targetPosition = new(targetX, transform.position.y, transform.position.z);
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

    public void Die()
    {
        GameManager.PlayerDied();
    }
}