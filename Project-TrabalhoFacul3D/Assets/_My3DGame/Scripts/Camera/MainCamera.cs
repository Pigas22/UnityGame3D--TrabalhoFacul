using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -8);
    [SerializeField] private float followSpeed = 7f;
    
    private Transform playerTransform;

    private void Start()
    {
        Transform playerObject = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerObject == null)
        {
            Debug.LogError("Player not found! Make sure the Player has the 'Player' tag.");
            return;
        }
        playerTransform = playerObject;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Câmera segue apenas a posição X do player (para movimento lateral)
        Vector3 desiredPosition = playerTransform.position + offset;
        desiredPosition.z = transform.position.z; // Z da câmera fica fixo

        transform.position = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            followSpeed * Time.deltaTime
        );
    }
}