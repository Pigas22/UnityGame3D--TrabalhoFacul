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
        transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Sin(Time.time * 10f) * 0.25f + 1f,
            transform.position.z
        );
    }
}