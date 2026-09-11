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

## Database setup

Run the full script against the local SQL Server instance (Windows Auth, database `BDCAPAS`):

```powershell
sqlcmd -S . -d BDCAPAS -E -f 65001 -i "DAL\script.sql"
```

**`-f 65001` is required** — the file is UTF-8 with BOM. Without this flag, `sqlcmd` reads it with the default codepage and mangles accented literals (`'Español'`, `'Inglés'`), so the language-lookup variables in the i18n seed block resolve to `NULL` and every `TRADUCCION_GUARDAR` call for that language fails with "cannot insert NULL into IDIOMA_ID".

The script is append-only and mostly idempotent: `CONTROL_REGISTRAR` and `TRADUCCION_GUARDAR` are upserts, but early `CREATE TABLE` / `CREATE PROCEDURE` blocks will error if objects already exist (harmless — the rest of the batch still runs). Later blocks use `IF OBJECT_ID ... DROP` before recreating, so they are fully idempotent.

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

**DAL** — All DB access goes through `Acceso` (internal), which uses stored procedures exclusively — never inline SQL. `SqlParameter` objects are mandatory to prevent SQL injection. Connection string is in `Acceso.cs` targeting `BDCAPAS` on the local SQL Server instance.

**BLL** — Thin orchestration layer. Creates DAL instances directly (`new DAL.UsuarioDAL()`). No direct DB calls.

**SeguridadYServicios** — Cross-cutting concerns. Does NOT reference BLL or DAL; callers pass data in. Contains:
- `SessionManager` — Singleton, holds authenticated user and permissions (`List<string>`) for the session. `TienePermiso(string nombre)` checks against that list. `cerrarSesion()` nulls `_instance` to allow re-authentication.
- `IdiomaManager` — Singleton/Subject for the Observer pattern. Holds active-language translations as `Dictionary<string, string>`. Callers (CAPAS) fetch translations via BLL and pass them in via `CambiarIdioma()`.
- `IObservadorIdioma` — Observer interface (`ActualizarIdioma()`).
- `Hasher` — SHA-256 password hashing (one-way).
- `ValidadorContrasena` — Password rules (min 6 chars, uppercase, digit).
- `CalculadorDVH` — Static utility. `Calcular(string[])` for DVH; `CalcularVertical(List<string[]>, int)` for DVV. Formula: `Σ Unicode(char) × posAtributo × posChar` (1-based).

**CAPAS** — Windows Forms. References BLL and SeguridadYServicios. Every `Form` that should react to language changes implements `IObservadorIdioma`. All forms inherit from `ReaLTaiizor.Forms.MaterialForm` (not `System.Windows.Forms.Form`).

## UI theme (ReaLTaiizor)

The app uses **ReaLTaiizor 3.8.1.8** (NuGet, .NET Framework 4.8). Two layers cooperate:

**MaterialSkinManager** (global, `Program.cs`) — configured once before `Application.Run`:
```csharp
var skin = MaterialSkinManager.Instance;
skin.Theme = MaterialSkinManager.Themes.LIGHT;
skin.ColorScheme = new MaterialColorScheme(
    MaterialPrimary.Blue700, MaterialPrimary.Blue900, MaterialPrimary.Blue200,
    MaterialAccent.LightBlue200, MaterialTextShade.LIGHT);
```

**Per-form** — every form's `Load` event must call, in this order:
```csharp
MaterialSkinManager.Instance.AddFormToManage(this);
AppTheme.AplicarTema(this);
```

**`AppTheme.AplicarTema(form)`** (`CAPAS/AppTheme.cs`) — applies the corporate palette to Button, TextBox, Label, DataGridView, TreeView, ComboBox, Panel, GroupBox, MenuStrip, StatusStrip, and DateTimePicker controls recursively. Color constants: `FondoForm=#F5F7FA`, header/accent `#1565C0`.

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

**Integrity errors are logged** to `integridad_error.log` in the same folder as the `.exe` (via `Program.GuardarLogIntegridad`), with timestamp, each time `frmRestaurarIntegridad` loads.

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

1. Inherit from `ReaLTaiizor.Forms.MaterialForm` (not `Form`). Add `using ReaLTaiizor.Forms; using ReaLTaiizor.Manager;` at the top.
2. Implement `SeguridadYServicios.IObservadorIdioma`.
3. Add `Dictionary<string, Control> _controles` and `Dictionary<string, string> _defaults` fields.
4. In `Load`:
   - Call `GuardarDefaults(this.Controls)`.
   - Add `_controles[this.Name] = this; _defaults[this.Name] = this.Text;` (registers the title bar).
   - Call `IdiomaManager.getInstance().Registrar(this)`, then `ActualizarIdioma()`.
   - Call `IdiomaUIHelper.AgregarSelector(this)` **before** the two lines below.
   - Call `MaterialSkinManager.Instance.AddFormToManage(this);` then `AppTheme.AplicarTema(this);` **last**.
5. Wire `FormClosed` → `IdiomaManager.getInstance().Desregistrar(this)`.
6. `ActualizarIdioma()` iterates `_controles` and applies `Traducir(key) ?? _defaults[key]`. If the form has DataGridViews, also call `ActualizarEncabezados()`.
7. In `.Designer.cs`: place all content controls at `Location.Y ≥ 70` to clear the ~64 px MaterialForm title bar. Set `ClientSize.Height` to accommodate the shifted layout. Add `Load` and `FormClosed` event wiring.
8. Add the `.cs` and `.Designer.cs` entries to `CAPAS/UI.csproj`.
9. Append `EXEC CONTROL_REGISTRAR` calls to `DAL/script.sql` for all new control keys and form name, plus `EXEC TRADUCCION_GUARDAR` for each supported language, then re-run the script.

## Documentation artifacts

All diagram sources, generated images, and doc-generation scripts live under `DIAGRAMAS/`. PlantUML sources are rendered via `DIAGRAMAS/generar_png.py` (uses kroki.io, requires internet).

| File | Type | Purpose |
|---|---|---|
| `DIAGRAMAS/DER.puml` / `.png` | DER | Modelo entidad-relación de BDCAPAS |
| `DIAGRAMAS/DiagramaClases.puml` / `.png` | Diagrama de clases | Capas BE, BLL, DAL, SeguridadYServicios |
| `DIAGRAMAS/DiagramaClases_Composite.puml` / `.png` / `.svg` | Diagrama de clases (patrón) | Composite — árbol de roles/permisos (`NodoPermiso`/`Rol`/`Permiso`), con comentarios |
| `DIAGRAMAS/DiagramaClases_Observer.puml` / `.png` / `.svg` | Diagrama de clases (patrón) | Observer — multiidioma (`IdiomaManager`/`IObservadorIdioma`), con comentarios |
| `DIAGRAMAS/DiagramaClases_Singleton.puml` / `.png` / `.svg` | Diagrama de clases (patrón) | Singleton — `SessionManager`/`IdiomaManager`, con comentarios |
| `DIAGRAMAS/DiagramaComponentes.puml` | Diagrama de componentes | Proyectos del .sln (BE, DAL, BLL, SeguridadYServicios, CAPAS), interfaces entre capas y BDCAPAS |
| `DIAGRAMAS/DiagramaComponentes/DiagramaComponentes - TP_IS.png` | Diagrama de componentes (render) | PNG renderizado del anterior |
| `DIAGRAMAS/DiagramaSecuencia_LoginIntegridad.puml` / `.png` | Secuencia (detallado) | Versión técnica del login + integridad, incluye Hasher, DALs, etc. |
| `DIAGRAMAS/DiagramaSecuencia_CU01..CU20_*.puml` / `.png` | Secuencia por CU | Nivel UI/BLL/DB, uno por cada uno de los 20 casos de uso del esqueleto base |
| `DIAGRAMAS/DiagramaActividad_CatalogoStock.puml` / `.png` / `.svg` | Actividad (proceso de negocio) | 4 carriles (Bodega/Proveedor, Depósito, Compras, Administrador) del dominio Gestión de Catálogo y Stock de Vinos |
| `DIAGRAMAS/ModeloConceptual_CatalogoStock.puml` / `.png` | Modelo conceptual | BODEGA-VINO-MOVIMIENTO_STOCK, sin detalle físico |
| `DIAGRAMAS/DiagramaClases_CatalogoStock.puml` / `.png` | Diagrama de clases | BE/BLL/DAL/CAPAS del dominio de catálogo y stock de vinos |
| `DIAGRAMAS/DiagramaSecuencia_CU21_ProponerAltaVino.puml` / `.png` | Secuencia por CU | CU-21 Proponer Alta de Vino |
| `DIAGRAMAS/DiagramaSecuencia_CU22_AutorizarAltaVino.puml` / `.png` | Secuencia por CU | CU-22 Autorizar Alta de Vino |
| `DIAGRAMAS/DiagramaSecuencia_CU23_RegistrarMovimientoStock.puml` / `.png` | Secuencia por CU | CU-23 Registrar Movimiento de Stock (Entrada) |
| `DIAGRAMAS/DiagramaSecuencia_CU24_SolicitarDescontinuacion.puml` / `.png` | Secuencia por CU | CU-24 Solicitar Descontinuación de Vino |
| `DIAGRAMAS/DiagramaSecuencia_CU25_AutorizarDescontinuacion.puml` / `.png` | Secuencia por CU | CU-25 Autorizar Descontinuación de Vino |
| `DIAGRAMAS/DiagramaSecuencia_CU27_RegistrarAjusteInventario.puml` / `.png` | Secuencia por CU | CU-27 Registrar Ajuste de Inventario |
| `DIAGRAMAS/CasosDeUso.docx` | Documento unificado | Los 27 CUs (CU-01..CU-20 esqueleto base + CU-21..CU-25/CU-27 principales + CU-26 soporte, dominio de catálogo y stock de vinos) en un solo Word |
| `DIAGRAMAS/generar_casos_uso_docx.py` | Generador | Regenera `CasosDeUso.docx` desde el dict `CUS` definido en el script |

Regenerar un PNG individual (desde `DIAGRAMAS/`):

```powershell
python generar_png.py <archivo.puml> <archivo.png>
```

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
- **Diagramas de secuencia:** nivel UI / BLL / DB. `:DB` consolida todos los DALs como un `database`. Servicios transversales (`:SessionManager`, `:IntegridadBLL`) aparecen solo cuando son relevantes al flujo del actor.
- **Estilo PlantUML:** sin `box` agrupador; cada participante con color tenue por capa — CAPAS `#FEFECE`, BLL `#E8F4E8`, DAL `#E8E8F4`, Seguridad `#F4E8E8`. Skinparams comunes: `shadowing false`, `roundcorner 6`, `hide footbox`, `scale 0.75–0.85`.
- **CU descriptions:** plantilla Cockburn — ID, actor primario, frecuencia, prioridad, propósito, precondiciones, postcondiciones (éxito y fallo), disparador, flujo principal numerado, flujos alternativos (`Xa`, `Xb`, etc.), excepciones, reglas de negocio y relaciones (`«include»` / `«extend»`).
