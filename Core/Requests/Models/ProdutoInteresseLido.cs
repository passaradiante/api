using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Models
{
    public class ProdutoInteresseLido
    {
        public int idNotificacao { get; set; }
    }
}