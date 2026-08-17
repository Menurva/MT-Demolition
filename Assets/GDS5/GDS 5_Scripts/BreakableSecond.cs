using UnityEngine;

public class BreakableSecond : MonoBehaviour
{
    [SerializeField] private GameObject _replacementObject;
    [SerializeField] private float _breakforce = 2f;
    [SerializeField] private float _collisionmultiplier = 100f;
    [SerializeField] private bool _preserveReplacementScale;
    [SerializeField] private bool _broken;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnCollisionEnter(Collision collision)
    {
        if (_broken) return;
        if (collision.relativeVelocity.magnitude >= _breakforce)
        {
            if (_replacementObject == null)
            {
                Debug.LogWarning($"{nameof(BreakableSecond)} has no replacement object assigned.", this);
                return;
            }

            _broken = true;
            Vector3 originalWorldScale = transform.lossyScale;
            var replacement = Instantiate(_replacementObject, transform.position, transform.rotation);
            if (!_preserveReplacementScale)
            {
                replacement.transform.localScale = originalWorldScale;
            }

            var rbs = replacement.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                rb.AddExplosionForce(collision.relativeVelocity.magnitude * _collisionmultiplier, collision.contacts[0].point, 2f);
            }
            Destroy(gameObject);
        }
    }
}

// BreakableSecond replaces a hit object, optionally keeps the replacement prefab's saved scale, pushes the fractured pieces, and removes the intact object.
