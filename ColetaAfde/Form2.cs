using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Diagnostics;

namespace ColetaAfde
{
    public partial class Form2 : Form
    {
        private static String response = String.Empty;
        private static readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static int port = 3000;
        private Socket client;
        private byte[] chaveAes = new byte[16];
        private byte[] bufferBytes = new byte[1024];
        byte[] registros = new byte[1024];
        private static int quantBytesRec = 0;


        public Form2()
        {
            InitializeComponent();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "3000")
            {
                port = Convert.ToInt32(textBox1.Text);
            }
            int varAux = 0;
            int varAux2 = 0;
            while (varAux == 0)
            {
                try
                {
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
                    string quantReg = "";

                    int i = 0;

                    if (client == null)
                    {
                        client = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);
                    }

                    bool conectado = client.Connected;

                    if (conectado == false)
                    {
                        client.Connect(remoteEP);
                    }

                    GerarChaveAesAleatorias();

                    command = "";
                    command += (char)(2);
                    // start byte

                    preCommand += (char)(7);
                    // tamanho do comando
                    preCommand += (char)(0);
                    // tamanho do comando
                    preCommand += "1+RA+00";
                    chkSum = calcCheckSumString(preCommand);

                    command += preCommand;
                    command += Convert.ToChar(chkSum);
                    // checksum
                    // end byte
                    command += (char)(3);

                    Debug.WriteLine("comando montado: " + command.ToString());

                    // Send test data to the remote device.
                    Send(client, command);
                    sendDone.WaitOne();

                    quantBytesRec = client.Receive(bufferBytes);

                    response = "";
                    while (i < quantBytesRec)
                    {
                        response += (char)bufferBytes[i];

                        i++;
                    }

                    Debug.WriteLine("string da resposta: " + response.ToString());

                    while (idxByte < quantBytesRec) //loop para separar o startbyte e endbyte e retirar apenas a chave Aes, porém, contem os +.
                    {
                        if (idxByte >= 3)
                        {
                            if (idxByte <= quantBytesRec - 3)
                            {
                                strRec += response.ElementAt(idxByte);
                            }
                        }
                        idxByte++;
                    }
                    Debug.WriteLine("strRec----------->" + strRec.ToString());
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1); // 
                    Debug.WriteLine("strRec2---------->" + strRec.ToString());
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);
                    Debug.WriteLine("strRec3---------->" + strRec.ToString());
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);
                    Debug.WriteLine("strRec4---------->" + strRec.ToString());

                    strModulo = Mid(strRec, 1, strRec.IndexOf("]"));
                    strExpodente = Trim(Mid(strRec, strRec.IndexOf("]") + 2, strRec.Length - strRec.IndexOf("]") - 1));

                    Debug.WriteLine("strModulo-------->" + strModulo.ToString());
                    Debug.WriteLine("strExpodente----->" + strExpodente.ToString());

                    strAux = "1]" + txtUsuario.Text + "]" + txtSenha.Text + "]" + System.Convert.ToBase64String(chaveAes);

                    Debug.WriteLine("strAux----->" + strAux.ToString());
                    RSAPersistKeyInCSP(strModulo);
                    byte[] dataToEncrypt = Encoding.Default.GetBytes(strAux); // gerado um array contendo a string de dados de login
                    byte[] encryptedData = null; //array que irá receber os valores criptografados

                    RSAParameters RSAKeyInfo = new RSAParameters();

                    RSAKeyInfo.Modulus = System.Convert.FromBase64String(strModulo);
                    RSAKeyInfo.Exponent = System.Convert.FromBase64String(strExpodente);

                    encryptedData = RSAEncrypt(dataToEncrypt, RSAKeyInfo, false); // recebeu os valores criptografados

                    strAux = System.Convert.ToBase64String(encryptedData);


                    strComandoComCriptografia = "2+EA+00+" + strAux;

                    preCommand = "";
                    command = "";
                    command += Convert.ToChar(2);
                    // start byte
                    preCommand += Convert.ToChar(strComandoComCriptografia.Length);
                    // tamanho do comando
                    preCommand += Convert.ToChar(0);
                    // tamanho do comando
                    preCommand += strComandoComCriptografia;
                    chkSum = calcCheckSumString(preCommand);

                    command += preCommand;
                    command += Convert.ToChar(chkSum);
                    // checksum

                    command += Convert.ToChar(3);
                    // end byte
                    Thread.Sleep(800);
                    Send(client, command);
                    sendDone.WaitOne();
                    quantBytesRec = client.Receive(bufferBytes);
                    response = "";
                    i = 0;
                    while (i < quantBytesRec)
                    {

                        response += Convert.ToChar(bufferBytes[i]);
                        i++;
                    }

                    strRec = "";
                    idxByte = 0;
                    while (idxByte < quantBytesRec)
                    {
                        if (idxByte >= 3)
                        {
                            if (idxByte <= quantBytesRec - 3)
                            {
                                strRec += response.ElementAt(idxByte);
                            }
                        }
                        idxByte++;
                    }
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);
                    strRec = Mid(strRec, strRec.IndexOf("+") + 2, strRec.Length - strRec.IndexOf("+") - 1);

                    if (strRec == "000")
                    {
                        Debug.WriteLine("Autenticado");
                        textBoxt.AppendText("\nAutenticado!");
                        varAux = 1; // variável para loop de autenticação
                        varAux2 = 0;
                    }
                    else
                    {
                        Debug.WriteLine("Não autenticado.(" + strRec + ")");
                        // MessageBox.Show("Não autenticado!");
                        textBoxt.AppendText("\nAutenticado!");
                        textBoxt.AppendText("\nTentando reconectar com o socket...");
                        varAux2++;
                        if (varAux2 > 5)
                        {
                            MessageBox.Show("Não foi possível conectar ao equipamento.");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            //backgroundWorker1.RunWorkerAsync();
            TesteReg();

        }

        private void GerarChaveAesAleatorias()
        {
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
        }

        public void TesteReg()
        {
            int y = 5;
            int quantRegistros = 0;
            int counterReg = 0;
            var strComandoComCriptografia = "";
            bool arqSave = false;

            do
            {

                if (counterReg > 0)
                {
                    strComandoComCriptografia = "01+RR+00+N]" + y.ToString() + "]" + (counterReg * y - (y - 1)).ToString(); //counterReg.ToString();// + txtRegistros.Text + "]1";
                }
                // if (counterReg > 0) strComandoComCriptografia = "01+RR+00+N]" + y.ToString() + "]999";
                //  if (counterReg > 0) strComandoComCriptografia = "01+RR+00+N]1]" + (counterReg+1).ToString();


                else strComandoComCriptografia = "01+RQ+00+R";

                counterReg++;
                int i = 0;
                int chkSum = 0;
                string strRec = "";
                int idxByte = 0;
                string dados = "";

                Random rnd = new Random();

                byte[] IV = new byte[16];
                IV[0] = Convert.ToByte(rnd.Next(1, 256));
                IV[1] = Convert.ToByte(rnd.Next(1, 256));
                IV[2] = Convert.ToByte(rnd.Next(1, 256));
                IV[3] = Convert.ToByte(rnd.Next(1, 256));
                IV[4] = Convert.ToByte(rnd.Next(1, 256));
                IV[5] = Convert.ToByte(rnd.Next(1, 256));
                IV[6] = Convert.ToByte(rnd.Next(1, 256));
                IV[7] = Convert.ToByte(rnd.Next(1, 256));
                IV[8] = Convert.ToByte(rnd.Next(1, 256));
                IV[9] = Convert.ToByte(rnd.Next(1, 256));
                IV[10] = Convert.ToByte(rnd.Next(1, 256));
                IV[11] = Convert.ToByte(rnd.Next(1, 256));
                IV[12] = Convert.ToByte(rnd.Next(1, 256));
                IV[13] = Convert.ToByte(rnd.Next(1, 256));
                IV[14] = Convert.ToByte(rnd.Next(1, 256));
                IV[15] = Convert.ToByte(rnd.Next(1, 256));
                int tamanhoPacote;
                if ((counterReg * y - (y - 1)) < 1001)
                    tamanhoPacote = 32;
                else if ((counterReg * y - (y - 1)) < 10001) tamanhoPacote = 48;
                else tamanhoPacote = 64;
                byte[] comandoByte = new byte[1024]; //37                         
                int IdxComandoByte = 3;
                comandoByte[0] = 2;
                // start byte
                comandoByte[1] = (byte)(tamanhoPacote & 0xff);
                // tamanho
                comandoByte[2] = (byte)((tamanhoPacote >> 8) & 0xff);
                // tamanho
                byte[] cmdCrypt = Encoding.Default.GetBytes(Encoding.Default.GetChars(EncryptStringToBytes_Aes(strComandoComCriptografia, chaveAes, IV)));
                chkSum = 0;
                i = 0;

                while (i < IV.Length)
                {
                    comandoByte[IdxComandoByte] = IV[i];
                    IdxComandoByte++;
                    i++;
                }
                i = 0;
                while (i < cmdCrypt.Length)
                {
                    comandoByte[IdxComandoByte] = cmdCrypt[i];
                    IdxComandoByte++;
                    i++;
                }

                i = 1;
                while (i < IdxComandoByte)
                {
                    chkSum ^= comandoByte[i];
                    i++;
                }
                comandoByte[IdxComandoByte] = (byte)chkSum;
                IdxComandoByte++;

                comandoByte[IdxComandoByte] = 3;

                string strAux = "";
                i = 0;
                while (i < IdxComandoByte)
                {
                    strAux += Convert.ToChar(comandoByte[i]);
                    i++;
                }
                byte[] envCommand = new byte[IdxComandoByte + 1];
                for (i = 0; i < IdxComandoByte + 1; i++)
                {
                    envCommand[i] = comandoByte[i];
                    //   Debug.WriteLine("comandoByte[" + i + "] = " + comandoByte[i] + "  envCommand[" + i + "] = " + envCommand[i]);
                }
                Thread.Sleep(1000);
                Send2(client, envCommand);
                sendDone.WaitOne();

                quantBytesRec = client.Receive(bufferBytes);

                response = "";
                i = 0;
                while (i < quantBytesRec)
                {
                    response += (char)bufferBytes[i];
                    bufferBytes[i] = 0;
                    i++;
                }

                i = 0;
                strRec = "";
                idxByte = 0;
                byte[] byteData = new byte[quantBytesRec - 5];
                while (idxByte < quantBytesRec)
                {
                    if (idxByte >= 3)
                    {
                        if (idxByte <= quantBytesRec - 3)
                        {
                            byteData[i] = Convert.ToByte(response.ElementAt(idxByte)); //contém a resposta em bytes armazenada
                            i++;
                            strRec += response.ElementAt(idxByte);
                        }
                    }
                    idxByte++;
                }
                i = 0;
                while (i < 16)
                {
                    IV[i] = byteData[i];
                    i++;
                }

                byte[] byteData2 = new byte[quantBytesRec - 16 - 5];
                i = 0;

                while (i < byteData.Length - 16)
                {
                    byteData2[i] = byteData[i + 16];
                    byteData[i + 16] = 0;
                    i++;
                }

                byte[] bufferRecDecrypt = DecryptStringFromBytes_Aes2(byteData2, chaveAes, IV);
                for (i = 0; i < byteData2.Length; i++)
                {
                    byteData2[i] = 0;
                }

                i = 0;
                while (i < bufferRecDecrypt.Length)
                {
                    if (Convert.ToChar(bufferRecDecrypt[i]) == '{')
                    {
                        break;
                    }
                    i++;
                }

                for (i = 0; i < bufferRecDecrypt.Length; i++)
                {
                    registros[i] = bufferRecDecrypt[i];
                    bufferRecDecrypt[i] = 0;
                    //Console.WriteLine("registros[" + i + "] = " + registros[i]);
                }
                i = 0;

                for (i = 0; i < registros.Length; i++)
                {
                    if (registros[i] != 0)
                    {
                        dados += Convert.ToChar(registros[i]);
                        registros[i] = 0;
                    }
                }

                Debug.WriteLine("Resposta do equipamento: " + dados);
                dados = Mid(dados, dados.IndexOf("]") + 2, dados.Length - dados.IndexOf("]") - 1);
                Debug.WriteLine("Resposta do equipamento2: " + dados);
                if (dados.Contains("+") || dados.Contains("]"))
                {
                    dados = Mid(dados, dados.IndexOf("]") + 2, dados.Length - dados.IndexOf("]") - 1);
                    continue;
                }
                if (quantRegistros == 0)
                {
                    textBoxt.AppendText("\n\nQuantidade de registros: " + dados + "\n");

                    quantRegistros = Convert.ToInt32(dados);
                    progressBar1.Maximum = quantRegistros;
                }
                else
                {
                    textBoxt.AppendText("Registros salvos: " + dados + "\n");
                    using (StreamWriter saida = new StreamWriter(@"E:\HenryREP\ColetaAfde\registerDataBase.txt", arqSave))
                    {
                        saida.Write(dados.ToString());
                        dados = null;
                    }
                    arqSave = true;
                }
                var valor = counterReg * y;
                if (valor < progressBar1.Maximum)
                {
                    progressBar1.Value = valor;
                }
                Debug.WriteLine("Recebidos " + valor + " de " + quantRegistros);
            } while (counterReg * y < quantRegistros);
        }

        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Debug.WriteLine(e.Message);

                return null;
            }
        }
        //static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        //{
        //    try
        //    {
        //        //Create a new instance of RSACryptoServiceProvider.
        //        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

        //        //Import the RSA Key information. This only needs
        //        //toinclude the public key information.
        //        RSA.ImportParameters(RSAKeyInfo);

        //        //Encrypt the passed byte array and specify OAEP padding.  
        //        //OAEP padding is only available on Microsoft Windows XP or
        //        //later.  
        //        return RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
        //    }
        //    //Catch and display a CryptographicException  
        //    //to the console.
        //    catch (CryptographicException e)
        //    {
        //        Debug.WriteLine(e.Message);

        //        return null;
        //    }

        //}

        static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                //Create a new instance of RSACryptoServiceProvider.
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                //Import the RSA Key information. This needs
                //to include the private key information.
                RSA.ImportParameters(RSAKeyInfo);

                //Decrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                return RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Debug.WriteLine(e.ToString());

                return null;
            }

        }

        public static void RSAPersistKeyInCSP(string ContainerName) //recebe a string com a chave
        {
            try
            {
                // Create a new instance of CspParameters.  Pass
                // 13 to specify a DSA container or 1 to specify
                // an RSA container.  The default is 1.
                CspParameters cspParams = new CspParameters();

                // Specify the container name using the passed variable.
                cspParams.KeyContainerName = ContainerName;

                //Create a new instance of RSACryptoServiceProvider to generate
                //a new key pair.  Pass the CspParameters class to persist the 
                //key in the container.  The PersistKeyInCsp property is true by 
                //default, allowing the key to be persisted. 
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(cspParams);

                //Indicate that the key was persisted.
                Debug.WriteLine("The RSA key was persisted in the container, \"{0}\".", ContainerName);
            }
            catch (CryptographicException e)
            {
                Debug.WriteLine(e.Message);

            }
        }

        public static void RSADeleteKeyInCSP(string ContainerName)
        {
            try
            {
                // Create a new instance of CspParameters.  Pass
                // 13 to specify a DSA container or 1 to specify
                // an RSA container.  The default is 1.
                CspParameters cspParams = new CspParameters();

                // Specify the container name using the passed variable.
                cspParams.KeyContainerName = ContainerName;

                //Create a new instance of RSACryptoServiceProvider. 
                //Pass the CspParameters class to use the 
                //key in the container.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider(cspParams);

                //Explicitly set the PersistKeyInCsp property to false
                //to delete the key entry in the container.
                RSAalg.PersistKeyInCsp = false;

                //Call Clear to release resources and delete the key from the container.
                RSAalg.Clear();

                //Indicate that the key was persisted.
                Debug.WriteLine("The RSA key was deleted from the container, \"{0}\".", ContainerName);
            }
            catch (CryptographicException e)
            {
                Debug.WriteLine(e.Message);

            }
        }
        public static string Trim(string s)
        {
            return s.Trim();
        }
        public static string Mid(string s, int a, int b)
        {
            string temp = s.Substring(a - 1, b);
            return temp;
        }
        public byte calcCheckSumString(string data)
        {
            String strBuf = "";
            String strAux = "";
            byte cks = 0;
            int i = 0;

            while (i < data.Length)
            {
                strAux = ((byte)(data.ElementAt(i))).ToString("X2");
                strBuf += strAux;
                cks = (byte)(cks ^ (byte)(data.ElementAt(i)));
                i++;
            }
            return cks;
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Debug.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.Default.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void Send2(Socket client, byte[] byteData)
        {

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                int teste3 = aesAlg.BlockSize;
                int teste4 = aesAlg.KeySize;
                Debug.WriteLine("tamanho: " + teste3 + "  chave: " + teste4);
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);

                            int quant = plainText.Length;

                            if (quant > 16)
                            {
                                quant %= 16;
                            }
                            else
                            {

                            }
                            quant = 16 - quant;
                            while (quant < 17 && quant != 0)
                            {
                                swEncrypt.Write(Convert.ToChar(Convert.ToByte("0")));
                                quant--;
                            }


                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static byte[] EncryptStringToBytes_Aes2(String plainText, byte[] plainText2, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (plainText2 == null || plainText2.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] bytesStr = Encoding.Default.GetBytes(plainText);
                        csEncrypt.Write(bytesStr, 0, bytesStr.Length);
                        csEncrypt.Write(plainText2, 0, plainText2.Length);
                        int quant = bytesStr.Length + plainText2.Length;

                        if (quant > 16)
                        {
                            quant %= 16;
                        }
                        quant = 16 - quant;
                        byte[] bytesZeros = new byte[16] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
                        if (quant < 16 && quant != 0)
                        {
                            csEncrypt.Write(bytesZeros, 0, quant);
                            quant--;
                        }
                        //using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        //{

                        //swEncrypt.Write(plainText);


                        //swEncrypt.BaseStream.Write(plainText2, 0, plainText2.Length);

                        /*
                        int x = 0;
                        while (x < plainText2.Length) {
                            swEncrypt.Write(Convert.ToChar(plainText2.ElementAt(x)));
                            //swEncrypt.BaseStream.Write(plainText2, x, 1);
                            x = x + 1;
                        }
                        /**/

                        //}
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }
        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        static byte[] DecryptStringFromBytes_Aes2(byte[] cipherText, byte[] Key, byte[] IV)
        {

            byte[] bufferDecrypt = new byte[cipherText.Length];

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                aesAlg.Padding = PaddingMode.None;
                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        csDecrypt.Read(bufferDecrypt, 0, bufferDecrypt.Length);
                    }
                }

            }

            return bufferDecrypt;

        }


        private void Button3_Click(object sender, EventArgs e)
        {
            client.Close();
        }

        private void TextBoxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            TesteReg();
        }
    }

    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }


}
