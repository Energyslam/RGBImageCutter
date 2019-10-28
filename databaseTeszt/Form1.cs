using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace databaseTeszt
{
    public partial class Form1 : Form
    {

        Bitmap[,] imgArray;
        Color[,] colorArray;
        SolidBrush brush;
        float xGrid = 15; //amount of horizontal gridspaces
        float yGrid = 4; //amount of horizontal gridspaces
        private Graphics GFX;
        float xValue; //width of image divided by amount of horizontal gridspaces
        float yValue;//height of image divided by amount of vertical gridspaces
        Bitmap bitImage;
        Image image = null;
        int databaseEntries = 0;
        int databaseIdStart = 0;

        public Form1()
        {
            openImage();
            InitializeComponent();
            init();
            cutImage();
            drawImage();

            //readDatabase();
            //deleteDatabase();
            //addDatabase();
            //MessageBox.Show("Succesfully made the database");
            //updateDatabase();

        }

        void openImage()
        {
            try
            {
                //Getting The Image From The System
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    System.IO.FileInfo file = new System.IO.FileInfo(open.FileName);
                    bitImage = new Bitmap(open.FileName);
                }
                else bitImage = new Bitmap(image);
            }
            catch (Exception)
            {

                throw new ApplicationException("Failed loading image");
            }
        }

        void init()
        {
            xValue = (float)bitImage.Width / xGrid;
            yValue = (float)bitImage.Height / yGrid;
            this.Size = new Size(bitImage.Width, bitImage.Height);
            GFX = this.CreateGraphics();

            this.WindowState = FormWindowState.Minimized;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }



        private void cutImage()
        {
            var imgarray = new Bitmap[(int)xGrid, (int)yGrid];
            colorArray = new Color[(int)xGrid, (int)yGrid];
            for (int i = 0; i < xGrid; i++)
            {
                for (int j = 0; j < yGrid; j++)
                {
                    imgarray[i, j] = new Bitmap((int)xValue, (int)yValue);
                    var graphics = Graphics.FromImage(imgarray[i, j]);
                    graphics.DrawImage(bitImage, new RectangleF(0, 0, xValue, yValue), new RectangleF(i * xValue, j * yValue, xValue, yValue), GraphicsUnit.Pixel);
                    graphics.Dispose();
                }
            }
            imgArray = imgarray;
        }

        private void drawImage()
        {
            for (int i = 0; i < xGrid; i++)
            {
                //Debug.Write("\n");
                for (int j = 0; j < yGrid; j++)
                {
                    GFX.DrawImage(imgArray[i, j], new PointF(i * xValue, j * yValue));
                    Color dominant = DominantColor.getDominantColor(imgArray[i, j]);
                    int red = dominant.R;
                    int green = dominant.G;
                    int blue = dominant.B;
                    colorArray[i, j] = dominant;
                    brush = new SolidBrush(dominant);
                    GFX.FillRectangle(brush, i * xValue, j * yValue, xValue, yValue);
                    //Debug.Write(dominant + " | ");
                }
            }
        }

        void updateDatabase()
        {
            string connectionString = "";
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand("", databaseConnection);
            commandDatabase.CommandTimeout = 60;
            MySqlDataReader reader;

            try
            {
                databaseConnection.Open();
                //reader = commandDatabase.ExecuteReader();
                updateDatabaseEntries(databaseConnection);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        void addDatabase()
        {
            string connectionString = "";
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand("", databaseConnection);
            commandDatabase.CommandTimeout = 60;
            MySqlDataReader reader;

            try
            {
                databaseConnection.Open();
                //reader = commandDatabase.ExecuteReader();
                addDatabaseEntries(databaseConnection);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }

        void readDatabase()
        {
            string connectionString = "";
            // Your query,
            string query = "SELECT * FROM leds";

            // Prepare the connection
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand(query, databaseConnection);
            commandDatabase.CommandTimeout = 60;
            MySqlDataReader reader;

            try
            {
                databaseConnection.Open();

                //addDatabaseEntries(databaseConnection); //Create the table. Do this one time after emptying/first time creating your database in phpMyAdmin
                //updateDatabaseEntries(databaseConnection);

                // Execute the query
                reader = commandDatabase.ExecuteReader();

                // IMPORTANT : 
                // If your query returns result, use the following processor :
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // As our database, the array will contain : ID 0, RED 1,GREEN 2, BLUE 3, POSITION 4
                        // Do something with every received database ROW
                        string[] row = { reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4)};
                        //Debug.Write(row[0]);
                        if (databaseIdStart == 0)
                        {
                            
                            databaseIdStart = reader.GetInt16(0);
                            Debug.Write("\n\n\nSTART = " + databaseIdStart);
                        }
                        databaseEntries++;
                        //Debug.Write("AMOUNT OF ENTRIES" + databaseEntries);
                    }
                }
                else
                {
                    Debug.WriteLine("No rows found.");
                }
                reader.Close();
                databaseConnection.Close();
            }
            catch (Exception ex)
            {
                // Show any error message.
                MessageBox.Show(ex.Message);
            }
        }

        void deleteDatabase()
        {
            string connectionString = "";
            MySqlConnection databaseConnection = new MySqlConnection(connectionString);
            MySqlCommand commandDatabase = new MySqlCommand("SELECT * FROM LEDS", databaseConnection);
            commandDatabase.CommandTimeout = 60;
            MySqlDataReader reader;

            try
            {
                databaseConnection.Open();
                //reader = commandDatabase.ExecuteReader();
                deleteDatabaseEntries(databaseConnection);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void deleteDatabaseEntries(MySqlConnection databaseConnection)
        {
            for (int i = 0; i < databaseEntries; i++)
            {
                MySqlCommand deleteCommand = new MySqlCommand("DELETE FROM leds WHERE id='" + (i + databaseIdStart) + "'", databaseConnection);
                Debug.Write("Deleting from ... " + (i + databaseIdStart));
                deleteCommand.ExecuteNonQuery();
            }
        }

        void addDatabaseEntries(MySqlConnection databaseConnection)
        {
            MySqlCommand restartIncrement = new MySqlCommand("ALTER TABLE leds AUTO_INCREMENT = 1", databaseConnection);
            restartIncrement.ExecuteNonQuery();

            for (int i = 0; i < xGrid; i++)
            {
                for (int j = 0; j < yGrid; j++)
                {
                    MySqlCommand insertCommand = new MySqlCommand("INSERT INTO leds(r,g,b,position) VALUES(?r,?g,?b,?position)", databaseConnection);
                    insertCommand.Parameters.Add("?r", MySqlDbType.Int16).Value = colorArray[i, j].R;
                    insertCommand.Parameters.Add("?g", MySqlDbType.Int16).Value = colorArray[i, j].G;
                    insertCommand.Parameters.Add("?b", MySqlDbType.Int16).Value = colorArray[i, j].B;
                    insertCommand.Parameters.Add("?position", MySqlDbType.Decimal).Value = i + (j*0.01);
                    insertCommand.ExecuteNonQuery();
                }
            }
        }
        
        void updateDatabaseEntries(MySqlConnection databaseConnection)
        {
            MySqlCommand insertCommand;
            for (int i = 0; i < xGrid; i++)
            {
                for (int j = 0; j < yGrid; j++)
                {
                    insertCommand = new MySqlCommand("UPDATE leds SET r = ?r, g = ?g, b = ?b WHERE position = ?position", databaseConnection);
                    //Debug.Write(i + "." + j + "\n");
                    insertCommand.Parameters.Add("?r", MySqlDbType.Int16).Value = colorArray[i, j].R;
                    insertCommand.Parameters.Add("?g", MySqlDbType.Int16).Value = colorArray[i, j].G;
                    insertCommand.Parameters.Add("?b", MySqlDbType.Int16).Value = colorArray[i, j].B;
                    insertCommand.Parameters.Add("?position", MySqlDbType.Decimal).Value = i + (j * 0.01);
                    insertCommand.ExecuteNonQuery();
                }
            }
        }
    }

}
