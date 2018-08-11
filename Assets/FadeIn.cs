using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour {

    [SerializeField] SpriteRenderer shadow;
    [SerializeField] float waitForSeconds;
    [SerializeField] float increment;
    [SerializeField] bool doneFading;

	void Start ()
    {
        shadow = GetComponent<SpriteRenderer>();
        StartCoroutine(ShadowFadeIn());
	}
	

    IEnumerator ShadowFadeIn()
    {
        while (!doneFading)
        {
            Color tmp = shadow.color;
            tmp.a += increment;
            shadow.color = tmp;


            if (shadow.color.a > 0.41f)
            {
                doneFading = true;
            }

            yield return new WaitForSeconds(waitForSeconds);

        }
    }
}
