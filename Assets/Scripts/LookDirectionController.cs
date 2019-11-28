using System;
using UnityEngine;
//This class is literally just a rip off of First Person Control where only the aspects I needed were included
public class LookDirectionController : MonoBehaviour
{
    private CharacterController m_CharacterController;
 public Transform camera;
    private Quaternion m_CameraTargetRot;
    private Quaternion m_transformRot;
    public float XSensitivity =1;
    public float YSensitivity = 1;
    public bool clampRotation = true;
    public float MinimumX = -45;
    public float MaximumX = 45;
    public float MinimumY = -180;
    public float MaximumY = 180;
    // Start is called before the first frame update
    private void Start()
    { 
        
        if (camera == null) camera = gameObject.transform;
        m_CameraTargetRot = camera.localRotation;
        m_transformRot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        RotateView();
    }

    private void RotateView()
    {
        float yRot = Input.GetAxis("Mouse X") * XSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

        m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
        m_transformRot *= Quaternion.Euler(0f, yRot, 0f);
     //   m_transformRot *=
        if (clampRotation)
            m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            camera.localRotation = m_CameraTargetRot;
        transform.localRotation = m_transformRot;

    }


    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z = 0;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
        angleY = Mathf.Clamp(angleY, MinimumY, MaximumY);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

        return q;
    }
}
