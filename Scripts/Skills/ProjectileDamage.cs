using UnityEngine;
using System.Collections.Generic;

public class ProjectileDamage : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public float lifeTime = 5f;
    [HideInInspector] public bool isSingleTarget = true;

    private List<GameObject> hitEnemies = new List<GameObject>();

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }

        if (isSingleTarget)
        {
            if (other.CompareTag("Enemy"))
            {
                if (other.TryGetComponent<EnemyStats>(out var enemy))
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            if (other.CompareTag("Enemy"))
            {
                if (!hitEnemies.Contains(other.gameObject))
                {
                    EnemyStats enemy = other.GetComponent<EnemyStats>();

                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                        hitEnemies.Add(other.gameObject);
                    }
                }
            }
            // else if (other.CompareTag("Wall") || other.CompareTag("Environment"))
            // {
            //     Destroy(gameObject);
            // }
        }
    }
}
