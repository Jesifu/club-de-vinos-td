# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build

```powershell
# Build the full solution
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" TP_IS.sln /p:Configuration=Debug /v:minimal

# Build a single project
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" BE\BE.csproj /p:Configuration=Debug
```

The entry point is `CAPAS` (outputs `CAPAS.exe`). There are no automated tests; verification is manual via the running application.

## Installer

`Instalador/` holds a WiX 7 MSI project (`Instalador.wixproj` + `Package.wxs`), deliberately outside `TP_IS.sln`. Build with `Instalador/build.ps1` (Release solution build via MSBuild, then the MSI at `Instalador/bin/x64/Release/es-ES/ClubDeVinos.msi`; UI in Spanish via `<Cultures>es-ES</Cultures>` and `Package.es-es.wxl`). It installs to Program Files, shows `SqlInstanceDlg` (instances detected by the DTF custom action in `Instalador/CustomActions`, a net472 project also outside the .sln; the combo is editable, so any `server\instance` can be typed and is validated on Next), creates `BDCAPAS` if missing in the chosen instance (`Server="[SQLINSTANCE]"`) and persists the choice in `HKLM\SOFTWARE\Club de Vinos\SqlInstance` (REG_SZ, written even for "solo aplicación", removed on uninstall). `DAL/Acceso.cs` resolves the server once (lazy, never throws): env var `CLUBVINOS_SQL` (support override) > that registry value (64-bit then 32-bit view) > `.`. It then runs `DAL/script.sql` (converted to UTF-16 LE at build time in `Instalador/obj`; the WiX SQL action does not read UTF-8). The database is never dropped on uninstall. If the DB is unreachable at launch (e.g. "solo aplicación" before running `EjecutarScript.cmd`), `Program.Main` runs `ErrorInicio.VerificarConexion()` (probe in `DAL/ConexionDAL.cs` via `BLL/ConexionBLL.cs`, 5 s timeout) and shows a Spanish Reintentar/Cancelar message instead of the raw .NET dialog; `ErrorInicio` also installs global `ThreadException`/`UnhandledException` handlers (texts hardcoded: translations live in the DB). Because Program Files is read-only for regular users, the app must not write to the app folder: remitos go to `Documents\ClubDeVinos\Remitos` and the integrity log to `%LocalAppData%\ClubDeVinos`. Requires `wix eula accept wix7` once per machine.

## Database setup

Run the full script against the local SQL Server instance (Windows Auth, database `BDCAPAS`):

```powershell
sqlcmd -S . -d BDCAPAS -E -I -f 65001 -i "DAL\script.sql"
```

**`-f 65001` is required** — the file is UTF-8 with BOM. Without this flag, `sqlcmd` reads it with the default codepage and mangles accented literals (`'Español'`, `'Inglés'`), so the language-lookup variables in the i18n seed block resolve to `NULL` and every `TRADUCCION_GUARDAR` call for that language fails with "cannot insert NULL into IDIOMA_ID".

The script is append-only and mostly idempotent: `CONTROL_REGISTRAR` and `TRADUCCION_GUARDAR` are upserts, but early `CREATE TABLE` / `CREATE PROCEDURE` blocks will error if objects already exist (harmless — the rest of the batch still runs). Later blocks use `IF OBJECT_ID ... DROP` before recreating, so they are fully idempotent.

**Datos de prueba** — el final de `DAL/script.sql` (sección `-- DATOS DE PRUEBA`) siembra, de forma idempotente, datos para recorrer los CU-21..CU-32 sin cargar nada a mano: 3 bodegas más, 15 vinos `DEMO-001..015` en todos los estados (9 Activo, 3 Pendiente, 2 Rechazado con motivo, 1 Descontinuado; 19 solicitudes, entre ellas una BAJA Pendiente en `DEMO-008` y una BAJA Rechazada en `DEMO-009`), kardex coherente (stock 0 en `DEMO-007/008/015`, stock bajo mínimo en `DEMO-005/006/007/008`, `DEMO-009` con stock para ver RN-15), 7 socios, 6 cajas (2 Despachadas, 2 Armadas listas, 1 Armada que exige sustitución, 1 con sustitución ya hecha, 1 Cancelada) y bitácora de ejemplo. Un vino, socio o caja que ya existe no se vuelve a tocar, así que reejecutar el script no pisa lo operado desde la app. Cuentas de prueba, **todas con la contraseña `Club2026`** (`admin` conserva `1234`): `admin2` (Administrador, necesario por RN-01), `compras`/`compras2` (Encargado de Compras/Bodega), `deposito` (Encargado de Depósito), `curador` (Curador de Producto), `atencion` (Atención al Socio), `logistica` (Logística), `usuario1` (Usuario) y `bloqueado1` (Usuario, cuenta bloqueada). Los usuarios se insertan con `DVH = 0` y, solo si se agregó alguno, el script borra los DVV de `USUARIO`: en el siguiente inicio la app recalcula la integridad sola (mismo procedimiento que el reset de abajo), sin avisar de manipulación.

**Resetting integrity for testing** — if you need to force the app to reinitialize DVH/DVV (e.g., after a direct DB edit):
```sql
DELETE FROM DIGITO_VERIFICADOR_VERTICAL WHERE TABLA = 'USUARIO';
```
On next startup `EstaInicializado()` returns false → `RecalcularIntegridad()` runs automatically.

## Architecture

Five projects in the solution:

```
CAPAS (UI/WinForms)
  ├── BLL
  │     ├── DAL
  │     │     └── BE
  │     └── BE
  └── SeguridadYServicios
              └── BE
```

**BE** — Plain entity classes (`USUARIO`, `BITACORA`, `IDIOMA`, `CONTROL_IDIOMA`, `NodoPermiso`, `Rol`, `Permiso`, `UsuarioHistorial`, etc.) plus the `LoginResultado` enum. No logic. `USUARIO` has a `DVH int` property (the only integrity-protected entity). `NodoPermiso.ToString()` returns `Nombre` (used by `CheckedListBox` in `frmPerfiles`).

**DAL** — All DB access goes through `Acceso` (internal), which uses stored procedures exclusively — never inline SQL. `SqlParameter` objects are mandatory to prevent SQL injection. Connection string is built in `Acceso.cs` targeting `BDCAPAS` (Windows auth); the server comes from `CLUBVINOS_SQL`, then the installer's registry value, then `.` (see Installer).

**BLL** — Thin orchestration layer. Creates DAL instances directly (`new DAL.UsuarioDAL()`). No direct DB calls.

**SeguridadYServicios** — Cross-cutting concerns. Does NOT reference BLL or DAL; callers pass data in. Contains:
- `SessionManager` — Singleton, holds authenticated user and permissions (`List<string>`) for the session. `TienePermiso(string nombre)` checks against that list. `cerrarSesion()` nulls `_instance` to allow re-authentication.
- `IdiomaManager` — Singleton/Subject for the Observer pattern. Holds active-language translations as `Dictionary<string, string>`. Callers (CAPAS) fetch translations via BLL and pass them in via `CambiarIdioma()`.
- `IObservadorIdioma` — Observer interface (`ActualizarIdioma()`).
- `Hasher` — SHA-256 password hashing (one-way).
- `ValidadorContrasena` — Password rules (min 6 chars, uppercase, digit).
- `CalculadorDVH` — Static utility. `Calcular(string[])` for DVH; `CalcularVertical(List<string[]>, int)` for DVV. Formula: `Σ Unicode(char) × posAtributo × posChar` (1-based).

**CAPAS** — Windows Forms. References BLL and SeguridadYServicios. Every `Form` that should react to language changes implements `IObservadorIdioma`. All forms inherit from `FormBase` (a `ReaLTaiizor.Forms.MaterialForm` subclass), not `System.Windows.Forms.Form`.

## UI theme (ReaLTaiizor)

The app uses **ReaLTaiizor 3.8.1.8** (NuGet, .NET Framework 4.8). Two layers cooperate:

**MaterialSkinManager** (global, `Program.cs`) — configured once before `Application.Run`:
```csharp
var skin = MaterialSkinManager.Instance;
skin.Theme = MaterialSkinManager.Themes.LIGHT;
skin.ColorScheme = new MaterialColorScheme(
    SystemColors.Highlight, SystemColors.HotTrack, SystemColors.ControlLight,
    SystemColors.Highlight, MaterialTextShade.WHITE);
```

**`FormBase`** (`CAPAS/FormBase.cs`) — every form inherits `FormBase`, never `MaterialForm` directly. `MaterialForm.OnPaint` clears the whole form with the Material theme backdrop (gray) and never calls `base.OnPaint`, so neither `BackColor` nor the `Paint` event can change it. `FormBase.OnPaint` lets Material draw (title bar, buttons) and then fills `UserArea` with `SystemColors.Control`. `FormBase.OnLoad` also sets the app icon and, after the form's `Load` handlers, resets TextBox/ComboBox/NumericUpDown/ListBox/TreeView/RichTextBox colors (MaterialSkinManager grays them).

**Online help (D02, F1)** — `FormBase` handles F1 in `ProcessCmdKey` (independent of `KeyPreview` and the focused control) and opens `frmAyuda` on the current form's topic, so forms need no code of their own. The topic id comes from the `protected virtual string AyudaTema` property (default `AyudaContexto.TemaPara(GetType())`); a form may override it for a dynamic topic. `protected virtual bool AyudaHabilitada` (default `true`) lets a form opt out (`frmAyuda` returns `false` to avoid help over help). `CAPAS/Ayuda/AyudaContexto.cs` holds the map form-type-name -> topic id (unmapped forms fall back to `inicio`). The content is `CAPAS/Ayuda/ayuda_es.xml`, embedded in `CAPAS.exe` (`LogicalName` `CAPAS.Ayuda.ayuda_es.xml`); it is named with an underscore on purpose: `.es.` in the file name makes MSBuild treat it as a satellite culture resource.

**Per-form** — every form's `Load` event must call `MaterialSkinManager.Instance.AddFormToManage(this);` (last).

**Colors convention** — the brand palette (burgundy `AppTheme`) was removed: controls use the Windows system defaults (`SystemColors.*`; the Material title bar uses `SystemColors.Highlight`). Do not assign decorative `BackColor`/`ForeColor`/`FlatAppearance` colors. Only colors with meaning stay: recommendation colors of the rotation grid and charts/PDF (`RotacionGraficos`), stock/integrity/error warnings, and `MsgBox` icon colors by type.

**App icon** — `CAPAS/Resources/ClubDeVinos.ico` (16–256 px). Embedded in `CAPAS.exe` via `<ApplicationIcon>` in `UI.csproj`; `FormBase.OnLoad` takes it from the executable; the installer uses it for both shortcuts and the Apps & features entry (`ARPPRODUCTICON`).

**Critical layout constraint** — `MaterialForm` renders its own title bar (~64 px) **inside** the client area at `y=0`. Controls in `.Designer.cs` must have `Location.Y ≥ ~70` or they will be hidden under the title bar. When designing a new form or adjusting an existing one, offset all content controls by at least 70 px from the top of the client area. The `ClientSize.Height` must be increased by the same amount relative to the visible content.

## Key design patterns

**Composite (roles and permissions tree)** — `NodoPermiso` is the abstract component; `Rol` (file: `PerfilPermiso.cs`) is a branch node stored in the `ROL` table; `Permiso` is a leaf stored in the `PERMISO` table. The tree is built in-memory in `PerfilBLL.ObtenerArbol()`:
1. `PerfilDAL.ListarRoles()` fetches all `ROL` rows and builds the rol hierarchy from `PADRE_ID`.
2. `PerfilDAL.ListarRolPermisos()` fetches all rows from `ROL_PERMISO` joined with `PERMISO`; each returned `Permiso` carries its rol's ID in the `PadreId` field as a carrier value, which `ObtenerArbol()` uses to attach the permiso to the correct rol branch.

**Roles and permissions data model** — Roles and permissions live in two separate tables (migrated off the old single-table `NODO_PERMISO` design; the migration is consolidated into `DAL/script.sql`, not a separate file):
- **`ROL`** (`ID, NOMBRE, PADRE_ID, PROTEGIDO`): created and deleted from the UI, `PADRE_ID` used for role hierarchy.
- **`PERMISO`** (`ID, NOMBRE`): pre-established catalog, never created or deleted from the UI. Currently: *Ver bitácora*, *Administrar usuarios*, *Gestión de roles*, *Gestión de idiomas*, *Cambiar contraseña*.

The many-to-many link between roles and permissions lives in `ROL_PERMISO (ROL_ID, PERMISO_ID)`, with real FKs to both `ROL` and `PERMISO`. `PERMISO_LISTAR_TODOS` lists the `PERMISO` catalog; `ROL_LISTAR_TODOS` lists all roles.

`ROL_ELIMINAR` cascades via a recursive CTE: it collects the entire subtree of descendant role IDs and deletes their entries from `USUARIO_PERFIL` and `ROL_PERMISO` before removing them from `ROL`. `PerfilDAL.GuardarPermisosDeRol` runs inside a transaction (LIMPIAR + N × INSERTAR).

`NODO_PERMISO` is kept as an unused legacy remnant (not dropped, for safety) — its data was copied into `ROL`/`PERMISO` and no current SP references it.

**Role parent assignment** — `PADRE_ID` on a `ROL` row establishes role hierarchy. `PerfilBLL.CambiarPadre` calls two in-memory guards before persisting, each throwing `InvalidOperationException`:
1. `GenerariaCiclo` — walks the parent chain up from the candidate parent; rejects it if `rolId` appears (candidate is a descendant of the role, would create a cycle).
2. `EsAncestroDelPadreActual` — walks the parent chain up from the role's *current* parent (excluding it); rejects the candidate if it appears there (candidate is an ancestor above the current parent — only "lateral" moves or the no-op current parent are allowed).

`frmPerfiles.PopularComboPadre` mirrors rule 1 by excluding the role's subtree (`RecolectarDescendientes`) from the combo entirely. Rule 2's targets — ancestors above the current parent (`RecolectarAncestrosSuperiores`, stored in `_padresDeshabilitados`) — remain visible in the combo but are rendered grayed-out (`cboPadre_DrawItem`, `OwnerDrawFixed`) and cannot be selected (`cboPadre_SelectedIndexChanged` reverts to `_cboPadreIndiceAnterior`). The SP `ROL_CAMBIAR_PADRE` does a plain `UPDATE ROL SET PADRE_ID`.

**No permission inheritance** — `ROL.PADRE_ID` is purely organizational: it controls how roles are nested in the tree shown by `frmPerfiles` (via `PerfilBLL.ObtenerArbol()`), nothing else. A role's actual permissions are exactly its own rows in `ROL_PERMISO` — never a parent's. `USUARIO_PERMISOS_LISTAR` looks up permissions directly from `USUARIO_PERFIL → ROL_PERMISO → PERMISO` for the user's own assigned role(s), with no walk up `PADRE_ID`. This was previously CU-15 ("inherited permissions"), removed after it caused a privilege-escalation bug: a role accidentally parented under `Administrador` silently inherited all of its permissions. `chkPermisos` in `frmPerfiles` renders with default (non-owner-draw) checkboxes — checked state reflects only the role's own `ROL_PERMISO` rows.

**Role deletion guards** — `PerfilBLL.Eliminar` enforces two rules before calling the DAL, throwing `InvalidOperationException` for each:
1. `Rol.Protegido == true` → system role, cannot be deleted. Currently only *Administrador* (`PROTEGIDO=1`); all other roles default to `PROTEGIDO=0`.
2. `PerfilDAL.TieneUsuariosAsignados(id)` → the SP `ROL_TIENE_USUARIOS` uses a recursive CTE to check whether any role in the subtree has entries in `USUARIO_PERFIL`. Blocks deletion if so.

`frmPerfiles` shows three context-sensitive panels when a `Rol` node is selected: `panelPadre` (ComboBox to reassign parent, excludes the role's own subtree from candidates) and `panelPermisos` (CheckedListBox of catalog permissions). The Eliminar button is disabled in the UI when `rol.Protegido` is true.

**Permissions check flow** — `LoginBLL` calls `UsuarioPerfilBLL.ObtenerPermisos(usuarioId)` → `USUARIO_PERMISOS_LISTAR` SP → walks role inheritance (recursive CTE on `ROL.PADRE_ID`) then joins `ROL_PERMISO → PERMISO` to get distinct permission names → stored in `SessionManager._permisos`. UI checks via `SessionManager.TienePermiso("Ver bitácora")`. **Never use `SessionManager.EsAdmin()` to control visibility** — it only compares `USUARIO.ROL` and is not aligned with the permission system; it exists but should stay unused.

**Observer (multi-language)** — `IdiomaManager` is the Subject. Forms are Observers. Each form has its own language selector added dynamically by `IdiomaUIHelper.AgregarSelector(form)` (called at the end of every form's `Load`), because `ShowDialog()` disables the parent form. On change: CAPAS calls `BLL.IdiomaBLL.CargarTraducciones(idiomaId)`, then passes the dictionary to `IdiomaManager.CambiarIdioma()`, which calls `Notificar()` → `ActualizarIdioma()` on every registered form. Forms fall back to their design-time text when a key has no translation.

**`IDIOMA.PREDETERMINADO`** — `BIT` column marking the system's default language (`Español`, seeded via `UPDATE IDIOMA SET PREDETERMINADO = 1 WHERE NOMBRE = 'Español'`). `frmIdiomas.btnEliminarIdioma_Click` blocks deletion in two cases, each with its own warning message:
1. `_idiomaSeleccionado.Predeterminado == true` — the default language can never be deleted.
2. `IdiomaBLL.EstaEnUso(id)` (SP `IDIOMA_ESTA_EN_USO`, `SELECT COUNT(*) FROM USUARIO WHERE IDIOMA_ID = @id`) — a language assigned to *any* user (not just the current session's) cannot be deleted.

**Per-user language preference (`USUARIO.IDIOMA_ID`)** — nullable FK to `IDIOMA(ID)` (`FK_USUARIO_IDIOMA`); `NULL` means "no preference saved yet". Three write sites call `UsuarioBLL.ActualizarIdioma(usuarioId, idiomaId)` (SP `USUARIO_ACTUALIZAR_IDIOMA`) whenever a logged-in user changes language: `IdiomaUIHelper.AgregarSelector`'s combo handler and `frmMenu.cboIdiomaStatus_SelectedIndexChanged` (both guarded by `SessionManager.getInstance().getUsuario() != null` — the pre-login `LogIn` selector does not persist). On successful login, `LoginBLL.AutenticarUsuario` reads `u.IdiomaId` (now returned by `USUARIO_LOGIN`); if set, it loads the `IDIOMA` (`IdiomaBLL.ObtenerPorId`, SP `IDIOMA_OBTENER_POR_ID`) and its translations and calls `IdiomaManager.CambiarIdioma(...)` directly — this happens *before* `frmMenu` is constructed, so the menu loads already translated. `BE.USUARIO.IdiomaId` (`int?`) is hidden from `dgvUsuarios` in `frmAdminUsuarios` (not user-relevant in that grid).

Two categories of UI text are **not** captured by `GuardarDefaults(this.Controls)` and need explicit handling:
- **Form title bar**: register with `_controles[this.Name] = this; _defaults[this.Name] = this.Text;` after `GuardarDefaults`.
- **DataGridView column headers**: not in `Controls` collection; translate in a separate `ActualizarEncabezados()` method called from both `ActualizarIdioma()` and after any `DataSource` assignment. Use keys like `colhdr_Id`, `colhdr_Usuario`, etc.

**Key collision between forms** — if two forms have a control with the same `Name`, fix by removing the generic key and registering a form-specific one in `Load`:
```csharp
_controles.Remove("lblTitulo");
_defaults.Remove("lblTitulo");
_controles["lblTitulo_Bitacora"] = lblTitulo;
_defaults["lblTitulo_Bitacora"]  = lblTitulo.Text;
```

**Dynamic content (TreeView, generated text)** — controls whose text is built at runtime (e.g., node prefixes `"[Rol] "`, `"[Permiso] "`) are not in `_controles`. Call `IdiomaManager.getInstance().Traducir(key)` directly. Translation keys: `prefijo_Rol`, `prefijo_Permiso`.

**Singleton** — Both `SessionManager` and `IdiomaManager` use double-checked locking.

## Solicitud de autorización (alta y baja de vinos)

La autorización de altas y bajas de vinos vive en la tabla **`SOLICITUD_AUTORIZACION`** (`ID, TIPO ALTA|BAJA, VINO_ID, ESTADO Pendiente|Aprobada|Rechazada, EMISOR_ID, FECHA_EMISION, RESOLUTOR_ID, FECHA_RESOLUCION, MOTIVO_RECHAZO`), fuente de verdad de las aprobaciones. Reemplaza a las columnas `AUTORIZADO_POR`, `FECHA_AUTORIZACION`, `BAJA_SOLICITADA_POR`, `FECHA_SOLICITUD_BAJA`, `DESCONTINUADO_POR` y `FECHA_DESCONTINUACION` de `VINO`, que fueron migradas (backfill) y eliminadas; los SP viejos (`VINO_AUTORIZAR`, `VINO_AUTORIZAR_BAJA`, `VINO_RECHAZAR_BAJA`, `VINO_LISTAR_PENDIENTES`, `VINO_LISTAR_PENDIENTES_BAJA`) ya no existen. Los SP actuales son `SOLICITUD_LISTAR_PENDIENTES / OBTENER_POR_ID / OBTENER_PENDIENTE / APROBAR_ALTA / RECHAZAR_ALTA / APROBAR_BAJA / RECHAZAR_BAJA`, más `VINO_INSERTAR`, `VINO_REPROPONER`, `VINO_SOLICITAR_BAJA` y `VINO_LISTAR_CATALOGO(@usuario_id)`.

- **Estados:** `VINO.ESTADO` solo guarda el ciclo de vida (`Pendiente`, `Activo`, `Rechazado`, `Descontinuado`; CHECK `CK_VINO_ESTADO`). Un vino nace `Pendiente` con su solicitud ALTA; al rechazarse pasa a `Rechazado` y se corrige y repropone con el mismo SKU (`VINO_REPROPONER`, nueva solicitud ALTA, la anterior queda como historial). La baja no cambia el estado del vino hasta aprobarse (`Activo` → `Descontinuado`). Solo los vinos `Activo` son operables (stock, cajas, alertas); todos los `ESTADO='Activo'` de los SP lectores dependen de eso.
- **RN-01 (triple guard):** quien resuelve una solicitud (aprobar **o** rechazar) no puede ser su emisor. Se verifica en la UI (botones deshabilitados con `EmisorId == sesión.Id`), en `VinoBLL.ValidarSeparacionDeFunciones` y en el `WHERE EMISOR_ID <> @resolutor_id` de cada SP de resolución (0 filas = rechazado), y además por el CHECK `CK_SOLAUT_RN01`.
- **Rechazo con motivo:** obligatorio, recortado, 1 a 200 caracteres (UI, `VinoBLL.ValidarMotivo`, SP y CHECK `CK_SOLAUT_MOTIVO`).
- **Unicidad:** índice único filtrado `UX_SOLAUT_PENDIENTE (VINO_ID, TIPO) WHERE ESTADO='Pendiente'`; las carreras llegan como `SqlException` 2601/2627 y la BLL las traduce.
- **Orden de bloqueo:** todos los SP de resolución bloquean primero `VINO` (`UPDLOCK, HOLDLOCK`) y recién después actualizan `SOLICITUD_AUTORIZACION`, para evitar deadlocks entre aprobar/rechazar y proponer/reproponer.
- **Guard de borrado de columnas:** el script elimina las columnas legadas de `VINO` solo si `COL_LENGTH` las encuentra **y** todo vino tiene al menos una solicitud (si el backfill excluyó o no migró alguno, las conserva con un `PRINT` para revisarlo a mano y volver a correr el script), en un batch dinámico transaccional que primero borra sus FK y DEFAULT; el backfill previo mapea los vinos legados sin solicitudes y es idempotente. Correr el script siempre con `-I` (QUOTED_IDENTIFIER ON; sin él falla el DML sobre el índice filtrado, error 1934).
- **Patrón de SP:** los SP de escritura devuelven `SELECT @filas AS FILAS` (se leen con `Acceso.Leer`; `ExecuteNonQuery` suma los rowcounts de todas las sentencias).

## Dígitos verificadores de integridad

The system protects **USUARIO** against unauthorized out-of-system DB modifications. **BITACORA**, **ROL**, **PERMISO**, and **ROL_PERMISO** are not integrity-protected.

**DVH (horizontal)** — one `int` per row, stored in the row's own `DVH` column. Formula: `Σᵢ Σⱼ Unicode(atrib[i][j]) × (i+1) × (j+1)`.

**DVV (vertical)** — one `int` per logical column, stored in `DIGITO_VERIFICADOR_VERTICAL (TABLA, COLUMNA, DVV)`. Formula: `Σₖ Σⱼ Unicode(fila[k][col][j]) × (k+1) × (j+1)` where `k` is row position ordered by `ID`.

**Multi-table state (USUARIO)** — the virtual attribute `PERFILES` (sorted comma-separated role IDs from `USUARIO_PERFIL`) is appended as the 7th attribute in USUARIO's DVH. Changes to `USUARIO_PERFIL` outside the system invalidate the affected user's DVH.

**Canonical attribute order** — fixed, must never change once data is stored:
- `USUARIO`: `ID, USUARIO, PASS, INTENTOS_FALLIDOS, BLOQUEADO("1"/"0"), ROL, PERFILES`

**Valid ROL values** — defined by `frmNuevoUsuario`'s ComboBox: `'admin'` and `'usuario'`. The `ROL` string column is a legacy display/classification field, kept for compatibility and included in the DVH canonical attribute array (its position must never change). `USUARIO.ROL_ID` (nullable FK to `ROL(ID)`, added by the ROL/PERMISO migration block in `DAL/script.sql`, backfilled from the legacy string) is the real source of truth: actual permissions are resolved via `USUARIO_PERFIL → ROL_PERMISO → PERMISO`.

**Startup and login flow** (`Program.cs` + `LogIn.cs`):

1. `Program.cs` calls `InicializarIntegridad()` before opening the login form:
   - If not initialized (no DVVs for USUARIO), calls `RecalcularIntegridad()` first.
   - Calls `VerificarIntegridad()` and stores the result in `Program.ResultadoIntegridad` (static).
   - **Does not block the app** — login form always opens.

2. After a successful login, `LogIn.cs` evaluates `Program.ResultadoIntegridad`:
   - If **valid**: `RecalcularIntegridadUsuarios()` is called (to update DVH after `ResetearIntentos` mutation), then `frmMenu` opens normally.
   - If **invalid**, two conditions determine who can restore:
     - **Admin whose own ID is NOT in `ResultadoIntegridad.IdsUsuariosAfectados`** → sees `frmRestaurarIntegridad` with two options (see below).
     - **Everyone else** (regular users, or admin whose own data is compromised) → sees a generic message and is blocked.

3. On **failed login** (wrong credentials or blocked user), `RecalcularIntegridadUsuarios()` is called **only if integrity is OK**. If integrity is compromised, no recalculation happens — corruption must not be silently healed by a login attempt.

**Critical invariant**: `LoginBLL.AutenticarUsuario()` never calls `RecalcularIntegridadUsuarios()`. All recalculation responsibility lives in `LogIn.cs` (CAPAS layer), conditioned on `Program.ResultadoIntegridad`.

**`ResultadoIntegridad`** has two fields:
- `Errores: List<string>` — human-readable descriptions of each failure.
- `IdsUsuariosAfectados: List<int>` — IDs of USUARIO rows with DVH mismatch (populated only for USUARIO, not DVV failures).

**`frmRestaurarIntegridad`** — shown to a trusted admin when integrity fails. Displays the error detail and offers:
- **Recalcular y continuar**: accepts the current DB state as legitimate, recalculates all DVH/DVV, and proceeds to `frmMenu`.
- **Restaurar desde historial**: opens `frmHistorialUsuario` for each affected user so the admin can roll back to a prior snapshot. Disabled if no affected user has any history records. After restoring, recalculates and proceeds to `frmMenu`.
- **Cancelar**: clears the session, returns to the login form.

**Integrity errors are logged** to `%LocalAppData%\ClubDeVinos\integridad_error.log` (via `Program.GuardarLogIntegridad`), with timestamp, each time `frmRestaurarIntegridad` loads.

**After every mutation of USUARIO**, the appropriate recalculation method must be called:

| Where | Trigger | Recalculation |
|---|---|---|
| `LogIn.cs` | Successful login, integrity OK | `RecalcularIntegridadUsuarios()` |
| `LogIn.cs` | Failed login, integrity OK | `RecalcularIntegridadUsuarios()` |
| `frmRestaurarIntegridad` | Admin chooses Recalcular or Restaurar | `RecalcularIntegridadUsuarios()` |
| `UsuarioBLL.*` (Crear, Eliminar, Desbloquear, CambiarContrasena) | Any user mutation | `RecalcularIntegridadUsuarios()` |
| `UsuarioPerfilBLL.GuardarAsignaciones()` | Profile assignment change | `RecalcularIntegridadUsuarios()` |
| `UsuarioHistorialBLL.Rollback()` | Rollback applied | `RecalcularIntegridadUsuarios()` |

`RecalcularIntegridad()` (no suffix) delegates to `RecalcularIntegridadUsuarios()` and is used only during initialization.

**Extending to a new entity** — add a `DVH int` column to the table, add rows to `DIGITO_VERIFICADOR_VERTICAL`, define the canonical attribute array and `ExtraerAtributos*` method in `IntegridadBLL`, then call `VerificarTabla` and `RecalcularTabla` (the generic internal helpers) following the existing USUARIO pattern.

## Control de cambios (USUARIO_HISTORIAL)

`USUARIO_HISTORIAL` is an append-only audit table: no UPDATEs, no DELETEs. Each row is a full snapshot of a user's state at a point in time. `PASS` is never stored.

**Change types**: `ALTA`, `BAJA`, `BLOQUEO`, `DESBLOQUEO`, `CAMBIO_CLAVE`, `ASIGNACION_PERFIL`, `ROLLBACK`, `EDICION_DATOS`.

**Snapshot timing** — `RegistrarCambio` must be called **after** the change, with one exception: `BAJA` is recorded **before** `_dal.Eliminar()` so the last known state is captured while the row still exists.

**Rollback** (`UsuarioHistorialBLL.Rollback`) — applies `ROL`, `BLOQUEADO`, `INTENTOS_FALLIDOS`, `PERFILES`, `NOMBRE`, and `APELLIDO` from the chosen snapshot. `PASS` is untouched. The rollback itself is recorded as a new `ROLLBACK` entry with `VERSION_ORIGEN` pointing to the restored snapshot ID. Calls `RecalcularIntegridadUsuarios()` internally.

**`NOMBRE`/`APELLIDO` (non-critical fields)** — `USUARIO.NOMBRE` and `USUARIO.APELLIDO` are free-text personal data, optional and not part of any business rule. They are deliberately **excluded from the DVH/DVV canonical attribute array** (`IntegridadBLL.ColumnasUsuario`) — editing them never affects integrity digests. They ARE snapshotted in `USUARIO_HISTORIAL` and restored on rollback (see above), so history/rollback stays a faithful point-in-time copy, but `UsuarioBLL.ActualizarDatos` does **not** call `RecalcularIntegridadUsuarios()`. Edited via `frmEditarUsuario` ("Editar datos" button in `frmAdminUsuarios`), which records an `EDICION_DATOS` entry.

**Anti-recursion** — a `ROLLBACK` entry cannot itself be the target of another rollback. Enforced in two places: `frmHistorialUsuario` rejects the selection before asking for confirmation, and `UsuarioHistorialBLL.Rollback()` throws `InvalidOperationException` as a backstop if called anyway.

**No FK to USUARIO** — intentional: the historial row survives the deletion of the user it describes.

**Limitation**: `frmRestaurarIntegridad` can only offer rollback for users who have at least one entry in `USUARIO_HISTORIAL`. Users created before the historial feature was implemented have no records and can only be resolved via "Recalcular y continuar".

## Database conventions

- All DB operations call stored procedures by name string (e.g., `"USUARIO_LOGIN"`).
- `Acceso.Leer()` returns a `DataTable`; `Acceso.Escribir()` returns affected row count.
- `Acceso` only has `CrearParametro` overloads for `string` and `int`. Pass `bool` values as `habilitado ? 1 : 0`.
- For **nullable int** parameters, `CrearParametro` cannot be used — construct manually: `new SqlParameter("@version_origen", SqlDbType.Int) { Value = (object)h.VersionOrigen ?? DBNull.Value }`.
- New tables and SPs are appended to `DAL/script.sql`.
- `EXEC` does not accept expression parameters — assign to a variable first.
- **PERSONA** table exists in the DB but is not used — legacy leftover from the initial scaffold.
- When a batch in `script.sql` uses `DECLARE @var` variables, a `GO` statement resets scope. Each batch after a `GO` must re-declare any variables it needs.
- To drop a column that has a DEFAULT constraint, first drop the constraint dynamically (its auto-generated name varies per instance), then drop the column.

## Adding a new form with language support

1. Inherit from `FormBase` (not `MaterialForm` or `Form`, see UI theme). Add `using ReaLTaiizor.Manager;` at the top.
2. Implement `SeguridadYServicios.IObservadorIdioma`.
3. Add `Dictionary<string, Control> _controles` and `Dictionary<string, string> _defaults` fields.
4. In `Load`:
   - Call `GuardarDefaults(this.Controls)`.
   - Add `_controles[this.Name] = this; _defaults[this.Name] = this.Text;` (registers the title bar).
   - Call `IdiomaManager.getInstance().Registrar(this)`, then `ActualizarIdioma()`.
   - Call `IdiomaUIHelper.AgregarSelector(this)` **before** the two lines below.
   - Call `MaterialSkinManager.Instance.AddFormToManage(this);` **last**.
5. Wire `FormClosed` → `IdiomaManager.getInstance().Desregistrar(this)`.
6. `ActualizarIdioma()` iterates `_controles` and applies `Traducir(key) ?? _defaults[key]`. If the form has DataGridViews, also call `ActualizarEncabezados()`.
7. In `.Designer.cs`: place all content controls at `Location.Y ≥ 70` to clear the ~64 px MaterialForm title bar. Set `ClientSize.Height` to accommodate the shifted layout. Add `Load` and `FormClosed` event wiring.
8. Add the `.cs` and `.Designer.cs` entries to `CAPAS/UI.csproj`.
9. Append `EXEC CONTROL_REGISTRAR` calls to `DAL/script.sql` for all new control keys and form name, plus `EXEC TRADUCCION_GUARDAR` for each supported language, then re-run the script.
10. Map the form in `CAPAS/Ayuda/AyudaContexto.cs` (type name -> topic id) and add or adjust its topic in `CAPAS/Ayuda/ayuda_es.xml`; otherwise F1 falls back to the `inicio` topic.

## N03 — Análisis de rotación y reposición de stock

Proceso de apoyo a la decisión (permiso `Analizar rotación de stock`, menú Catálogo → `frmAnalisisRotacion`). `ROTACION_ANALIZAR(@desde, @hasta)` devuelve por vino Activo el stock derivado, las unidades vendidas (Salida `CAJA`), las perdidas (Salida `AJUSTE`) y los socios atendidos (cajas Despachadas con línea efectiva, honrando `SUSTITUCION`); `RotacionBLL.Evaluar` (función pura) calcula consumo mensual, cobertura, clasificación, recomendación y **cantidad sugerida** (`ResultadoRotacion.CantidadSugerida`) con los umbrales de `BE.ConfiguracionRotacion`. Cantidad sugerida = `ceil(max(ConsumoMensual × MesesCoberturaObjetivo, StockMinimo) − StockActual)` con piso 0 para Reponer y piso 1 para Urgente (nunca muestra 0), solo si la recomendación es Urgente o Reponer (Mantener/Sobrestock = 0); se muestra como columna `CantidadSugerida` (`colhdr_CantidadSugerida`), como total "Unidades sugeridas a reponer" (`rot_UnidadesSugeridas`) en el resumen del formulario y como columna "A reponer" + total en el PDF. `MesesCoberturaObjetivo` (default 3, `lblCoberturaObjetivo`) debe ser mayor a `MesesCoberturaReposicion` y no mayor a `MesesCoberturaSobrestock` (`Validar`); un XML anterior sin ese elemento (detectado en el `XmlDocument`, antes de validar) lo deriva de sus umbrales: `min(Sobrestock, max(3, Reposicion + 1))`, así una configuración personalizada válida no se descarta (si `Sobrestock <= Reposicion` el archivo es inválido y cae a defaults). Esa configuración se **serializa con `XmlSerializer`** a `%LocalAppData%\ClubDeVinos\configuracion_rotacion.xml` (`CAPAS/ConfiguracionRotacionStore`: crea el archivo con defaults si falta y, si está corrupto o incoherente, usa defaults sin fallar; `Guardar` es atómico: temp + `File.Replace`). **El XML manda:** "Analizar" valida lo que hay en pantalla, lo guarda en el XML y analiza con la configuración **releída** del archivo (si no puede guardar/leer, no analiza); "Guardar configuración" sigue siendo un botón explícito. El período por defecto son los últimos `DiasAnalisis` días con hoy inclusive; al cambiar `nudDiasAnalisis` se recalcula `Desde = Hasta - (días - 1)`, y las fechas editadas a mano se respetan (guardar no las pisa). Si `Hasta` es anterior a hoy se muestra un aviso no bloqueante (y una nota en el PDF): el stock es el actual, la cobertura de rangos pasados puede no ser representativa. Clasificación y recomendación se guardan como constantes en español (`BE.ClasificacionRotacion/RecomendacionReposicion`) pero se muestran traducidas (claves `rot_clas_*`, `rot_rec_*`, `msg_RotacionRangoPasado`; `TraducirValor` en `CellFormatting` y en `lblResumen`). El informe PDF (`CAPAS/RotacionReposicionPdf`, A4 apaisado, nombre único con milisegundos, **en el idioma activo de la app**: `Generar(..., Func<string,string> traducir = null)` recibe del formulario el mismo traductor de la grilla; `null` o clave sin traducción = español; claves `rot_pdf_*`, reutilizando `colhdr_*`, `rot_rec_*`, `rot_clas_*`, `rot_UnidadesSugeridas`, `rot_gra_EjeVino`; los textos con `{n}` pasan por `RotacionGraficos.Formatear`, que cae al español ante `FormatException`; los mensajes de exportación también; fechas y números siguen en es-AR porque `BE.IDIOMA` no tiene cultura; ojo: `TRADUCCION.TEXTO` admite 200 caracteres y trunca en silencio, por eso la nota de fórmulas son dos claves) va a `Documents\ClubDeVinos\Informes`. Los datos de prueba N03 (6 socios, `DEMO-016/017`, ~126 líneas de cajas Despachadas en 6 meses) están al final de `DAL/script.sql`, protegidos por el MOTIVO `Compra a bodega - demo rotación`. **Gráficos:** `CAPAS/RotacionGraficos` (única clase que arma los `Chart` de `System.Windows.Forms.DataVisualization`, referencia de framework: viene con .NET 4.x, el instalador no necesita nada extra) crea tres gráficos sin 3D y con paleta común (Urgente rojo, Reponer naranja, Mantener verde, Sobrestock azul grisáceo; fondo claro tipo tarjeta): `ConsumoVsStock` (columnas de consumo mensual y stock actual + marcador de stock mínimo, top 10 por prioridad, el título dice "N de M vinos"), `Distribucion` (dona por recomendación con cantidad y %) y `UnidadesSugeridas` (barras horizontales de `CantidadSugerida > 0`, top 10, coloreadas por recomendación); sin filas muestran "Sin datos para graficar" (`rot_gra_SinDatos`). Reciben `RotacionGraficos.Opciones` (`Traducir` = función de claves, `null` = español; cultura; `Escala` de fuentes; tamaño en px). El formulario arma en código (no en el Designer) dos "pestañas" con botones `btnTabDetalle`/`btnTabGraficos` (claves i18n = nombre del control) sobre un panel `pnlResultados`: Detalle contiene `lblResumen` + `dgvRotacion` (movidos en `ConstruirPestanas`) y Gráficos un `TableLayoutPanel` con scroll; `ActualizarGraficos` los reconstruye al analizar y en `ActualizarIdioma`. Se usan botones y no `TabControl` por su aspecto con el tema Material. Claves nuevas: `btnTabDetalle`, `btnTabGraficos`, `rot_gra_*` (títulos con `{0} de {1} vinos`, ejes, series, sin datos). El PDF (`Traducir` = el traductor recibido, `Escala = 200/96`) incrusta los PNG en memoria como `base64:` (sin archivos temporales): página 1 = título + encabezado + resumen + consumo vs stock y distribución lado a lado; página 2 = unidades sugeridas (si hay) + tabla.

### N03 — Pedido de reposición (backend)

El análisis deja de ser de solo lectura: de las filas elegidas se genera un **Pedido de Reposición** (un pedido por bodega) con ciclo `Pendiente -> Aprobado | Rechazado` y `Aprobado -> Recibido` (terminales; sin cancelación). Tablas `PEDIDO_REPOSICION` (cabecera: bodega, estado, creador, resolutor + fecha, motivo de rechazo, receptor + fecha, observación) y `PEDIDO_REPOSICION_LINEA` (`CANTIDAD_SUGERIDA` snapshot, `CANTIDAD_PEDIDA`, `CANTIDAD_RECIBIDA` nullable, `ABIERTO`); SP `PEDIDO_REPOSICION_INSERTAR / LISTAR / OBTENER_POR_ID / OBTENER_LINEAS / AUTORIZAR / RECHAZAR / RECIBIR`. `INSERTAR` y `RECIBIR` reciben las líneas con table-valued parameters (`dbo.PEDIDO_LINEA_TT`, `dbo.PEDIDO_RECEPCION_TT`; `SqlDbType.Structured` en `PedidoReposicionDAL`) y son transaccionales con rollback total; todos los SP de escritura devuelven un único `SELECT` (`FILAS`, o `ID` en `INSERTAR`; 0 = rechazado). Lógica en `BLL/PedidoReposicionBLL` (`Generar`, `GenerarPedido`, `Listar`, `Obtener`, `ObtenerLineas`, `Autorizar`, `Rechazar`, `Recibir`).

- **RN-31** solo vinos Activo, cantidad pedida > 0, misma bodega, al menos una línea. **RN-32** un vino no puede estar en más de un pedido abierto (Pendiente o Aprobado): índice único filtrado `UX_PEDREPLIN_VINO_ABIERTO (VINO_ID) WHERE ABIERTO = 1` (`ABIERTO` lo apagan `RECHAZAR` y `RECIBIR`), más verificación en BLL y SP. **RN-33** quien autoriza o rechaza no es quien generó el pedido (BLL, `WHERE CREADO_POR <> @resolutor_id` de cada SP y CHECK `CK_PEDREP_RN33`). **RN-34** rechazo con motivo de 1 a 200 caracteres (CHECK `CK_PEDREP_MOTIVO`). **RN-35** la recepción solo aplica a pedidos Aprobados, informa la cantidad recibida de cada línea (`0 <= recibida <= pedida`, al menos una > 0) y genera en la misma transacción una Entrada de `MOVIMIENTO_STOCK` por cada línea con recibida > 0 (`MOTIVO` `Recepción de pedido de reposición`, `REFERENCIA_TIPO` `PEDIDO`, `REFERENCIA_ID` = id del pedido); lo no recibido se da por no entregado (el pedido es terminal). **RN-36** recibe cualquiera con el permiso de recepción. **RN-37** "en camino" = unidades de líneas de pedidos Aprobados sin recibir; `ROTACION_ANALIZAR` devuelve `EN_CAMINO`, `PEDIDO_ABIERTO_ESTADO` y `BODEGA_ID`, y `RotacionBLL.Evaluar` calcula `CantidadSugeridaBruta` (la fórmula de siempre) y `CantidadSugerida` neta = `max(0, bruta - EnCamino)`; el piso 1 de Urgente (RN-24) aplica a la bruta, así que la neta puede ser 0.
- **RN-15 extendida y topes:** no se puede solicitar ni autorizar la baja de un vino que está en un pedido de reposición abierto (Pendiente o Aprobado; `VinoBLL.SolicitarBaja/AutorizarBaja`, `VINO_SOLICITAR_BAJA`, `SOLICITUD_APROBAR_BAJA`, SP `PEDIDO_REPOSICION_ABIERTO_POR_VINO`); tope de 100000 unidades por línea, pedida y recibida (`PedidoReposicionBLL.CantidadMaximaPorLinea`, UI y CHECK `CK_PEDREPLIN_TOPE`); si `Generar` crea pedidos de algunas bodegas y falla uno posterior, lanza `PedidoParcialException` (con `Creados`) y la UI reanaliza.
- **Permisos:** `Gestionar pedidos de reposición` (Encargado de Compras/Bodega, Administrador), `Autorizar pedidos de reposición` (solo Administrador), `Recibir pedidos de reposición` (Encargado de Depósito, Administrador). Se controlan en la UI.
- **Bitácora:** `PEDIDO_GENERADO:<id>`, `PEDIDO_AUTORIZADO:<id>`, `PEDIDO_RECHAZADO:<id>`, `PEDIDO_RECIBIDO:<id>`.
- **Datos de prueba:** cuatro pedidos (uno por estado) marcados con la observación `Demo pedidos de reposición`, al final del bloque de datos N03 (DEMO-006 Pendiente, DEMO-001 Aprobado y en camino, DEMO-005 Rechazado, DEMO-004 Recibido con su Entrada; no usan DEMO-007/008, para no bloquear los demo de baja de CU-24/CU-25; el script migra la versión anterior).

### N03 — Pedido de reposición (interfaz)

- **CU-37 Generar pedido** (`frmAnalisisRotacion`): con el permiso `Gestionar pedidos de reposición` la grilla agrega las columnas `Seleccionar` ("Pedir") y `CantidadAPedir` (creadas en código en `PrepararSeleccionPedido`, editables solo en filas pedibles: Reponer urgente/Reponer, sugerida neta > 0 y `PedidoAbiertoEstado` nulo), más `EnCamino` y `PedidoAbiertoEstado` (traducido con `valped_*`); botón `btnGenerarPedido` ("Generar pedido", habilitado con al menos una fila marcada) llama a `PedidoReposicionBLL.Generar(filas, cantidades, sesión)`, muestra los pedidos creados (uno por bodega) y reanaliza con las mismas fechas y configuración. Errores de regla = `MsgBox` "Atención".
- **CU-38 / CU-39 / CU-40**: `frmAutorizarPedidoReposicion` (lista Pendientes, detalle con líneas, `btnAutorizarPedido`/`btnRechazarPedido`; el rechazo abre `FrmMotivoRechazoPedido`, motivo 1..200; botones deshabilitados si `CreadoPor == sesión.Id`, RN-33), `frmRecibirPedidoReposicion` (lista Aprobados, `CantidadRecibida` editable 0..pedida con default = pedida, `btnRegistrarRecepcion`) y `frmConsultarPedidosReposicion` (solo lectura, filtro por estado `cboEstado`). Comparten `CAPAS/PedidoReposicionUI.cs` (estados traducidos, cabecera de detalle, grillas, diálogo de motivo). Menú Catálogo: ítems `autorizarPedidoToolStripMenuItem`, `recibirPedidoToolStripMenuItem`, `consultarPedidosToolStripMenuItem` creados en código en `frmMenu.CrearMenuPedidos`, cada uno oculto por su permiso; la consulta se muestra con cualquiera de los tres permisos del ciclo. Ojo: el padre `catalogoToolStripMenuItem` se decide con los permisos, no con `Visible` de los hijos (mientras el desplegable no se muestra, `Visible` de un hijo devuelve false).
- PDF y gráficos: columna "En camino" (`colhdr_EnCamino`) antes de "A reponer", nota `rot_pdf_NotaEnCamino` y título del gráfico de unidades "netas de lo en camino". Ayuda F1: temas `pedido-generar` (bajo `rotacion`), `pedido-autorizar`, `pedido-recibir`, `pedido-consultar`.
- Los selectores de idioma agregan una franja de 28 px abajo: las grillas con ancla inferior de los formularios nuevos se diseñaron 28 px más cortas para no pisar el panel de botones.

## Documentation artifacts

All diagram sources, generated images, and doc-generation scripts live under `DIAGRAMAS/`. PlantUML sources are rendered via `DIAGRAMAS/generar_png.py` (uses kroki.io, requires internet).

| File | Type | Purpose |
|---|---|---|
| `DIAGRAMAS/DER.puml` / `.png` | DER | Modelo entidad-relación de BDCAPAS (incluye `SOLICITUD_AUTORIZACION`) |
| `DIAGRAMAS/DiagramaClases.puml` / `.png` | Diagrama de clases | Capas BE, BLL, DAL, SeguridadYServicios |
| `DIAGRAMAS/DiagramaClases_Composite.puml` / `.png` / `.svg` | Diagrama de clases (patrón) | Composite — árbol de roles/permisos (`NodoPermiso`/`Rol`/`Permiso`), con comentarios |
| `DIAGRAMAS/DiagramaClases_Observer.puml` / `.png` / `.svg` | Diagrama de clases (patrón) | Observer — multiidioma (`IdiomaManager`/`IObservadorIdioma`), con comentarios |
| `DIAGRAMAS/DiagramaClases_Singleton.puml` / `.png` / `.svg` | Diagrama de clases (patrón) | Singleton — `SessionManager`/`IdiomaManager`, con comentarios |
| `DIAGRAMAS/DiagramaComponentes.puml` | Diagrama de componentes | Proyectos del .sln (BE, DAL, BLL, SeguridadYServicios, CAPAS), interfaces entre capas y BDCAPAS |
| `DIAGRAMAS/DiagramaComponentes/DiagramaComponentes - TP_IS.png` | Diagrama de componentes (render) | PNG renderizado del anterior |
| `DIAGRAMAS/DiagramaSecuencia_LoginIntegridad.puml` / `.png` | Secuencia (detallado) | Versión técnica del login + integridad, incluye Hasher, DALs, etc. |
| `DIAGRAMAS/DiagramaSecuencia_CU01..CU20_*.puml` / `.png` | Secuencia por CU | Nivel UI/BLL/DB, uno por cada uno de los 20 casos de uso del esqueleto base |
| `DIAGRAMAS/DiagramaActividad_CatalogoStock.puml` / `.png` / `.svg` | Actividad (proceso de negocio) | 4 carriles (Bodega/Proveedor, Depósito, Compras, Administrador) del dominio Gestión de Catálogo y Stock de Vinos; documentos como nodos de objeto con flujos punteados |
| `DIAGRAMAS/ModeloConceptual_CatalogoStock.puml` / `.png` | Modelo conceptual | BODEGA-VINO-MOVIMIENTO_STOCK-SOLICITUD_AUTORIZACION, sin detalle físico |
| `DIAGRAMAS/DiagramaClases_CatalogoStock.puml` / `.png` | Diagrama de clases | BE/BLL/DAL/CAPAS del dominio de catálogo y stock de vinos (incluye `SolicitudAutorizacion` y `SolicitudAutorizacionDAL`) |
| `DIAGRAMAS/DiagramaSecuencia_CU21_ProponerAltaVino.puml` / `.png` | Secuencia por CU | CU-21 Proponer Alta de Vino (incluye corregir y reproponer un vino rechazado) |
| `DIAGRAMAS/DiagramaSecuencia_CU22_AutorizarAltaVino.puml` / `.png` | Secuencia por CU | CU-22 Autorizar o Rechazar Alta de Vino (panel de detalle, rechazo con motivo) |
| `DIAGRAMAS/DiagramaSecuencia_CU23_RegistrarMovimientoStock.puml` / `.png` | Secuencia por CU | CU-23 Registrar Movimiento de Stock (Entrada) |
| `DIAGRAMAS/DiagramaSecuencia_CU24_SolicitarDescontinuacion.puml` / `.png` | Secuencia por CU | CU-24 Solicitar Descontinuación de Vino (stock 0, RN-15) |
| `DIAGRAMAS/DiagramaSecuencia_CU25_AutorizarDescontinuacion.puml` / `.png` | Secuencia por CU | CU-25 Autorizar o Rechazar Descontinuación de Vino (rechazo con motivo, RN-01 en ambos) |
| `DIAGRAMAS/DiagramaSecuencia_CU26_ConsultarAlertaStockMinimo.puml` / `.png` | Secuencia por CU | CU-26 Consultar Alerta de Stock Mínimo |
| `DIAGRAMAS/DiagramaSecuencia_CU27_RegistrarAjusteInventario.puml` / `.png` | Secuencia por CU | CU-27 Registrar Ajuste de Inventario |
| `DIAGRAMAS/ModeloConceptual_CuracionCajas.puml` / `.png` | Modelo conceptual | SOCIO-SOCIO_VARIETAL-CAJA_MENSUAL-CAJA_VINO-SUSTITUCION, sin detalle físico, dominio de curación y armado de cajas mensuales |
| `DIAGRAMAS/DiagramaActividad_CuracionCajas.puml` / `.png` | Actividad (proceso de negocio) | 4 carriles (Atención al Socio, Curador de Producto, Encargado de Depósito, Logística) del dominio de curación y armado de cajas mensuales; documentos como nodos de objeto con flujos punteados |
| `DIAGRAMAS/DiagramaClases_CuracionCajas.puml` / `.png` | Diagrama de clases | BE/BLL/DAL/CAPAS del dominio de curación y armado de cajas mensuales |
| `DIAGRAMAS/DiagramaSecuencia_CU28_ArmarCajaMensual.puml` / `.png` | Secuencia por CU | CU-28 Armar Caja Mensual |
| `DIAGRAMAS/DiagramaSecuencia_CU29_RegistrarSustitucion.puml` / `.png` | Secuencia por CU | CU-29 Registrar Sustitución por Falta de Stock |
| `DIAGRAMAS/CasosDeUso.docx` | Documento unificado | Los 32 CUs (CU-01..CU-20 esqueleto base + CU-21..CU-25/CU-27 principales + CU-26 soporte, dominio de catálogo y stock de vinos + CU-28..CU-32 dominio de curación y cajas mensuales) en un solo Word |
| `DIAGRAMAS/generar_casos_uso_docx.py` | Generador | Regenera `CasosDeUso.docx` desde el dict `CUS` definido en el script |

**Diagramas de actividad — documentos como nodos de objeto.** Por regla de cátedra, cada documento de negocio (Remito, Orden de Compra, Acta, Solicitud de Autorización, Ficha/Solicitud/Caja, etc.) va como nodo de objeto en línea en el flujo (no como `note`), y las flechas que entran y salen del nodo son punteadas (`-[dashed]->`). La sintaxis `:texto]` de PlantUML NO funciona en la versión actual (los terminadores SDL no se reconocen), así que el nodo se dibuja como acción con estereotipo: `:**Remito**
[Emitido]; <<doc>>` más `skinparam roundcorner<<doc>> 0` y `BackgroundColor<<doc>> #FFFFFF`. Un `backward:` de `repeat` no admite un nodo de objeto: los lazos con rechazo se escriben con `if` dentro del `repeat` y `repeat while (¿Se rechazó…?)`. Se mantienen sin carril "Sistema", mínimo 3 carriles y actividades verbo+sustantivo.

Regenerar un PNG individual (desde `DIAGRAMAS/`):

```powershell
python generar_png.py <archivo.puml> <archivo.png>
```

`DiagramaClases.puml` es muy grande: la URL GET de `generar_png.py` da HTTP 414 y el PNG de PlantUML se recorta a 4096 px. Se renderiza con POST del texto a `https://kroki.io/plantuml/svg` (header `User-Agent: Mozilla/5.0`) y conversión local del SVG a PNG con Edge headless (cairosvg no funciona aquí). El layout vertical (capas apiladas, clases en grilla) está en el bloque final del `.puml`.

Regenerar en lote todos los diagramas de secuencia (con reintentos):

```powershell
python generar_pngs_lote.py
```

Regenerar el `.docx` de casos de uso (tras editar `CUS` en el script):

```powershell
python generar_casos_uso_docx.py
```

## Modeling conventions (use cases & sequence diagrams)

- **Actores:** solo dos — `Usuario` (base) y `Administrador` (especialización que generaliza a Usuario). Los permisos individuales (*Administrar usuarios*, *Gestión de roles*, *Gestión de idiomas*, *Ver bitácora*, *Cambiar contraseña*) son precondiciones del CU, no actores separados.
- **Diagramas de secuencia:** nivel UI / BLL / DAL / DB. Cada clase DAL que participa aparece como participante propio (color de capa `#E8E8F4`) entre la BLL y `:DB` (`database`); una BLL nunca habla con `:DB` directamente. Los nombres de métodos deben coincidir entre el diagrama de clases, los diagramas de secuencia y el código real. Servicios transversales (`:SessionManager`, `:IntegridadBLL`) aparecen solo cuando son relevantes al flujo del actor.
- **Estilo PlantUML:** sin `box` agrupador; cada participante con color tenue por capa — CAPAS `#FEFECE`, BLL `#E8F4E8`, DAL `#E8E8F4`, Seguridad `#F4E8E8`. Skinparams comunes: `shadowing false`, `roundcorner 6`, `hide footbox`, `scale 0.75–0.85`.
- **CU descriptions:** plantilla Cockburn — ID, actor primario, frecuencia, prioridad, propósito, precondiciones, postcondiciones (éxito y fallo), disparador, flujo principal numerado, flujos alternativos (`Xa`, `Xb`, etc.), excepciones, reglas de negocio y relaciones (`«include»` / `«extend»`).
