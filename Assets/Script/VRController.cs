using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class VRController : MonoBehaviour
{

    Rigidbody rigidBody;

    public float controllerRotationTime = 0.6f;
    public float controllerRotationDistance = 0.3f;
    public InputDeviceCharacteristics deviceTarget;
    public float axisPressThreshold = 0.2f;
    public float throwAssist = 0.5f;

    Vector3 lastPos;
    Quaternion lastRot;
    Vector3 controllerVel;
    Vector3 controllerAngVel;

    Vector3 legacyThrowVel;

    Queue<Vector3> velQueue = new Queue<Vector3>();
    Queue<Vector3> angVelQueue = new Queue<Vector3>();

    //Vector3 throwVel;
    float throwVelMagnitude;
    Vector3 controllerVelocityCross;
    Vector3 controllerCenterOfMass;

    List<Grabbable> grabTargets = new List<Grabbable>();
    ButtonState gripButtonState;
    Grabbable inHand = null;
    Grabbable flyingToward = null;
    Grabbable lockOn = null;
    Transform lineTarget = null;
    Transform oldLineTarget = null;

    Transform uiTarget = null;
    Transform oldUiTarget = null;

    float forceGrabTime = 0.7f;


    Transform tipTransform;
    Transform holdTransform;
    Transform simpleHoldTransform;


    float lockOnDistance;

    ButtonState primaryButtonState;
    ButtonState triggerButtonState;

    VRButton hoveringButton = null;
    VRButton pressedButton = null;

    public Transform modelPrefab;
    public float modelMass = 0.05f;

    Vector3 modelTruePosition;
    GameObject simpleModel;
    GameObject model;
    GameObject modelOffset;
    Vector3 modelOffsetPosition;
    Rigidbody modelRigidbody;
    CapsuleCollider modelCollider;
    Quaternion modelOffsetRotation;

    GameObject grabconeObject;
    GrabCone grabcone;

    public Transform selectLinePrefab;
    GameObject selectLine;
    LineRenderer lineRenderer;

    public Transform uiLinePrefab;
    GameObject uiLine;
    LineRenderer uiLineRenderer;


    InputDevice deviceInput;
    Vector3 devicePosition;
    Quaternion deviceRotation;

    void SetupModel()
    {
        model = Instantiate(modelPrefab).gameObject;
        //model.transform.parent = transform;
        model.transform.position = transform.position;
        model.transform.rotation = transform.rotation;
        model.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        model.transform.gameObject.SetActive(true);
        model.GetComponentInChildren<VRControllerModel>().parent = this;
        model.GetComponentInChildren<VRControllerModel>().Floating();
        modelOffset = model.transform.GetChild(0).gameObject;
        modelOffsetRotation = modelOffset.transform.rotation *  Quaternion.Inverse(model.transform.rotation);
        modelOffsetPosition = modelOffset.transform.position - model.transform.position;
        holdTransform = modelOffset.transform.GetChild(1).transform;

        simpleModel = Instantiate(modelPrefab).gameObject;
        simpleModel.transform.parent = transform;
        simpleModel.transform.localRotation = Quaternion.identity;
        simpleModel.transform.localPosition = Vector3.zero;
        simpleModel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        simpleModel.transform.gameObject.SetActive(true);
        simpleModel.GetComponentInChildren<VRControllerModel>().parent = this;
        tipTransform = simpleModel.transform.GetChild(0).GetChild(0);
        simpleHoldTransform = simpleModel.transform.GetChild(0).GetChild(1);
    }

    void SetupGrabCone()
    {
        grabconeObject = simpleModel.transform.GetChild(0).GetComponentInChildren<GrabCone>().gameObject;
        Vector3 temp = grabconeObject.transform.localPosition;
        Quaternion temp2 = grabconeObject.transform.localRotation;
        Vector3 temp3 = grabconeObject.transform.localScale;


        grabconeObject.transform.gameObject.SetActive(true);
        //grabconeObject.transform.parent = model.transform;
        //grabconeObject.transform.localPosition = temp;
        //grabconeObject.transform.localRotation = temp2;
        //grabconeObject.transform.localScale = temp3;


        grabcone = grabconeObject.GetComponent<GrabCone>();
    }

    internal struct ButtonState
    {
        public bool active;
        public bool activatedThisFrame;
        public bool deActivatedThisFrame;
    }

    void HandleButtonState(bool pressed, ref ButtonState buttonState)
    {
        buttonState.activatedThisFrame = buttonState.deActivatedThisFrame = false;
        if (pressed)
        {
            if (!buttonState.active)
            {
                buttonState.activatedThisFrame = true;
                buttonState.active = true;
            }
        }
        else
        {
            if (buttonState.active)
            {
                buttonState.deActivatedThisFrame = true;
                buttonState.active = false;
            }
        }
    }

    void HandleInteractionAction(InputFeatureUsage<float> button, ref ButtonState buttonState)
    {
        deviceInput.TryGetFeatureValue(button, out float buttonValue);
        if (buttonValue > axisPressThreshold)
        {
            HandleButtonState(true, ref buttonState);
        }
        else
        {
            HandleButtonState(false, ref buttonState);
        }
    }

    void HandleInteractionAction(InputFeatureUsage<bool> button, ref ButtonState buttonState)
    {
        deviceInput.TryGetFeatureValue(button, out bool pressed);
        HandleButtonState(pressed, ref buttonState);
    }

    void UpdateInputs()
    {
        HandleInteractionAction(CommonUsages.grip, ref gripButtonState);

        HandleInteractionAction(CommonUsages.primaryButton, ref primaryButtonState);

        HandleInteractionAction(CommonUsages.trigger, ref triggerButtonState);
    }

    void UpdateTracking()
    {
        lastPos = transform.position;
        lastRot = transform.rotation;
        if (deviceInput.TryGetFeatureValue(CommonUsages.devicePosition, out devicePosition))
            transform.localPosition = devicePosition;

        if (deviceInput.TryGetFeatureValue(CommonUsages.deviceRotation, out deviceRotation))
            transform.localRotation = deviceRotation;

        deviceInput.TryGetFeatureValue(CommonUsages.deviceVelocity, out controllerVel);
        deviceInput.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out controllerAngVel);

        //transform.localEulerAngles =  new Vector3(50 * Time.time, 0, 0);

        //controllerVel = (transform.position - lastPos) / Time.deltaTime;
        //controllerAngVel = ((transform.rotation * Quaternion.Inverse(lastRot)).eulerAngles / 180f * (float)Math.PI) / Time.deltaTime;
    }

    void UpdateModel()
    {
        float distance = (transform.position - modelTruePosition).magnitude;

        if (distance > 1 && modelCollider.enabled)
        {
            modelCollider.enabled = false;

        }
        else if (distance < 0.1 && !modelCollider.enabled)
        {
            modelCollider.enabled = true;
        }
        float velocity;
        velocity = 50f * (float)System.Math.Pow(50f, distance * -1f) + 20f;
        //velocity = (float)(10 / (1 + 10 * System.Math.Pow(System.Math.E, -10 * (distance - 0.2))));
        //if (distance < 0.1f)
        //{
        //    velocity = -7000f * distance + 750;
        //}else
        //{
        //    velocity = 100f;
        //}

        modelRigidbody.velocity = modelRigidbody.velocity / 2f;
        Vector3 forceDirection = (transform.position - modelTruePosition);
        //if (forceDirection.magnitude > 0.2) forceDirection = forceDirection.normalized / 5f;
        modelRigidbody.AddForce(forceDirection * velocity);
        //modelRigidbody.velocity = ((transform.position - modelTruePosition) * velocity);
        //if (distance < controllerRotationDistance)
        //    modelRigidbody.rotation = Quaternion.Lerp(modelOffset.transform.rotation, transform.rotation * modelOffsetRotation, controllerRotationTime * controllerRotationDistance / ((distance + controllerRotationDistance)));
        modelRigidbody.rotation = transform.rotation * modelOffsetRotation;

        //modelRigidbody.angularVelocity = transform.up - model.transform.up;

        //controllerRigidbody.rotation = Quaternion.LookRotation(transform.forward, transform.up);
    }

    public bool Rumble(float amplitude, float duration)
    {
        HapticCapabilities capabilities;
        if (deviceInput.TryGetHapticCapabilities(out capabilities) &&
            capabilities.supportsImpulse)
        {
            return deviceInput.SendHapticImpulse(0, amplitude, duration);
        }
        return false;
    }

    void HandleUI()
    {
        RaycastHit hit;
        Physics.Raycast(tipTransform.position, tipTransform.up, out hit, Mathf.Infinity, LayerMask.GetMask("UI"));
        uiTarget = hit.transform;
        if(uiTarget != oldUiTarget)
        {
            //any change
            if (uiTarget != null)
            {
                //moved from anything, to a UI element

                Rumble(0.2f, 0.02f);
                if (hoveringButton != null)
                {
                    //if the last object is a button
                    hoveringButton.ButtonUp();
                    hoveringButton.LeaveHover();
                }
                if (pressedButton != null)
                {
                    pressedButton.ButtonUp();
                    pressedButton.LeaveHover();
                }
                pressedButton = null;
                hoveringButton = uiTarget.GetComponent<VRButton>();
                if(hoveringButton != null)
                {
                    //if the current object is a button
                    hoveringButton.Hover();
                }
            }
            else
            {
                //moved off canvas

                if (hoveringButton != null)
                {
                    hoveringButton.ButtonUp();
                    hoveringButton.LeaveHover();
                }
                if (pressedButton != null)
                {
                    pressedButton.ButtonUp();
                    pressedButton.LeaveHover();
                }
                pressedButton = null;
                hoveringButton = null;
            }
        }

        //if hovering on a UI Target
        if (uiTarget != null)
        {
            uiLineRenderer.SetPosition(0, tipTransform.position);
            uiLineRenderer.SetPosition(1, hit.point);
            if (!uiLineRenderer.enabled)
                uiLineRenderer.enabled = true;
        }
        else if (uiLineRenderer.enabled)
        {
            uiLineRenderer.enabled = false;
        }

        oldUiTarget = uiTarget;

        if (triggerButtonState.activatedThisFrame && hoveringButton != null)
        {
            pressedButton = hoveringButton;
            pressedButton.ButtonDown();
        }

        if (triggerButtonState.deActivatedThisFrame && pressedButton != null)
        {
            pressedButton.ButtonUp();
            pressedButton = null;
        }
    }

    void HandleTargetLine()
    {
        if (grabTargets.Count == 0 && grabcone.grabTargets.Count > 0)
        {
            if(grabcone.grabTargets[0] != null)
                lineTarget = grabcone.grabTargets[0].transform;
            else
                lineTarget = null;
        }
        else
        {
            lineTarget = null;
        }

        if (lineTarget != oldLineTarget && lineTarget!=null)
            Rumble(0.2f, 0.02f);
        oldLineTarget = lineTarget;

        if (lineTarget != null )
        {
            lineRenderer.SetPosition(0, tipTransform.position);
            lineRenderer.SetPosition(1, lineTarget.position);
            if (!lineRenderer.enabled)
                lineRenderer.enabled = true;
        }
        else if(lineRenderer.enabled)
        {
                lineRenderer.enabled = false;
        }
    }

    void CatchObject()
    {
        Vector3 offset = inHand.GetComponent<Rigidbody>().velocity - GetComponent<Rigidbody>().velocity;

        if(offset.magnitude < (transform.position - inHand.transform.position).magnitude * 10)
        {
            offset = (transform.position - inHand.transform.position) * 10;
        }

        float distance = offset.magnitude;
        inHand.transform.SetParent(holdTransform);
        inHand.transform.localPosition = new Vector3(0, 0, 0);
        inHand.transform.localRotation = Quaternion.Euler(0, 0, 0);
        float objectMass = inHand.TransferMass();
        modelRigidbody.mass = modelMass + objectMass;
        //modelRigidbody.AddForceAtPosition(offset.normalized * distance * distance * objectMass * 10, holdTransform.position);
        inHand.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
    }

    Vector3 lockOnPosition;

    void HandleGrab()
    {
        if (gripButtonState.activatedThisFrame)
        {
            if (grabTargets.Count > 0)
            {
                Rumble(0.4f, 0.02f);
                lockOn = null;
                flyingToward = null;
                inHand = grabTargets[0];
                CatchObject();
            }
            else if (flyingToward != null) { 
                if( (flyingToward.transform.position - transform.position).magnitude < 1f)
                {
                    Rumble(0.8f, 0.04f);
                    inHand = flyingToward;
                    CatchObject();
                    lockOn = null;
                }else if (grabcone.grabTargets.Count > 0)
                {
                    lockOn = grabcone.grabTargets[0];
                    if (lockOn != null)
                    {
                        lockOnPosition = lockOn.transform.position;
                        lockOnDistance = (transform.position - lockOn.transform.position).magnitude;
                    }
                }
                flyingToward = null;
            }
            else if (grabcone.grabTargets.Count > 0)
            {
                lockOn = grabcone.grabTargets[0];
                if (lockOn != null)
                {
                    lockOnPosition = lockOn.transform.position;
                    lockOnDistance = (transform.position - lockOn.transform.position).magnitude;
                }
            }
        }else if (gripButtonState.active)
        {
            if(lockOn != null)
            {
                if ((transform.position - lockOnPosition).magnitude > (lockOnDistance + 0.05) && (controllerVel.magnitude > 1.5 || controllerAngVel.magnitude > 1 ||simpleHoldTransform.GetComponent<Rigidbody>().velocity.magnitude > 1.5))
                {
                    Rumble(0.6f, 0.02f);
                    lockOn.setTarget(holdTransform, forceGrabTime);
                    flyingToward = lockOn;
                    lockOn = null;
                }
            }
        }
        else if (gripButtonState.deActivatedThisFrame)
        {
            if (inHand != null)
            {
                Rumble(0.4f, 0.02f);
                inHand.transform.SetParent(null);
                modelRigidbody.mass = modelMass;
                //controllerVelocityCross = Vector3.Cross(rigidBody.angularVelocity, )
                Vector3 avgAngularVel = GetVectorAverage(angVelQueue);
                Vector3 avgVel = GetVectorAverage(velQueue);

                Vector3 cross = Vector3.Cross(avgAngularVel, (simpleHoldTransform.position + transform.position ) * 0.5f - transform.position);
                Vector3 fullThrowVel = legacyThrowVel;
                //inHand.RecoverMasss(throwVel * (1f + throwAssist), rigidBody.angularVelocity);
                inHand.RecoverMasss(fullThrowVel * (1f + throwAssist), avgAngularVel);

                inHand = null;
            }
        }
    }

    Vector3 GetVectorAverage(Queue<Vector3> vectors)
    {
        float x = 0f, y = 0f, z = 0f;
        int numVectors = 0;
        foreach (var ele in vectors)
        {
            x += ele.x;
            y += ele.y;
            z += ele.z;
            numVectors++;
        }
        if (numVectors > 0)
        {
            return (new Vector3(x, y, z) / numVectors) ;
        }
        return Vector3.zero;
    }

    void HandleVelHistory()
    {
        /* Calculate velocity of the controller */
        velQueue.Enqueue(controllerVel);
        angVelQueue.Enqueue(controllerAngVel);

        if (velQueue.Count > 10) velQueue.Dequeue();
        if (angVelQueue.Count > 10) angVelQueue.Dequeue();

        if (velQueue.Count > 0)
        {
            legacyThrowVel = GetVectorAverage(velQueue).normalized;
            throwVelMagnitude = 0;
            foreach (var ele in velQueue)
            {
                if (ele.magnitude > throwVelMagnitude)
                    throwVelMagnitude = ele.magnitude;
            }
            legacyThrowVel = legacyThrowVel * throwVelMagnitude;
        }
        else
        {
            legacyThrowVel = controllerVel;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>(); ;
        SetupModel();
        SetupGrabCone();
        selectLine = Instantiate(selectLinePrefab).gameObject;
        lineRenderer = selectLine.GetComponent<LineRenderer>();

        uiLine = Instantiate(uiLinePrefab).gameObject;
        uiLineRenderer = uiLine.GetComponent<LineRenderer>();

        modelRigidbody = model.GetComponentInChildren<Rigidbody>();
        modelCollider = model.GetComponentInChildren<CapsuleCollider>();

        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rControlCharacteristics = deviceTarget | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rControlCharacteristics, devices);

        if(devices.Count > 0)
        {
            deviceInput = devices[0];
        }
    }

    private void FixedUpdate()
    {
        UpdateTracking();
        UpdateModel();
        HandleVelHistory();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInputs();

        if (grabTargets.Count > 0)
        {
            if (grabTargets[0] == null) {
                grabTargets.RemoveAt(0);
            }
        }

        modelTruePosition = (modelOffset.transform.position - modelOffsetPosition);

        if (primaryButtonState.activatedThisFrame && inHand != null)
        {
            Grenade grenade = inHand.GetComponent<Grenade>();
            if (grenade != null)
            {
                grenade.Activate();
                Rumble(0.5f, 0.02f);
            }
        }

        HandleUI();
        HandleTargetLine();
        HandleGrab();
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabbable target = other.GetComponent<Grabbable>();
        if (target != null && other.gameObject.layer == 11)
        {
            if(!grabTargets.Contains(target))
                grabTargets.Add(target);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Grabbable target = other.GetComponent<Grabbable>();
        if (target != null)
        {
            if (grabTargets.Contains(target))
                grabTargets.Remove(target);
        }
    }

    public void Collided(float speed)
    {
        if (speed > 5) speed = 5;
        Rumble(speed / 5, 0.02f);
    }
}
