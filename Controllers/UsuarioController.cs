﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.JsonRetorno;
using WebApi.Models;
using WebApi.Repositorio;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : Controller
    {

        readonly dynamic retornoJSON = new Retorno();

        private UserManager<UsuarioIdentity> _userManager;
        private SignInManager<UsuarioIdentity> _signInManager;
        private readonly ApplicationSettings _appSettings;

        public UsuarioController(UserManager<UsuarioIdentity> userManager, SignInManager<UsuarioIdentity> signInManager, IOptions<ApplicationSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        [Route("cadastro")]
        public async Task<JsonResult> Cadastrar(UsuarioIdentityModel request)
        {
            var novoUsuario = new UsuarioIdentity()
            {
                Id = string.Empty,
                UserName = request.UserName,
                Email = request.Email,
                FullName = request.FullName,
                City = request.City,
                Address = request.Address,
                AddressNumber = request.AddressNumber
            };

            #region Validacao
            bool existeEmail = (await _userManager.FindByEmailAsync(request.Email) == null);
            bool existeEmailUserName = (await _userManager.FindByNameAsync(request.UserName) == null);

            if (!existeEmail && !existeEmailUserName)
            {
                retornoJSON.Validado = true;
                retornoJSON.Mensagem = "Login ou Email já cadastrado!";
                return Json(retornoJSON);
            }
            #endregion

            try
            {
                var result = await _userManager.CreateAsync(novoUsuario, request.Password);
                if (result.Succeeded)
                {
                    retornoJSON.Validado = true;
                    retornoJSON.Mensagem = "Usuario cadastrado";
                    return Json(retornoJSON);
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPut]
        public async Task<JsonResult> AtualizarCadastro(UsuarioIdentityModel request)
        {
            UsuarioIdentity dadosUsuario = await _userManager.FindByIdAsync(request.Id);

            dadosUsuario.City = request.City;
            dadosUsuario.Address = request.Address;
            dadosUsuario.AddressNumber = request.AddressNumber;

            try
            {
                var resultadoTransacao = await _userManager.UpdateAsync(dadosUsuario);
                if (resultadoTransacao.Succeeded)
                {
                    retornoJSON.Validado = true;
                    retornoJSON.Mensagem = "Dados atualizados!";
                    return Json(retornoJSON);
                }
                return Json(resultadoTransacao);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel request)
        {
            var usuario = await _userManager.FindByNameAsync(request.UserName);
            var checkPassword = await _userManager.CheckPasswordAsync(usuario, request.Password);

            if (usuario != null && checkPassword)
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UsuarioID",usuario.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(
                       new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_KEY)),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
            {
                retornoJSON.Validado = false;
                retornoJSON.Mensagem = "Login/Senha inválidos!";
                return Json(retornoJSON);
            }
        }

         

        [HttpGet]
        [Authorize]
        public async Task<Object> GetDados()
        {
            string usuarioId = User.Claims.First(c => c.Type == "UsuarioID").Value;
            var user = await _userManager.FindByIdAsync(usuarioId);

            return new
            {
                user.FullName,
                user.Email,
                user.UserName,
                user.Id,
                user.City,
                user.Address,
                user.AddressNumber
            };

        }

    }
}
