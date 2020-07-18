using UnityEngine;

public class KillZoneController : MonoBehaviour
{
  public PlayerController playerController;

  void OnTriggerEnter2D(Collider2D other)
  {
    RaycastHit2D hit = Physics2D.Linecast(transform.position, other.transform.position);

    if (hit.collider != null)
    {
      if (hit.transform.gameObject.layer == 8 || hit.transform.gameObject.layer == 9)
      {
        playerController.Kill(other.gameObject.GetComponent<HumanController>());
      }
    }
  }
}