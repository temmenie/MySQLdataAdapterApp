using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace MySQLdataAdapterApp
{
    public partial class mainForm : Form
    {
        //objecten van belang bij het maken van een connectie met de database
        MySqlConnection myConnection;
        MySqlDataAdapter myDataAdapter;
        MySqlCommandBuilder myCommandBuidler;
        DataTable myTable;

        string connectionString;

        string selectQuery = "SELECT * FROM producten";
        string toggleForeign_key_checks_uit = "set foreign_key_checks = 0;";
        string toggleForeign_key_checks_aan = "set foreign_key_checks = 1;";

        invulForm invulForm = new invulForm();
        public mainForm()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString;
            invulForm.wijzigingenOpslaan += InvulFormOnWijzigingenOpslaan;
            invulForm.nieuwRecordOpslaan += InvulFormOnNieuwRecordOpslaan;
        }

        private void BtnExecuteSelectQuery_Click(object sender, EventArgs e)
        {
            myConnection = new MySqlConnection(connectionString);
            using (myDataAdapter = new MySqlDataAdapter(selectQuery, myConnection))
            {
                myCommandBuidler = new MySqlCommandBuilder(myDataAdapter);
                myTable = new DataTable();
                myDataAdapter.Fill(myTable);
                DgvProducten.DataSource = myTable;
                myTable.AcceptChanges();
            }

            DgvProducten.ClearSelection();
            BtnRecordVerwijderen.Enabled = false;
            BtnRecordWijzigen.Enabled = false;
        }

        private void BtnUpdateTabel_Click(object sender, EventArgs e)
        {
            DataTable myChanges = myTable.GetChanges();
            if (myChanges == null)
            {
                MessageBox.Show("Er is niets veranderd.");
            }
            else
            {
                MySqlCommand cmdOff = new MySqlCommand();
                MySqlCommand cmdOn = new MySqlCommand();

                cmdOff.CommandText = toggleForeign_key_checks_uit;
                cmdOff.Connection = myConnection;
                cmdOn.CommandText = toggleForeign_key_checks_aan;
                cmdOn.Connection = myConnection;

                myConnection.Open();
                cmdOff.ExecuteNonQuery();

                using (myDataAdapter = new MySqlDataAdapter(selectQuery, myConnection))
                {
                    myCommandBuidler = new MySqlCommandBuilder(myDataAdapter);
                    myDataAdapter.Update(myChanges);
                    myTable.AcceptChanges();
                }

                cmdOn.ExecuteNonQuery();
                myConnection.Close();
            }

            BtnRecordVerwijderen.Enabled = true;
            BtnRecordWijzigen.Enabled = true;
        }

        private void DgvProducten_DoubleClick(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection geselecteerdeRijen = DgvProducten.SelectedRows;

            StringBuilder sb = new StringBuilder();

            foreach (DataGridViewRow r in geselecteerdeRijen)
            {
                sb.Append(r.Index.ToString());
            }

            BtnRecordVerwijderen.Enabled = true;
            BtnRecordWijzigen.Enabled = true;

            MessageBox.Show("rij "+ (Int32.Parse(sb.ToString())+1) + " geselecteerd...");
        }

        private void PasDataTabelAan(DataTable tabel, int rij, int kol, string data)
        {
            if (rij < tabel.Rows.Count && kol < tabel.Columns.Count)
            {
                tabel.Rows[rij][kol] = data;
            }
        }

        private void verwijderRecordData(DataGridView grid,int rij)
        {
            if (rij < grid.RowCount)
            {
                grid.Rows.RemoveAt(rij);
                BtnRecordVerwijderen.Enabled = false;
                BtnRecordWijzigen.Enabled = false;
            }
        }

        private void BtnRecordVerwijderen_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(text: "Ben je zeker dat je " + 
                                                       DgvProducten.SelectedRows[0].Cells[1].Value + 
                                                       " wilt verwijderen? ", "Zeker delete?", MessageBoxButtons.YesNo);
            if(confirmResult == DialogResult.Yes)
            {
                verwijderRecordData(DgvProducten, DgvProducten.SelectedRows[0].Index);
            }
        }

        private void BtnRecordWijzigen_Click(object sender, EventArgs e)
        {
            DataGridViewRow temp = DgvProducten.SelectedRows[0];
            invulForm.recordAanpassen(temp.Index, temp.Cells[1].Value.ToString(),temp.Cells[2].Value.ToString(), temp.Cells[3].Value.ToString());

            invulForm.Show();
            invulForm.BringToFront();
        }

        private void BtnRecordToevoegen_Click(object sender, EventArgs e)
        {
            invulForm.recordToevoegen();
            invulForm.Show();
            invulForm.BringToFront();
        }

        private void InvulFormOnNieuwRecordOpslaan(object sender, List<string> e)
        {
            myTable.Rows.Add();
            PasDataTabelAan(myTable, (myTable.Rows.Count)-1, 1, e[0]);
            PasDataTabelAan(myTable, (myTable.Rows.Count) - 1, 2, e[1]);
            PasDataTabelAan(myTable, (myTable.Rows.Count) - 1, 3, e[2]);
            invulForm.Hide();
        }

        private void InvulFormOnWijzigingenOpslaan(object sender, List<string> e)
        {
            int rij = Int32.Parse(e[3]);
            PasDataTabelAan(myTable, rij, 1, e[0]);
            PasDataTabelAan(myTable, rij, 2, e[1]);
            PasDataTabelAan(myTable, rij, 3, e[2]);
            invulForm.Hide();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
