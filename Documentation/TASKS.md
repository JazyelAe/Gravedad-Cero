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
| N1 | Nivel 1 `Level_01_DockingBay`: placa de presión, salida, `LevelManager`, HUD, reinicio | `feat: add playable Level 1 docking bay` | Sí | Sí: Play Mode, ver abajo | Sí (25-10-2026) |

## Última prueba automática en Play Mode (25-10-2026)

`Level_01_DockingBay`: **23 de 23 comprobaciones superadas.**

- Referencias de escena completas. La caja descansa sobre la plataforma. La placa y la compuerta empiezan inactivas.
- R gira la gravedad a +X. La caja suelta cae sobre la placa de la pared este, la placa se activa y la compuerta se abre. Kai cae y se alinea con la pared.
- El raycast desde la cámara selecciona la caja. Apuntando a otro lado no selecciona nada. El clic ancla la caja y la vuelve cinemática.
- Al volver la gravedad normal, la caja anclada se mueve 0.000 m, la placa sigue presionada y la compuerta sigue abierta.
- Kai, caminando en línea recta, cruza la compuerta y completa el nivel en 1.6 s.
- Al soltar la caja, esta cae al suelo, la placa se libera y la compuerta se cierra.
- Consola sin errores ni advertencias durante toda la prueba.

`Test_Gravity` (regresión): **5 de 5.** Kai en el suelo, la puerta presente, F lleva a Kai al techo y lo alinea, y la consola queda limpia.

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
