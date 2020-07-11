using UnityEngine;

public class ProjectileController : MonoBehaviour
{
  void Start()
  {
    Destroy(gameObject, 5.0f);
  }

  void OnCollisionEnter2D(Collision2D other)
  {
    Destroy(gameObject);
  }
}