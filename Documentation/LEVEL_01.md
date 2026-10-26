# Sector 1: Módulo de Acoplamiento

El Sector 1 tiene tres secciones, cada una en su propia escena. Al completar una sección se desbloquea la siguiente en el menú y se puede pasar a ella con **Enter**.

| Sección | Escena | Enseña |
|---|---|---|
| 1-1 Muelle de acoplamiento | `Level_01_DockingBay` | Gravedad, física de las cajas y anclaje |
| 1-2 Pozo de mantenimiento | `Level_01_2_MaintenanceShaft` | Caminar por paredes y techo, e interruptores en superficies giradas |
| 1-3 Esclusa de carga | `Level_01_3_CargoLock` | Combinar dos cajas: una anclada y otra que se sostiene por su peso |

---

# Sección 1-1: Muelle de acoplamiento (`Level_01_DockingBay`)

Sección tutorial. Enseña a combinar tres mecánicas: el cambio de gravedad, la física de las cajas y el anclaje.

## Objetivo

Abrir la compuerta norte y llegar a la zona de salida luminosa de la sala del fondo.

## Distribución

| Zona | Posición aproximada | Contenido |
|---|---|---|
| Inicio | Extremo sur, `(0, 0, 2)` | Kai aparece mirando al norte |
| Sala principal | 12 × 28 m, 8 m de alto, con techo | Recinto cerrado: un cambio de gravedad siempre lleva a una superficie |
| Plataforma de carga | Lado oeste, `z = 16`, 2.4 m de alto | La caja de carga. Kai no la alcanza saltando, porque su salto llega a 1.4 m |
| Placa de presión | Pared este, `z = 16`, de 1.6 a 7.9 m de altura | Marco ámbar que se vuelve cian cuando tiene peso encima |
| Compuerta norte | `(0, 0, 26)` | Se abre mientras la placa está presionada |
| Sala de salida | Detrás de la compuerta | Zona cian en el piso: al entrar, el nivel se completa |

## Solución

1. Camina hacia el norte, sin girar a Kai, hasta la altura de la plataforma.
2. Pulsa **R**. La gravedad pasa a apuntar a la derecha de Kai (el este). La caja sale de la plataforma, cae sobre la placa de la pared este y la compuerta se abre. Kai también cae sobre esa pared.
3. Camina por la pared este hasta tener la caja a menos de 8 m, apúntala con la mira (el indicador muestra `[Clic] Anclar caja`) y haz **clic**. La caja queda anclada y brilla en cian.
4. Devuelve la gravedad al piso con **Q** o **R**, según hacia dónde mire Kai. La caja anclada sigue sobre la placa y la compuerta sigue abierta.
5. Cruza la compuerta y entra en la zona de salida.

**Si no se ancla la caja**, al volver la gravedad la caja cae al suelo, la placa se libera y la compuerta se cierra. No hay forma de volver a subir la caja a la plataforma, así que hay que reiniciar con **Retroceso**.

## Reglas de los mecanismos

- **Placa de presión:** cuenta solo objetos anclables (cajas). Kai no la activa.
- **Anclaje:** se puede tener una caja anclada a la vez. Una caja anclada no se mueve con ningún cambio de gravedad. Al soltarla, cae según la gravedad que haya en ese momento.
- **Selección:** el rayo sale del centro de la cámara. Una pared entre la cámara y la caja, o entre Kai y la caja, impide seleccionarla.
- **Caídas largas:** si Kai pasa más de 4 s en el aire, reaparece en el inicio con la gravedad normal. La caja no se reinicia.

---

# Sección 1-2: Pozo de mantenimiento (`Level_01_2_MaintenanceShaft`)

**Objetivo:** cruzar el pozo y abrir la compuerta norte.

| Zona | Contenido |
|---|---|
| Inicio | Extremo sur. Kai aparece mirando al norte |
| Pozo | `z = 14..26`, 8 m de profundidad, con bordes ámbar. El fondo rojo es una `HazardZone`: tocarlo devuelve a Kai al inicio |
| Otro lado | Suelo de `z = 26` a `z = 42`, con un interruptor **en el techo** (`z = 36`, marcado en ámbar) |
| Compuerta norte | `z = 42`. La abre el interruptor del techo |

**Solución:**
1. Desde el inicio, mirando al norte, pulsa **R**: Kai cae sobre la pared este.
2. Camina por la pared hacia el norte hasta pasar el borde ámbar del otro lado del pozo.
3. Pulsa **Q**, si sigues mirando al norte, para volver al suelo.
4. Pulsa **F**: Kai sube al techo. Camina hasta el interruptor y pulsa **E**.
5. Pulsa **F** para volver al suelo, cruza la compuerta y entra en la zona de salida.

Saltar no basta para cruzar: el pozo mide 12 m y el salto horizontal llega a unos 5 m.

---

# Sección 1-3: Esclusa de carga (`Level_01_3_CargoLock`)

**Objetivo:** presionar las dos placas a la vez y salir por la compuerta norte.

| Elemento | Posición | Detalle |
|---|---|---|
| Caja A | Suelo, `(1, 0, 8)` | Debe terminar en la placa baja |
| Caja B | Plataforma oeste de 2.4 m, `z = 16` | Debe terminar en la placa alta |
| Placa A (baja) | Pared oeste, `z = 8`, de 0 a 2.6 m | Una caja apoyada en la esquina del suelo la sigue presionando |
| Placa B (alta) | Pared este, `z = 16`, de 1.6 a 7.9 m | Solo se mantiene presionada con la caja anclada |
| `ActivationGate` | — | Abre la compuerta solo mientras **las dos** placas están presionadas |

**Solución:**
1. Mirando al norte, pulsa **R**. Las dos cajas vuelan al este: la caja B cae sobre la placa alta y la caja A queda abajo, sin presionar nada.
2. Camina por la pared este hasta la caja B y **ánclala** con clic.
3. Pulsa **F**: la gravedad apunta al oeste. La caja A vuela a la pared oeste y presiona la placa baja, y la compuerta se abre.
4. Vuelve al suelo con **Q** o **R**. La caja A cae a la esquina y **sigue sobre la placa por su propio peso**; la caja B sigue anclada.
5. Cruza la compuerta y entra en la zona de salida. Es el final del Sector 1: **Enter** vuelve al menú.
