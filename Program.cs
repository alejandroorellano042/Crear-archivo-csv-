using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using Renci.SshNet;

namespace Ferrocons_csv
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AccesoBase conectar = new AccesoBase("dsn=Basededatos;Uid=SYSDBA;Pwd=********");          
            DateTime fecha = DateTime.Now;            
            string archivo = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"..\Archivo.csv";            
            StreamWriter fil = new StreamWriter(archivo, false, Encoding.UTF8);            
            string contenido = "";
            string encabezado = "";
            string lines = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory.ToString() + @"..\Query.bat"); // CONSULTA SQL
            DataTable tabla = new DataTable();
            tabla = conectar.consultartabla(lines);
            encabezado = "SKU,Nombre,Precio normal,Stock,Meta: precio_de_m2,Meta: m2_por_caja,¿En inventario?";
            fil.WriteLine(encabezado);

            for (int i = 0; i < tabla.Rows.Count; i++)
            {
                double PreMxC = 0;
                double mtxcj = 0;

                double coefi = 0;
                string descripcion = tabla.Rows[i][0].ToString();
                string codigo = tabla.Rows[i][1].ToString();
                string unidmedi = tabla.Rows[i][2].ToString();
                string codigorubro = tabla.Rows[i][3].ToString();
                double stock = Convert.ToDouble(tabla.Rows[i][4]);
                double precio = Convert.ToDouble(tabla.Rows[i][5]);
                double coefivari = Convert.ToDouble(tabla.Rows[i][6]);
                string codigosuperrubro = "";
                string codigogruposuper = "";
                try
                {
                    string query1 = "Select codigosuperrubro from rubros r where r.codigorubro='" + codigorubro + "'";
                    conectar.leerTabla2(query1);
                    conectar.pdr2.Read();
                    codigosuperrubro = conectar.pdr2.GetString(0);
                    conectar.pdr2.Close();
                    conectar.desconectar();
                }
                catch
                {
                    conectar.pdr2.Close();
                    conectar.desconectar();
                }
                try
                {
                    string query2 = "Select * from superrubros r where r.codigosuperrubro='" + codigosuperrubro + "'";
                    conectar.leerTabla2(query2);
                    conectar.pdr2.Read();
                    codigogruposuper = conectar.pdr2.GetString(4);
                    conectar.pdr2.Close();
                    conectar.desconectar();
                }
                catch
                {
                    conectar.pdr2.Close();
                    conectar.desconectar();
                }
                if(codigosuperrubro == "01101" || codigosuperrubro == "01102" || codigosuperrubro == "01103" || codigosuperrubro == "3")
                {
                    //calculo de metros cuadrados por caja y precio por metro cuadrado
                    string query = "select u.coeficientemedicion from unidadesmedida u where u.codigounidadmedida='" + unidmedi + "'";
                    conectar.leerTabla2(query);
                    conectar.pdr2.Read();
                    coefi = conectar.pdr2.GetDouble(0);
                    mtxcj = 1 / coefi; 
                    PreMxC = mtxcj * precio; 

                    conectar.pdr2.Close();
                    conectar.desconectar();
                }
                int eninventario;
                double pasalo = Math.Truncate(stock * 1000) / 1000;
                double pasalo2 = Math.Truncate(precio * 100) / 100;
                NumberFormatInfo nf = new CultureInfo("es-Es", false).NumberFormat;
                nf.NumberDecimalSeparator = ".";
                NumberFormatInfo nf2 = new CultureInfo("es-Es", false).NumberFormat;
                nf2.NumberGroupSeparator = "";
                string stk = stock.ToString("", nf);
                string stock1 = string.Format(stk, nf, pasalo);
                if (stock > 0)
                {
                    eninventario = 1;
                }
                else { eninventario = 0; }
                double stockporc = stock;
                
                var desc = descripcion.Replace(",", ".");
                var cod = codigo.Replace(",", ".");
                var des = desc.Replace(Environment.NewLine, "");
                cod = codigo.Replace("-", ".");
                double preciopor = precio * coefivari;
                precio = precio * coefivari;
                double precioporce = preciopor* coefi;
                              
                if (codigosuperrubro == "01101" || codigosuperrubro == "01102" || codigosuperrubro == "01103" || codigosuperrubro == "3")
                {
                    if(stockporc<=70)
                    {
                        eninventario = 0;                       
                    }
                    contenido = codigo.ToString() + "," + des.ToString() + ",\"" + precioporce.ToString("F", nf) + "\",\"" + stock.ToString("F", nf) + "\",\"" + preciopor.ToString("F", nf) + "\",\"" + coefi.ToString("F", nf) + "\"," + eninventario.ToString();
                }
                else
                {
                    contenido = codigo.ToString() + "," + des.ToString() + ",\"" + precio.ToString("F", nf) + "\",\"" + stock.ToString("F", nf) + "\",\"" + PreMxC.ToString("F", nf) + "\",\"" + coefi.ToString("F", nf) + "\"," + eninventario.ToString();
                }
                fil.WriteLine(contenido);


            }






            fil.Close();
            //  Ftp
            try
            {
                var host = @"192.168.000.000"; //@"sftp.flexxus.com.ar";
                var username = @"Usuario";//@"";
                var pass = @"Password";// @"";
                var port = 22;

                ConnectionInfo connectionInfo = new ConnectionInfo(host, port, username, new PasswordAuthenticationMethod(username, pass));

                SftpClient sftp = new SftpClient(connectionInfo);
                sftp.Connect();

                FileStream stream = new FileStream(archivo, FileMode.Open);
                sftp.BufferSize = 1024 * 10;
                sftp.UploadFile(stream, @"/home/subcarpeta1/Archivo.csv");

                sftp.Disconnect();
                sftp.Dispose();
            }
            catch (Exception ex)
            {
                string archivo2 = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"..\ERRORftp.txt";
                StreamWriter file2 = new StreamWriter(archivo2, false, Encoding.UTF8);
                file2.WriteLine(ex.ToString());
                file2.Close();

            }

        }
    }
}
