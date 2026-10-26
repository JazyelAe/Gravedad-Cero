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

## Pendientes

Ningún otro recurso de terceros está incluido por ahora.
