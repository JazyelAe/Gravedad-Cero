using GravedadCero.Cameras;
using GravedadCero.Gravity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GravedadCero.Player
{
    /// <summary>
    /// Lee el mapa de acciones "Player" y lo traduce en órdenes para PlayerMotor y
    /// GravityCamera, y Q/R/F para GravityController. Es el único script del jugador que
    /// conoce el Input System. Las acciones de interacción, anclaje y pausa ya existen en
    /// el mapa, pero todavía no se usan.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] InputActionAsset actions;
        [SerializeField] PlayerMotor motor;
        [SerializeField] GravityCamera gravityCamera;
        [SerializeField] GravityController gravityController;

        InputActionMap playerMap;
        InputAction moveAction;
        InputAction lookAction;
        InputAction jumpAction;
        InputAction gravityLeftAction;
        InputAction gravityRightAction;
        InputAction gravityFlipAction;

        void Awake()
        {
            playerMap = actions.FindActionMap("Player", throwIfNotFound: true);
            moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
            lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
            jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
            gravityLeftAction = playerMap.FindAction("GravityLeft", throwIfNotFound: true);
            gravityRightAction = playerMap.FindAction("GravityRight", throwIfNotFound: true);
            gravityFlipAction = playerMap.FindAction("GravityFlip", throwIfNotFound: true);
        }

        void OnEnable()
        {
            playerMap.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnDisable()
        {
            playerMap.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            gravityCamera.AddLookInput(lookAction.ReadValue<Vector2>());

            // W/S y A/D se interpretan respecto a hacia dónde mira la cámara, aplanado
            // sobre el plano perpendicular a la gravedad de Kai.
            Vector2 input = moveAction.ReadValue<Vector2>();
            Transform view = gravityCamera.transform;
            Vector3 up = motor.Up;
            Vector3 forward = Vector3.ProjectOnPlane(view.forward, up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(view.right, up).normalized;
            motor.SetMoveInput(forward * input.y + right * input.x);

            if (jumpAction.WasPressedThisFrame()) motor.RequestJump();

            // Q/R giran la gravedad hacia la izquierda/derecha de Kai (no de la cámara).
            if (gravityLeftAction.WasPressedThisFrame()) gravityController.RotateLeft(motor.transform);
            if (gravityRightAction.WasPressedThisFrame()) gravityController.RotateRight(motor.transform);
            if (gravityFlipAction.WasPressedThisFrame()) gravityController.Flip();
        }
    }
}
