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
| N1.2 | Secciones 1-2 (pozo, `HazardZone`, interruptor en el techo) y 1-3 (dos placas, `ActivationGate`) | `feat: add Sector 1 sections 1-2 and 1-3` | Sí | Sí: 16 + 18 comprobaciones en Play Mode | **Pendiente** |

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
