namespace BiblioTech2.Api.Respuestas
{
    public class Resultado
    {
        public bool Exito { get; } //solo se puede leer - Nadie puede cambiarlo después de creado (así no se "maquilla" un error).

        public String Mensaje { get; }// texto que explica qué pasó. También de solo lectura.

        public TipoResultado Tipo { get; }

        protected Resultado(bool exito, String mensaje, TipoResultado tipo)
        {
            Exito = exito;
            Mensaje = mensaje;
            Tipo = tipo;
        }

        public static Resultado ok(String mensaje) => new(true, mensaje, TipoResultado.Exito);
        // "=>" = forma corta de escribir "return ...".
        public static Resultado NoEncontrado(string mensaje) => new(false, mensaje, TipoResultado.NoEncontrado);
        public static Resultado ReglaNegocio(string mensaje) => new(false, mensaje, TipoResultado.ReglaNegocio);
        public static Resultado Validacion(string mensaje) => new(false, mensaje, TipoResultado.Validacion);
    }

    public class Resultado<T> : Resultado
    {
        public T? Datos { get; }

        private Resultado(bool exito, String mensaje, TipoResultado tipo, T? datos)
            : base(exito, mensaje, tipo)
        {
            Datos = datos;
        }

        public static Resultado<T> ok(T datos, String mensaje) => new(true, mensaje, TipoResultado.Exito, datos);
        public static new Resultado<T> NoEncontrado(String mensaje) => new(false, mensaje, TipoResultado.NoEncontrado, default);
        public static new Resultado<T> ReglaNegocio(String mensaje) => new(false, mensaje, TipoResultado.ReglaNegocio, default);
    }
}