using UnityEngine;
using System.Collections;

public class Point : MonoBehaviour
{

        public float mass = 1.0f;
        public Vector3 force{ get; private set; }
        public Vector3 acceleration;

        public Vector3 position;
        public Vector3 oldPosition;

        public Vector3 velocity;



    public Point(Vector3 initialPos) {
      position = initialPos;
      oldPosition = initialPos;
    }

    public void setPosition(Vector3 pos) {
      position = pos;
    }

    public void setOldPosition(Vector3 oldPos) {
      oldPosition = oldPos;
    }

    public Vector3 getPosition() {
      return position;
    }


    public void ApplyForce(Vector3 appliedForce)
    {
        force += appliedForce;
    }

    public void ClearForce()
    {
        force = Vector3.zero;
    }


}
