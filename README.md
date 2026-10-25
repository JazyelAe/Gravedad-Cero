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

Abre `Assets/GravedadCero/Scenes/Level_01_DockingBay.unity` y pulsa **Play**. Es la primera escena de Build Settings. La solución del nivel está en [`Documentation/LEVEL_01.md`](Documentation/LEVEL_01.md).

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
| Retroceso (Backspace) | Reiniciar el nivel |

## Escenas

| Escena | Uso |
|---|---|
| `Level_01_DockingBay` | Primer nivel jugable: tutorial con un puzle de caja, gravedad y anclaje |
| `Test_Gravity` | Escena de pruebas de movimiento, gravedad, puertas e interruptores |
| `Showcase_Kenney` | Catálogo de las piezas del Space Station Kit |

## Estado de desarrollo

Prototipo jugable del Nivel 1. El estado de cada tarea, separado entre implementado, probado automáticamente y pendiente de prueba manual, está en [`Documentation/TASKS.md`](Documentation/TASKS.md).

## Licencias y créditos

Los assets de terceros y sus licencias se registran en [`Documentation/ASSET_CREDITS.md`](Documentation/ASSET_CREDITS.md). Por ahora el proyecto incluye el Space Station Kit de Kenney (CC0).
