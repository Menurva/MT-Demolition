using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarWallDamage : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        DestructibleWall wall = collision.collider.GetComponentInParent<DestructibleWall>();
        if (wall == null)
        {
            return;
        }

        Vector3 impactPosition = collision.contactCount > 0
            ? collision.GetContact(0).point
            : collision.collider.ClosestPoint(transform.position);

        wall.TakeCarImpactDamage(
            collision.relativeVelocity.magnitude,
            impactPosition);
    }
}

// CarWallDamage reads collisions from the car Rigidbody and forwards each wall impact to the matching DestructibleWall parent.
