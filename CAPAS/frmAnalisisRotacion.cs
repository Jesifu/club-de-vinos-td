using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;

namespace CAPAS
{
    // N03 — Análisis de rotación y reposición de stock. Cruza el kardex (proceso 1) con las
    // cajas despachadas (proceso 2). Los umbrales salen de la configuración serializada en XML
    // (ConfiguracionRotacionStore): el formulario la carga al abrir y permite guardarla de nuevo.
    public partial class frmAnalisisRotacion : FormBase, SeguridadYServicios.IObservadorIdioma
    {
        private readonly BLL.RotacionBLL _bll = new BLL.RotacionBLL();
        private readonly Dictionary<string, Control> _controles = new Dictionary<string, Control>();
        private readonly Dictionary<string, string> _defaults = new Dictionary<string, string>();

        // Configuración vigente: la que lee el análisis.
        private BE.ConfiguracionRotacion _config;

        // Último análisis, para exportarlo tal como se vio en pantalla.
        private List<BE.ResultadoRotacion> _ultimoResultado;
        private BE.ConfiguracionRotacion _ultimaConfig;
        private DateTime _ultimoDesde;
        private DateTime _ultimoHasta;

        // Evita que los cambios de código sobre los controles disparen la lógica de los eventos.
        private bool _cargando;

        public frmAnalisisRotacion()
        {
            InitializeComponent();
        }

        private void frmAnalisisRotacion_Load(object sender, EventArgs e)
        {
            GuardarDefaults(this.Controls);
            _controles.Remove("lblTitulo");
            _defaults.Remove("lblTitulo");
            _controles["lblTitulo_AnalisisRotacion"] = lblTitulo;
            _defaults["lblTitulo_AnalisisRotacion"] = lblTitulo.Text;
            _controles[this.Name] = this;
            _defaults[this.Name] = this.Text;
            SeguridadYServicios.IdiomaManager.getInstance().Registrar(this);
            ActualizarIdioma();

            CargarConfiguracion();

            IdiomaUIHelper.AgregarSelector(this);
            MaterialSkinManager.Instance.AddFormToManage(this);
            AppTheme.AplicarTema(this);
            EstilizarNumericos();
            lblAdvertenciaFecha.ForeColor = AppTheme.Acento;

            // Se enganchan al final para que la carga inicial no los dispare.
            nudDiasAnalisis.ValueChanged += nudDiasAnalisis_ValueChanged;
            dtpHasta.ValueChanged += dtpHasta_ValueChanged;
            ActualizarAdvertenciaFecha();
        }

        private void frmAnalisisRotacion_FormClosed(object sender, FormClosedEventArgs e)
        {
            SeguridadYServicios.IdiomaManager.getInstance().Desregistrar(this);
        }

        public void ActualizarIdioma()
        {
            foreach (var kvp in _controles)
            {
                string t = SeguridadYServicios.IdiomaManager.getInstance().Traducir(kvp.Key)
                           ?? _defaults[kvp.Key];
                kvp.Value.Text = t;
            }
            ActualizarEncabezados();
            ActualizarAdvertenciaFecha();
            if (_ultimoResultado != null)
            {
                ActualizarResumen(_ultimoResultado);
                dgvRotacion.Invalidate();
            }
        }

        // Traducción para mostrar de los valores de clasificación y recomendación. Los valores
        // guardados siguen siendo las constantes en español de BE; solo cambia lo que se ve.
        // Si no hay traducción se muestra el valor original.
        private static string TraducirValor(string valor)
        {
            string clave;
            switch (valor)
            {
                case BE.ClasificacionRotacion.Alta: clave = "rot_clas_Alta"; break;
                case BE.ClasificacionRotacion.Media: clave = "rot_clas_Media"; break;
                case BE.ClasificacionRotacion.Baja: clave = "rot_clas_Baja"; break;
                case BE.ClasificacionRotacion.SinMovimiento: clave = "rot_clas_SinMovimiento"; break;
                case BE.RecomendacionReposicion.Urgente: clave = "rot_rec_Urgente"; break;
                case BE.RecomendacionReposicion.Reponer: clave = "rot_rec_Reponer"; break;
                case BE.RecomendacionReposicion.Mantener: clave = "rot_rec_Mantener"; break;
                case BE.RecomendacionReposicion.Sobrestock: clave = "rot_rec_Sobrestock"; break;
                default: return valor;
            }
            return SeguridadYServicios.IdiomaManager.getInstance().Traducir(clave) ?? valor;
        }

        private void ActualizarResumen(List<BE.ResultadoRotacion> filas)
        {
            string unidades = SeguridadYServicios.IdiomaManager.getInstance().Traducir("rot_UnidadesSugeridas")
                              ?? "Unidades sugeridas a reponer";
            lblResumen.Text = string.Join("   |   ",
                    BLL.RotacionBLL.ResumirPorRecomendacion(filas).Select(p => TraducirValor(p.Key) + ": " + p.Value))
                + "   |   " + unidades + ": " + BLL.RotacionBLL.TotalUnidadesSugeridas(filas);
        }

        // Aviso no bloqueante: el stock es el actual, así que la cobertura de un rango pasado
        // puede no ser representativa.
        private void ActualizarAdvertenciaFecha()
        {
            bool pasado = dtpHasta.Value.Date < DateTime.Today;
            lblAdvertenciaFecha.Visible = pasado;
            if (pasado)
            {
                lblAdvertenciaFecha.Text = SeguridadYServicios.IdiomaManager.getInstance().Traducir("msg_RotacionRangoPasado")
                    ?? "Atención: el stock es el del día de hoy; la cobertura puede no ser representativa para rangos que terminan antes de hoy.";
            }
        }

        private void dtpHasta_ValueChanged(object sender, EventArgs e)
        {
            if (_cargando) return;
            ActualizarAdvertenciaFecha();
        }

        // Los días de análisis definen el rango: desde = hasta - (días - 1), hoy inclusive si
        // hasta es hoy. Si el usuario edita las fechas a mano, se respetan.
        private void nudDiasAnalisis_ValueChanged(object sender, EventArgs e)
        {
            if (_cargando) return;
            AjustarDesdeSegunDias((int)nudDiasAnalisis.Value);
        }

        private void AjustarDesdeSegunDias(int dias)
        {
            DateTime desde = dtpHasta.Value.Date.AddDays(-(dias - 1));
            if (desde < dtpDesde.MinDate) desde = dtpDesde.MinDate;
            dtpDesde.Value = desde;
        }

        private void ActualizarEncabezados()
        {
            if (dgvRotacion.Columns.Count == 0) return;
            var mgr = SeguridadYServicios.IdiomaManager.getInstance();
            AsignarEncabezado("Codigo", mgr.Traducir("colhdr_Codigo") ?? "SKU");
            AsignarEncabezado("Nombre", mgr.Traducir("colhdr_NombreVino") ?? "Nombre");
            AsignarEncabezado("BodegaNombre", mgr.Traducir("colhdr_Bodega") ?? "Bodega");
            AsignarEncabezado("StockMinimo", mgr.Traducir("colhdr_StockMinimo") ?? "Stock mínimo");
            AsignarEncabezado("StockActual", mgr.Traducir("colhdr_StockActual") ?? "Stock actual");
            AsignarEncabezado("UnidadesVendidas", mgr.Traducir("colhdr_Vendidas") ?? "Vendidas");
            AsignarEncabezado("UnidadesPerdidas", mgr.Traducir("colhdr_Perdidas") ?? "Pérdidas");
            AsignarEncabezado("SociosAtendidos", mgr.Traducir("colhdr_SociosAtendidos") ?? "Socios atendidos");
            AsignarEncabezado("ConsumoMensual", mgr.Traducir("colhdr_ConsumoMensual") ?? "Consumo mensual");
            AsignarEncabezado("MesesCobertura", mgr.Traducir("colhdr_Cobertura") ?? "Cobertura (meses)");
            AsignarEncabezado("Clasificacion", mgr.Traducir("colhdr_Clasificacion") ?? "Clasificación");
            AsignarEncabezado("Recomendacion", mgr.Traducir("colhdr_Recomendacion") ?? "Recomendación");
            AsignarEncabezado("CantidadSugerida", mgr.Traducir("colhdr_CantidadSugerida") ?? "Cantidad sugerida");
        }

        private void AsignarEncabezado(string columna, string texto)
        {
            if (dgvRotacion.Columns[columna] != null)
                dgvRotacion.Columns[columna].HeaderText = texto;
        }

        // Solo se registran los controles cuyo texto es una etiqueta traducible. NumericUpDown y
        // DateTimePicker exponen su valor como Text: registrarlos haría que cada cambio de idioma
        // les restituya el valor de diseño.
        private void GuardarDefaults(Control.ControlCollection controles)
        {
            foreach (Control c in controles)
            {
                bool traducible = c is Label || c is Button || c is GroupBox;
                if (traducible && !string.IsNullOrEmpty(c.Name) && !string.IsNullOrEmpty(c.Text))
                {
                    _controles[c.Name] = c;
                    _defaults[c.Name] = c.Text;
                }
                if (c.HasChildren) GuardarDefaults(c.Controls);
            }
        }

        // NumericUpDown no está contemplado por AppTheme: se le da el mismo estilo que a los TextBox.
        private void EstilizarNumericos()
        {
            foreach (NumericUpDown n in new[] { nudDiasAnalisis, nudUmbralAlta, nudUmbralBaja,
                                                nudCoberturaUrgente, nudCoberturaReposicion, nudCoberturaSobrestock,
                                                nudCoberturaObjetivo })
            {
                n.BackColor = AppTheme.FondoControl;
                n.ForeColor = AppTheme.TextoPrincipal;
                n.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        // Lee el XML (o lo crea con defaults) y vuelca los valores en pantalla.
        private void CargarConfiguracion()
        {
            bool archivoInvalido;
            _config = ConfiguracionRotacionStore.Cargar(out archivoInvalido);
            _cargando = true;
            try
            {
                AplicarConfiguracionEnPantalla(_config);
                PonerPeriodoPorDefecto(_config.DiasAnalisis);
            }
            finally
            {
                _cargando = false;
            }

            if (archivoInvalido)
            {
                MsgBox.Show("El archivo de configuración de rotación estaba dañado o tenía valores inválidos. " +
                            "Se usan los valores por defecto hasta que guarde la configuración.",
                    "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
            }
        }

        private void AplicarConfiguracionEnPantalla(BE.ConfiguracionRotacion c)
        {
            nudDiasAnalisis.Value = Acotar(c.DiasAnalisis, nudDiasAnalisis);
            nudUmbralAlta.Value = Acotar(c.UmbralAltaRotacion, nudUmbralAlta);
            nudUmbralBaja.Value = Acotar(c.UmbralBajaRotacion, nudUmbralBaja);
            nudCoberturaUrgente.Value = Acotar(c.MesesCoberturaUrgente, nudCoberturaUrgente);
            nudCoberturaReposicion.Value = Acotar(c.MesesCoberturaReposicion, nudCoberturaReposicion);
            nudCoberturaSobrestock.Value = Acotar(c.MesesCoberturaSobrestock, nudCoberturaSobrestock);
            nudCoberturaObjetivo.Value = Acotar(c.MesesCoberturaObjetivo, nudCoberturaObjetivo);
        }

        private static decimal Acotar(decimal valor, NumericUpDown nud)
        {
            return Math.Min(nud.Maximum, Math.Max(nud.Minimum, valor));
        }

        private void PonerPeriodoPorDefecto(int dias)
        {
            // El período incluye hoy: "últimos N días" abarca exactamente N días.
            dtpHasta.Value = DateTime.Today;
            dtpDesde.Value = DateTime.Today.AddDays(-(dias - 1));
        }

        private BE.ConfiguracionRotacion LeerConfiguracionDePantalla()
        {
            return new BE.ConfiguracionRotacion
            {
                DiasAnalisis = (int)nudDiasAnalisis.Value,
                UmbralAltaRotacion = nudUmbralAlta.Value,
                UmbralBajaRotacion = nudUmbralBaja.Value,
                MesesCoberturaUrgente = nudCoberturaUrgente.Value,
                MesesCoberturaReposicion = nudCoberturaReposicion.Value,
                MesesCoberturaSobrestock = nudCoberturaSobrestock.Value,
                MesesCoberturaObjetivo = nudCoberturaObjetivo.Value
            };
        }

        private void btnGuardarConfiguracion_Click(object sender, EventArgs e)
        {
            BE.ConfiguracionRotacion nueva = LeerConfiguracionDePantalla();
            try
            {
                BLL.RotacionBLL.Validar(nueva);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            try
            {
                ConfiguracionRotacionStore.Guardar(nueva);
            }
            catch (Exception)
            {
                MsgBox.Show("No se pudo guardar el archivo de configuración. Verifique los permisos de la carpeta " +
                            ConfiguracionRotacionStore.Carpeta + ".",
                    "Error", MsgBox.Botones.OK, MsgBox.Icono.Error);
                return;
            }

            // Las fechas elegidas no se tocan: los días solo mueven "Desde" cuando se edita su campo.
            _config = nueva;

            MsgBox.Show("Configuración guardada correctamente.", "Configuración",
                MsgBox.Botones.OK, MsgBox.Icono.Exito);
        }

        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            // El XML manda: se persiste lo que hay en pantalla y el análisis usa la configuración
            // releída del archivo, de modo que lo analizado es exactamente lo serializado.
            BE.ConfiguracionRotacion enPantalla = LeerConfiguracionDePantalla();
            DateTime desde = dtpDesde.Value.Date;
            DateTime hasta = dtpHasta.Value.Date;

            try
            {
                BLL.RotacionBLL.Validar(enPantalla);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
                return;
            }

            BE.ConfiguracionRotacion vigente;
            try
            {
                ConfiguracionRotacionStore.Guardar(enPantalla);
                bool archivoInvalido;
                vigente = ConfiguracionRotacionStore.Cargar(out archivoInvalido);
                if (archivoInvalido)
                    throw new InvalidOperationException("La configuración guardada no se pudo releer.");
            }
            catch (Exception)
            {
                MsgBox.Show("No se pudo guardar o leer el archivo de configuración, por lo que no se realizó el análisis. " +
                            "Verifique los permisos de la carpeta " + ConfiguracionRotacionStore.Carpeta + ".",
                    "Error", MsgBox.Botones.OK, MsgBox.Icono.Error);
                return;
            }

            try
            {
                List<BE.ResultadoRotacion> filas = _bll.Analizar(desde, hasta, vigente);
                _config = vigente;
                _ultimoResultado = filas;
                _ultimaConfig = vigente;
                _ultimoDesde = desde;
                _ultimoHasta = hasta;
                MostrarResultado(filas);
            }
            catch (InvalidOperationException ex)
            {
                MsgBox.Show(ex.Message, "Atención", MsgBox.Botones.OK, MsgBox.Icono.Atencion);
            }
            catch (Exception)
            {
                MsgBox.Show("No se pudo realizar el análisis de rotación.", "Error",
                    MsgBox.Botones.OK, MsgBox.Icono.Error);
            }
        }

        private void MostrarResultado(List<BE.ResultadoRotacion> filas)
        {
            dgvRotacion.DataSource = filas;

            if (dgvRotacion.Columns.Count > 0)
            {
                dgvRotacion.Columns["VinoId"].Visible = false;
                dgvRotacion.Columns["Prioridad"].Visible = false;
                dgvRotacion.Columns["ConsumoMensual"].DefaultCellStyle.Format = "0.0";
                dgvRotacion.Columns["MesesCobertura"].DefaultCellStyle.Format = "0.0";
                dgvRotacion.Columns["MesesCobertura"].DefaultCellStyle.NullValue = "—";
                dgvRotacion.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                dgvRotacion.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                string[] orden = { "Codigo", "Nombre", "BodegaNombre", "StockActual", "StockMinimo", "UnidadesVendidas",
                                   "UnidadesPerdidas", "ConsumoMensual", "MesesCobertura", "SociosAtendidos",
                                   "Clasificacion", "Recomendacion", "CantidadSugerida" };
                for (int i = 0; i < orden.Length; i++)
                    dgvRotacion.Columns[orden[i]].DisplayIndex = i;

                ActualizarEncabezados();
            }

            ActualizarResumen(filas);
            btnExportarPdf.Enabled = filas.Count > 0;
        }

        // Colorea la fila según la recomendación leyendo el objeto enlazado, así el color
        // acompaña a la fila aunque el usuario reordene la grilla.
        private void dgvRotacion_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            BE.ResultadoRotacion fila = dgvRotacion.Rows[e.RowIndex].DataBoundItem as BE.ResultadoRotacion;
            if (fila == null) return;

            // Clasificación y recomendación se muestran traducidas; el valor enlazado no cambia.
            string columna = dgvRotacion.Columns[e.ColumnIndex].Name;
            if (columna == "Clasificacion" || columna == "Recomendacion")
            {
                e.Value = TraducirValor(e.Value as string);
                e.FormattingApplied = true;
            }

            Color fondo;
            switch (fila.Recomendacion)
            {
                case BE.RecomendacionReposicion.Urgente: fondo = Color.FromArgb(122, 31, 31); break;
                case BE.RecomendacionReposicion.Reponer: fondo = Color.FromArgb(138, 106, 31); break;
                case BE.RecomendacionReposicion.Mantener: fondo = Color.FromArgb(47, 93, 58); break;
                default: fondo = Color.FromArgb(47, 79, 111); break;
            }
            e.CellStyle.BackColor = fondo;
            e.CellStyle.ForeColor = Color.White;
        }

        private void btnExportarPdf_Click(object sender, EventArgs e)
        {
            if (_ultimoResultado == null || _ultimoResultado.Count == 0) return;

            try
            {
                string ruta = RotacionReposicionPdf.Generar(_ultimoResultado, _ultimoDesde, _ultimoHasta, _ultimaConfig);
                RotacionReposicionPdf.Abrir(ruta);
                MsgBox.Show("Informe generado en:\n" + ruta, "Informe", MsgBox.Botones.OK, MsgBox.Icono.Exito);
            }
            catch (Exception)
            {
                MsgBox.Show("No se pudo generar el informe en PDF. Verifique que la carpeta Documentos sea escribible.",
                    "Error", MsgBox.Botones.OK, MsgBox.Icono.Error);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
