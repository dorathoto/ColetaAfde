using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColetaAfde
{
    public class ListaConfiguracaoEquipamento
    {
        private List<ParametroConfiguracaoEquipamento> listaParametros = new List<ParametroConfiguracaoEquipamento>();

        public ListaConfiguracaoEquipamento()
        {

        }

        public List<ParametroConfiguracaoEquipamento> getListaParametros()
        {
            return listaParametros;
        }

        public List<ParametroConfiguracaoEquipamento> addParametrosString(String parametros)
        {

            String[] lista = parametros.Trim().Split("" + Command.SEP_PARAMETER);

            String strAux;

            if (lista.Length > 0)
            {
                for (int i = 0; i < lista.Length; i++)
                {
                    if (lista[i].Length > 0)
                    {
                        strAux = lista[i] + Command.SEP_ATTRIBUTE;
                        String[] params = strAux.Split("\\" + Command.SEP_ATTRIBUTE);
                        if (params.Length > 0) {
                    listaParametros.Add(new ParametroConfiguracaoEquipamento(params[0], params[1]));
                }
            }
        }
    }
        
        return listaParametros;
    }

public void addParametro(ParametroConfiguracaoEquipamento parametroConfiguracao)
{
    listaParametros.Add(parametroConfiguracao);
}
    }
}
