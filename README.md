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

**Menú de pausa:** Reanudar, Controles (tabla de teclas), Configuración (sensibilidad del mouse e invertir el eje Y, que se guardan automáticamente), Reiniciar sección y Volver al menú. Las dos últimas piden confirmación; al volver al menú se conservan las secciones completadas.

La pausa muestra también el objetivo y la pista de la sección.

Al empezar cada sección aparece una tarjeta con el nombre, el objetivo, la pista y los controles esenciales. Se desvanece cuando Kai empieza a moverse o a los 8 segundos.

**Pantalla de victoria:** botones Continuar, Repetir sección y Menú principal, que se pueden pulsar con el mouse o elegir con flechas y Enter. Retroceso repite la sección y Esc vuelve al menú.

## Interfaz

Todas las pantallas comparten la identidad visual **Helios-7**, construida por código en `Assets/GravedadCero/Scripts/UI/`:
- `UITheme`: paleta, tamaños de texto, fuentes y sprites generados (paneles con esquinas recortadas, iconos);
- `UIFactory`: paneles, botones, deslizadores, casillas y recuadros de tecla;
- `UIButtonFX`: el foco es el mismo con teclado y mouse;
- `SceneTransition`: fundido entre escenas que impide cargas dobles.

Los textos usan TextMeshPro, con Orbitron para los títulos y Exo 2 para el resto (licencia SIL OFL, ver créditos).

## Escenas

| Escena | Uso |
|---|---|
| `MainMenu` | Menú principal con vista previa de Kai |
| `Level_01_DockingBay` | Sección 1-1, Muelle de acoplamiento: caja, gravedad y anclaje |
| `Level_01_2_MaintenanceShaft` | Sección 1-2, Pozo de mantenimiento: caminar por paredes y techo |
| `Level_01_3_CargoLock` | Sección 1-3, Esclusa de carga: dos cajas y dos placas |
| `Test_Gravity` | Escena de pruebas de movimiento, gravedad, puertas e interruptores (fuera del build) |
| `Showcase_Kenney` | Catálogo de las piezas del Space Station Kit (fuera del build) |

## Pruebas automáticas

Las pruebas de regresión recorren en Play Mode el menú, las tres secciones y `Test_Gravity`. Están en `Assets/GravedadCero/Tests/Editor/RegressionRunner.cs`, que no se incluye en el build.

- **Desde el editor:** menú **Gravedad Cero → Ejecutar pruebas de regresión**.
- **Desde la línea de comandos**, con el editor cerrado:
  ```bash
  "C:\Program Files\Unity\Hub\Editor\6000.6.2f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod GravedadCero.Tests.RegressionRunner.RunFromCommandLine -logFile Logs/regression.log
  ```

El resultado queda en `Logs/RegressionReport.txt`. Las pruebas simulan al jugador por código, sin teclado ni mouse reales, y restauran el progreso guardado al terminar.

**Capturas de interfaz:** menú **Gravedad Cero → Capturas de interfaz**, o `-executeMethod GravedadCero.Tests.UIScreenshotRunner.RunFromCommandLine`, sin `-nographics`. Captura el menú, el HUD, la pausa y la victoria a 1280×720, 1920×1080, 2560×1080 y 1024×768 en `Logs/UIScreens/`. El informe `report.txt` señala los textos que no caben en su recuadro o se salen de la pantalla.

## Generar el ejecutable

**File → Build Profiles → Windows → Build**, con la salida en `Builds/Windows`. Las escenas activas son `MainMenu` y las tres secciones. Para compartir el juego, comprime la carpeta completa sin `GravedadCero_BackUpThisFolder_ButDontShipItWithYourGame`. `Builds/` está fuera de Git.

## Estado de desarrollo

Prototipo jugable del Sector 1, con tres secciones, menú principal, guardado de progreso y cinco skins. El estado de cada tarea, separado entre implementado, probado automáticamente y pendiente de prueba manual, está en [`Documentation/TASKS.md`](Documentation/TASKS.md).

## Licencias y créditos

Los assets de terceros y sus licencias se registran en [`Documentation/ASSET_CREDITS.md`](Documentation/ASSET_CREDITS.md). Por ahora el proyecto incluye dos paquetes de Kenney (CC0): Space Station Kit y Animated Characters Protagonists.
