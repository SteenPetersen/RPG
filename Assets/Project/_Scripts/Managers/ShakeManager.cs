using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeManager : MonoBehaviour {

    public static ShakeManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    //[SerializeField] bool shaking = false;

    /// <summary>
    /// Shakes a GameObject for a set amount of time and starts to decrease the shaking after a set
    /// time as well. can be used for 2D and 3D objects
    /// </summary>
    /// <param name="objectToShake"></param>
    /// <param name="shakeDuration"></param>
    /// <param name="decreasePoint"></param>
    /// <param name="objectIs2D"></param>
    public void shakeGameObject(Destructable des, GameObject objectToShake, float shakeDuration, float decreasePoint, bool objectIs2D = false)
    {
        if (des.shaking)
        {
            return;
        }
        des.shaking = true;
        StartCoroutine(shakeGameObjectCOR(des, objectToShake, shakeDuration, decreasePoint, objectIs2D));
    }

    IEnumerator shakeGameObjectCOR(Destructable des, GameObject objectToShake, float totalShakeDuration, float decreasePoint, bool objectIs2D = false)
    {
        if (decreasePoint >= totalShakeDuration)
        {
            //Debug.LogError("decreasePoint must be less than totalShakeDuration...Exiting");
            yield break; //Exit!
        }

        //Get Original Pos and rot
        Transform objTransform = objectToShake.transform;
        Vector3 defaultPos = objTransform.position;
        Quaternion defaultRot = objTransform.rotation;

        float counter = 0f;

        //Shake Speed
        const float speed = 0.05f;

        //Angle Rotation(Optional)
        const float angleRot = 4;

        //Do the actual shaking
        while (counter < totalShakeDuration)
        {
            counter += Time.deltaTime;
            float decreaseSpeed = speed;
            float decreaseAngle = angleRot;

            //Shake GameObject
            if (objectIs2D)
            {
                //Don't Translate the Z Axis if 2D Object
                Vector3 tempPos = defaultPos + UnityEngine.Random.insideUnitSphere * decreaseSpeed;
                tempPos.z = defaultPos.z;
                objTransform.position = tempPos;

                //Only Rotate the Z axis if 2D
                objTransform.rotation = defaultRot * Quaternion.AngleAxis(UnityEngine.Random.Range(-angleRot, angleRot), new Vector3(0f, 0f, 1f));
            }
            else
            {
                objTransform.position = defaultPos + UnityEngine.Random.insideUnitSphere * decreaseSpeed;
                objTransform.rotation = defaultRot * Quaternion.AngleAxis(UnityEngine.Random.Range(-angleRot, angleRot), new Vector3(1f, 1f, 1f));
            }
            yield return null;


            //Check if we have reached the decreasePoint then start decreasing  decreaseSpeed value
            if (counter >= decreasePoint)
            {
                //Debug.Log("Decreasing shake");

                //Reset counter to 0 
                counter = 0f;
                while (counter <= decreasePoint)
                {
                    counter += Time.deltaTime;
                    decreaseSpeed = Mathf.Lerp(speed, 0, counter / decreasePoint);
                    decreaseAngle = Mathf.Lerp(angleRot, 0, counter / decreasePoint);

                    //Debug.Log("Decrease Value: " + decreaseSpeed);

                    //Shake GameObject
                    if (objectIs2D)
                    {
                        //Don't Translate the Z Axis if 2D Object
                        Vector3 tempPos = defaultPos + UnityEngine.Random.insideUnitSphere * decreaseSpeed;
                        tempPos.z = defaultPos.z;
                        objTransform.position = tempPos;

                        //Only Rotate the Z axis if 2D
                        objTransform.rotation = defaultRot * Quaternion.AngleAxis(UnityEngine.Random.Range(-decreaseAngle, decreaseAngle), new Vector3(0f, 0f, 1f));
                    }
                    else
                    {
                        objTransform.position = defaultPos + UnityEngine.Random.insideUnitSphere * decreaseSpeed;
                        objTransform.rotation = defaultRot * Quaternion.AngleAxis(UnityEngine.Random.Range(-decreaseAngle, decreaseAngle), new Vector3(1f, 1f, 1f));
                    }
                    yield return null;
                }

                //Break from the outer loop
                break;
            }
        }
        objTransform.position = defaultPos; //Reset to original postion
        objTransform.rotation = defaultRot;//Reset to original rotation

        des.shaking = false; //So that we can call this function next time
                         // Debug.Log("Done!");
    }
}
