using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColetaAfde
{
    public partial class Form1 : Form
    {
       // public static MainScreen telaPrincipal;

        public static int conectadoNoEquipamento = 0; // 
        public static int statusComunication = 0; // 
        public static int statusConectar = 0; // 
        public static int REGISTRO_NAO_EXTRAIDO = 0;
        public static int REGISTRO_EXTRAIDO = 1;
        public static int REGISTRO_SEM_COMUNI = 2;
        public static int REGISTRO_SEM_RESPOSTA = 3;
        public static string ip = "";

        public EquipamentoRep equipamentoRep = new EquipamentoRep();
        public Form1()
        {
            InitializeComponent();
        }
        public bool conectaNoEquipamento()
        {
            try
            {
                Thread.Sleep(2000);
                Console.WriteLine("Socket(2);");
                TcpClient sck = new TcpClient(equipamentoRep.getIp(), equipamentoRep.getPort());
                Console.WriteLine("ClientHenry(.5);");
                Thread.Sleep(500);
                ip = equipamentoRep.getIp();
               // ClientHenry clienteHenry = new ClientHenry(sck, true, ip);
               // clienteHenry.run();
                //            new Thread(new ClientHenry(sck, true)).start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return true;
        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Text1: " + textBox1.Text);
            equipamentoRep.setUser(textBox1.Text);
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            equipamentoRep.setPass(textBox2.Text);
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            equipamentoRep.setIp(textBox3.Text);
        }

        private void TextBox4_TextChanged(object sender, EventArgs e)
        {
            int x;
            x = Convert.ToInt32(textBox4.Text);
            equipamentoRep.setPort(x);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Form2 authentic = new Form2();
            authentic.Show();
        }
    }
}
