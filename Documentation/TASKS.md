# Registro de tareas

Estados:
- **Implementado:** el código existe y compila sin errores ni advertencias.
- **Prueba automática:** se verificó con una prueba ejecutada en Unity, en modo batch.
- **Prueba manual:** el desarrollador lo probó en Play Mode.

"Pendiente" significa que esa verificación todavía no se ha hecho.

| Tarea | Descripción | Commit | Implementado | Prueba automática | Prueba manual |
|---|---|---|---|---|---|
| 0.1 | Proyecto Unity en el repositorio, `.gitignore` y `.gitattributes` | `203fbf8` | Sí | No aplica | Sí: el proyecto abre sin errores |
| 1.1 | Movimiento relativo a la superficie, salto, cámara orbital, escena `Test_Gravity` | `0023a77` | Sí | Sí: regresión en Play Mode (25-10-2026) | Sí |
| 2.1 | Cambio de gravedad con Q, R y F, `FallRecovery`, giro de 180° estable | `19cef60` | Sí | Sí: F lleva a Kai al techo, en regresión | Sí |
| 3.1 | Interacción con E, interruptores, puerta corrediza | `e3f69e5` | Sí | Parcial: la puerta, con la placa de presión del Nivel 1 | Sí |
| V.1 | Paleta de materiales, iluminación y posprocesado | `faaa41a` | Sí | No aplica | Sí: revisión visual |
| V.2 | Importación del Kenney Space Station Kit (CC0) y `Showcase_Kenney` | `7c57ed6` | Sí | No aplica | Sí: revisión visual |
| 3.2 | Caja de carga (`CargoBox`), anclaje con clic (`GravitationalTool`, `AnchorableBody`) | `4115bc7` | Sí | Sí: Play Mode, ver abajo | Sí (25-10-2026) |
| N1 | Nivel 1 `Level_01_DockingBay`: placa de presión, salida, `LevelManager`, HUD, reinicio | `7e147cd` | Sí | Sí: Play Mode, ver abajo | Sí (25-10-2026) |
| V.3 | Personaje animado de Kai: Kenney Animated Characters, skin propia, `PlayerAnimator`, `Kai.controller` | `7f9d27f` | Sí | Sí: Play Mode con capturas, ver abajo | Sí: el desarrollador jugó la build (25-10-2026) |
| B.1 | Build de Windows 64 bits (`Builds/Windows/GravedadCero.exe`, no versionado) | `fe9b80d` | Sí | Prueba de humo: el .exe se ejecutó 20 s sin errores | Sí |
| M.1 | Menú principal, progreso con `GameProgress` (PlayerPrefs), flujo Enter/Esc entre secciones | `b4fc5fd` | Sí | Sí: 10 comprobaciones en Play Mode | **Pendiente** |
| S.1 | Cinco skins para Kai (`SkinCatalog`, `KaiSkinApplier`) elegibles desde el menú | `283d40b` | Sí | Sí: cubierta por la prueba del menú | **Pendiente** |
| N1.2 | Secciones 1-2 (pozo, `HazardZone`, interruptor en el techo) y 1-3 (dos placas, `ActivationGate`) | `d0fe3b2` | Sí | Sí: 16 + 18 comprobaciones en Play Mode | **Pendiente** |
| T.1 | Ejecutor permanente de pruebas de regresión (`Tests/Editor/RegressionRunner.cs`, solo editor) | `207987f` | Sí | Sí: 66/66 (25-10-2026) | No aplica |
| B.2 | Build Beta de Windows: menú y 3 secciones; `SampleScene` y `Test_Gravity` fuera del build; `companyName` = JazyelAe | `7f9fa47` | Sí | Prueba de humo del .exe y del ZIP descomprimido | **Pendiente** |
| P2.1 | Menú de pausa (`PauseController`, `PauseMenuUI`): Reanudar, Controles, Configuración, Reiniciar y Volver al menú con confirmación | `9fe0baf`, `43929cf` | Sí | Sí: 26 comprobaciones en Play Mode, ver abajo | **Pendiente** |
| P2.2 | Configuración de cámara (`GameSettings`): sensibilidad del mouse e invertir Y, guardadas en PlayerPrefs | `9fe0baf` | Sí | Sí: incluida en la prueba de pausa | **Pendiente** |
| P2.3 | HUD: tarjeta de introducción que se desvanece, objetivo compacto, controles movidos a la pausa | `70d27f2` | Sí | Sí: 5 comprobaciones (aparición, ocultar al moverse, ocultar a los 8 s) | **Pendiente** |
| R1.1 | Identidad visual Helios-7: TextMeshPro, fuentes Orbitron y Exo 2 (OFL), `UITheme`, `UIFactory`, `UIButtonFX`, `UIReveal` | `ee392bd`, `b68b374` | Sí | Sí: capturas a 4 resoluciones sin textos cortados | **Pendiente** |
| R1.2 | Transiciones con fundido (`SceneTransition`) y protección contra cargas dobles | `b68b374` | Sí | Sí: una sola carga con clic y Enter, y desde la pausa | **Pendiente** |
| R1.3 | Menú principal rediseñado: fondo espacial, tarjetas de sección, ficha de personaje, confirmación de Nueva partida | `bb99458` | Sí | Sí: regresión y capturas | **Pendiente** |
| R1.4 | HUD, pausa, controles, configuración, confirmación y victoria con el estilo Helios-7; botones de victoria con mouse y teclado | Etapa B | Sí | Sí: 19 comprobaciones nuevas y capturas | **Pendiente** |
| T.2 | Capturas de interfaz con revisión automática de textos (`Tests/Editor/UIScreenshotRunner.cs`) | `f5d15b4` | Sí | No aplica | No aplica |

## Rediseño visual, fase 1 (26-10-2026)

**Regresión: 114 de 114 comprobaciones superadas**, con la consola sin errores ni advertencias. Hay 19 nuevas en el bloque Victoria, que comprueban que:
- se ve la pantalla de victoria con el cursor libre y Continuar resaltado;
- las flechas cambian la opción resaltada;
- Enter en Continuar carga la siguiente sección una sola vez;
- un clic en Repetir y Enter en el mismo instante producen una sola carga;
- pasar el mouse por un botón lo resalta y Enter lo pulsa;
- Nueva partida con progreso pide confirmación y cancelar conserva el progreso;
- Retroceso desde la pausa recarga una sola vez, sin pausa ni tiempo congelado, aunque se pulse Esc o se pida otro reinicio durante el fundido;
- todos los textos del menú, el HUD, la pausa y la victoria tienen sus caracteres en su fuente, sin depender de la de respaldo.

**Capturas:** las 13 pantallas a 1280×720, 1920×1080, 2560×1080 y 1024×768, sin textos a revisar ni mensajes de consola. Se revisaron a ojo.

El clic y el paso del mouse se simulan con eventos de la interfaz, no con un mouse real.

**Pendiente de prueba manual:**
- clic real con el mouse en los botones de victoria, pausa y menú;
- aspecto de las animaciones y del fundido entre escenas;
- bloqueo real del cursor al reanudar y al empezar una sección tras la victoria;
- legibilidad en un monitor real a cada resolución.

## Regresión con la Prioridad 2 (25-10-2026)

**95 de 95 comprobaciones superadas en dos ejecuciones seguidas**, con la consola sin errores ni advertencias.

| Bloque | Comprobaciones |
|---|---|
| Menú | 9 |
| Sección 1-1 | 21 (incluye que en la victoria no se abre la pausa) |
| Sección 1-2 | 14 |
| Sección 1-3 | 17 |
| Pausa | 26 |
| HUD | 2 |
| Test_Gravity | 5 |
| Consola | 1 |

Las pruebas de pausa pulsan **Esc, R, W y Retroceso en un teclado virtual del Input System**, así que pasan por el `PlayerInputHandler` real. No es un teclado físico. Comprueban que:
- la pausa congela la caída de Kai (desplazamiento 0.0000 m) y al reanudar sigue cayendo;
- R no cambia la gravedad en pausa;
- Esc dos veces seguidas no duplica la pausa, y Esc dentro de un submenú vuelve al panel principal;
- Reiniciar y Volver al menú piden confirmación, Esc cancela esa confirmación y, al confirmar, `timeScale` vuelve a 1 y se conserva el progreso;
- Retroceso reinicia sin confirmar;
- la sensibilidad se aplica al instante (10 px × 0.3 = 3°), invertir Y cambia el sentido, el valor se limita a un máximo seguro y todo persiste tras reiniciar;
- la tarjeta de introducción se oculta al moverse y a los 8 s.

**Corrección encontrada por las pruebas:** `LevelManager.Restart()` recargaba la escena por su índice de Build Settings y, si no lo encontraba, cargaba el índice 0, que es el menú. Ahora recarga por la ruta de la escena.

**Pendiente de prueba manual:**
- el clic real en los botones de pausa, el deslizador y la casilla;
- la navegación con flechas en la pausa;
- el bloqueo real del cursor al reanudar (en modo batch no hay ventana para bloquearlo);
- el aspecto del fundido de la introducción.

## Regresión completa (25-10-2026, `RegressionRunner`)

**66 de 66 comprobaciones superadas** en una sola sesión de Play Mode, con la consola sin errores ni advertencias:

| Bloque | Comprobaciones |
|---|---|
| Menú | 9 |
| Sección 1-1 | 20 |
| Sección 1-2 | 14 |
| Sección 1-3 | 17 |
| Test_Gravity | 5 |
| Consola | 1 |

Se ejecuta desde el editor con **Gravedad Cero → Ejecutar pruebas de regresión**, o por línea de comandos (ver `README.md`). El informe se guarda en `Logs/RegressionReport.txt`.

**Build Beta (25-10-2026):**
- `Builds/Windows/GravedadCero.exe`, 107.6 MB, con 4 escenas: `MainMenu` y las tres secciones. 0 errores y 3 advertencias conocidas (colliders de malla sin pre-bake y dos shaders de depuración de URP descartados).
- Prueba de humo sin ventana: 25 s sin errores en el log. La build anterior se conservó en `Builds/Windows_anterior_solo_1-1`.
- ZIP: `Builds/GravedadCero_Beta_2026-10-25.zip`, 39.2 MB, sin la carpeta de depuración. Se descomprimió en otra carpeta y se ejecutó 20 s sin errores.
- **Pendiente de prueba manual:** la interfaz, los gráficos, los controles reales y el guardado entre sesiones.

## Pruebas del menú y de las secciones 1-2 y 1-3 (25-10-2026)

Todas se superaron a la primera, con la consola sin errores ni advertencias. Cada prueba restaura el progreso guardado al terminar.

- **Menú (10/10):**
  - EventSystem presente y 3 secciones configuradas.
  - Una sección bloqueada no se carga.
  - Siguiente y Anterior cambian la skin y dan la vuelta al catálogo, y la elección se guarda.
  - Nueva partida carga la sección 1-1 con la skin elegida aplicada.
  - Salir al menú vuelve a `MainMenu`.
- **Sección 1-2 (16/16):**
  - Caer al fondo rojo devuelve a Kai al inicio en 0.7 s.
  - R lleva a Kai a la pared este, por la que cruza el pozo caminando (4.8 s). Q lo devuelve al suelo al otro lado.
  - F lo lleva al techo, donde el interruptor queda al alcance y abre la compuerta.
  - Kai completa la sección, se desbloquea la 1-3 y Continuar la carga.
- **Sección 1-3 (18/18):**
  - Con la gravedad al este, la caja B presiona la placa alta; la caja A queda abajo y la compuerta sigue cerrada.
  - Se ancla la caja B. Con F, la caja A presiona la placa baja y la compuerta se abre.
  - Con la gravedad normal, la caja A sigue en su placa por su propio peso.
  - Kai completa la sección, que es el final del sector, y Continuar vuelve al menú.

**No cubierto por estas pruebas:**
- El clic real con el mouse sobre los botones del menú: la prueba llama a las mismas funciones que los botones.
- La navegación con teclado del menú.
- Las pruebas de 1-2 y 1-3 teletransportan a Kai en algunos tramos: junto al interruptor y junto a la caja.

## Última prueba automática del Nivel 1 en Play Mode (25-10-2026)

`Level_01_DockingBay`, después de la Tarea V.3: **28 de 28 comprobaciones superadas.** Son las 23 del nivel más 5 del personaje:
- Kai tiene un Animator humanoide con su controlador.
- La cápsula visible ya no existe.
- En reposo, el Animator recibe velocidad 0 y estado "en el suelo".
- Al caer, pasa al estado de salto.
- Corriendo, recibe la velocidad completa (6 m/s).

Se ejecutó con gráficos y se generaron capturas de Kai en reposo, corriendo y sobre la pared.

Detalle de las 23 comprobaciones del nivel:

- Referencias de escena completas. La caja descansa sobre la plataforma. La placa y la compuerta empiezan inactivas.
- R gira la gravedad a +X. La caja suelta cae sobre la placa de la pared este, la placa se activa y la compuerta se abre. Kai cae y se alinea con la pared.
- El raycast desde la cámara selecciona la caja. Apuntando a otro lado no selecciona nada. El clic ancla la caja y la vuelve cinemática.
- Al volver la gravedad normal, la caja anclada se mueve 0.000 m, la placa sigue presionada y la compuerta sigue abierta.
- Kai, caminando en línea recta, cruza la compuerta y completa el nivel en 1.6 s.
- Al soltar la caja, esta cae al suelo, la placa se libera y la compuerta se cierra.
- Consola sin errores ni advertencias durante toda la prueba.

`Test_Gravity` (regresión, después de V.3): **5 de 5.** Kai tiene el modelo animado, está en el suelo, F lo lleva al techo y lo alinea, y la consola queda limpia.

**Qué no cubren estas pruebas:**
- El teclado y el mouse reales: la prueba da las órdenes por código.
- El HUD: la prueba se ejecutó sin gráficos.
- La sensación de control.
- Caminar por la pared hasta la caja: la prueba teletransporta a Kai cerca de ella.

## Siguientes tareas propuestas

| Tarea | Descripción |
|---|---|
| 4.1 | Vida, daño y checkpoints, que sustituirán a `FallRecovery` |
| 4.2 | Dron de seguridad mínimo: patrulla, detección, persecución y ataque |
| 5.x | Ampliar el Nivel 1 con más salas y narrativa de ORION |
