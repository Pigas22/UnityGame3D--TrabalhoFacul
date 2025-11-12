using UnityEngine;

public class GroundTile : MonoBehaviour
{
    [SerializeField] private float tileLength = 10f;
    public float GetTileLength() => tileLength;
}