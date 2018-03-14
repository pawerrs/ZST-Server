using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Configuration;


namespace Serwer
{
    public partial class Serwer : Form
    {
        private BackgroundWorker m_oBackgroundWorker = null;
        public SqlConnection connection;
        public IPAddress addressIP;
        public int port;
        public SqlCommandBuilder commandbuilder;
        public SqlDataAdapter adapter;
        public DataTable datatable;
        public string command;
        private string s;

        public Serwer()
        {
            InitializeComponent();
        }

        private void Odbierzbutton_Click(object sender, EventArgs e)
        {
            if (null == m_oBackgroundWorker)
            {
                m_oBackgroundWorker = new BackgroundWorker();
                m_oBackgroundWorker.DoWork +=
                    new DoWorkEventHandler(m_oBackgroundWorker_DoWork);
                m_oBackgroundWorker.WorkerSupportsCancellation = true;
            }
            m_oBackgroundWorker.RunWorkerAsync();
        }

        void m_oBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int nCounter = 1; nCounter <= 1000; ++nCounter)
            {
                if (m_oBackgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                try
                {
                    Thread.Sleep(1000);
                    SimpleListenerExample("http://localhost:" + porttb.Text + "/");
                }
                catch (Exception err)
                {
                    listBox1.Invoke(new Action(delegate ()
                    {
                        listBox1.Items.Add("Błąd");
                    }));
                    MessageBox.Show(err.ToString());
                }
            }
        }


        public void ShowRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                listBox1.Invoke(new Action(delegate ()
                {
                    listBox1.Items.Add("Brak danych przesłanych przez klienta");
                }));
                return;
            }

            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);
            if (request.ContentType != null)
            {
                listBox1.Invoke(new Action(delegate ()
                {
                    listBox1.Items.Add("Typ wiadomości " + request.ContentType);
                }));
            }

            listBox1.Invoke(new Action(delegate ()
            {
                listBox1.Items.Add("Długośc wiadomości " + request.ContentLength64);
                listBox1.Items.Add("Początek wiadomości:");
            }));
           // s = String.Empty;
            s = reader.ReadToEnd();

            listBox1.Invoke(new Action(delegate ()
            {
                listBox1.Items.Add(s);
                listBox1.Items.Add("Koniec wiadomości:");
            }));
            body.Close();
            reader.Close();
           
            // żródłem strona: https://msdn.microsoft.com/en-us/library/system.net.httplistenerrequest.contenttype(v=vs.110).aspx
        }


        private void Zakonczbutton_Click(object sender, EventArgs e)
        {
            Thread.Sleep(500);
            this.Close();
        }


        private void Wyswietlbutton_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = Wypelnij();
        }
    
        public DataTable Wypelnij()
        {
            datatable = new DataTable();
            connection = new SqlConnection(ConfigurationManager.AppSettings["constring"]);
            try
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(@"Select* FROM [dbo].[" + tabelatb.Text + "]", connection);
                adapter.Fill(datatable);
                dataGridView1.DataSource = adapter;

            }
            catch (Exception exp)
            {
                MessageBox.Show("Błąd \n" + exp.Message, "Błąd", MessageBoxButtons.OK);
            }
            connection.Close();
            return datatable;
        }

        private void Uaktualnijbutton_Click(object sender, EventArgs e)
        {


            connection = new SqlConnection(ConfigurationManager.AppSettings["constring"]);
            try
            {
                connection.Open();
                adapter = new SqlDataAdapter("SELECT * FROM " + tabelatb.Text, connection);
                SqlCommandBuilder commandbuilder = new SqlCommandBuilder(adapter);
                adapter.Update(datatable);
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd \n" + ex.Message, "Błąd", MessageBoxButtons.OK);
            }
            connection.Close();

        }
        private void Filtrujbutton_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = filtruj();
        }

        private DataTable filtruj()
        {
            DataTable dt = new DataTable();
            connection = new SqlConnection(ConfigurationManager.AppSettings["constring"]);
            try
            {
                connection.Open();
                adapter = new SqlDataAdapter(komendatb.Text, connection);
                adapter.Fill(dt);
               

            }
            catch (Exception err)
            {
                MessageBox.Show("Wpisz dane ponownie");
                MessageBox.Show("Błąd \n" + err.Message, "Błąd", MessageBoxButtons.OK);
            }
            connection.Close();
            return dt;
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {

            dataGridView1.DataSource = sortuj();

        }

        private DataTable sortuj()
        {

            DataTable dt = new DataTable();

            if (radioButton4.Checked)
            {
                command = "SELECT* FROM " + tabelatb.Text + " WHERE " + kolumnatb.Text + " > " + "'" + wartosctb.Text + "'";

            }
            else if (radioButton5.Checked)
            {
                command = "SELECT* FROM " + tabelatb.Text + " WHERE " + kolumnatb.Text + " < " + "'" + wartosctb.Text + "'";

            }
            else if (radioButton6.Checked)
            {
                command = "SELECT* FROM " + tabelatb.Text + " WHERE " + kolumnatb.Text + " = " + "'" + wartosctb.Text + "'";

            }
            else if (radioButton1.Checked)
            {
                command = "SELECT* FROM " + tabelatb.Text + " WHERE " + kolumnatb.Text + " != " + "'" + wartosctb.Text + "'";

            }
            else if (radioButton2.Checked)
            {
                command = "SELECT* FROM " + tabelatb.Text + " WHERE " + kolumnatb.Text + " >= " + "'" + wartosctb.Text + "'";

            }
            else if (radioButton3.Checked)
            {
                command = "SELECT* FROM " + tabelatb.Text + " WHERE " + kolumnatb.Text + " <= " + "'" + wartosctb.Text + "'";

            }
            else
            {
                MessageBox.Show("Zaznacz jak chcesz sortować");
            }
            try {
                
                connection = new SqlConnection(ConfigurationManager.AppSettings["constring"]);
                connection.Open();
                adapter = new SqlDataAdapter(command, connection);
                adapter.Fill(dt);
                connection.Close();
            }
            catch(Exception error)
            {
                MessageBox.Show("Błąd + {0}", error.Message);
            }
            return dt;
        }

        private void Generujbutton_Click(object sender, EventArgs e)
        {

            try
            {
                s = s.Remove(s.Length - 1);
                s = s + ");";
                MessageBox.Show("CREATE TABLE " + s);
                connection = new SqlConnection(ConfigurationManager.AppSettings["constring"]);
                SqlCommand command = new SqlCommand("CREATE TABLE " + s, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
                listBox1.Invoke(new Action(delegate ()

                {
                    MessageBox.Show("W bazie danych utworzono tabelę", "Komunikat", MessageBoxButtons.OK);
                }));

                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd \n" + ex.Message, "Błąd", MessageBoxButtons.OK);
                connection.Close();
            }
            
            
        }

        private void Przeslijbutton_Click(object sender, EventArgs e)
        {
            
           
            try
            {
                s = s.Remove(s.Length - 1);
                s = s + ");";
                MessageBox.Show(s);
                connection = new SqlConnection(ConfigurationManager.AppSettings["constring"]);
                SqlCommand command = new SqlCommand(s, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
                listBox1.Invoke(new Action(delegate ()

                {
                    MessageBox.Show(" Przesłano rekord!");
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd \n" + ex.Message, "Błąd", MessageBoxButtons.OK);
            }
            connection.Close();
           
        }

        private void Usuńbutton_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(ConfigurationManager.AppSettings["constring"]);

            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(" DELETE FROM " + tabelatb.Text + " WHERE " + rekordtb.Text + " = '" + deletewartosctb.Text + "'", connection);
                command.ExecuteNonQuery();
                MessageBox.Show("Usunięto rekord o " + rekordtb.Text + " = " + deletewartosctb.Text);

            }
            catch (Exception error)
            {
                MessageBox.Show("Błąd \n" + error.Message, "Błąd", MessageBoxButtons.OK);
            }
            connection.Close();
        }

       

        private void Wyswietltabele(object sender, EventArgs e)
        {
            try
            {
                connection = new SqlConnection(ConfigurationManager.AppSettings["constring"]);
                connection.Open();
                DataTable t = connection.GetSchema("Tables");
                dataGridView1.DataSource = t;
            }
            catch(Exception error)
            {
                MessageBox.Show("Nie udało się pobrac nazw tabel");
            }
            connection.Close();
        }

        public void SimpleListenerExample(string prefixes)
        {

            try { 
                if (prefixes == null || prefixes.Length == 0)
                    throw new ArgumentException("prefixes");

                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(prefixes);
                listener.Start();
                listBox1.Invoke(new Action(delegate ()
                {
                    listBox1.Items.Add("Nasłuchuje...");
                }));
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                ShowRequestData(request);
                HttpListenerResponse response = context.Response;
                string responseString = "Odebrałem wiadomość";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                listBox1.Invoke(new Action(delegate ()
                {

                    listBox1.Items.Add(response.StatusCode);
                }));

                output.Close();
                listener.Stop();
                /* źródłem strona: https://msdn.microsoft.com/en-us/library/system.net.httplistener(v=vs.110).aspx */
               
            }

            catch (Exception error)
            {
                MessageBox.Show("Nieudana próba kontaktu");
            }


            }

        private void Połaczbutton_Click(object sender, EventArgs e)
        {

        }
    }
  }


