using UnityEngine;

public class InputMove : MonoBehaviour
{
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var x = Input.GetAxis("Horizontal") * 10;
        var z = Input.GetAxis("Vertical") * 10;
        var vel = rb.velocity;
        vel.x = x;
        vel.z = z;
        rb.velocity = vel;
    }
}
