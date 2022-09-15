
## Explicativo do protocolo do relógio (REP) Henry de forma detalhada

Inicialmente é utilizada uma chave assimétrica(chave pública) para realizar a autenticação, e após esse comando os dados são criptografados com criptografia simétrica.

São utilizadas criptografia *RSA 1024 bits*, e *AES CBC 16*.


Programa de exemplo fará a seguinte consulta no aparelho:
01+RR+00+N]5]1
Verificar Excel com o significado da consulta

**O processo para autenticação no Hexa é basicamente o seguinte:**
1. O software envia o comando RA.
2. O equipamento devolve a sua chave pública RSA.
3. O software deverá montar o pacote contendo o login, senha e chave AES (em base64) que será usada nos próximos comandos:
  3.1- Gerar uma chave AES aleatória
  3.2- Encodar em base64.
  3.3- Criar o pacote de dados. Exemplo: `"1]testefabrica]111111]MTExMTExMTExMTExMTExMQ=="`
  3.4- Criptografar esse pacote com a chave pública RSA recebida do equipamento no comando 2.
  3.5- Encodar o pacote criptografado em base64.
  3.6- Enviar o comando `EA. Ex: EA+00+<DADOS ACIMA CRIPTOGRAFADOS COM RSA E EM BASE64>`
1. À partir deste comando o usuário deverá criptografar os pacotes usando AES CBC com a chave informada no comando EA.

Apenas o pacote de dados deve ser criptografado com o AES. O Start Byte, tamanho do pacote, Checksum e End byte devem ser enviados normalmente.

Ex:

`0x02 0x?? 0x?? <DADOS CRIPTOGRAFADOS> 0xCS 0x03`



#### Observações importantes:
- O pacote de dados criptografados com AES CBC sempre será múltiplo de 16 bytes.
- Os primeiros 16 bytes do pacote criptografado são o Initial Value (IV - necessário para descriptografar).
- Se o pacote de dados não completar um múltiplo de 16, devem ser acrescentados bytes 0x00 até alcançar o tamanho necessário.
- A chave AES informada pode ser trocada a cada sessão aberta.
- O equipamento mantém a conexão por até 20 minutos de ociosidade, após isso será necessária uma nova autenticação.


###### Sugestão:
- Entender como funciona criptografia RSA e AES.


--------------------------
## Tecnologia
- Windows Form
- C#
- .Net Framework 4.8

