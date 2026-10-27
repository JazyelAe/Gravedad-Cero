# Sector 1: Módulo de Acoplamiento

El Sector 1 tiene cinco secciones, cada una en su propia escena y de dificultad creciente. Al completar una sección se desbloquea la siguiente en el menú y se puede pasar a ella con **Continuar**. Las secciones 1-1, 1-2 y 1-3 tienen dos salas.

| Sección | Escena | Enseña |
|---|---|---|
| 1-1 Muelle de acoplamiento | `Level_01_DockingBay` | Gravedad, cajas y anclaje en un muelle de carga y un hangar; primer dron de seguridad y un registro secreto |
| 1-2 Pozo de mantenimiento | `Level_01_2_MaintenanceShaft` | Pozo vertical de 48 m: bajar por las paredes, ventilas de vapor intermitentes, dos válvulas, un dron y un registro secreto |
| 1-3 Esclusa de carga | `Level_01_3_CargoLock` | Una caja anclada y otra que se sostiene por su peso; en la esclusa 2, una placa en el techo |
| 1-4 Anillo de rotación | `Level_01_4_RotationRing` | Recorrer suelo, pared y techo esquivando franjas peligrosas |
| 1-5 Núcleo del reactor | `Level_01_5_Core` | La gravedad también apunta al norte o al sur (girando mirando a una pared lateral) |

**Cajas:** si una caja cae en una zona roja, vuelve sola a su posición inicial.

**Puntos de control:** al entrar en ellos, por cualquier superficie, aparece el aviso "Punto de control". Al caer, tocar una zona roja o perder el traje aparece la pantalla de derrota; **Reintentar** devuelve a Kai al último punto de control, con la gravedad normal. Retroceso sigue reiniciando la sección entera. Las cajas no vuelven a su sitio al reaparecer; si una queda inalcanzable, hay que reiniciar con Retroceso.

**La gravedad es global:** cada giro mueve todas las cajas sueltas de la sección, también las de la sala siguiente. Las salas están pensadas para que eso no resuelva la siguiente por adelantado.

---

# Sección 1-1: Muelle de acoplamiento (`Level_01_DockingBay`)

Zona industrial de carga en cuatro partes, de 36 × 76 m y hasta 16 m de alto. Enseña las mecánicas de forma gradual y presenta el primer dron de seguridad. Se genera con `Assets/GravedadCero/Editor/LevelBuilding/Level01Builder.cs` (menú **Gravedad Cero → Niveles → Reconstruir 1-1**).

**Objetivo:** abrir el muelle y la grúa del hangar, y llegar a la sala de control.

```
            z=76 ┌────────────┐
                 │ D: CONTROL │  salida
            z=68 └────┤▓├─────┘  compuerta C (la abre la grúa)
 ┌───────────────────────────────────────────┐
 │═══ pasarela oeste, y = 8 ═════ pasarela este │  C: HANGAR (36 × 32 × 16 m)
 │ ▓▓  ▓▓   ┊  ◇ → → → ◇  ┊   ▓▓  ▓▓       [G]│  [G] pinza de la grúa (placa, y 12.5–15.9)
 │ [2] ▓▓   ┊  pasillo del ┊   ▓▓  ▓▓       [G]│  ◇ dron: z 42–62, por debajo de 6.5 m
 │ ▓▓  ▓▓   ┊     dron     ┊   ▓▓  ▓▓         │  [2] caja sobre un contenedor
 │      CP Hangar (z 38.5), zona segura z < 42 │  CP Compuerta (z 64.5), zona segura z > 62
 └────────────────────┤▓├──────────────────────┘ z=36  compuerta B
    ┌─────────────────────────────────────┐
    │ ═ pasarela norte y = 8 → ✦ registro  │  B: MUELLE (28 × 24 × 12 m)
    │ ╔══════╗                    [P]      │  [P] báscula del montacargas (pared este)
    │ ║ [1]  ║ andén, y = 2.4             │  [1] caja del andén
    │ ╚══════╝   ▫ maquinaria             │  CP Muelle (z 13.5)
    └─────────────────┤ ├─────────────────┘ z=12
              ┌──────────────┐
              │ ▪  paso  ▪   │  A: ESCLUSA (12 × 12 × 8 m), escalones de 1.2 m
              │      ▲ Kai   │
              └──────────────┘ z=0
```

## A: esclusa de entrada (`z = 0..12`)

Dos escalones de 1.2 m a los lados del paso central: se pueden saltar o rodear.

## B: muelle de carga (`z = 12..36`)

| Elemento | Detalle |
|---|---|
| Andén de carga | Lado oeste, 2.4 m de alto: Kai no llega saltando. La caja está encima (`z = 24`) |
| Báscula del montacargas | Placa en la pared este, de 2 a 7.5 m. Abre la compuerta B |
| Pasarela norte | A 8 m, a lo largo de la pared norte. Al final hay un **registro de datos** |

1. Pulsa **R** mirando al norte: la caja vuela del andén a la báscula (y Kai cae a la pared este). La compuerta B se abre.
2. Camina por la pared hasta la caja y **ánclala**. Vuelve al suelo con **Q** o **R**.
3. **Ruta alternativa:** gira la gravedad hacia la pared oeste, sube por ella por encima de la pasarela norte y vuelve al suelo: caes sobre la pasarela. Al final está el registro.

La caja, al salir del borde del andén, se eleva un poco en el vuelo: por eso la báscula llega hasta 7.5 m.

## C: hangar (`z = 36..68`)

| Elemento | Detalle |
|---|---|
| Contenedores | Bloques a los dos lados del pasillo central: cortan la línea de visión del dron |
| Dron de seguridad | Patrulla el pasillo central (`z = 42..62`, hasta 6.5 m de alto). Fuera de esa zona no ve a Kai |
| Pasarelas altas | A 8 m en las paredes oeste y este: ruta por encima del dron |
| Caja del hangar | Sobre un contenedor del lado oeste (`z = 52`) |
| Pinza de la grúa | Placa en lo alto de la pared este (12.5–15.9 m). Abre la compuerta C |

1. Al entrar (zona segura), observa la ruta del dron. Cruza escondiéndote tras los contenedores o sube a las paredes, las pasarelas o el techo: ahí el dron no llega.
2. Pulsa **F**: la caja (y Kai) suben al techo.
3. Gira la gravedad hacia el **este**: la caja se desliza por el techo, entre los raíles de la grúa, hasta la pinza. La compuerta C se abre.
4. Ancla la caja (se suelta la del muelle, que ya no hace falta), vuelve al suelo y cruza la compuerta C.

**Dron:**
- Cian: patrulla. Ámbar: te ha visto (medio segundo de aviso). Rojo: te persigue, más despacio que Kai.
- Al tocar a Kai le quita una unidad de **integridad del traje** (hay 3), lo empuja y se retira. Kai es invulnerable 1.5 s.
- Si pierde de vista a Kai 2.5 s, vuelve a su ruta.
- Una caja que lo golpea con fuerza lo **aturde** 4 s.
- La gravedad no le afecta: flota con imanes y nunca sale de su zona.

Sin integridad, Kai reaparece en el último punto de control con el traje reparado.

## D: sala de control (`z = 68..76`)

La zona de salida completa la sección.

---

# Sección 1-2: Pozo de mantenimiento (`Level_01_2_MaintenanceShaft`)

Estructura vertical: una galería de servicio y un pozo de 20 × 16 m y 48 m de profundidad, con refrigerante en el fondo. Se baja caminando por las paredes y volviendo a la gravedad normal sobre cada descansillo. Se genera con `Editor/LevelBuilding/Level02Builder.cs` (menú **Gravedad Cero → Niveles → Reconstruir 1-2**).

**Objetivo:** bajar hasta la sala de bombas y abrir sus dos válvulas.

```
 Vista lateral (oeste ← → este)                     y
 ┌──────────────── techo del pozo ───────────────┐  +8   ◇ marca ámbar: encima de la plataforma
 │▓ plataforma superior (CP) ▓  ← compuerta      │   0   (desde la galería)
 │                                  ≋ vapor este  │  -7
 │                              ▓▓ descansillo E  │ -14   válvula 1, CP
 │ ═ tuberías                                     │ -16
 │ ◉ válvula 2   ▓ plataforma ▓   ◇ dron          │ -20   registro en la plataforma
 │ ≋ vapor oeste                                  │ -23
 │ ▓▓ descansillo O (CP)                          │ -28
 │          ═ tuberías y ≋ vapor (pared norte)    │ -29…-31
 │        ▓▓▓ cornisa (CP) → sala de bombas       │ -36
 └────────────── refrigerante (rojo) ─────────────┘ -40
```

## Galería de servicio (`z = -20..0`)

Un foso de 8 m corta la galería; la compuerta del pozo se abre con un interruptor **en el techo**, sobre el foso.

1. Pulsa **F**, camina por el techo hasta el interruptor (marca ámbar) y pulsa **E**.
2. Vuelve al suelo con **F** pasado el foso y entra en el pozo.

## Pozo (`z = 0..16`, de `y = -40` a `y = 8`)

| Elemento | Altura | Detalle |
|---|---|---|
| Plataforma superior | 0 | Punto de control. Barandilla hacia el pozo |
| Ventila este | -7 | Vapor intermitente en toda la pared este (apagado 2.2 s, aviso 0.7 s, encendido 1.8 s) |
| Descansillo este | -14 | Válvula 1 y punto de control |
| Tuberías oeste | -16 | Tumbadas en la pared oeste: hay que saltarlas al bajar por ella |
| Plataforma suspendida | -20 | En el centro. Solo se llega desde el techo. Tiene un **registro de datos** |
| Válvula 2 | -20 | En la pared oeste |
| Dron | -25 a -15 | Vigila la parte central; los descansillos quedan fuera de su zona |
| Ventila oeste | -23 | Desfasada respecto a la este |
| Descansillo oeste | -28 | Punto de control |
| Tuberías y ventila norte | -29 y -31 | En la pared norte, sobre la cornisa |
| Cornisa | -36 | Punto de control y compuerta de la sala de bombas (se abre con las dos válvulas) |

1. Desde la plataforma superior, mirando al norte, pulsa **R**: Kai cae a la pared este. Baja por ella cruzando la ventila cuando esté apagada.
2. Encima del descansillo este, vuelve a la gravedad normal (Q o R) y gira la **válvula 1** con **E**.
3. Gira la gravedad hacia el **oeste**: Kai cruza el pozo hasta la pared oeste. Baja (salta las tuberías) hasta la **válvula 2** y úsala; el dron vigila esta zona.
4. Pasa la ventila oeste y, encima del descansillo oeste, vuelve a la gravedad normal.
5. Gira la gravedad hacia el **norte**: Kai cruza a la pared norte. Baja saltando las tuberías y esquivando el vapor, y sobre la cornisa vuelve a la gravedad normal.
6. Cruza la compuerta de la sala de bombas.

**Ruta secreta:** desde la plataforma superior o un descansillo, sube al techo del pozo (**F**), camina hasta la marca ámbar del centro y vuelve a pulsar **F**: Kai cae 28 m sobre la plataforma suspendida y su registro. Fallar el salto lleva al refrigerante.

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
| Placa D | Dentro de un contenedor abierto junto a la pared este, `z = 42` | La caja que cae desde el techo queda dentro y la presiona por su peso; una caja que llega deslizándose por el suelo choca con el contenedor |
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
| Placa | Dentro de un contenedor abierto junto a la pared norte, lado oeste (`z = 32`): la caja tiene que llegar desde arriba |
| Compuerta | `z = 34`: necesita el interruptor **y** la placa |

**Truco nuevo:** Q y R giran la gravedad hacia la izquierda o la derecha de Kai. Si Kai mira hacia una pared lateral, la gravedad puede apuntar al **norte** o al **sur**.

1. Pulsa **F**: Kai y la caja suben al techo.
2. Camina por el techo hasta el interruptor sobre el pozo y pulsa **E**.
3. Gira a Kai hacia una pared lateral y usa **Q** o **R** para que la gravedad apunte al norte. Kai y la caja cruzan el pozo volando y caen sobre la pared norte (punto de control al pasar).
4. Vuelve al suelo: la caja cae sobre la placa y la compuerta se abre. Sal por ella.
