using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class impGiantFireballLauncher : MonoBehaviour {

    public GameObject giantFireball;
    public float projectileSpeed;
    Transform centerOfRotation;

    public float delay;
    public float timer;

	void Start () {
        centerOfRotation = transform.parent;
	}
	
	void Update () {

        timer -= Time.deltaTime;

        if (timer < 0)
        {
            Vector2 direction = new Vector2(centerOfRotation.position.x - transform.position.x, centerOfRotation.position.y - transform.position.y);
            direction.Normalize();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GameObject projectile = Instantiate(giantFireball, null);

            //var projectileScript = projectile.GetComponent<enemy_Projectile>();
            //projectileScript.MakeProjectileReady();

            projectile.transform.position = transform.position;
            projectile.transform.rotation = Quaternion.identity;

            projectile.transform.Rotate(0, 0, angle, Space.World);

            projectile.GetComponent<Rigidbody2D>().AddForce(-direction * projectileSpeed);

            timer = delay;
        }




    }
}
