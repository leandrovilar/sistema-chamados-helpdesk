using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SistemaChamados.Models;

namespace SistemaChamados.Rules;

public static class TransicoesStatus
{
    private static readonly Dictionary<StatusChamado, List<StatusChamado>> TransicoesValidas = new()
    {
        [StatusChamado.Aberto] = new List<StatusChamado>
        {
            StatusChamado.EmAndamento
        },

        [StatusChamado.EmAndamento] = new List<StatusChamado>
        {
            StatusChamado.Resolvido
        },

        [StatusChamado.Resolvido] = new List<StatusChamado>
        {
            StatusChamado.Fechado
        },

        [StatusChamado.Fechado] = new List<StatusChamado>()
    };

    public static bool PodeAlterar(StatusChamado statusAtual, StatusChamado novoStatus)
    {
        return TransicoesValidas[statusAtual].Contains(novoStatus);
    }

    public static string ObterTransicoesPermitidas(StatusChamado statusAtual)
    {
        var transicoes = TransicoesValidas[statusAtual];

        if (transicoes.Count == 0)
        {
            return "Nenhuma transição é permitida. O chamado está fechado.";
        }

        return string.Join(", ", transicoes);
    }
}