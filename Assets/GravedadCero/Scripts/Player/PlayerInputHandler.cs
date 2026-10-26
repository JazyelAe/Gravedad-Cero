using GravedadCero.Cameras;
using GravedadCero.Gravity;
using GravedadCero.Interaction;
using GravedadCero.Level;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GravedadCero.Player
{
    /// <summary>
    /// Lee el mapa de acciones "Player" y lo traduce en órdenes para PlayerMotor y
    /// GravityCamera, Q/R/F para GravityController, E para PlayerInteractor, clic izquierdo
    /// para GravitationalTool, Retroceso para reiniciar, Esc para la pausa (o para volver al
    /// menú tras completar la sección) y Enter para continuar. En la victoria no se mueve a Kai
    /// ni la cámara: el cursor queda libre para los botones. Es el único script del jugador
    /// que conoce el Input System. Mientras el juego está en pausa no envía ninguna orden de
    /// juego: timeScale no detiene el Input System. Las referencias de herramienta, nivel y
    /// pausa son opcionales (Test_Gravity no las usa).
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] InputActionAsset actions;
        [SerializeField] PlayerMotor motor;
        [SerializeField] GravityCamera gravityCamera;
        [SerializeField] GravityController gravityController;
        [SerializeField] PlayerInteractor interactor;
        [SerializeField] GravitationalTool gravitationalTool;
        [SerializeField] LevelManager levelManager;
        [SerializeField] PauseController pauseController;

        InputActionMap playerMap;
        InputAction moveAction;
        InputAction lookAction;
        InputAction jumpAction;
        InputAction gravityLeftAction;
        InputAction gravityRightAction;
        InputAction gravityFlipAction;
        InputAction interactAction;
        InputAction anchorAction;
        InputAction restartAction;
        InputAction menuAction;
        InputAction confirmAction;

        void Awake()
        {
            playerMap = actions.FindActionMap("Player", throwIfNotFound: true);
            moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
            lookAction = playerMap.FindAction("Look", throwIfNotFound: true);
            jumpAction = playerMap.FindAction("Jump", throwIfNotFound: true);
            gravityLeftAction = playerMap.FindAction("GravityLeft", throwIfNotFound: true);
            gravityRightAction = playerMap.FindAction("GravityRight", throwIfNotFound: true);
            gravityFlipAction = playerMap.FindAction("GravityFlip", throwIfNotFound: true);
            interactAction = playerMap.FindAction("Interact", throwIfNotFound: true);
            anchorAction = playerMap.FindAction("Anchor", throwIfNotFound: true);
            restartAction = playerMap.FindAction("Restart", throwIfNotFound: true);
            menuAction = playerMap.FindAction("Pause", throwIfNotFound: true);
            confirmAction = playerMap.FindAction("Confirm", throwIfNotFound: true);
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
            // Durante un fundido entre escenas no se acepta ninguna orden.
            if (SceneTransition.IsTransitioning)
            {
                motor.SetMoveInput(Vector3.zero);
                return;
            }

            if (levelManager != null)
            {
                // Retroceso reinicia al instante, también desde la pausa.
                if (restartAction.WasPressedThisFrame())
                {
                    if (pauseController != null) pauseController.RestartSection();
                    else levelManager.Restart();
                    return;
                }
                if (menuAction.WasPressedThisFrame())
                {
                    // Con la sección completada, Esc vuelve al menú (el progreso ya está guardado).
                    if (levelManager.IsCompleted || pauseController == null) levelManager.ExitToMenu();
                    else pauseController.HandleCancel();
                    return;
                }
                if (levelManager.IsCompleted)
                {
                    // Con un botón de la victoria resaltado, Enter lo pulsa la interfaz (Submit);
                    // aquí solo se cubre el caso de que no haya ninguno seleccionado.
                    if (confirmAction.WasPressedThisFrame() && !UIHasSelection()) levelManager.Continue();
                    motor.SetMoveInput(Vector3.zero);
                    return;
                }
            }

            // En pausa no se mueve a Kai ni la cámara: se suelta cualquier movimiento pendiente.
            if (pauseController != null && pauseController.IsPaused)
            {
                motor.SetMoveInput(Vector3.zero);
                return;
            }

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

            if (interactor != null && interactAction.WasPressedThisFrame()) interactor.TryInteract();
            if (gravitationalTool != null && anchorAction.WasPressedThisFrame()) gravitationalTool.ToggleAnchor();
        }

        static bool UIHasSelection()
        {
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            return selected != null && selected.activeInHierarchy;
        }
    }
}
