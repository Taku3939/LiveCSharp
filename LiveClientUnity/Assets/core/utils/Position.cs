using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Position : MonoBehaviour
{
  //  [SerializeField] private Transform actor, chara;
    [SerializeField] private Transform origin;
    private const float Range = 127f;

    public void OnDrawGizmos()
    {
        if(origin == null) return;
        //Gizmos.DrawLine(origin.position, );
        Gizmos.DrawCube(origin.position, Vector3.one * Range);
        Gizmos.color = Color.blue;
    }
    public Vector3 GetPosition(Vector3 pos)
    {
        pos = pos;
        var x = (int) Mathf.Clamp( pos.x, -Range, Range);
        var y = (int) Mathf.Clamp( pos.y, -Range, Range);
        var z = (int) Mathf.Clamp( pos.z, -Range, Range);
        return new Vector3(x, y, z);
    }

    public Vector4 GetRotation(Vector4 rot)
    {
        var x = (int) Mathf.Clamp( rot.x * 100f, -100f, 100f);
        var y = (int) Mathf.Clamp(rot.y * 100f, -100f, 100f);
        var z = (int) Mathf.Clamp(rot.z * 100f, -100f, 100f);
        var w = (int) Mathf.Clamp(rot.w * 100f, -100f, 100f);
        return new Vector4(x, y, z, w);
    }

    // public void SetPosition(Vector3 pos)
    // {
    //     pos += origin.position;
    //     actor.position = origin.position;
    // }
    //
    // public void SetRotation(Vector4 rot)
    // {
    //     rot *= 0.01f;
    //     actor.rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w);
    // }


    public float GetRadY()
    {
        return 0;
    }

}
