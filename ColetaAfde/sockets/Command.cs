using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColetaAfde
{
    class Command
    {
        public static int MAX_INDEX = 99;
        public static int STATUS_SUCCESS = 0;
        public static int STATUS_FAIL = -1;
        public static int INDEX_DATA = 13;
        public static int INDEX_SIZE = 14;
        public static char SEP_DATA = '+';
        public static char SEP_ATTRIBUTE = '[';
        public static char SEP_PARAMETER = ']';
        public static char SEP_ATTRIBUTE_VELTI_REP = ']';
        public static char SEP_ATTRIBUTE2 = '{';
        public static char SEP_ATTRIBUTE3 = '}';
        public static int START_BYTE = 0x02;
        public static string RECEIVE_AUT = "RA";
    public static string RECEIVE_SIZE = "RQ";
    public static string RECEIVE_BIO = "RD";
    public static string RECEIVE_CONFIG = "RC";
    public static string RECEIVE_DATE_TIME = "RH";
    public static string RECEIVE_EMPLOYER = "RE";
    public static string RECEIVE_RECORD = "RR";
    public static string RECEIVE_USER = "RU";
    public static string RECEIVE_ONLINE = "RO";
    public static string RECEIVE_ONLINE_EVENT_ACS = "REON";
    public static string RECEIVE_CARD = "RCAR";
    public static string RECEIVE_PERIOD = "RPER";
    public static string RECEIVE_SCHEDULE = "RHOR";
    public static string RECEIVE_TIME_SCALE = "RESC";
    public static string RECEIVE_HOLIDAY = "RFER";
    public static string RECEIVE_FIRMWARE = "RF";
    public static string SEND_AUT = "EA";
    public static string SEND_BIO = "ED";
    public static string SEND_CONFIG = "EC";
    public static string SEND_DATE_TIME = "EH";
    public static string SEND_EMPLOYER = "EE";
    public static string SEND_USER = "EU";
    public static string SEND_CARD = "ECAR";
    public static string SEND_PERIOD = "EPER";
    public static string SEND_SCHEDULE = "EHOR";
    public static string SEND_TIME_SCALE = "EESC";
    public static string SEND_HOLIDAY = "EFER";
    public static string SEND_BEEP = "ES";
    public static string SEND_FIRMWARE = "EF";
    public static string SEND_USER_SYSTEM = "ES";
    public static string RECEIVE_USER_SYSTEM = "RS";

    private static int indexCmd = 1;

        private string bCode;
        private int bIndex;
        private string bData;
        private int bVersao;

        private int index;
        private char[] body;

        private bool usaAutenticacao;

        //string com o comando nao criptografado
        private String strCmd;

        private AnwerCommand anwerCmd;

        public Command(String code, int index, String data, bool usaAutenticacao)
        {
            this.usaAutenticacao = usaAutenticacao;
            setCommand(code, index, data, 0);
        }

        public Command(String code, int index, String data)
        {
            this(code, index, data, 0);
        }

        public Command(String code, int index, String data, int versao)
        {
            setCommand(code, index, data, versao);
        }

        public void setAnwerCommand(AnwerCommand anwerCommand)
        {
            anwerCmd = anwerCommand;
        }

        public AnwerCommand getAnwerCommand()
        {
            return anwerCmd;
        }

        public void setNewIndex(int index)
        {
            setCommand(bCode, index, bData, bVersao);
        }

        private void setCommand(String code, int index, String data, int versao)
        {
            this.bCode = code;
            this.bIndex = index;
            this.bData = data;
            this.bVersao = versao;

            this.index = index;
            strCmd = "";
            //transformando indice em string
            index = index > 99 ? 99 : index < 0 ? 0 : index;

            string msgIndex = string.Parse(index);
            int corpoSize;
            if (data.Length > 0)
            {
                corpoSize = data.Length + 10 + msgIndex.Length + code.Length;
            }
            else
            {
                corpoSize = 9 + msgIndex.Length + code.Length;
            }
            char[] bodyDecript = new char[corpoSize];
            int idxD = 0;

            int checksum = 0;

            for (int i = 0; i < msgIndex.Length; i++)
            {
                bodyDecript[idxD++] = msgIndex[i];
                strCmd = strCmd + msgIndex[i];
            }
            bodyDecript[idxD++] = SEP_DATA;
            strCmd = strCmd + SEP_DATA;
            //Carregando o codigo do comando
            for (int i = 0; i < code.Length; i++)
            {
                bodyDecript[idxD++] = code[i];
                strCmd = strCmd + code[i];
            }
            bodyDecript[idxD++] = SEP_DATA;
            strCmd = strCmd + SEP_DATA;
            //codigo de erro. Indica a versao usada do comando. Ex: RR
            if (versao < 0 || versao > 99)
            {
                versao = 0;
            }
            String versaoStr = (new NumberFormatInfo("00")).format(versao);
            bodyDecript[idxD++] = versaoStr[0];
            bodyDecript[idxD++] = versaoStr[1];
            strCmd = strCmd + versaoStr;
            if (data.Length > 0)
            {
                bodyDecript[idxD++] = SEP_DATA;
                strCmd = strCmd + SEP_DATA;
                //Adicionando comandos
                for (int i = 0; i < data.Length; i++)
                {
                    bodyDecript[idxD++] = data[i];
                    strCmd = strCmd + data[i];
                }
            }

            if (usaAutenticacao)
            {

                char[] bodyCript = criptogravaBuffer(bodyDecript);
                bodyDecript = new char[bodyCript.Length + 5];
                for (int i = 0; i < bodyCript.Length; i++)
                {
                    bodyDecript[i] = bodyCript[i];
                }
            }

            int idx = 0;
            body = new char[bodyDecript.Length];
            body[idx++] = (char)0x02;
            body[idx++] = (char)((bodyDecript.Length - 5) & 0xFF); //-5 desconta SB, Tam, CS e EB
            body[idx++] = (char)((bodyDecript.Length - 5) >> 8);

            for (int i = 0; i < bodyDecript.Length - 5; i++)
            {
                body[idx++] = bodyDecript[i];
            }
            //calculando checksum
            for (int i = 1; i < idx; i++)
            {
                checksum ^= body[i];
            }
            body[idx++] = (char)(checksum & 0xFF); //checksum
            body[idx++] = (char)0x03; //endbyte
        }

        public char[] criptogravaBuffer(char[] bufferChar)
        {
            char[] retorno = new char[bufferChar.Length];
            byte[] bufferCript = new byte[bufferChar.Length - 5];
            //String aux = "";
            for (int i = 0; i < bufferChar.Length - 5; i++)
            {
                bufferCript[i] = (byte)bufferChar[i];
            }

            try
            {
                byte dadosCript[] = AuthenticationUtils.encodeAES(AuthenticationUtils.generateAESKey(), bufferCript);
                retorno = new char[dadosCript.Length];
                for (int i = 0; i < dadosCript.Length; i++)
                {
                    retorno[i] = (char)(dadosCript[i] & 0x00FF);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return retorno;
        }


        public char[] getBody()
        {
            return body;
        }

        public int getIndex()
        {
            return index;
        }

        public void setIndex(int index)
        {
            this.index = index;
        }

        public String toString()
        {
            StringBuilder linha = new StringBuilder("");
            if (body != null && body.Length > 0)
            {
                for (int i = 0; i < body.Length; i++)
                {
                    linha.Append(body[i]);
                }
            }
            return linha.ToString();
        }

        public String toBufferString()
        {
            StringBuilder linha = new StringBuilder("");
            int length = body.Length;
            if (body != null && length > 0)
            {
                linha.Append(String.Format("%02X", (int)body[0]));
                linha.Append(" ");
                linha.Append(String.Format("%02X", (int)body[1]));
                linha.Append(" ");
                linha.Append(String.Format("%02X", (int)body[2]));
                linha.Append(" ");
                for (int i = 3; i < length - 2; i++)
                {
                    linha.Append(body[i]);
                }
                linha.Append(" ");
                linha.Append(String.Format("%02X", (int)body[length - 2]));
                linha.Append(" ");
                linha.Append(String.Format("%02X", (int)body[length - 1]));
            }
            return linha.ToString();
        }

        public String getStrComando()
        {
            return strCmd;
        }

        public bool getUsaAutenticacao()
        {
            return usaAutenticacao;
        }

        /*
        public void setUsaAutenticacao(boolean usaAutenticacao) {
            this.usaAutenticacao = usaAutenticacao;
        }
        /**/
        public static String caracterPCToCaracterEquip(String registros)
        {
            String novaStr = "";
            String str = registros;
            for (int j = 0; j < str.Length; j++)
            {
                switch ((int)str[j])
                {
                    case 0xe1:
                        novaStr = novaStr + (char)0xffe1;
                        break;
                    case 0xe9:
                        novaStr = novaStr + (char)0xffe9;
                        break;
                    case 0xed:
                        novaStr = novaStr + (char)0xffed;
                        break;
                    case 0xf3:
                        novaStr = novaStr + (char)0xfff3;
                        break;
                    case 0xfa:
                        novaStr = novaStr + (char)0xfffa;
                        break;
                    case 0xc1:
                        novaStr = novaStr + (char)0xffc1;
                        break;
                    case 0xc9:
                        novaStr = novaStr + (char)0xffc9;
                        break;
                    case 0xcd:
                        novaStr = novaStr + (char)0xffcd;
                        break;
                    case 0xd3:
                        novaStr = novaStr + (char)0xffd3;
                        break;
                    case 0xda:
                        novaStr = novaStr + (char)0xffda;
                        break;
                    case 0xe0:
                        novaStr = novaStr + (char)0xffe0;
                        break;
                    case 0xe8:
                        novaStr = novaStr + (char)0xffe8;
                        break;
                    case 0xec:
                        novaStr = novaStr + (char)0xffec;
                        break;
                    case 0xf2:
                        novaStr = novaStr + (char)0xfff2;
                        break;
                    case 0xf9:
                        novaStr = novaStr + (char)0xfff9;
                        break;
                    case 0xc0:
                        novaStr = novaStr + (char)0xffc0;
                        break;
                    case 0xc8:
                        novaStr = novaStr + (char)0xffc8;
                        break;
                    case 0xcc:
                        novaStr = novaStr + (char)0xffcc;
                        break;
                    case 0xd2:
                        novaStr = novaStr + (char)0xffd2;
                        break;
                    case 0xd9:
                        novaStr = novaStr + (char)0xffd9;
                        break;
                    case 0xe4:
                        novaStr = novaStr + (char)0xffe4;
                        break;
                    case 0xeb:
                        novaStr = novaStr + (char)0xffeb;
                        break;
                    case 0xef:
                        novaStr = novaStr + (char)0xffef;
                        break;
                    case 0xf6:
                        novaStr = novaStr + (char)0xfff6;
                        break;
                    case 0xfc:
                        novaStr = novaStr + (char)0xfffc;
                        break;
                    case 0xc4:
                        novaStr = novaStr + (char)0xffc4;
                        break;
                    case 0xcb:
                        novaStr = novaStr + (char)0xffcb;
                        break;
                    case 0xcf:
                        novaStr = novaStr + (char)0xffcf;
                        break;
                    case 0xd6:
                        novaStr = novaStr + (char)0xffd6;
                        break;
                    case 0xdc:
                        novaStr = novaStr + (char)0xffdc;
                        break;
                    case 0xe2:
                        novaStr = novaStr + (char)0xffe2;
                        break;
                    case 0xea:
                        novaStr = novaStr + (char)0xffea;
                        break;
                    case 0xee:
                        novaStr = novaStr + (char)0xffee;
                        break;
                    case 0xf4:
                        novaStr = novaStr + (char)0xfff4;
                        break;
                    case 0xfb:
                        novaStr = novaStr + (char)0xfffb;
                        break;
                    case 0xc2:
                        novaStr = novaStr + (char)0xffc2;
                        break;
                    case 0xca:
                        novaStr = novaStr + (char)0xffca;
                        break;
                    case 0xce:
                        novaStr = novaStr + (char)0xffce;
                        break;
                    case 0xd4:
                        novaStr = novaStr + (char)0xffd4;
                        break;
                    case 0xdb:
                        novaStr = novaStr + (char)0xffdb;
                        break;
                    case 0xe3:
                        novaStr = novaStr + (char)0xffe3;
                        break;
                    case 0xf5:
                        novaStr = novaStr + (char)0xfff5;
                        break;
                    case 0xc3:
                        novaStr = novaStr + (char)0xffc3;
                        break;
                    case 0xd5:
                        novaStr = novaStr + (char)0xffd5;
                        break;
                    case 0xe7:
                        novaStr = novaStr + (char)0xffe7;
                        break;
                    case 0xc7:
                        novaStr = novaStr + (char)0xffc7;
                        break;
                    case 0xf1:
                        novaStr = novaStr + (char)0xfff1;
                        break;
                    case 0xd1:
                        novaStr = novaStr + (char)0xffd1;
                        break;
                    default:
                        novaStr = novaStr + str[j];
                }
            }
            return novaStr;
        }

        public static String caracterEquipToCaracterPC(String registros)
        {

            String novaStr = "";
            String str = registros;
            for (int j = 0; j < str.Length; j++)
            {
                switch ((int)str[j])
                {
                    case 0xffe1:
                        novaStr = novaStr + "Ã¡";
                        break;
                    case 0xffe9:
                        novaStr = novaStr + "Ã©";
                        break;
                    case 0xffed:
                        novaStr = novaStr + "Ã­";
                        break;
                    case 0xfff3:
                        novaStr = novaStr + "Ã³";
                        break;
                    case 0xfffa:
                        novaStr = novaStr + "Ãº";
                        break;
                    case 0xffc1:
                        novaStr = novaStr + "Ã�";
                        break;
                    case 0xffc9:
                        novaStr = novaStr + "Ã‰";
                        break;
                    case 0xffcd:
                        novaStr = novaStr + "Ã�";
                        break;
                    case 0xffd3:
                        novaStr = novaStr + "Ã“";
                        break;
                    case 0xffda:
                        novaStr = novaStr + "Ãš";
                        break;
                    case 0xffe0:
                        novaStr = novaStr + "Ã ";
                        break;
                    case 0xffe8:
                        novaStr = novaStr + "Ã¨";
                        break;
                    case 0xffec:
                        novaStr = novaStr + "Ã¬";
                        break;
                    case 0xfff2:
                        novaStr = novaStr + "Ã²";
                        break;
                    case 0xfff9:
                        novaStr = novaStr + "Ã¹";
                        break;
                    case 0xffc0:
                        novaStr = novaStr + "Ã€";
                        break;
                    case 0xffc8:
                        novaStr = novaStr + "Ãˆ";
                        break;
                    case 0xffcc:
                        novaStr = novaStr + "ÃŒ";
                        break;
                    case 0xffd2:
                        novaStr = novaStr + "Ã’";
                        break;
                    case 0xffd9:
                        novaStr = novaStr + "Ã™";
                        break;
                    case 0xffe4:
                        novaStr = novaStr + "Ã¤";
                        break;
                    case 0xffeb:
                        novaStr = novaStr + "Ã«";
                        break;
                    case 0xffef:
                        novaStr = novaStr + "Ã¯";
                        break;
                    case 0xfff6:
                        novaStr = novaStr + "Ã¶";
                        break;
                    case 0xfffc:
                        novaStr = novaStr + "Ã¼";
                        break;
                    case 0xffc4:
                        novaStr = novaStr + "Ã„";
                        break;
                    case 0xffcb:
                        novaStr = novaStr + "Ã‹";
                        break;
                    case 0xffcf:
                        novaStr = novaStr + "Ã�";
                        break;
                    case 0xffd6:
                        novaStr = novaStr + "Ã–";
                        break;
                    case 0xffdc:
                        novaStr = novaStr + "Ãœ";
                        break;
                    case 0xffe2:
                        novaStr = novaStr + "Ã¢";
                        break;
                    case 0xffea:
                        novaStr = novaStr + "Ãª";
                        break;
                    case 0xffee:
                        novaStr = novaStr + "Ã®";
                        break;
                    case 0xfff4:
                        novaStr = novaStr + "Ã´";
                        break;
                    case 0xfffb:
                        novaStr = novaStr + "Ã»";
                        break;
                    case 0xffc2:
                        novaStr = novaStr + "Ã‚";
                        break;
                    case 0xffca:
                        novaStr = novaStr + "ÃŠ";
                        break;
                    case 0xffce:
                        novaStr = novaStr + "ÃŽ";
                        break;
                    case 0xffd4:
                        novaStr = novaStr + "Ã”";
                        break;
                    case 0xffdb:
                        novaStr = novaStr + "Ã›";
                        break;
                    case 0xffe3:
                        novaStr = novaStr + "Ã£";
                        break;
                    case 0xfff5:
                        novaStr = novaStr + "Ãµ";
                        break;
                    case 0xffc3:
                        novaStr = novaStr + "Ãƒ";
                        break;
                    case 0xffd5:
                        novaStr = novaStr + "Ã•";
                        break;
                    case 0xffe7:
                        novaStr = novaStr + "Ã§";
                        break;
                    case 0xffc7:
                        novaStr = novaStr + "Ã‡";
                        break;
                    case 0xfff1:
                        novaStr = novaStr + "Ã±";
                        break;
                    case 0xffd1:
                        novaStr = novaStr + "Ã‘";
                        break;
                    default:
                        novaStr = novaStr + str.charAt(j);
                }
            }
            return novaStr;
        }

        public static int getNextIndex()
        {
            indexCmd = indexCmd + 1;
            return indexCmd;
        }

        public void commandAuth(){
            {
                try
                {
                    // Establish the remote endpoint for the socket.
                    // The name of the 
                    // remote device is "host.contoso.com".
                    IPHostEntry ipHostInfo = Dns.Resolve(txtIP.Text);
                    IPAddress ipAddress = ipHostInfo.AddressList[0];
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                    string command = "";
                    string preCommand = "";
                    int idxByte = 0;
                    string strModulo = "";
                    string strExpodente = "";
                    string strRec = "";
                    byte chkSum = 0;
                    string strComandoComCriptografia = "";
                    string strAux = "";

                    int i = 0;

                    if (client == null)
                    {
                        client = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);
                    }
                    // Create a TCP/IP socket.
                    bool conectado = client.Connected;

                    if (conectado == false)
                    {
                        // Connect to the remote endpoint.
                        client.BeginConnect(remoteEP,
                            new AsyncCallback(ConnectCallback), client);
                        connectDone.WaitOne();
                    }


                    Random rnd = new Random();

                    chaveAes[0] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[1] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[2] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[3] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[4] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[5] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[6] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[7] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[8] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[9] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[10] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[11] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[12] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[13] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[14] = Convert.ToByte(rnd.Next(1, 256));
                    chaveAes[15] = Convert.ToByte(rnd.Next(1, 256));

                    command = "";
                    command = command + (char)(2);
                    // start byte

                    preCommand = preCommand + (char)(7);
                    // tamanho do comando
                    preCommand = preCommand + (char)(0);
                    // tamanho do comando
                    preCommand = preCommand + "1+RA+00";
                    chkSum = calcCheckSumString(preCommand);

                    command = command + preCommand;
                    command = command + Convert.ToChar(chkSum);
                    // checksum
                    // end byte
                    command = command + (char)(3);


                    // Send test data to the remote device.
                    Send(client, command);
                    sendDone.WaitOne();

                    quantBytesRec = client.Receive(bufferBytes);

                    response = "";
                    while (i < quantBytesRec)
                    {
                        response = response + (char)bufferBytes[i];

                        i = i + 1;
                    }



                    while (idxByte < quantBytesRec)
                    {
                        if (idxByte >= 3)
                        {
                            if (idxByte <= quantBytesRec - 3)
                            {
                                strRec = strRec + response.ElementAt(idxByte);
                            }
                        }
                        idxByte = idxByte + 1;
                    }
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);

                    strModulo = Mid(strRec, 1, strRec.IndexOf("]"));
                    strExpodente = Trim(Mid(strRec, strRec.IndexOf("]") + 2, strRec.Length - strRec.IndexOf("]") - 1));

                    strAux = "1]" + txtUsuario.Text + "]" + txtSenha.Text + "]" + System.Convert.ToBase64String(chaveAes);

                    RSAPersistKeyInCSP(strModulo);
                    byte[] dataToEncrypt = Encoding.Default.GetBytes(strAux);
                    byte[] encryptedData = null;

                    RSAParameters RSAKeyInfo = new RSAParameters();

                    RSAKeyInfo.Modulus = System.Convert.FromBase64String(strModulo);
                    RSAKeyInfo.Exponent = System.Convert.FromBase64String(strExpodente);

                    encryptedData = RSAEncrypt(dataToEncrypt, RSAKeyInfo, false);

                    strAux = System.Convert.ToBase64String(encryptedData);


                    strComandoComCriptografia = "2+EA+00+" + strAux;

                    preCommand = "";
                    command = "";
                    command = command + Convert.ToChar(2);
                    // start byte
                    preCommand = preCommand + Convert.ToChar(strComandoComCriptografia.Length);
                    // tamanho do comando
                    preCommand = preCommand + Convert.ToChar(0);
                    // tamanho do comando
                    preCommand = preCommand + strComandoComCriptografia;
                    chkSum = calcCheckSumString(preCommand);

                    command = command + preCommand;
                    command = command + Convert.ToChar(chkSum);
                    // checksum

                    command = command + Convert.ToChar(3);
                    // end byte
                    Send(client, command);
                    sendDone.WaitOne();

                    quantBytesRec = client.Receive(bufferBytes);

                    response = "";
                    i = 0;
                    while (i < quantBytesRec)
                    {

                        response = response + Convert.ToChar(bufferBytes[i]);
                        i = i + 1;
                    }

                    strRec = "";
                    idxByte = 0;
                    while (idxByte < quantBytesRec)
                    {
                        if (idxByte >= 3)
                        {
                            if (idxByte <= quantBytesRec - 3)
                            {
                                strRec = strRec + response.ElementAt(idxByte);
                            }
                        }
                        idxByte = idxByte + 1;
                    }
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);

                    if (strRec == "000")
                    {
                        // Write the response to the console.
                        MessageBox.Show("Autenticado");
                    }
                    else
                    {
                        MessageBox.Show("Não autenticado.(" + strRec + ")");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

    }
}
