using UnityEngine;

public class BreakableSecond : MonoBehaviour
{
    [SerializeField] private GameObject _replacementObject;
    [SerializeField] private float _breakforce = 2f;
    [SerializeField] private float _collisionmultiplier = 100f;
    [SerializeField] private bool _broken;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnCollisionEnter(Collision collision)
    {
        if (_broken) return;
        if (collision.relativeVelocity.magnitude >= _breakforce)
        {
            _broken = true;
          var replacement = Instantiate(_replacementObject, transform.position, transform.rotation);

          var rbs = replacement.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                rb.AddExplosionForce(collision.relativeVelocity.magnitude * _collisionmultiplier, collision.contacts[0].point, 2f);
            }
            Destroy(gameObject);
        }
    }
}
