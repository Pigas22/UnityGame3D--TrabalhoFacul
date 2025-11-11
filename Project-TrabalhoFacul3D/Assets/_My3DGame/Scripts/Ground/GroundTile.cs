using UnityEngine;

public class GroundTile : MonoBehaviour
{
    [SerializeField] private float tileLength = 10f;
    [SerializeField] private Material groundMaterial;

    public float GetTileLength() => tileLength;
}