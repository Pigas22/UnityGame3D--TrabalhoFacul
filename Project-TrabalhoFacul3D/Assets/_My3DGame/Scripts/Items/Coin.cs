using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 90f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.IncrementScoreText();
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        transform.Rotate(0, 0, turnSpeed * Time.deltaTime);
    }
}