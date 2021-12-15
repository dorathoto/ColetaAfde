using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColetaAfde
{
    public class EquipamentoRep
    {
        private String user = "";
        private String pass = "";
        private String ip = "";
        private int port;
        private String nrSerie = "";
        private String mac = "";
        private String modelo = "";

        private String chaveRSA = "";
        private String expoenteRSA = "";

        public EquipamentoRep()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////    dados de usuario
        public String getUser()
        {
            return user;
        }

        public void setUser(String user)
        {
            this.user = user;
        }
        ////////////////////////////////////////////////////////////////////////////
        ////    dados de Senha    
        public String getPass()
        {
            return pass;
        }

        public void setPass(String pass)
        {
            this.pass = pass;
        }
        ////////////////////////////////////////////////////////////////////////////
        ////    dados de Porta
        public int getPort()
        {
            return port;
        }

        public void setPort(int port)
        {
            this.port = port;
        }

        public String getIp()
        {
            return ip;
        }

        public void setIp(String ip)
        {
            this.ip = ip;
        }

        public String getNrSerie()
        {
            return nrSerie;
        }

        public void setNrSerie(String nrSerie)
        {
            this.nrSerie = nrSerie;
        }

        public String getMac()
        {
            return mac;
        }

        public void setMac(String mac)
        {
            this.mac = mac;
        }


        public String getModelo()
        {
            return modelo;
        }

        public void setModelo(String modelo)
        {
            this.modelo = modelo;
        }

        public String getChaveRSA()
        {
            return chaveRSA;
        }

        public void setChaveRSA(String chaveRSA)
        {
            this.chaveRSA = chaveRSA;
        }

        public String getExpoenteRSA()
        {
            return expoenteRSA;
        }

        public void setExpoenteRSA(String expoenteRSA)
        {
            this.expoenteRSA = expoenteRSA;
        }
    }
}
