using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColetaAfde
{
    public class ParametroConfiguracaoEquipamento
    {
        private String parametro;
        private String valor;

        public ParametroConfiguracaoEquipamento(String parametro, String valor)
        {
            this.parametro = parametro;
            this.valor = valor;
        }

        public String getParametro()
        {
            return parametro;
        }

        public void setParametro(String parametro)
        {
            this.parametro = parametro;
        }

        public String getValor()
        {
            return valor;
        }

        public void setValor(String valor)
        {
            this.valor = valor;
        }
    }
}
