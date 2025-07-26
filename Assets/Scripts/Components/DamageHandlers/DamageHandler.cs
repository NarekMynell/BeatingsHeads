using UnityEngine;

public abstract class DamageHandler<T> : MonoBehaviour where T : DamageDealer
{
    [SerializeField] protected Range _damageRange;

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out T damageDealer))
        {
            OnDamaged(collision, damageDealer);
        }
    }

    protected abstract void OnDamaged(Collision collision,T damageDealer);
}
