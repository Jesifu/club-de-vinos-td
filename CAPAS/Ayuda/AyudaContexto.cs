using System;
using System.Collections.Generic;

namespace CAPAS
{
    // Ayuda sensible al contexto: asocia el nombre del tipo de cada formulario con el id del tema
    // de ayuda (ver ayuda_es.xml) que se abre al presionar F1. Un formulario que no figura acá
    // abre la ayuda en la raíz (el tema "inicio"). FormBase consulta esta tabla en su propiedad
    // virtual AyudaTema, que un formulario puede sobrescribir si necesita un tema dinámico.
    internal static class AyudaContexto
    {
        internal const string TemaRaiz = "inicio";

        private static readonly Dictionary<string, string> _mapa =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "frmMenu", "inicio" },
                { "LogIn", "login" },
                { "frmContraseña", "clave" },
                { "frmAdminUsuarios", "usuarios" },
                { "frmNuevoUsuario", "usuarios" },
                { "frmEditarUsuario", "usuarios" },
                { "frmAsignarPerfiles", "usuarios" },
                { "frmHistorialUsuario", "usuarios-historial" },
                { "frmPerfiles", "perfiles" },
                { "frmIdiomas", "idiomas" },
                { "frmBitacora", "bitacora" },
                { "frmRestaurarIntegridad", "integridad" },
                { "frmCatalogoVinos", "cat-alta" },
                { "frmAutorizarAltaVino", "cat-autorizar-alta" },
                { "frmAutorizarBajaVino", "cat-autorizar-baja" },
                { "frmRegistrarEntradaStock", "stock-entrada" },
                { "frmRegistrarAjusteInventario", "stock-ajuste" },
                { "frmAlertaStockMinimo", "stock-alerta" },
                { "frmSocios", "socios" },
                { "frmArmarCaja", "caja-armar" },
                { "frmPickingCaja", "caja-picking" },
                { "frmDespacharCaja", "caja-despacho" },
                { "frmHistorialDespachos", "caja-historial" },
                { "frmAnalisisRotacion", "rotacion" }
            };

        // Id del tema para un tipo de formulario; la raíz si no hay un tema específico.
        internal static string TemaPara(Type tipoFormulario)
        {
            string id;
            if (tipoFormulario != null && _mapa.TryGetValue(tipoFormulario.Name, out id))
                return id;
            return TemaRaiz;
        }
    }
}
