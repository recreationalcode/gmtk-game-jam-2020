using UnityEngine;

public class KillZoneController : MonoBehaviour
{
  public PlayerController playerController;

  private CircleCollider2D circleCollider;

  void Start()
  {
    circleCollider = GetComponent<CircleCollider2D>();
  }

  void Update()
  {
    Collider2D human = Physics2D.OverlapCircle(transform.position, circleCollider.radius, LayerMask.GetMask("Humans"));

    if (human != null)
    {
      HandleTrigger(human);
    }
  }

  void HandleTrigger(Collider2D other)
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

  void OnTriggerEnter2D(Collider2D other)
  {
    HandleTrigger(other);
  }

  void OnTriggerStay2D(Collider2D other)
  {
    HandleTrigger(other);
  }

  void OnTriggerExit2D(Collider2D other)
  {
    if (other.gameObject.layer == 8)
    {
      playerController.Forget(other.gameObject.GetComponent<HumanController>());
    }
  }
}