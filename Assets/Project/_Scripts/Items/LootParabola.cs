using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootParabola : MonoBehaviour {

    Vector2 startPosition;
    Vector2 endPosition;
    float height;
    private float timeToDestination;

    bool go;

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

    public bool Go
    {
        get
        {
            return go;
        }

        set
        {
            go = value;
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

    bool bounce;

    public IEnumerator Parabola()
    {
        GameObject tmpObject = new GameObject();
        tmpObject.transform.rotation = CameraController.instance.transform.rotation;
        tmpObject.transform.parent = null;
        tmpObject.transform.position = _StartPosition;

        transform.parent = tmpObject.transform;

        if (!positionsSet)
        {
            localStartPos = transform.InverseTransformPoint(_StartPosition);
            localEndPos = transform.InverseTransformPoint(_EndPosition);
            positionsSet = true;
        }

        gameObject.layer = 28;

        while (Vector2.Distance(transform.localPosition, localEndPos) > 0.2f
            && _TimeToDestination < 0.5f)
        {
            _TimeToDestination += Time.deltaTime;

            transform.localPosition = ExtMethods.Parabola(localStartPos, localEndPos, _Height, _TimeToDestination * 2);

            yield return null;
        }

        transform.parent = null;
        SoundManager.instance.PlayUiSound("lootdrop");
        gameObject.layer = 10;

        Destroy(tmpObject);
    }
}
