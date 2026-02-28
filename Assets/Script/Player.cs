using UnityEngine;
using UnityEngine.InputSystem;

// บังคับอัญเชิญมวลสารและม่านพลังคุ้มกายเสมอ!
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Player : MonoBehaviour
{
    [Header("--- 👁️ ดวงเนตรแห่งเทพ (Camera Look) ---")]
    public Transform playerCamera;
    public float mouseSensitivity = 20f;

    [Header("--- 🏃‍♂️ พลังกายา (Movement) ---")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;

    [Header("--- 🌍 สัมผัสปฐพี (Ground Check) ---")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;
    private float xRotation = 0f;

    // ==========================================
    // 🔮 ภาชนะกักเก็บพลังงานเวท (Input Values)
    // ==========================================
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpTriggered;

    // 🌟 ภาชนะเก็บความทรงจำว่ากำลังจ้องมองวัตถุใดอยู่
    private ObjectInteractable currentTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 🛡️ ปิดผนึกการกลิ้ง! ป้องกันร่างอวตารล้มคะมำเมื่อชนกำแพง
        rb.freezeRotation = true;

        // จองจำวิญญาณเมาส์ไว้กลางจอ
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // การหันหน้าต้องรับกระแสพลังทุกเฟรมเพื่อความลื่นไหล
        HandleMouseLook();

        // ตรวจสอบว่าฝ่าเท้าแตะพื้นหรือไม่
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    // ⚙️ มหาเวทแห่งฟิสิกส์ต้องขับเคลื่อนใน FixedUpdate!
    void FixedUpdate()
    {
        HandleMovement();
    }

    // ========================================================
    // ⚡ มหาเวทรับคำสั่ง (Input System Callbacks)
    // ========================================================

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            jumpTriggered = true;
        }
    }

    // ⚡ มหาเวทสั่งการปลดผนึก! (ใช้คู่กับ OnInteract ที่ตั้งไว้ใน Input Actions)
    public void OnInteract(InputAction.CallbackContext context)
    {
        // เมื่อปลายนิ้วสัมผัสปุ่ม และมีแท่นบูชาอยู่ตรงหน้า
        if (context.started && currentTarget != null)
        {
            // 1. สั่งแท่นบูชาให้เปิดหน้าต่าง UI
            currentTarget.BreakTheSeal();

            // 2. ⚡ สลับมิติเข้าสู่โหมด UI (หยุดการเดินและการหันกล้องทันที!)
            GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");

            // 3. 🔓 มหาเวทกระชากวิญญาณ! บังคับให้เมาส์โผล่ออกมาและขยับได้อิสระ!
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("🌌 [Player] เปิด UI -> สลับมิติ -> ปลดผนึกเมาส์ สำเร็จ!");
        }
    }
    // ========================================================
    // 🔮 วิชาควบคุมดวงตาและเรือนร่าง
    // ========================================================

    private void HandleMouseLook()
    {
        // 🛑 กางอาณาเขตป้องกัน: ถ้าเมาส์ถูกปลดล็อค (เปิด UI อยู่) ห้ามหันกล้องเด็ดขาด!
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (playerCamera == null) return;

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        // 🛑 กางอาณาเขตป้องกัน: ถ้าเมาส์ถูกปลดล็อค (เปิด UI อยู่) ห้ามเดินเด็ดขาด!
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            // หยุดแรงเดินทั้งหมด แต่ปล่อยให้แรงโน้มถ่วง (แกน Y) ทำงานตามปกติ
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.velocity.y;

        rb.velocity = targetVelocity;

        if (jumpTriggered)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpTriggered = false;
        }
    }

    // ========================================================
    // 👁️ สัมผัสวิญญาณวัตถุเวทมนตร์ (Trigger Zones)
    // ========================================================



    void OnTriggerEnter(Collider other)
    {
        ObjectInteractable interactable = other.GetComponent<ObjectInteractable>();
        if (interactable != null)
        {
            currentTarget = interactable;
            currentTarget.ShowPrompt(); // สั่งให้วัตถุเปล่งแสง/ข้อความ
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (currentTarget != null && other.gameObject == currentTarget.gameObject)
        {
            currentTarget.HidePrompt(); // สั่งให้วัตถุดับแสง/ข้อความ
            currentTarget = null;       // ลืมเลือนวัตถุนั้นไปซะ       
        }
    }
}