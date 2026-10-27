# Gravedad Cero

Aventura 3D de puzzles y plataformas en tercera persona, ambientada en la estación orbital Helios-7. El jugador controla a Kai, un ingeniero de mantenimiento que usa un Módulo Gravitacional para caminar por paredes y techos, resolver puzzles y escapar de ORION, la IA de la estación.



## Requisitos

- **Unity 6000.6.2f1** (Unity 6), instalada desde Unity Hub
- Universal Render Pipeline (URP) 17.6, incluida en el proyecto
- Input System 1.20, incluido en el proyecto
- PC con Windows, teclado y mouse

## Cómo abrir el proyecto

1. Clona el repositorio:
   ```bash
   git clone https://github.com/JazyelAe/Gravedad-Cero.git
   ```
2. En Unity Hub, elige **Add → Add project from disk** y selecciona la carpeta clonada.
3. Ábrelo con la versión **6000.6.2f1**. La primera apertura tarda varios minutos, porque Unity regenera la carpeta `Library/`.

## Cómo jugar

Abre `Assets/GravedadCero/Scenes/MainMenu.unity` y pulsa **Play**. Es la primera escena de Build Settings. Desde el menú:
- **Nueva partida** empieza en la sección 1-1;
- **Continuar** carga la última sección desbloqueada;
- **Secciones** permite elegir entre las desbloqueadas;
- **Personaje** cambia la skin de Kai.

El progreso y la skin se guardan solos. Las soluciones están en [`Documentation/LEVEL_01.md`](Documentation/LEVEL_01.md).

## Controles

| Tecla | Acción |
|---|---|
| W / A / S / D | Moverse, relativo a la cámara |
| Mouse | Girar la cámara |
| Espacio | Saltar, en dirección opuesta a la gravedad |
| Q / R | Girar la gravedad 90° hacia la izquierda o la derecha de Kai |
| F | Invertir la gravedad 180° |
| Clic izquierdo | Anclar o soltar la caja que está bajo la mira, a 8 m como máximo |
| E | Usar interruptores |
| Retroceso (Backspace) | Reiniciar la sección al instante, sin confirmación |
| Enter | En los menús y en la pantalla de victoria, pulsar la opción resaltada |
| Flechas | Moverse entre las opciones de los menús |
| Esc | Abrir o cerrar la pausa, o volver atrás en un menú. En la pantalla de victoria, volver al menú principal |

**Menú de pausa:** Reanudar, Controles (tabla de teclas), Configuración (sensibilidad del mouse, invertir el eje Y y volumen de la música y de los efectos, que se guardan automáticamente), Reiniciar sección y Volver al menú. Las dos últimas piden confirmación; al volver al menú se conservan las secciones completadas.

La pausa muestra también el objetivo y la pista de la sección.

Al empezar cada sección aparece una tarjeta con el nombre, el objetivo, la pista y los controles esenciales. Se desvanece cuando Kai empieza a moverse o a los 8 segundos.

**Ventilas de vapor (1-2):** se encienden a intervalos fijos; un parpadeo ámbar avisa antes de cada encendido. Solo son peligrosas encendidas.

**Drones de seguridad (1-1 y 1-2):** patrullan una zona; si ven a Kai se ponen en alerta y lo persiguen. Cada golpe quita una de las 3 unidades de **integridad del traje** (abajo a la izquierda); sin integridad, Kai reaparece en el último punto de control. Subir a paredes, pasarelas o techo deja a Kai fuera de su alcance, y una caja que cae sobre un dron lo aturde.

**Registros de datos:** objetos luminosos escondidos en rutas alternativas. Se guardan y el menú de secciones muestra cuántos hay encontrados.

**Pantalla de derrota:** al caer al vacío o al fondo de un pozo, tocar una zona roja o el vapor, o perder toda la integridad del traje, el juego se detiene y aparece la pantalla de derrota (HAS CAÍDO, ZONA PELIGROSA o TRAJE DESTRUIDO) con Reintentar (desde el último punto de control), Reiniciar sección y Menú principal. Funciona con el mouse y con Enter, Retroceso y Esc.

**Pantalla de victoria:** botones Continuar, Repetir sección y Menú principal, que se pueden pulsar con el mouse o elegir con flechas y Enter. Retroceso repite la sección y Esc vuelve al menú.

## Interfaz

Todas las pantallas comparten la identidad visual **Helios-7**, construida por código en `Assets/GravedadCero/Scripts/UI/`:
- `UITheme`: paleta, tamaños de texto, fuentes y sprites generados (paneles con esquinas recortadas, iconos);
- `UIFactory`: paneles, botones, deslizadores, casillas y recuadros de tecla;
- `UIButtonFX`: el foco es el mismo con teclado y mouse;
- `SceneTransition`: fundido entre escenas que impide cargas dobles.

**Audio:** `Scripts/Audio/GameAudio` se crea solo al arrancar, sobrevive a los cambios de escena y hace un fundido cruzado hacia la pista de cada escena. Escucha los eventos del juego (gravedad, anclaje, compuertas, placas, interruptores, reaparición, pausa y victoria) para reproducir los efectos. La asignación está en `Audio/Resources/AudioCatalog.asset`. Música de SRG774 y efectos de Kenney, ambos CC0.

Los textos usan TextMeshPro, con Orbitron para los títulos y Exo 2 para el resto (licencia SIL OFL, ver créditos).

## Escenas

| Escena | Uso |
|---|---|
| `MainMenu` | Menú principal con vista previa de Kai |
| `Level_01_DockingBay` | Sección 1-1, Muelle de acoplamiento: caja, gravedad y anclaje, en dos salas |
| `Level_01_2_MaintenanceShaft` | Sección 1-2, Pozo de mantenimiento: pozo vertical de 48 m que se baja por las paredes, con ventilas de vapor |
| `Level_01_3_CargoLock` | Sección 1-3, Esclusa de carga: dos esclusas, placas en paredes, techo y suelo |
| `Level_01_4_RotationRing` | Sección 1-4, Anillo de rotación: franjas peligrosas en suelo, pared y techo |
| `Level_01_5_Core` | Sección 1-5, Núcleo del reactor: la gravedad también apunta al norte |
| `Test_Gravity` | Escena de pruebas de movimiento, gravedad, puertas e interruptores (fuera del build) |
| `Showcase_Kenney` | Catálogo de las piezas del Space Station Kit (fuera del build) |

## Pruebas automáticas

Las pruebas de regresión recorren en Play Mode el menú, las cinco secciones (resolviendo cada sala), la pausa, la victoria, el audio y `Test_Gravity`. Están en `Assets/GravedadCero/Tests/Editor/RegressionRunner.cs`, que no se incluye en el build.

- **Desde el editor:** menú **Gravedad Cero → Ejecutar pruebas de regresión**.
- **Desde la línea de comandos**, con el editor cerrado:
  ```bash
  "C:\Program Files\Unity\Hub\Editor\6000.6.2f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod GravedadCero.Tests.RegressionRunner.RunFromCommandLine -logFile Logs/regression.log
  ```

El resultado queda en `Logs/RegressionReport.txt`. Las pruebas simulan al jugador por código, sin teclado ni mouse reales, y restauran el progreso guardado al terminar.

**Constructor de niveles:** `Assets/GravedadCero/Editor/LevelBuilding/` (solo editor). `LevelKit` tiene las piezas (salas cerradas, paredes con puertas, andenes, pasarelas, mecanismos, drones, registros) `Level01Builder` y `Level02Builder` generan las secciones 1-1 y 1-2: menú **Gravedad Cero → Niveles → Reconstruir 1-1 / 1-2**, o `-executeMethod GravedadCero.EditorTools.Level0XBuilder.RunFromCommandLine`. Conserva Kai, cámara, HUD y pausa, y rehace la geometría y los puzles.

**Capturas de interfaz:** menú **Gravedad Cero → Capturas de interfaz**, o `-executeMethod GravedadCero.Tests.UIScreenshotRunner.RunFromCommandLine`, sin `-nographics`. Captura el menú, el HUD, la pausa y la victoria a 1280×720, 1920×1080, 2560×1080 y 1024×768 en `Logs/UIScreens/`. El informe `report.txt` señala los textos que no caben en su recuadro o se salen de la pantalla.

## Generar el ejecutable

**File → Build Profiles → Windows → Build**, con la salida en `Builds/Windows`. Las escenas activas son `MainMenu` y las cinco secciones. Para compartir el juego, comprime la carpeta completa sin `GravedadCero_BackUpThisFolder_ButDontShipItWithYourGame`. `Builds/` está fuera de Git.

## Estado de desarrollo

Prototipo jugable del Sector 1, con cinco secciones (tres de ellas con dos salas), puntos de control, música y efectos, menú principal, guardado de progreso y cinco skins. El estado de cada tarea, separado entre implementado, probado automáticamente y pendiente de prueba manual, está en [`Documentation/TASKS.md`](Documentation/TASKS.md).

## Licencias y créditos

Los assets de terceros y sus licencias se registran en [`Documentation/ASSET_CREDITS.md`](Documentation/ASSET_CREDITS.md). Por ahora el proyecto incluye dos paquetes de Kenney (CC0): Space Station Kit y Animated Characters Protagonists.
