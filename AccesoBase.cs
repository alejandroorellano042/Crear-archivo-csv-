using System.Data.Odbc;
using System.Data;

namespace Ferrocons_csv
{
    class AccesoBase
    {

        OdbcDataReader dr2;
        OdbcConnection conexion;
        OdbcCommand comando;
        OdbcDataReader dr;
        DataTable dt;
        string cadenaConexion;

       
        public OdbcDataReader pdr2
        {
            get { return dr2; }
            set { dr2 = value; }
        }

        public string pcadenaconexion
        {
            get { return cadenaConexion; }
            set { cadenaConexion = value; }
        }

        public OdbcCommand pcomando
        {
            get { return comando; }
            set { comando = value; }

        }
        public AccesoBase()
        {
            conexion = new OdbcConnection();
            comando = new OdbcCommand();
            dt = new DataTable();
            dr = null;
            cadenaConexion = "";
        }

        public AccesoBase(string strconexion)
        {
            conexion = new OdbcConnection(strconexion);
            comando = new OdbcCommand();
            dt = new DataTable();
            dr = null;
            cadenaConexion = strconexion;

        }

        public void conectar()
        {
            conexion.ConnectionString = cadenaConexion;
            conexion.Open();
            comando.Connection = conexion;
            comando.CommandType = CommandType.Text;

        }
        
        public void desconectar()
        {
            conexion.Close();
            conexion.Dispose();

        }

        public DataTable consultartabla(string tabla)
        {
            dt = new DataTable();
            conectar();
            comando.CommandText = tabla;
            dt.Load(comando.ExecuteReader());
            desconectar();
            return dt;
        }
             
        
        public void leerTabla2(string query)
        {

            conectar();
            comando.CommandText = query;
            dr2 = comando.ExecuteReader();

        }


    }
}
