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

    void Update ()
    {
        if (Go)
        {
            gameObject.layer = 28;
            if (!positionsSet)
            {
                localStartPos = transform.InverseTransformPoint(_StartPosition);
                localEndPos = transform.InverseTransformPoint(_EndPosition);
                positionsSet = true;
            }


            _TimeToDestination += Time.deltaTime;

            transform.localPosition = ExtMethods.Parabola(localStartPos, localEndPos, _Height, _TimeToDestination);

            if (Vector2.Distance(transform.localPosition, localEndPos) < 0.1f)
            {
                transform.parent = null;
                Go = false;
                SoundManager.instance.PlayUiSound("lootdrop");
                gameObject.layer = 10;
            }
        }
	}
}
