using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 10f;   // Velocidade para frente (Z)
    [SerializeField] private float laneDistance = 5f;    // Distância entre as faixas
    [SerializeField] private float laneSwitchSpeed = 15f; // Velocidade de troca de faixa
    [SerializeField] private int numLanes = 3;           // Número total de faixas
    [SerializeField] private float jumpForce = 7f;

    private int currentLane = 1; // 0 = esquerda, 1 = meio, 2 = direita
    private Vector3 targetPosition;
    private Rigidbody rb;

    private void Start()
    {
        // Começa na faixa do meio
        targetPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInput();
        MovePlayer();
    }

    private void HandleInput()
    {
        // Movimentação lateral (A = esquerda / D = direita)
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentLane = Mathf.Max(0, currentLane - 1);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            currentLane = Mathf.Min(numLanes - 1, currentLane + 1);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Calcula a posição alvo com base na faixa atual
        float targetX = (currentLane - 1) * laneDistance;
        targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    private void Jump()
    {
        // Verifica se o player está "no chão" (sem considerar pequenas variações de flutuação)
        if (transform.position.y <= 0.01f)
        {
            // Mantém a velocidade atual em X e Z e aplica força no Y
            Vector3 vel = rb.linearVelocity;
            vel.y += jumpForce;
            rb.linearVelocity = vel;
        }
    }

    private void MovePlayer()
    {
        // Movimento para frente constante
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // Movimento lateral suave até a posição alvo
        Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, laneSwitchSpeed * Time.deltaTime);
        transform.position = newPosition;
    }
}
