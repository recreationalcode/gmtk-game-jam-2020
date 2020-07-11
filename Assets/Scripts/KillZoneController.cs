using UnityEngine;

public class KillZoneController : MonoBehaviour
{
  public PlayerController playerController;

  private PolygonCollider2D polygonCollider;

  void Start()
  {
    polygonCollider = GetComponent<PolygonCollider2D>();
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    playerController.Kill(other.gameObject.GetComponent<HumanController>());
  }
}