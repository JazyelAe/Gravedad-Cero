# Créditos y licencias de assets

Registro de todos los recursos de terceros incluidos en el proyecto. Antes de añadir un recurso nuevo hay que verificar su licencia y registrarlo aquí.

Los materiales, scripts, escenas y la configuración creados para Gravedad Cero son propios del proyecto y no se listan aquí.

## Modelos 3D

### Space Station Kit (versión 1.0)

| Campo | Valor |
|---|---|
| Autor | Kenney (www.kenney.nl) |
| URL | https://kenney.nl/assets/space-station-kit |
| Licencia | Creative Commons Zero (CC0 1.0), http://creativecommons.org/publicdomain/zero/1.0/ |
| Atribución | No es obligatoria; se acredita de forma voluntaria |
| Fecha de creación del paquete | 10-04-2024 |
| Fecha de incorporación | 25-10-2026 |
| Archivo original | `kenney_space-station-kit.zip` (1,835,875 bytes, SHA-256 `215e79bd5415cff93665183390f0343ed9acf87780306331013b78520170c6d8`) |
| Ubicación | `Assets/GravedadCero/Models/Kenney_SpaceStation/` |
| Licencia incluida | `Assets/GravedadCero/Models/Kenney_SpaceStation/License.txt` |

**Contenido incorporado:** los 97 modelos en formato FBX y la textura de paleta `colormap.png`. Los formatos OBJ y GLB, las imágenes de vista previa y la textura alternativa `variation-a.png` no se incluyeron porque duplican contenido.

**Cambios respecto al original:**
- Los modelos se importan a escala ×4: un módulo de 1 unidad pasa a medir 4 m, para que coincida con Kai (2 m).
- Todos los modelos comparten el material `M_Kenney_Colormap` (URP Lit), en lugar de los materiales del FBX.
- La paleta se importa sin mipmaps, sin compresión y con filtro Point.
- Se generan colliders de malla al importar.

## Personajes

### Animated Characters Protagonists (versión 1.1)

| Campo | Valor |
|---|---|
| Autor | Kenney (www.kenney.nl) |
| URL | https://kenney.nl/assets/animated-characters-protagonists |
| Licencia | Creative Commons Zero (CC0 1.0), http://creativecommons.org/publicdomain/zero/1.0/ |
| Atribución | No es obligatoria; se acredita de forma voluntaria |
| Fecha de incorporación | 25-10-2026 |
| Archivo original | `kenney_animated-characters-protagonists.zip` (581,441 bytes, SHA-256 `ec3787de70fa2200256848d74201b10f6b6c3126594e9857bf989753312c2b84`) |
| Ubicación | `Assets/GravedadCero/Models/Kenney_Characters/` |
| Licencia incluida | `Assets/GravedadCero/Models/Kenney_Characters/License.txt` |

**Contenido incorporado:** el modelo con rig `characterMedium.fbx`, las animaciones `idle.fbx`, `run.fbx` y `jump.fbx`, y las skins `skaterMaleA.png` y `cyborgFemaleA.png`. Esta última se añadió el 25-10-2026 desde el mismo zip. Las otras dos skins, las fuentes SVG y la imagen de vista previa no se incluyeron.

**Skins disponibles en el juego** (`Settings/KaiSkinCatalog.asset`):

| Skin | Textura | Origen |
|---|---|---|
| Ingeniero | `Kai_Engineer.png` | Derivada de `skaterMaleA.png` |
| Técnico de carga | `Kai_Cargo.png` | Derivada de `skaterMaleA.png` |
| Piloto | `Kai_Pilot.png` | Derivada de `skaterMaleA.png` |
| Cyborg | `cyborgFemaleA.png` | Original de Kenney |
| Skater | `skaterMaleA.png` | Original de Kenney |

**Cambios respecto al original:**
- `Skins/Kai_Engineer.png` es una **obra derivada** de `skaterMaleA.png`, recoloreada con la paleta del proyecto: traje blanco, pantalón gris oscuro, detalles y calzado cian. CC0 permite modificarla sin restricciones.
- El rig se importa como **Humanoid**. Cada FBX de animación crea su propio avatar humanoide, y Unity retargetea entre ellos. Solo se importa la toma principal de cada animación; se descarta la "Targeting Pose".
- El modelo se escala ×0.48 en el prefab `Kai_Model` (de 3.96 a 1.9 m). El FBX se mantiene a escala 1 para que el avatar sea válido.
- El material compartido es `M_Kai` (URP Lit).

## Tipografías

| Fuente | Uso | Autor | Licencia | Origen | SHA-256 |
|---|---|---|---|---|---|
| **Orbitron** (variable, `Orbitron-Variable.ttf`, 38,576 bytes) | Títulos | The Orbitron Project Authors (Matt McInerney / The League of Moveable Type) | SIL Open Font License 1.1 | https://github.com/google/fonts/tree/main/ofl/orbitron | `f42db2dd16e642258e35782916eceb1dcdbea06fb958d77ad71dc5963587e8fd` |
| **Exo 2** (variable, `Exo2-Variable.ttf`, 303,940 bytes) | Textos, botones e instrucciones | The Exo 2 Project Authors (Natanael Gama) | SIL Open Font License 1.1 | https://github.com/google/fonts/tree/main/ofl/exo2 | `205a448676a2586f9c57c25f3d5c58ca8db7e6cf5edf7506783a010c6fe2bfb5` |

- **Fecha de incorporación:** 26-10-2026.
- **Ubicación:** `Assets/GravedadCero/UI/Fonts/`, con su `*-OFL.txt`. La licencia OFL exige distribuirlas junto con su licencia.
- **Fuentes TextMeshPro generadas:** `Assets/GravedadCero/UI/Resources/Fonts/` (`Exo2 SDF`, `Orbitron SDF`). Son atlas estáticos con ASCII y caracteres del español (á é í ó ú ñ ü ¿ ¡ · « » “ ” — …).
- **Caracteres que faltan, comprobados con TextMeshPro:**
  - Orbitron no tiene `^ · ‹ › « »`;
  - ninguna de las dos tiene ✓ ● ▲ ▼ ◀ ▶ ↑ ↓ ← →.

  Por eso las flechas, la marca de verificación, los puntos y el candado se dibujan como iconos generados por código (`UITheme`), y los títulos en Orbitron no usan esos signos.
- **TextMeshPro Essential Resources:** se importaron del paquete `com.unity.ugui` ya instalado (Unity Technologies), sin descargar nada. Incluyen LiberationSans (SIL OFL 1.1), que se usa como fuente de respaldo.

## Audio

### Dark Sci-Fi Audio Pack (música)

| Campo | Valor |
|---|---|
| Autor | SRG774 |
| URL | https://opengameart.org/content/dark-sci-fi-audio-pack |
| Licencia | Creative Commons Zero (CC0 1.0), http://creativecommons.org/publicdomain/zero/1.0/ |
| Atribución | No es obligatoria; se acredita de forma voluntaria |
| Fecha de incorporación | 26-10-2026 |
| Archivo original | `ogg.zip` (13,267,628 bytes, SHA-256 `c17c40f2b5ff32a51016d121a017cf1a307e0065c7acfcc30c66f05f1d047ce1`) |
| Ubicación | `Assets/GravedadCero/Audio/SRG774_DarkSciFi/` |
| Licencia incluida | `License.txt` (el zip no traía ninguna: el archivo registra la licencia que indica la página del pack) |

**Contenido incorporado:** las 8 pistas del pack, sin modificar.

| Pista | Duración | Uso |
|---|---|---|
| `title.ogg` | 7:06 | Menú principal |
| `sector.ogg` | 0:57 | Sección 1-1 |
| `airy.ogg` | 1:04 | Sección 1-2 |
| `pulse.ogg` | 1:11 | Sección 1-3 |
| `transmission.ogg` | 1:34 | Sección 1-4 (en preparación) |
| `urgent.ogg` | 0:57 | Sección 1-5 (en preparación) |
| `victory.ogg` | 0:06 | Pantalla de victoria |
| `hover.ogg` | – | Moverse entre botones |

### Sci-Fi Sounds (versión 1.0, efectos)

| Campo | Valor |
|---|---|
| Autor | Kenney (www.kenney.nl) |
| URL | https://kenney.nl/assets/sci-fi-sounds |
| Licencia | Creative Commons Zero (CC0 1.0), http://creativecommons.org/publicdomain/zero/1.0/ |
| Atribución | No es obligatoria; se acredita de forma voluntaria |
| Fecha de creación del paquete | 11-10-2020 |
| Fecha de incorporación | 26-10-2026 |
| Archivo original | `kenney_sci-fi-sounds.zip` (5,875,104 bytes, SHA-256 `119340f351a5098ad814f78719438c0da355a9ce8a4c8a3af6a8d48aa3d49e04`) |
| Ubicación | `Assets/GravedadCero/Audio/Kenney_SciFiSounds/` |
| Licencia incluida | `Assets/GravedadCero/Audio/Kenney_SciFiSounds/License.txt` (original del zip) |

**Contenido incorporado:** 16 de los 70 sonidos, sin modificar (9 el 26-10-2026; 6 para los drones y el daño y 1 para las ventilas, del mismo zip ya descargado y verificado). Los demás no se incluyeron porque no se usan.

| Sonido | Uso |
|---|---|
| `forceField_000` | Cambio de gravedad |
| `laserRetro_000` / `laserSmall_000` | Anclar / soltar caja |
| `doorOpen_000` / `doorClose_000` | Compuertas |
| `impactMetal_000` | Placa de presión |
| `laserSmall_003` | Interruptor |
| `laserSmall_001` | Pulsar un botón de la interfaz |
| `lowFrequency_explosion_000` | Reaparecer tras caer o tocar una zona de peligro |
| `spaceEngineSmall_000` | Zumbido de los drones de seguridad (sonido 3D) |
| `computerNoise_000` | Un dron detecta a Kai |
| `explosionCrunch_000` | Un dron queda aturdido |
| `impactMetal_002` | Kai recibe un golpe |
| `forceField_002` | Registro de datos encontrado |
| `laserRetro_002` | Una caja vuelve a su sitio |
| `thrusterFire_000` | Ventilas de vapor (sonido 3D) |

La asignación de cada pista y sonido está en `Assets/GravedadCero/Audio/Resources/AudioCatalog.asset`.

## Pendientes

Ningún otro recurso de terceros está incluido por ahora.
