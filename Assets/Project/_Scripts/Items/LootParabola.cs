using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootParabola : MonoBehaviour {

    Vector2 startPosition;
    Vector2 endPosition;
    float height;
    private float timeToDestination;

    public Vector2 _StartPosition
    {
        get
        {
            return startPosition;
        }

        set
        {
            startPosition = value;
        }
    }

    public Vector2 _EndPosition
    {
        get
        {
            return endPosition;
        }

        set
        {
            endPosition = value;
        }
    }

    public float _Height
    {
        get
        {
            return height;
        }

        set
        {
            height = value;
        }
    }

    public bool positionsSet;

    Vector2 localStartPos;
    Vector2 localEndPos;

    protected float _TimeToDestination
    {
        get
        {
            return timeToDestination;
        }

        set
        {
            timeToDestination = value;
        }
    }

    public IEnumerator Parabola()
    {
        /// Create a temporary Object so we can move in local positions
        GameObject tmpObject = new GameObject();
        tmpObject.transform.rotation = CameraController.instance.transform.rotation;
        tmpObject.transform.parent = null;
        tmpObject.transform.position = _StartPosition;

        /// Parent this Object to the temporary object
        transform.parent = tmpObject.transform;

        /// Convert the desired positions to local positions
        if (!positionsSet)
        {
            localStartPos = transform.InverseTransformPoint(_StartPosition);
            localEndPos = transform.InverseTransformPoint(_EndPosition);
            positionsSet = true;
        }

        /// Set the layer of the item so that it will render infront of everything while in the air
        gameObject.layer = 28;

        /// Perform the parabola movement
        if (transform != null)
        {
            while (Vector2.Distance(transform.localPosition, localEndPos) > 0.2f && _TimeToDestination < 0.5f)
            {
                _TimeToDestination += Time.deltaTime;

                transform.localPosition = ExtMethods.Parabola(localStartPos, localEndPos, _Height, _TimeToDestination * 2);

                yield return null;
            }
        }

        /// Unparent, play sound, set the approriate layer and make it lootable
        transform.parent = null;
        SoundManager.instance.PlayUiSound("lootdrop");
        gameObject.layer = 10;
        GetComponent<BoxCollider>().enabled = true;

        /// destroy that temporary Object
        Destroy(tmpObject);
    }
}
