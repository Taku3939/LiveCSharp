using UnityEngine;

public class Origin : MonoBehaviour
{
    private const float Range = 127f;

    [SerializeField] private Color color;
    [SerializeField] private bool visible;

    public void OnDrawGizmos()
    {
        if(!visible) return;
        Gizmos.color = color;
        Gizmos.DrawCube(this.transform.position, Vector3.one * Range);
    }

}
