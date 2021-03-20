using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    #region Variables
    [SerializeField] private float runSpeed;
    [SerializeField] private float turnSpeed;

    private Animator mAnimator;
    private Collider mCollider;
    private Rigidbody mRigidbody;

    private List<Rigidbody> ragdollRB = new List<Rigidbody>();

    private Transform lookAt = null;
    private Quaternion rotation = Quaternion.identity;
    private CharacterState state = CharacterState.None;

    private bool move = false;
    private Vector3 lastPosition;
    #endregion
    #region Awake - Start
    private void Awake()
    {
        mAnimator = GetComponentInChildren<Animator>();
        mCollider = GetComponent<Collider>();
        mRigidbody = GetComponent<Rigidbody>();
        ragdollRB = GetComponentsInChildren<Rigidbody>().Where(x => x.gameObject != gameObject).ToList();
    }
    private void Start()
    {
        if(lookAt == null) 
        {
            GameObject go = new GameObject();
            go.transform.SetParent(transform);
            go.transform.localPosition = go.transform.localEulerAngles = Vector3.zero;
            lookAt = go.transform;
        }
        foreach (Rigidbody rb in ragdollRB) { rb.isKinematic = true; rb.velocity = Vector3.zero; rb.gameObject.layer = 9; }
        LevelManager.instance.startEvent.AddListener(() => move = true);
        state = CharacterState.Idle;
    }
    #endregion
    #region Update - FixedUpdate - Collision - Phyics
    private void Update()
    {
        InputValues();
    }
    private void FixedUpdate()
    {
        Movement();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Obstacle"))
        {
            EnableRagdoll();
        }
    }
    private void Movement()
    {
        if (!move) return;

        if (hold)
        {
            if (state != CharacterState.Running)
            {
                mAnimator.SetTrigger("Run");
                mRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; state = CharacterState.Running;
            }

            float speed = (lastPosition - transform.position).magnitude * 10;
            lastPosition = transform.position;
            mAnimator.SetFloat("Speed", 0.5f + (speed/2));
            var lookPos = lookAt.position - transform.position;
            lookPos.y = 0;
            rotation = Quaternion.LookRotation(lookPos);
            lookAt.position = new Vector3(transform.position.x - x, transform.position.y, transform.position.z - y);
            mRigidbody.MovePosition(transform.position + (transform.forward * Time.fixedDeltaTime * runSpeed));
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);

        }
        else if (state != CharacterState.Idle)
        {
            mRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            lookAt.localPosition = Vector3.zero;
            mAnimator.SetTrigger("Idle");

            state = CharacterState.Idle;
        }
    }
    private void DisableRagdoll()
    {
        foreach(Rigidbody rb in ragdollRB) { rb.isKinematic = true; rb.velocity = Vector3.zero; }
        mAnimator.enabled = true;
        move = true;
    }
    private void EnableRagdoll()
    {
        mAnimator.enabled = false;
        foreach (Rigidbody rb in ragdollRB) { rb.isKinematic = false; }
        move = false;
    }
    #endregion
    #region Input - Enum - Coroutine

    [HideInInspector] public UnityEvent tap = new UnityEvent();

    private bool touch = false;
    private bool hold = false;

    private Vector2 startPos;
    private Vector2 touchPos;

    [HideInInspector] public float deltaX;
    [HideInInspector] public float deltaY;
    private float x;
    private float y;

    private float holdTimer = 0f;
    private void InputValues()
    {
        if (Input.GetMouseButton(0))
        {
            if (!touch)
            {
                startPos = Input.mousePosition;
                touch = true;
                holdTimer = Time.time;
            }
            else
            {
                if (holdTimer + 0.1f < Time.time) hold = true;
                touchPos = Input.mousePosition;
                deltaX = (startPos.x - touchPos.x);
                deltaY = (startPos.y - touchPos.y);
                if (Mathf.Abs(deltaX) > 200)
                {
                    if (deltaX > 0)
                        startPos.x = touchPos.x + 200;
                    else
                        startPos.x = touchPos.x - 200;
                }
                if (Mathf.Abs(deltaY) > 200)
                {
                    if (deltaY > 0)
                        startPos.y = touchPos.y + 200;
                    else
                        startPos.y = touchPos.y - 200;
                }
                x = Mathf.Clamp(deltaX / 200, -1, 1);
                y = Mathf.Clamp(deltaY / 200, -1, 1);

            }
        }
        else if (touch)
        {
            //if (!hold) tap.Invoke();
            holdTimer = 0;
            touch = hold = false;
            deltaX = deltaY = x = y = 0;
            startPos = Vector2.zero;
        }
    }

    [System.Serializable] 
    public enum CharacterState
    {
        Running,
        Idle,
        Falling,
        None
    }
    #endregion
}
