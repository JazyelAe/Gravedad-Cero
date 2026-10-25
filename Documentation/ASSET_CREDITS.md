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

## Pendientes

Ningún otro recurso de terceros está incluido por ahora.
