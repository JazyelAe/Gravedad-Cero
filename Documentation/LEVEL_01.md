# Nivel 1: Módulo de Acoplamiento (`Level_01_DockingBay`)

Nivel tutorial. Enseña a combinar tres mecánicas: el cambio de gravedad, la física de las cajas y el anclaje.

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
