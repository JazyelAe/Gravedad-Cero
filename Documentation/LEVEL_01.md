# Sector 1: Módulo de Acoplamiento

El Sector 1 tiene cinco secciones, cada una en su propia escena y de dificultad creciente. Al completar una sección se desbloquea la siguiente en el menú y se puede pasar a ella con **Continuar**. Las secciones 1-1, 1-2 y 1-3 tienen dos salas.

| Sección | Escena | Enseña |
|---|---|---|
| 1-1 Muelle de acoplamiento | `Level_01_DockingBay` | Gravedad, física de las cajas y anclaje; en la sala 2, el orden de los giros importa |
| 1-2 Pozo de mantenimiento | `Level_01_2_MaintenanceShaft` | Caminar por paredes y techo; en el pozo 2, dos interruptores en superficies distintas |
| 1-3 Esclusa de carga | `Level_01_3_CargoLock` | Una caja anclada y otra que se sostiene por su peso; en la esclusa 2, una placa en el techo |
| 1-4 Anillo de rotación | `Level_01_4_RotationRing` | Recorrer suelo, pared y techo esquivando franjas peligrosas |
| 1-5 Núcleo del reactor | `Level_01_5_Core` | La gravedad también apunta al norte o al sur (girando mirando a una pared lateral) |

**Puntos de control:** al entrar en ellos, por cualquier superficie, aparece el aviso "Punto de control" y Kai pasa a reaparecer allí, con la gravedad normal, al caer o tocar una zona roja. Retroceso sigue reiniciando la sección entera. Las cajas no vuelven a su sitio al reaparecer; si una queda inalcanzable, hay que reiniciar con Retroceso.

**La gravedad es global:** cada giro mueve todas las cajas sueltas de la sección, también las de la sala siguiente. Las salas están pensadas para que eso no resuelva la siguiente por adelantado.

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
5. Cruza la compuerta 1 y entra en la sala 2 (punto de control).

**Si no se ancla la caja**, al volver la gravedad la caja cae al suelo, la placa se libera y la compuerta se cierra. No hay forma de volver a subir la caja a la plataforma, así que hay que reiniciar con **Retroceso**.

## Reglas de los mecanismos

- **Placa de presión:** cuenta solo objetos anclables (cajas). Kai no la activa.
- **Anclaje:** se puede tener una caja anclada a la vez. Una caja anclada no se mueve con ningún cambio de gravedad. Al soltarla, cae según la gravedad que haya en ese momento.
- **Selección:** el rayo sale del centro de la cámara. Una pared entre la cámara y la caja, o entre Kai y la caja, impide seleccionarla.
- **Caídas largas:** si Kai pasa más de 4 s en el aire, reaparece en el inicio con la gravedad normal. La caja no se reinicia.

## Sala 2: Bodega alta (`z = 26..50`)

La placa está en la pared este, **entre 5.3 y 7.9 m de altura**. Si se lanza la caja al este desde el suelo, llega a la pared a 0.7 m y no la toca.

1. Pulsa **F**: la caja (y Kai) suben al techo.
2. Gira la gravedad hacia el este con **Q** o **R** (según hacia dónde mire Kai). La caja se desliza desde el techo hasta la pared, a la altura de la placa, y la compuerta 2 se abre.
3. Ancla la caja con clic. Anclarla suelta la caja de la sala 1, que ya no hace falta.
4. Vuelve al suelo, cruza la compuerta 2 y entra en la zona de salida.

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
5. Pulsa **F** para volver al suelo y cruza la compuerta 1.

Saltar no basta para cruzar: el pozo mide 12 m y el salto horizontal llega a unos 5 m.

## Pozo 2 (`z = 42..78`)

Repisa con punto de control (`z = 42..46`), pozo de 20 m (`z = 46..66`) y suelo hasta la compuerta 2 (`z = 78`). La compuerta 2 se abre con **dos** interruptores de un solo uso (una `ActivationGate` con dos `SignalLatch`): uno en la **pared este** (`z = 52`, a 2.5 m de altura) y otro en el **techo** (`z = 62`).

1. Desde la repisa, mirando al norte, pulsa **R** y camina por la pared este hasta el interruptor; pulsa **E**.
2. Sigue mirando al norte y pulsa **R** otra vez: Kai cae al techo, sobre el pozo.
3. Camina por el techo hasta el segundo interruptor y pulsa **E**. La compuerta 2 se abre.
4. Pasado el borde ámbar del otro lado, pulsa **F** para volver al suelo y sal.

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
5. Cruza la compuerta 1 y entra en la segunda esclusa (punto de control).

## Esclusa 2 (`z = 26..50`)

| Elemento | Posición | Detalle |
|---|---|---|
| Placa C | Techo, lado oeste, `z = 34` | Se presiona con la caja anclada en el techo |
| Placa D | Suelo, junto a la pared este, `z = 42` | Una caja apoyada en ella la presiona por su peso |
| Cajas C y D | Suelo, `z = 34` y `z = 42` | Al llegar, la primera esclusa ya las habrá llevado a la pared oeste |

1. Pulsa **F**: las dos cajas suben al techo y la caja C presiona la placa del techo.
2. Ancla la caja C. Se suelta la caja de la primera esclusa, que ya no hace falta.
3. Gira la gravedad hacia el **este**: la caja D se desliza por el techo hasta la pared este.
4. Vuelve al suelo: la caja D cae sobre la placa D y la compuerta 2 se abre.

Girar primero al este desde el suelo no sirve: la caja C también se va al este y ya no queda bajo la placa del techo.

---

# Sección 1-4: Anillo de rotación (`Level_01_4_RotationRing`)

**Objetivo:** recorrer el pasillo sin tocar las franjas rojas y activar los dos interruptores.

| Franja roja (`HazardZone`) | Tramo |
|---|---|
| Suelo | `z = 6..20` |
| Pared este | `z = 26..38` |
| Techo | `z = 44..56` |

Interruptores: pared este en `z = 13` (sobre la franja del suelo) y techo en `z = 32` (frente a la franja de la pared). Puntos de control en `z = 23` y `z = 41`.

1. Antes de la franja del suelo, mirando al norte, pulsa **R**: Kai pasa a la pared este. Usa el interruptor con **E**.
2. Entre `z = 20` y `z = 26` (antes de la franja de la pared), pulsa **R** otra vez: Kai cae al techo.
3. Camina por el techo hasta el segundo interruptor y pulsa **E**. La compuerta se abre.
4. Antes de la franja del techo (`z = 44`), pulsa **F** para volver al suelo y camina hasta la salida.

---

# Sección 1-5: Núcleo del reactor (`Level_01_5_Core`)

**Objetivo:** activar el interruptor del techo, llevar la caja a la placa norte y salir. Es el final del Sector 1: **Continuar** vuelve al menú.

| Elemento | Posición |
|---|---|
| Caja | Inicio, `(-3, 0, 3)` |
| Pozo con fondo rojo | `z = 6..18` |
| Interruptor | Techo, sobre el pozo, `z = 12` |
| Placa | Suelo junto a la pared norte, lado oeste, `z = 31..33.4` |
| Compuerta | `z = 34`: necesita el interruptor **y** la placa |

**Truco nuevo:** Q y R giran la gravedad hacia la izquierda o la derecha de Kai. Si Kai mira hacia una pared lateral, la gravedad puede apuntar al **norte** o al **sur**.

1. Pulsa **F**: Kai y la caja suben al techo.
2. Camina por el techo hasta el interruptor sobre el pozo y pulsa **E**.
3. Gira a Kai hacia una pared lateral y usa **Q** o **R** para que la gravedad apunte al norte. Kai y la caja cruzan el pozo volando y caen sobre la pared norte (punto de control al pasar).
4. Vuelve al suelo: la caja cae sobre la placa y la compuerta se abre. Sal por ella.
