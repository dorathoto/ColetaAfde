using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColetaAfde
{
    public class ClientHenry
    {

        private static List<ClientHenry> clientList = new List<ClientHenry>();

        public static String DEFAULT_USER = "";
        public static String DEFAULT_PASS = "";

        private bool conexaoAtiva;
        private TcpClient socketClient;

        private NetworkStream outByte;  // buffer no socket
        private BinaryWriter inByte;   // escrita de buffer no socket 
        private BinaryReader readByte; // leitura de buffer no socket
        public EquipamentoRep equipamentoRep;

        public ClientHenry(TcpClient socket, bool conexaoAtiva, string ipSocket)
        {
            this.socketClient = socket;
            this.conexaoAtiva = conexaoAtiva;
            this.equipamentoRep = new EquipamentoRep();

            equipamentoRep.setIp(ipSocket);  // adicionado por referência o ip nessa instância 

            outByte = socket.GetStream();
            inByte = new BinaryWriter(outByte);
            readByte = new BinaryReader(outByte);

        }

        private void printInfo()
        {
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Equipamento conectado");
            Console.WriteLine("USER:     " + DEFAULT_USER);
            Console.WriteLine("PASS:     " + DEFAULT_PASS);
            Console.WriteLine("IP:       " + equipamentoRep.getIp());
            Console.WriteLine("PORT:     " + equipamentoRep.getPort());
            Console.WriteLine("Chave:    " + equipamentoRep.getChaveRSA());
            Console.WriteLine("Expoente: " + equipamentoRep.getExpoenteRSA());
            Console.WriteLine("Modelo:   " + equipamentoRep.getModelo());
            Console.WriteLine("Serial:   " + equipamentoRep.getNrSerie());
            Console.WriteLine("MAC:      " + equipamentoRep.getMac());
            Console.WriteLine("-----------------------------------------------------");
        }
    }
}


