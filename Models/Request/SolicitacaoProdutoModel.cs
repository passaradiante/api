
using System;

namespace WebApi.Models
{
    public class SolicitacaoProdutoModel
    {
        public int ProdutoInteresseId { get; set; }
        public int StatusId { get; set; }
        public DateTime? DataTermino { get; set; }
        public int? MotivoCancelamentoId { get; set; }
    }
}
