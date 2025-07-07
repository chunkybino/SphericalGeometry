using UnityEngine;

public class GuyController4D : MonoBehaviour
{
    [SerializeField] Transform4D transform4;
    [SerializeField] Rigidbody4D rb;
    [SerializeField] Camera4D camera;
    Transform4D camTransform {get{return camera.transform4;}}

    [SerializeField] PlayerInput input;

    [SerializeField] float speed = 0.4f;
    [SerializeField] Vector3 moveVector;

    [SerializeField] float lookSpeed = 1;
    [SerializeField] Vector2 lookVector;

    [SerializeField] bool doCameraPitch;

    [SerializeField] float jumpForce;

    void Update()
    {
        //move
        moveVector = Vector3.zero;

        if (input.left) moveVector.x--;        
        if (input.right) moveVector.x++;
        if (input.backward) moveVector.z++;        
        if (input.forward) moveVector.z--;  

        if (input.down) moveVector.y--;      
        if (input.up) moveVector.y++;    

        rb.SetRelativeVelocityX(moveVector.x * speed);
        rb.SetRelativeVelocityZ(moveVector.z * speed);

        //jump
        if (input.spacePress) {
            rb.SetVelocityTowards(new Vector4(0,1,0,0), jumpForce);
        }

        //look
        lookVector = Vector2.zero;

        if (input.leftArrow) lookVector.x--;
        if (input.rightArrow) lookVector.x++;
        if (input.downArrow) lookVector.y--;
        if (input.upArrow) lookVector.y++;

        Vector3 angleVector = new Vector3(-lookVector.y, -lookVector.x, 0) * lookSpeed;
        if (camera && doCameraPitch) 
        {
            camera.pitchAngle = Mathf.Clamp(camera.pitchAngle + (-angleVector.x * Time.deltaTime), -Mathf.PI/2, Mathf.PI/2);
            angleVector.x = 0;
        }

        rb.angularVelocity = angleVector;
    }
}
