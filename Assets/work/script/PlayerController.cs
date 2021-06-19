using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;
    private Transform _transform;
    private Vector3 velocity;
    private Quaternion targetRotation;
    private bool isTired = false;
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isJump = false;
    private short crouchCnt = 0;
    [SerializeField]private float runTime = 10;
    private float cntTime = 0;
    [SerializeField] private float walkSpeed = 3;
    [SerializeField] private float runSpeed = 5;
    [SerializeField] private float jumpPower = 3;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        _transform = transform;
        targetRotation = transform.rotation;
        //マウスカーソルを画面内にロック
        Cursor.lockState = CursorLockMode.Locked;
    }

    private bool IsGrounded //床との当たり判定
    {
        get
        {
            var ray = new Ray(transform.position +
                new Vector3(0, 0.1f), Vector3.down);
            var raycastHits = new RaycastHit[1];
            var hitCount = Physics.RaycastNonAlloc(ray, raycastHits, 0.2f);

            return hitCount >= 1;
        }
    }

    // Update is called once per frame
    void Update()
    { 
        var rotationSpeed = 600 + Time.deltaTime;
        if(velocity.magnitude > 0.5f && !isJump)
        {
            targetRotation = Quaternion.LookRotation(velocity, Vector3.up);  
        }
        Debug.Log(IsGrounded? "地上" : "空中");

        if (IsGrounded)
        {

            //走るモーション
            if (isRun && cntTime <= runTime)
            {
              
                velocity.x = Input.GetAxis("Horizontal") * runSpeed;
                velocity.z = Input.GetAxis("Vertical") * runSpeed;

                if (velocity.magnitude > 0)
                {
                    animator.SetBool("Run", true);
                    cntTime += Time.deltaTime;
                }
                if(cntTime >= runTime)
                {
                    isRun = false;
                    animator.SetBool("Run", false);
                    animator.SetBool("Tired", true);
                    animator.SetTrigger("Tired 1");
                    isTired = true;
                }

            }

            //走るクールダウン
            if (cntTime > 0 && isTired)
            {
                
                cntTime -= Time.deltaTime;
                if(cntTime <= runTime - 3)
                {
                    animator.SetBool("Tired", false);
                    velocity.x = Input.GetAxis("Horizontal") * 2;
                    velocity.z = Input.GetAxis("Vertical") * 2;

                    if (velocity.magnitude > 0)
                    {
                        animator.SetBool("Walk", true);
                    }
                }
                else
                {
                    velocity.x = 0;
                    velocity.z = 0;
                }
                if(cntTime < 0)
                {
                    isTired = false;
                }

            }

            if (Input.GetButtonDown("runMode") && !isTired)
            {
                isRun = !isRun;
                cntTime = 0;
            }
            
            //歩くモーション
            if (!isRun && !isTired && !isCrouch)
            {
                animator.SetBool("Run", false);
                velocity.x = Input.GetAxis("Horizontal") * walkSpeed;
                velocity.z = Input.GetAxis("Vertical") * walkSpeed;

                if(velocity.magnitude > 0)
                {
                    animator.SetBool("Walk", true);
                }
            }

            //しゃがみ歩き
            if (Input.GetButton("Crouch") && !isRun)
            {
                if(crouchCnt == 0)
                {
                    isCrouch = true;
                    animator.SetTrigger("Crouch");
                    animator.SetBool("CrouchIdle", true);
                }

                crouchCnt = 1;
                velocity.x = Input.GetAxis("Horizontal") * 1;
                velocity.z = Input.GetAxis("Vertical") * 1;

                if (velocity.magnitude > 0)
                {
                    
                    animator.SetFloat("MoveSpeed", velocity.magnitude);
                }
            }
            else
            {
                animator.SetBool("CrouchIdle", false);
                animator.SetFloat("MoveSpeed", 0);
                isCrouch = false;
                crouchCnt = 0;
            }


            //操作していないとき
            if(!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))
            {
                isRun = false;
                velocity.x = 0;
                velocity.z = 0;
                animator.SetBool("Walk", false);
                animator.SetBool("Run", false);
                animator.SetFloat("MoveSpeed", 0);
            }

            //ジャンプモーション
            if (Input.GetButtonDown("Jump") && !isCrouch)
            {
                isJump = true;
                velocity.y = jumpPower;
                animator.SetTrigger("Jump");
            }
            
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }
        if(velocity.y < -2.0f)
        {
            isJump = false;
        }

        //transform.rotation = Quaternion.RotateTowards(transform.rotation,
        //        targetRotation, rotationSpeed);
        //移動方向にキャラを向かせる
        _transform.LookAt(_transform.position +
            new Vector3(velocity.x, 0, velocity.z));
        Debug.Log(isJump);
        characterController.Move(velocity * Time.deltaTime);
    }
}
