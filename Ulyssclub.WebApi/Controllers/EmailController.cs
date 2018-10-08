using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using NETCore.MailKit.Core;
using Newtonsoft.Json;
using Ulyssclub.WebApi.Models;

namespace Ulyssclub.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {       
        private readonly IHostingEnvironment _appEnvironment;

        public EmailController(IHostingEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }

        [HttpPost("SendEmail")]
        public async void SendEmail(IFormFile file, [FromForm]string form)
        {
            var files = Request.Form.Files;
            EmailViewModel emailViewModel = JsonConvert.DeserializeObject<EmailViewModel>(form);

            var ext = Path.GetExtension(files[0].FileName);

            String fileName = files[0].Name + Guid.NewGuid().ToString();

            string path = "/Files/" + fileName + ext;

            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
            {
                await files[0].CopyToAsync(fileStream);
            }

            var mail = $"Имя: {emailViewModel.Name}\n" +
                $"Город: {emailViewModel.City}\n" +
                $"Род деятельности: {emailViewModel.Сareer}\n" +
                $"Ссылка на Facebook: {emailViewModel.FacebookURL}\n" +
                $"Предсказание: {emailViewModel.Forecast}\n" +
                $"Дата: {emailViewModel.Date}\n" +
                $"Время: {emailViewModel.Hours} : {emailViewModel.Minutes}\n" +
                $"Подтверждение предсказания: {emailViewModel.Confirmation}\n";

            byte[] bytes = Encoding.Default.GetBytes(mail);
            mail = Encoding.UTF8.GetString(bytes);

            var builder = new BodyBuilder { TextBody = mail };

            builder.Attachments.Add(_appEnvironment.WebRootPath + path);

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("", "ulyssclub@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", "ulyssclub@gmail.com"));
            emailMessage.Subject = "Акция";
            emailMessage.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync("email@gmail.com", "password");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }   
    } 
}