using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpNova : MonoBehaviour {

    public ParticleSystem impNova;
    public ParticleSystem explosion;
    public float force = 0.005f;

    List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();


	void Start () {

	}

    private void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(impNova, other, collisionEvents);

        for (int i = 0; i < collisionEvents.Count; i++)
        {

            if (other.gameObject.tag == "Player")
            {
                Debug.Log("Collided with " + other.gameObject.name);

                var explode = Instantiate(explosion, null, true) as ParticleSystem;
                explode.transform.position = other.transform.position;
                explode.Play();

                var player = PlayerController.instance;
                player.interruptMovement = true;
                Vector3 dir = collisionEvents[i].intersection - transform.position;
                dir = dir.normalized;

                player.GetComponent<Rigidbody2D>().AddRelativeForce(dir * force);

                StartCoroutine(returnMovement());
            }

            else if (other.gameObject.tag == "ProjectileSurface")
            {
                Debug.Log("Collided with " + other.gameObject.name);

                var explode = Instantiate(explosion, null, true) as ParticleSystem;
                explode.transform.position = other.transform.position;
                explode.Play();
            }

        }
    }

    // amount of time player is interupted when being hit by a boss projectile
    IEnumerator returnMovement()
    {
        yield return new WaitForSeconds(0.2f);

        PlayerController.instance.interruptMovement = false;
    }
}
