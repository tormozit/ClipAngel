using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace ClipAngel
{
    public partial class Main : Form
    {
        private SQLiteConnection m_dbConnection;
        private String connectionString;
        private SQLiteDataAdapter dataAdapter;
        public Main()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click_1(object sender, EventArgs e)
        {

        }

        private void Text_TextChanged(object sender, EventArgs e)
        {

        }

        private void Main_Load(object sender, EventArgs e)
        {
            string sql;
            SQLiteCommand command;
            // Encryption http://stackoverflow.com/questions/12190672/can-i-password-encrypt-sqlite-database
            connectionString = "data source=D:\\VC\\ClipAngel\\DB\\db.sqlite";
            //String connectionString = "Data Source=MyDatabase.sqlite;Version=3;";

            // http://blog.tigrangasparian.com/2012/02/09/getting-started-with-sqlite-in-c-part-one/
            //m_dbConnection = new SQLiteConnection(connectionString);
            //m_dbConnection.Open();
            //sql = "CREATE TABLE Clips (Title VARCHAR(50), Text VARCHAR(0), Data BLOB, Size INT, Type VARCHAR(10), Created DATETIME, Application VARCHAR(50), Window VARCHAR(100))";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //try
            //{
            //    command.ExecuteNonQuery();
            //}
            //catch { };
            //sql = "insert into Clips (Title, Text, Type) values ('Test1', 'Test1 very big text', 'PlainText')";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();
            //sql = "insert into Clips (Title, Text, Type) values ('Test2', 'Test2 very big text', 'PlainText')";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();
            //sql = "insert into Clips (Title, Text, Type) values ('Test3', 'Test3 very big text', 'PlainText')";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();

            // https://msdn.microsoft.com/ru-ru/library/fbk67b6z(v=vs.110).aspx
            dataAdapter = new SQLiteDataAdapter("Select * From Clips", connectionString);
            // Populate a new data table and bind it to the BindingSource.
            dataGridView.DataSource = clipBindingSource;
            UpdateClipBindingSource();
        }

        private void UpdateStatusStrip()
        {
            DataRow CurrentRow = (clipBindingSource.Current as DataRowView).Row;
            statusStrip.Items.Find("Created", false)[0].Text = CurrentRow["Created"].ToString();
            statusStrip.Items.Find("Size", false)[0].Text = CurrentRow["Size"].ToString() + "b";
            statusStrip.Items.Find("Chars", false)[0].Text = CurrentRow["Chars"].ToString() + "ch";
            statusStrip.Items.Find("Type", false)[0].Text = CurrentRow["Type"].ToString();
        }

        private void Text_Click(object sender, EventArgs e)
        {
            string NewText;
            NewText = "" + Text.SelectionStart;
            if (Text.SelectionLength > 0)
            {
                //NewText += "+" + Text.SelectionLength + "=" + (Text.SelectionStart + Text.SelectionLength);
                NewText += "+" + Text.SelectionLength;
            }
            statusStrip.Items.Find("Position", false)[0].Text = NewText;

            //NewText = "" + Text.Cursor;
            //statusStrip.Items.Find("PositionXY", false)[0].Text = NewText;
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            UpdateClipBindingSource();
        }
        private void UpdateClipBindingSource()
        {
            string SelectCommand = "Select * From Clips";
            if (Filter.Text != "")
            {
                SelectCommand += " Where Text Like '%" + Filter.Text + "%'";
            }
            dataAdapter.SelectCommand.CommandText = SelectCommand;
            DataTable table = new DataTable();
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            dataAdapter.Fill(table);
            clipBindingSource.DataSource = table;
        }

        private void Filter_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ClearFilter_Click(object sender, EventArgs e)
        {
            Filter.Text = "";
            UpdateClipBindingSource();
        }

        private void dataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            UpdateStatusStrip();
        }

        private void Text_CursorChanged(object sender, EventArgs e)
        {
            // This event not working. Why? Decided to use Click instead.
        }

    }
}
